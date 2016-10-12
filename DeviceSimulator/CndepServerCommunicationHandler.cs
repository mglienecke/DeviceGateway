using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.Server;
using Common.Server.CNDEP;


namespace DeviceServer.Simulator
{
    /// <summary>
    /// The class implements the <see cref="IServerCommunicationHandler"/> interface using communication over CDNEP.
    /// </summary>
    public class CndepServerCommunicationHandler : IServerCommunicationHandler
    {
        /// <summary>
        /// Number of a local port to be used to receiving calls from the server.
        /// </summary>
        public const string CfgLocalPort = "CndepServerCommunicationHandler.LocalPort";

        /// <summary>
        /// Number of a server port to be used to sending calls to the server.
        /// </summary>
        public const string CfgServerPort = "CndepServerCommunicationHandler.ServerPort";

        /// <summary>
        /// Network protocol to be used to sending calls to the server.
        /// </summary>
        public const string CfgCommunicationProtocol = "CndepServerCommunicationHandler.CommunicationProtocol";

        /// <summary>
        /// Server IP address to be used to sending calls to the server.
        /// </summary>
        public const string CfgServerIpAddress = "CndepServerCommunicationHandler.ServerIpAddress";

        /// <summary>
        /// Number of retries for failed requests to the central server.
        /// </summary>
        public const string CfgRequestRetryCount = "CndepServerCommunicationHandler.RequestRetryCount";

        /// <summary>
        /// Request timeout for calls to the central server.
        /// </summary>
        public const string CfgRequestTimeout = "CndepServerCommunicationHandler.RequestTimeout";

        /// <summary>
        /// Default port number for the device-side communication server.
        /// </summary>
        public const UInt16 DefaultLocalPort = 41115;

        /// <summary>
        /// Default request retry count.
        /// </summary>
        public const int DefaultRequestRetryCount = 1;

        /// <summary>
        /// Default request timeout for calls to the central server.
        /// </summary>
        public const int DefaultRequestTimeout = 10000;

        /// <summary>
        /// Default network communication protocol.
        /// </summary>
        public const CommunicationProtocol DefaultCommunicationProtocol = CommunicationProtocol.UDP;

        private DeviceServerSimulator _masterDevice;
        private CndepDeviceServer _localCndepServer;
        private UInt16 _localPort = DefaultLocalPort;
        private UInt16 _serverPort;
        private string _serverIpAddress;
        private int _requestRetryCount;
        private int _requestTimeout;

        //private 
        private static readonly object LockObject = new object();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CndepServerCommunicationHandler()
        {
            LocalPort = String.IsNullOrEmpty(ConfigurationManager.AppSettings[CfgLocalPort])
                            ? DefaultLocalPort
                            : UInt16.Parse(ConfigurationManager.AppSettings[CfgLocalPort]);

            ServerPort = UInt16.Parse(ConfigurationManager.AppSettings[CfgServerPort]);

            ServerIpAddress = ConfigurationManager.AppSettings[CfgServerIpAddress];
            string serverIp = ConfigurationManager.AppSettings[CfgServerIpAddress];
            if (serverIp.Contains(DeviceServerSimulator.LocalHostMacro))
            {
                // find the real local IP address of the machine
                serverIp = NetworkUtilities.GetIpV4AddressForDns(Dns.GetHostName()).ToString();

                //// replace the macro with the real IP address (as this has to be called from the server) and append the port
                //defaultDeviceIp = defaultDeviceIp.IndexOf(':') >= 0
                //                      ? localIp + ":" + defaultDeviceIp.Split(':')[1]
                //                      : localIp;
            }

            ServerIpAddress = serverIp;

            RequestRetryCount = String.IsNullOrEmpty(ConfigurationManager.AppSettings[CfgRequestRetryCount])
                ? DefaultRequestRetryCount
                : Int32.Parse(ConfigurationManager.AppSettings[CfgRequestRetryCount]);

            RequestTimeout = String.IsNullOrEmpty(ConfigurationManager.AppSettings[CfgRequestTimeout])
                ? DefaultRequestTimeout
                : Int32.Parse(ConfigurationManager.AppSettings[CfgRequestTimeout]);

            CommunicationProtocol = String.IsNullOrEmpty(ConfigurationManager.AppSettings[CfgCommunicationProtocol])
                                        ? DefaultCommunicationProtocol
                                        : (CommunicationProtocol)Enum.Parse(typeof (CommunicationProtocol),
                                                     ConfigurationManager.AppSettings[CfgCommunicationProtocol]);

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="server"></param>
        public CndepServerCommunicationHandler(DeviceServerSimulator server)
            : this()
        {
            _masterDevice = server;
        }

        #region IServerCommunicationHandler members...

        /// <summary>
        /// The propery contains the port number to be used by the device for accepting incoming requests.
        /// </summary>
        public UInt16 LocalPort
        {
            get { return _localPort; }
            set
            {
                if (value <= 0 || value > UInt16.MaxValue)
                    throw new ArgumentOutOfRangeException("value");

                _localPort = value;
            }
        }

        /// <summary>
        /// The propery contains the server port number to be used by the device for sendibng requests.
        /// </summary>
        public UInt16 ServerPort
        {
            get { return _serverPort; }
            set
            {
                if (value <= 0 || value > UInt16.MaxValue)
                    throw new ArgumentOutOfRangeException("value");

                _serverPort = value;
            }
        }

        DeviceServerSimulator IServerCommunicationHandler.Server
        {
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _masterDevice = value;
            }
        }

