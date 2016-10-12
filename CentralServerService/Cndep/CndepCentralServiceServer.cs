using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Server;
using Common.Server.CNDEP;
using System.Configuration;
using GlobalDataContracts;

namespace CentralServerService.Cndep
{
    /// <summary>
    /// The class implements a UDP-based CNDEP server to accept CentralService-related requests from devices.
    /// </summary>
    public class CndepCentralServiceServer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string[] ContentType = new string[] { "application/json" };

        /// <summary>
        /// Default property value.
        /// </summary>
        public const UInt16 DefaultLocalServerPort = 41120;
        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgServerPort = "CentralServerService.Cndep.LocalServerPort";
        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgCommunicationProtocol = "CentralServerService.Cndep.CommunicationProtocol";
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

        private CndepServer _server;
        private UInt16 mLocalServerPort = DefaultLocalServerPort;
        private CommunicationProtocol _communincationProtocol = DefaultCommunicationProtocol;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CndepCentralServiceServer()
        {
        }

        /// <summary>
        /// Local port for receiving.
        /// </summary>
        public UInt16 LocalServerPort
        {
            get
            {
                return mLocalServerPort;
            }
            set
            {
                if (value >= 0 && value <= UInt16.MaxValue)
                    mLocalServerPort = value;
                else
                    throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// Initializes the server.
        /// </summary>
        public void Init()
        {
            //Server port
            string valueStr = ConfigurationManager.AppSettings[CfgServerPort];
            if (valueStr != null)
                LocalServerPort = Convert.ToUInt16(valueStr);
            else
                LocalServerPort = DefaultLocalServerPort;

            //Communication protocol
            valueStr = ConfigurationManager.AppSettings[CfgCommunicationProtocol];
            if (valueStr != null)
                _communincationProtocol = (CommunicationProtocol)Enum.Parse(typeof(CommunicationProtocol), valueStr);
            else
                _communincationProtocol = DefaultCommunicationProtocol;
        }

        /// <summary>
        /// Starts server operations.
        /// </summary>
        public void Start()
        {
            if (_server != null)
                Stop();

            //Log
            log.Debug("Starting CentralService CNDEP server...");

            switch (_communincationProtocol)
            {
                case CommunicationProtocol.UDP:
                    _server = new CndepUdpServer(LocalServerPort);
                    break;
                case CommunicationProtocol.TCP:
                    _server = new CndepTcpServer(LocalServerPort);
                    break;
                default:
                    throw new ConfigurationErrorsException(String.Format("The configuration property {0} has invalid value: {1}.",
                                                                   CfgCommunicationProtocol, _communincationProtocol));
            }


            _server.RequestReceived += mServer_RequestReceived;
            _server.Open();

            //Log
            log.Debug("Started CentralService CNDEP server.");
        }

        /// <summary>
        /// Stops server operations.
        /// </summary>
        public void Stop()
        {
            //Log
            log.Debug("Closing CentralService CNDEP server...");

            _server.Close();
            _server = null;

            //Log
            log.Debug("Closed CentralService CNDEP server.");
        }

        void mServer_RequestReceived(object sender, RequestReceivedEvArgs e)
        {
            byte responseId;
            byte[] responseData;
            string errorMessage;

            IContentParser contentParser = HttpHandlerUtils.FindParser(ContentType);

            string content = e.Request.Data == null ? String.Empty : UTF8Encoding.UTF8.GetString(e.Request.Data, 0, e.Request.Data.Length);

            //Log
            log.DebugFormat("=> {0}: {1}({2}): {3}", e.SendingEndpoint.ToString(), e.Request.CommandId, e.Request.FunctionId, content);

            switch (e.Request.CommandId)
            {
                case CndepCommands.CmdStoreSensorData:
                    StoreSensorDataResult resultStore;
                    try
                    {
                        StoreSensorDataRequest requestStore = (StoreSensorDataRequest)contentParser.Decode(content, typeof(StoreSensorDataRequest));
                        TrackingPoint.TrackingPoint.CreateTrackingPoint("Server: CNDEP receive multiple", "before processing: " + _communincationProtocol, requestStore.Data.Length);
                        resultStore = CentralServerServiceImpl.Instance.StoreSensorData(requestStore.DeviceId, new List<MultipleSensorData>(requestStore.Data));
                        TrackingPoint.TrackingPoint.CreateTrackingPoint("Server: CNDEP receive multiple", "after processing: " + _communincationProtocol, requestStore.Data.Length);
                        responseData = Encoding.UTF8.GetBytes(contentParser.Encode(resultStore));
                        responseId = CndepCommands.RspOK;
                    }
                    catch (Exception exc)
                    {
                        errorMessage = String.Format("Failed storing sensor data. Device address: {0}; Error: {1}.", e.SendingEndpoint.Address, exc.Message);

                        //Log
                        //log.Error(errorMessage);

                        resultStore = new StoreSensorDataResult() { Success = false };
                        resultStore.AddError(errorMessage);

                        responseId = CndepCommands.RspError;
                        responseData = Encoding.UTF8.GetBytes(contentParser.Encode(resultStore));
                    }
                    break;
                case CndepCommands.CmdRegisterDevice:
                    OperationResult resultRegDevice;
                    try
                    {
                        Device requestRegDevice = (Device)contentParser.Decode(content, typeof(Device));

                        //Check if the device is already registered
                        bool isUsed = CentralServerServiceImpl.Instance.IsDeviceIdUsed(requestRegDevice.Id);

                        //Create or update
                        if (isUsed)
                        {
                            resultRegDevice = CentralServerServiceImpl.Instance.UpdateDevice(requestRegDevice);
                        }
                        else
                        {
                            resultRegDevice = CentralServerServiceImpl.Instance.RegisterDevice(requestRegDevice);
                        }
                        
                        responseData = UTF8Encoding.UTF8.GetBytes(contentParser.Encode(resultRegDevice));
                        responseId = CndepCommands.RspOK;
                    }
                    catch (Exception exc)
                    {
                        errorMessage = String.Format("Failed registering device. Device address: {0}; Error: {1}.", e.SendingEndpoint.Address, exc.Message);

                        //Log
                        //log.Error(errorMessage);

                        resultRegDevice = new OperationResult(){Success = false};
                        resultRegDevice.AddError(errorMessage);
                        responseId = CndepCommands.RspError;
                        responseData = UTF8Encoding.UTF8.GetBytes(contentParser.Encode(resultRegDevice));
                    }
                    break;
                case CndepCommands.CmdRegisterSensors:
                    OperationResult resultRegSensors;
                    try
                    {
                        Sensor[] requestRegSensors = (Sensor[])contentParser.Decode(content, typeof(Sensor[]));

                        if (requestRegSensors != null && requestRegSensors.Length > 0)
                        {
                            List<string> collectedErrorMessages = new List<string>();
                            Sensor[] sensorsParam = new Sensor[1];

                            for (int i = 0; i < requestRegSensors.Length; i++)
                            {
                                if (CentralServerServiceImpl.Instance.IsSensorIdRegisteredForDevice(requestRegSensors[i].DeviceId, requestRegSensors[i].Id))
                                {
                                    //Update
                                    resultRegSensors = CentralServerServiceImpl.Instance.UpdateSensor(requestRegSensors[i]);
                                    
                                    //Error?
                                    if (resultRegSensors.Success == false)
                                    {
                                        collectedErrorMessages.Add(resultRegSensors.ErrorMessages);
                                    }
                                }
                                else
                                {
                                    //Register
                                    sensorsParam[0] = requestRegSensors[i];
                                    resultRegSensors = CentralServerServiceImpl.Instance.RegisterSensors(sensorsParam);
                                    
                                    //Error?
                                    if (resultRegSensors.Success == false)
                                    {
                                        collectedErrorMessages.Add(resultRegSensors.ErrorMessages);
                                    }
                                }
                            }

                            //Form the result. Any errors?
                            if (collectedErrorMessages.Count > 0)
                            {
                                responseData = UTF8Encoding.UTF8.GetBytes(contentParser.Encode(new OperationResult(false, collectedErrorMessages)));
                                responseId = CndepCommands.RspError;
                            }
                            else
                            {
                                responseData = UTF8Encoding.UTF8.GetBytes(contentParser.Encode(new OperationResult()));
                                responseId = CndepCommands.RspOK;
                            }
                        }
                        else
                        {
                            responseData = UTF8Encoding.UTF8.GetBytes(contentParser.Encode(new OperationResult(){Success = true, ErrorMessages = "Empty request"}));
                            responseId = CndepCommands.RspError;
                        }
                    }
                    catch (Exception exc)
                    {
                        errorMessage = String.Format("Failed registering sensors. Device address: {0}; Error: {1}.", e.SendingEndpoint.Address, exc.Message);
                        resultRegSensors = new OperationResult() { Success = false };
                        resultRegSensors.AddError(errorMessage);

                        responseId = CndepCommands.RspError;
                        responseData = UTF8Encoding.UTF8.GetBytes(contentParser.Encode(resultRegSensors));
                    }
                    break;
                default:
                    errorMessage = String.Format("Unknown request command. Device address: {0}; Command: {1}.", e.SendingEndpoint.Address, e.Request.CommandId);

                    //Log
                    log.Error(errorMessage);

                    OperationResult resultDefault = new OperationResult(){Success = false};
                    resultDefault.AddError(errorMessage);

                    responseId = CndepCommands.RspError;
                    responseData = Encoding.UTF8.GetBytes(contentParser.Encode(resultDefault));
                    break;
            }

            //Create and return the response
            CndepMessageResponse response = new CndepMessageResponse(e.Request.SessionId, responseId, responseData);
            e.Response = response;
        }
    }
}
