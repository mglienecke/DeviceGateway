using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalDataContracts;
using System.Configuration;
using System.Net;

namespace Common.Server.CNDEP
{
    /// <summary>
    /// The class implements the CNDEP-based <see cref="IDeviceCommunicationHandler"/> implementation.
    /// </summary>
    public class CndepTcpDeviceCommunicationHander:IDeviceCommunicationHandler
    {
        #region Constants...
        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgCndepContentType = "CndepDeviceCommunicationHander.Cndep.ContentType";

        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgCndepRemoteServerPort = "CndepDeviceCommunicationHander.Cndep.RemoteServerPort";

        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgCndepTimeout = "CndepDeviceCommunicationHander.Cndep.Timeout";

        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgCndepRequestRetryCount = "CndepDeviceCommunicationHander.Cndep.RequestRetryCount";

        /// <summary>
        /// Default CNDEP device port
        /// </summary>
        public const int DefaultCndepRemoteServerPort = 41120;

        /// <summary>
        /// Default CNDEP timeout (in millis)
        /// </summary>
        public const int DefaultCndepTimeout = 10000;

        /// <summary>
        /// Default CNDEP request retry count.
        /// </summary>
        public const int DefaultCndepRequestRetryCount = 1;

        /// <summary>
        /// Default CNDEP content type
        /// </summary>
        public const string DefaultCndepContentType = JsonContentParser.ContentType;
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static byte SessionId = 0;
        private static object SessionIdLock = new object();

        private int mRemoteServerPort;
        private int mTimeout;
        private string mContentType;
        private int mRequestRetryCount;

        /// <summary>
        /// Returns a unique session id.
        /// </summary>
        /// <returns></returns>
        private static byte GetSessionId()
        {
            lock (SessionIdLock)
            {
                //Wraps back to 0
                return ++SessionId;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CndepTcpDeviceCommunicationHander()
        {
            try
            {
                //Figure out the port
                string portStr = ConfigurationManager.AppSettings[CfgCndepRemoteServerPort];
                mRemoteServerPort = portStr == null ? DefaultCndepRemoteServerPort : Int32.Parse(portStr);
                }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed obtaining settings from the app.config. Key: {0}; Error: {1}.\nDefault value will be used: {2}", 
                    CfgCndepRemoteServerPort, exc.Message, DefaultCndepRemoteServerPort);
                mRemoteServerPort = DefaultCndepRemoteServerPort;
            }

            try{
                //Figure out the timeout
                string timeoutStr = ConfigurationManager.AppSettings[CfgCndepTimeout];
                mTimeout = timeoutStr == null ? DefaultCndepTimeout : Int32.Parse(timeoutStr);
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed obtaining settings from the app.config. Key: {0} Error: {1}.\nDefault value will be used: {2}", 
                    CfgCndepTimeout, exc.Message, DefaultCndepTimeout);
                mTimeout = DefaultCndepTimeout;
            }

            try
            {
                //Figure out the request retry count
                string countStr = ConfigurationManager.AppSettings[CfgCndepRequestRetryCount];
                mRequestRetryCount = countStr == null ? DefaultCndepRequestRetryCount : Int32.Parse(countStr);
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed obtaining settings from the app.config. Key: {0} Error: {1}.\nDefault value will be used: {2}",
                    CfgCndepTimeout, exc.Message, DefaultCndepRequestRetryCount);
                mRequestRetryCount = DefaultCndepRequestRetryCount;
            }

            //Figure out the content type
            mContentType = ConfigurationManager.AppSettings[CfgCndepContentType];
            if (mContentType == null) mContentType = DefaultCndepContentType; 
        }
            
        #region IDeviceCommunicationHandler members...
        /// <summary>
        /// The method retrieves current data of the specified sensor that belongs to the specified device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="sensor"></param>
        /// <returns></returns>
        public SensorData GetSensorCurrentData(Device device, Sensor sensor)
        {
            try
            {
                //Device address
                IPAddress deviceAddress = IPAddress.Parse(device.DeviceIpEndPoint);
                byte[] data = UTF8Encoding.UTF8.GetBytes(sensor.Id);

                SensorData sensorData = null;

                using (CndepTcpClient tcpClient = new CndepTcpClient(mRemoteServerPort, deviceAddress))
                {
                    //Create message
                    CndepMessageRequest request = new CndepMessageRequest(GetSessionId(), 
                        CndepCommands.CmdGetSensorData, 
                        CndepCommands.FncGetSensorDataCurrentValue, 
                        data);

                    //Connect and send in the sync mode
                    tcpClient.Open();

                    CndepMessageResponse response = null;
                    int retryCount = 0;

                    do
                    {
                        response = tcpClient.Send(request, mTimeout);

                        if (response != null)
                        {
                            switch (response.ResponseId)
                            {
                                case CndepCommands.RspOK:
                                    //Get data
                                    string sensorDataStr = Encoding.UTF8.GetString(response.Data, 0, response.Data.Length);

                                    //Check data
                                    if (String.IsNullOrEmpty(sensorDataStr))
                                        throw new Exception(String.Format("Device replied with an empty string when getting the device's sensor current data. Device id: {0}; Sensor id: {1}",
                                            device.Id, sensor.Id));

                                    //Parse data
                                    sensorData = (SensorData)ContentParserFactory.GetParser(mContentType).Decode(sensorDataStr, typeof(SensorData));
                                    break;
                                case CndepCommands.RspError:
                                    string errorMessage = Encoding.UTF8.GetString(response.Data, 0, response.Data.Length);
                                    throw new Exception(String.Format("Device replied with an error when getting the device's sensor current data. Device id: {0}; Sensor id: {1}; Error: {2}",
                                        device.Id, sensor.Id, errorMessage));
                                default:
                                    log.ErrorFormat("Unknown CNDEP response code received while getting sensor's current data. Device id: {0}; Sensor id: {1}; Response code: {2}",
                                        device.Id, sensor.Id, response.ResponseId);
                                    break;
                            }
                        }
                    }
                    while (response == null && retryCount++ < mRequestRetryCount);
                }

                return sensorData;
            }
            catch (Exception exc)
            {
                log.Error(String.Format(Properties.Resources.ExceptionFailedObtainingSensorCurrentDataUsingCndep, device.Id, sensor.Id), exc);
                throw;
            }
        }

        /// <summary>
        /// The method puts the specified data to the specified sensor that belongs to the specified device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="sensor"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public OperationResult PutSensorCurrentData(Device device, Sensor sensor, SensorData data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The property getter returns a flag that defines if the objects of the implementation class are reusable and thread-safe
        /// (or should be instantiated every time before use)
        /// </summary>
        public bool IsReusableAndThreadSafe { get { return true; } }
        #endregion
    }
}