        private CommunicationProtocol CommunicationProtocol { get; set; }

        private int RequestTimeout
        {
            get { return _requestTimeout; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("RequestTimeout");

                _requestTimeout = value;
            }
        }

        private int RequestRetryCount
        {
            get { return _requestRetryCount; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("RequestRetryCount");

                _requestRetryCount = value;
            }
        }

        private string ServerIpAddress
        {
            get { return _serverIpAddress; }
            set
            {
                if (String.IsNullOrEmpty(value))
                    throw new ArgumentNullException("ServerIpAddress");

                _serverIpAddress = value;
            }
        }

        bool IServerCommunicationHandler.StoreSensorData(DeviceConfig config, Device device, Sensor sensor, SensorData data)
        {
            try
            {
                object responseObject;
                StoreSensorDataResult resultTemplate = new StoreSensorDataResult();

                var request = new StoreSensorDataRequest()
                    {
                        Data = new MultipleSensorData[] {new MultipleSensorData() {SensorId = sensor.Id, Measures = new SensorData[] {data}}},
                        DeviceId = device.Id
                    };

                if (SendCndepRequest(ServerIpAddress, ServerPort, CndepCommands.CmdStoreSensorData, 0,
                                     Common.Server.ContentParserFactory.GetParser(config.ServerContentType).Encode(request), out responseObject))
                {
                    //Check the response
                    return HandleOperationResultResponse(config, responseObject as StoreSensorDataResult, Properties.Resources.ErrorFailedSendingSensorDataOverCndep);
                }
                else
                {
                    DeviceServerSimulator.RunningInstance.LogError(null, Properties.Resources.ErrorFailedSendingSensorDataOverCndep, responseObject.ToString());
                    return false;
                }
            }
            catch (Exception exc)
            {
                DeviceServerSimulator.RunningInstance.LogError(exc, Properties.Resources.ErrorFailedSendingSensorDataOverCndep, exc.Message);
                return false;
            }
        }

        bool IServerCommunicationHandler.RegisterDevice(DeviceConfig config, Device device)
        {
            try
            {
                object responseObject;

                if (SendCndepRequest(ServerIpAddress, ServerPort, CndepCommands.CmdRegisterDevice, 0,
                                        ContentParserFactory.GetParser(config.ServerContentType).CreatePutDeviceRequestContent(device),
                                        out responseObject))
                {
                    //Check the response
                    return HandleOperationResultResponse(config, responseObject, Properties.Resources.ErrorFailedRefsreshingDeviceRegistration);
                }
                else
                {
                    DeviceServerSimulator.RunningInstance.LogError(null, Properties.Resources.ErrorFailedRefsreshingDeviceRegistration, responseObject.ToString());
                    return false;
                }
            }
            catch (Exception exc)
            {
                DeviceServerSimulator.RunningInstance.LogError(exc, Properties.Resources.ErrorFailedRefsreshingDeviceRegistration, exc.Message);
                return false;
            }
        }

        bool IServerCommunicationHandler.RegisterSensors(DeviceConfig config, Device device, System.Collections.ArrayList sensors)
        {
            try
            {
                object responseObject;

                if (SendCndepRequest(ServerIpAddress, ServerPort, CndepCommands.CmdRegisterSensors, 0,
                                        ContentParserFactory.GetParser(config.ServerContentType).CreatePutSensorsRequestContent(sensors),
                                        out responseObject))
                {
                    //Check the response
                    return HandleOperationResultResponse(config, responseObject, Properties.Resources.ErrorFailedRefsreshingDeviceSensorsRegistration);
                }
                else
                {
                    DeviceServerSimulator.RunningInstance.LogError(null, Properties.Resources.ErrorFailedRefsreshingDeviceSensorsRegistration, responseObject.ToString());
                    return false;
                }
            }
            catch (Exception exc)
            {
                DeviceServerSimulator.RunningInstance.LogError(exc, Properties.Resources.ErrorFailedRefsreshingDeviceSensorsRegistration, exc.Message);
                return false;
            }
        }

        bool IServerCommunicationHandler.Init()
        {
            try
            {
                _localCndepServer = new CndepDeviceServer() { LocalPort = LocalPort, ContentType = _masterDevice.Config.ServerContentType, CommunicationProtocol = CommunicationProtocol };
                return true;
            }
            catch (Exception exc)
            {
                DeviceServerSimulator.RunningInstance.LogError(exc, "Failed initializing Device CNDEP server.");
                return false;
            }
        }

