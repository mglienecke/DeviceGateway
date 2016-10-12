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
    public class CndepDeviceCommunicationHander:IDeviceCommunicationHandler
    {
        #region Constants...
        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgCndepContentType = "CndepDeviceCommunicationHander.Cndep.ContentType";

        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgCndepDefaultRemoteServerPort = "CndepDeviceCommunicationHander.Cndep.DefaultRemoteServerPort";

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

        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgCommunicationProtocol = "CndepDeviceCommunicationHander.Cndep.CommunicationProtocol";
        /// <summary>
        /// Configuration property default value.
        /// </summary>
        public const CommunicationProtocol DefaultCommunicationProtocol = CommunicationProtocol.UDP;
        /// <summary>
        /// Communication protocol name.
        /// </summary>
        public static readonly string CommunicationProtocolUdp = CommunicationProtocol.UDP.ToString();
        /// <summary>
        /// Communication protocol name.
        /// </summary>
        public static readonly string CommunicationProtocolTcp = CommunicationProtocol.TCP.ToString();

        private static readonly string[] ContentType = new string[] { "application/json" };
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static byte SessionId = 0;
        private static object SessionIdLock = new object();

        private int mRemoteServerPort;
        private int mTimeout;
        private string mContentType;
        private int mRequestRetryCount;
        private CommunicationProtocol _communincationProtocol = DefaultCommunicationProtocol;

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
        public CndepDeviceCommunicationHander()
        {
            try
            {
                //Figure out the port
                string portStr = ConfigurationManager.AppSettings[CfgCndepDefaultRemoteServerPort];
                mRemoteServerPort = portStr == null ? DefaultCndepRemoteServerPort : Int32.Parse(portStr);
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed obtaining settings from the app.config. Key: {0}; Error: {1}.\nDefault value will be used: {2}",
                                CfgCndepDefaultRemoteServerPort, exc.Message, DefaultCndepRemoteServerPort);
                mRemoteServerPort = DefaultCndepRemoteServerPort;
            }

            try
            {
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

            try
            {
                //Communication protocol
                string valueStr = ConfigurationManager.AppSettings[CfgCommunicationProtocol];
                if (valueStr != null)
                    _communincationProtocol = (CommunicationProtocol) Enum.Parse(typeof (CommunicationProtocol), valueStr);
                else
                    _communincationProtocol = DefaultCommunicationProtocol;
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed obtaining settings from the app.config. Key: {0} Error: {1}.\nDefault value will be used: {2}",
                                CfgCndepTimeout, exc.Message, DefaultCndepRequestRetryCount);
                mRequestRetryCount = DefaultCndepRequestRetryCount;
            }
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
                IPAddress deviceAddress = IPAddress.Parse(GetIpAddress(device.DeviceIpEndPoint));
                byte[] data = Encoding.UTF8.GetBytes(sensor.Id);

                SensorData sensorData = null;

                using (CndepClient udpClient = GetCndepClient(deviceAddress, GetRemoteServerPort(device.DeviceIpEndPoint)))
                {
                    //Create message
                    CndepMessageRequest request = new CndepMessageRequest(GetSessionId(), 
                        CndepCommands.CmdGetSensorData, 
                        CndepCommands.FncGetSensorDataCurrentValue, 
                        data);

                    //Connect and send in the sync mode
                    udpClient.Open();

                    CndepMessageResponse response = null;
                    int retryCount = 0;

                    do
                    {
                        response = udpClient.Send(request, mTimeout);

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
            try
            {
                //Device address
                IPAddress deviceAddress = IPAddress.Parse(GetIpAddress(device.DeviceIpEndPoint));
                IContentParser contentParser = HttpHandlerUtils.FindParser(ContentType);
                byte[] requestData = Encoding.UTF8.GetBytes(contentParser.Encode(new PutActuatorDataRequest(){SensorId  = sensor.Id, Data = data}));


                using (CndepClient udpClient = GetCndepClient(deviceAddress, GetRemoteServerPort(device.DeviceIpEndPoint)))
                {
                    //Create message
                    CndepMessageRequest request = new CndepMessageRequest(GetSessionId(), CndepCommands.CmdPutActuatorValue, 0, requestData);

                    //Connect and send in the sync mode
                    udpClient.Open();

                    CndepMessageResponse response = null;
                    int retryCount = 0;

                    do
                    {
                        response = udpClient.Send(request, mTimeout);

                        if (response != null)
                        {
                            switch (response.ResponseId)
                            {
                                case CndepCommands.RspOK:
                                    //Get data
                                    string responseStr = Encoding.UTF8.GetString(response.Data, 0, response.Data.Length);

                                    //Check data
                                    if (String.IsNullOrEmpty(responseStr))
                                        throw new Exception(String.Format("Device replied with an empty string when setting the device's actuator value. Device id: {0}; Sensor id: {1}",
                                            device.Id, sensor.Id));

                                    //Parse data
                                    return (OperationResult)ContentParserFactory.GetParser(mContentType).Decode(responseStr, typeof(OperationResult));
                                    break;
                                case CndepCommands.RspError:
                                    string errorMessage = Encoding.UTF8.GetString(response.Data, 0, response.Data.Length);
                                    throw new Exception(String.Format("Device replied with an error when setting the device's actuator value. Device id: {0}; Sensor id: {1}; Error: {2}",
                                        device.Id, sensor.Id, errorMessage));
                                default:
                                    throw new Exception(String.Format("Unknown CNDEP response code received while setting the device's actuator value. Device id: {0}; Sensor id: {1}; Response code: {2}",
                                        device.Id, sensor.Id, response.ResponseId));
                                    break;
                            }
                        }
                    }
                    while (response == null && retryCount++ < mRequestRetryCount);
                }

                throw new Exception(String.Format("No response received. Request retry count exceeded. Device id: {0}; Sensor id: {1}", device.Id, sensor.Id));
            }
            catch (Exception exc)
            {
                log.Error(String.Format(Properties.Resources.ExceptionFailedSettingActuatorValueUsingCndep, device.Id, sensor.Id), exc);
                throw;
            }
        }

        /// <summary>
        /// The property getter returns a flag that defines if the objects of the implementation class are reusable and thread-safe
        /// (or should be instantiated every time before use)
        /// </summary>
        public bool IsReusableAndThreadSafe { get { return true; } }
        #endregion

        private string GetIpAddress(string deviceIpEndpoint)
        {
            int pos = deviceIpEndpoint.IndexOf(':');
            if (pos >= 0)
            {
                if (pos == 0 || pos == deviceIpEndpoint.Length - 1)
                    throw new ArgumentException(String.Format("Invalid device IP endpoint value: {0}", deviceIpEndpoint));

                return deviceIpEndpoint.Substring(0, pos);
            }

            //Return the original value
            return deviceIpEndpoint;
        }

        private Int32 GetRemoteServerPort(string deviceIpEndPoint)
        {
            int pos = deviceIpEndPoint.IndexOf(':');
            if (pos >= 0)
            {
                if (pos == 0 || pos == deviceIpEndPoint.Length-1)
                    throw new ArgumentException(String.Format("Invalid device IP endpoint value: {0}", deviceIpEndPoint));

                return Int32.Parse(deviceIpEndPoint.Substring(pos + 1));
            }

            //Return the default value
            return mRemoteServerPort;
        }

        private CndepClient GetCndepClient(IPAddress serverAddress, int serverPort)
        {
            CndepClient client;
            switch (_communincationProtocol)
            {
                case CommunicationProtocol.UDP:
                    client = new CndepUdpClient(serverPort, serverAddress);
                    break;
                case CommunicationProtocol.TCP:
                    client = new CndepTcpClient(serverPort, serverAddress);
                    break;
                default:
                    throw new ConfigurationErrorsException(String.Format("Unhandled CommunicationProtocol value: {0}.", _communincationProtocol));
            }

            return client;
        }
    }
}