        bool IServerCommunicationHandler.StartRequestHandling()
        {
            try
            {
                _localCndepServer.Start();
                return true;
            }
            catch (Exception exc)
            {
                DeviceServerSimulator.RunningInstance.LogError(exc, "Failed stating Device CNDEP server.");
                return false;
            }
        }

        bool IServerCommunicationHandler.StopRequestHandling()
        {
            try
            {
                if (_localCndepServer != null)
                {
                    _localCndepServer.Stop();
                }
                return true;
            }
            catch (Exception exc)
            {
                DeviceServerSimulator.RunningInstance.LogError(exc, "Failed stopping Device CNDEP server.");
                return false;
            }
        }

        #endregion

        private bool SendCndepRequest(string serverAddress, int serverPort, byte command, byte function, string requestData, out object result)
        {
            try
            {
                using (CndepClient udpClient = GetCndepClient(serverPort, Dns.GetHostAddresses(serverAddress)[0]))
                {
                    byte[] data = UTF8Encoding.UTF8.GetBytes(requestData);

                    //Create message
                    var request = new CndepMessageRequest(GetSessionId(), command, function, data);

                    //Connect and send in the sync mode
                    udpClient.Open();

                    CndepMessageResponse response = null;
                    int retryCount = 0;

                    do
                    {
                        response = udpClient.Send(request, RequestTimeout);

                        if (response != null)
                        {
                            string responseDataStr;

                            switch (response.ResponseId)
                            {
                                case CndepCommands.RspOK:
                                case CndepCommands.RspError:
                                    //Get data
                                    responseDataStr = UTF8Encoding.UTF8.GetString(response.Data, 0, response.Data.Length);

                                    //Check data
                                    if (responseDataStr == null || responseDataStr.Length == 0)
                                    {
                                        result = String.Format("Server replied with an empty string.");
                                        return false;
                                    }

                                    using (var reader = new StringReader(responseDataStr))
                                    {
                                        result = ContentParserFactory.GetParser(_masterDevice.Config.ServerContentType).ParseOperationResult(reader.ReadToEnd());
                                    }
                                    return true;
                                default:
                                    result = String.Format("Unknown CNDEP response code received. Response code: {0}",
                                                           response.ResponseId);
                                    return false;
                            }
                        }
                    } while (response == null && retryCount++ < RequestRetryCount);

                    result = "Response timeout";
                    return false;
                }

            }
            catch (Exception exc)
            {
                result = String.Format("CNDEP request failed. Error: {0}", exc.Message);
                return false;
            }
        }

        private CndepClient GetCndepClient(int serverPort, IPAddress serverAddress)
        {
            CndepClient client;
            switch (CommunicationProtocol)
            {
                case CommunicationProtocol.UDP:
                    client = new CndepUdpClient(serverPort, serverAddress);
                    break;
                case CommunicationProtocol.TCP:
                    client = new CndepTcpClient(serverPort, serverAddress);
                    break;
                default:
                    throw new Exception(String.Format("Unhandled CommunicationProtocol value: {0}.", CommunicationProtocol));
            }

            return client;
        }

        private static volatile byte _sessionId;

        private static byte GetSessionId()
        {
            lock (LockObject)
            {
                return _sessionId++;
            }
        }

        private bool HandleOperationResultResponse(DeviceConfig config, object responseObject, string baseErrorMessage)
        {
            if (responseObject != null)
            {
                var opResult = (OperationResult) responseObject;
                //Read the server time.
                if (opResult.Timestamp != 0)
                {
                    _masterDevice.ServerTimeMark = DeviceServerSimulator.TimeZero.AddTicks(opResult.Timestamp);
                    _masterDevice.ServerTimeOffset = _masterDevice.ServerTimeMark.Ticks - DateTime.Now.Ticks;
                }

                //Success or not?
                if (opResult.Success)
                {
                    //OK
                    return true;
                }
                
                //Log
                _masterDevice.LogError(null, baseErrorMessage, opResult.ErrorMessages);
            }
            else
            {
                //Log
                _masterDevice.LogError(null, baseErrorMessage, Properties.Resources.NoResult);
            }

            return false;
        }

        private bool HandleOperationResultResponse(DeviceConfig config, StoreSensorDataResult responseObject, string baseErrorMessage)
        {
            if (responseObject != null)
            {
                var opResult = (OperationResult)responseObject;
                //Read the server time.
                if (opResult.Timestamp != 0)
                {
                    _masterDevice.ServerTimeMark = DeviceServerSimulator.TimeZero.AddTicks(opResult.Timestamp);
                    _masterDevice.ServerTimeOffset = _masterDevice.ServerTimeMark.Ticks - DateTime.Now.Ticks;
                }

                //Success or not?
                if (opResult.Success)
                {
                    //OK
                    return true;
                }

                //Log
                _masterDevice.LogError(null, baseErrorMessage, opResult.ErrorMessages);
            }
            else
            {
                //Log
                _masterDevice.LogError(null, baseErrorMessage, Properties.Resources.NoResult);
            }

            return false;
        }
    }
}
