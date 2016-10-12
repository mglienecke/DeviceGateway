using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Server;
using Common.Server.CNDEP;
using System.Configuration;

namespace DeviceServer.Simulator
{
    /// <summary>
    /// The class implements a UDP-based CNDEP server to accept CentralService-related requests from devices.
    /// </summary>
    public class CndepDeviceServer
    {
        private CndepServer _server;
        private UInt16 _localPort;
        private CommunicationProtocol _communicationProtocol;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CndepDeviceServer()
        {
        }

        /// <summary>
        /// Local port for receiving.
        /// </summary>
        public UInt16 LocalPort
        {
            get
            {
                return _localPort;
            }
            set
            {
                if (value >= 0 && value <= UInt16.MaxValue)
                    _localPort = value;
                else
                    throw new ArgumentOutOfRangeException("value");
            }
        }

        /// <summary>
        /// The property contains the communication protocol type value.
        /// </summary>
        public CommunicationProtocol CommunicationProtocol
        {
            get { return _communicationProtocol; }
            set { _communicationProtocol = value; }
        }

        /// <summary>
        /// The property contains the content type for data load of CNDEP messages.
        /// </summary>
        public string ContentType
        {
            get; set;
        }

        /// <summary>
        /// Starts server operations.
        /// </summary>
        public void Start()
        {
            if (_server != null)
                Stop();

            //Log
            DeviceServerSimulator.RunningInstance.DisplayText("Starting Device CNDEP server...");

            switch (_communicationProtocol)
            {
                case CommunicationProtocol.UDP:
                    _server = new CndepUdpServer(LocalPort);
                    break;
                case CommunicationProtocol.TCP:
                    _server = new CndepTcpServer(LocalPort);
                    break;
                default:
                    throw new ConfigurationErrorsException(String.Format("The property CommunicationProtocol has invalid value: {0}.", _communicationProtocol));
            }


            _server.RequestReceived += mServer_RequestReceived;
            _server.Open();

            //Log
            DeviceServerSimulator.RunningInstance.DisplayText("Started Device CNDEP server.");
        }

        /// <summary>
        /// Stops server operations.
        /// </summary>
        public void Stop()
        {
            //Log
            DeviceServerSimulator.RunningInstance.DisplayText("Closing Device CNDEP server...");

            _server.Close();
            _server = null;

            //Log
            DeviceServerSimulator.RunningInstance.DisplayText("Closed Device CNDEP server.");
        }

        void mServer_RequestReceived(object sender, RequestReceivedEvArgs e)
        {
            byte responseId;
            byte[] responseData;
            string errorMessage;

            IContentParser contentParser = ContentParserFactory.GetParser(ContentType);

            string content = e.Request.Data == null ? String.Empty : UTF8Encoding.UTF8.GetString(e.Request.Data, 0, e.Request.Data.Length);

            //Log
            DeviceServerSimulator.RunningInstance.DisplayText("=> {0}: {1}({2}): {3}", e.SendingEndpoint.ToString(), e.Request.CommandId, e.Request.FunctionId, content);

            switch (e.Request.CommandId)
            {
                case CndepCommands.CmdGetSensorData:
                    string sensorId = content;
                    try
                    {
                        Sensor sensor = DeviceServerSimulator.RunningInstance.FindSensor(sensorId);

                        if (sensor == null)
                        {
                            //Sensor with this id is not found
                            responseId = CndepCommands.RspError;
                            responseData = Encoding.UTF8.GetBytes(String.Format(Properties.Resources.ErrorSensorNotFound, sensorId)); 
                        }
                        else
                        {
                            if (!sensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.Both) &&
                                !sensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.Pull))
                            {
                                //Not supported
                                responseId = CndepCommands.RspError;
                                responseData = Encoding.UTF8.GetBytes(Properties.Resources.ErrorPullModeNotSupported); 
                            }
                            else
                            {
                                SensorData sensorValue = sensor.ReadSensorValue();
                                //OK
                                responseId = CndepCommands.RspOK;
                                responseData = Encoding.UTF8.GetBytes(ContentParserFactory.GetParser(ContentType).CreateGetCurrentSensorDataResponseContent(sensorValue, sensor.SensorValueDataType));
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        errorMessage = String.Format("Failed getting sensor current data. Sensor: {0}; Error: {1}.", sensorId, exc.Message);

                        responseId = CndepCommands.RspError;
                        responseData = Encoding.UTF8.GetBytes(errorMessage);                        
                    }
                    break;
                case CndepCommands.CmdPutActuatorValue:
                    var requestPutActuatorValue = contentParser.ParsePutActuatorDataRequest(content);
                    if (requestPutActuatorValue == null)
                    {
                        responseId = CndepCommands.RspError;
                        responseData = Encoding.UTF8.GetBytes("No request content received.");
                    }
                    else
                    {
                        try
                        {
                            Sensor sensor = DeviceServerSimulator.RunningInstance.FindSensor(requestPutActuatorValue.SensorId);

                            if (sensor == null)
                            {
                                //Sensor with this id is not found
                                responseId = CndepCommands.RspError;
                                responseData =
                                    Encoding.UTF8.GetBytes(String.Format(Properties.Resources.ErrorSensorNotFound,
                                                                         requestPutActuatorValue.SensorId));
                            }
                            else
                            {
                                if (sensor.ValueSetter == null)
                                {
                                    //No value setter for this sensor
                                    responseId = CndepCommands.RspOK;
                                    responseData =
                                        Encoding.UTF8.GetBytes(
                                            ContentParserFactory.GetParser(ContentType).CreateOperationResultResponse(false,
                                                                                                                      String.Format(
                                                                                                                          Properties
                                                                                                                              .Resources
                                                                                                                              .ErrorSettingSensorValueNotSupported,
                                                                                                                          requestPutActuatorValue
                                                                                                                              .SensorId)));
                                }
                                else
                                {
                                    SensorData dataToSet = requestPutActuatorValue.Data;

                                    if (dataToSet != null)
                                    {
                                        dataToSet.SensorId = sensor.Id;
                                        //Set the value
                                        sensor.ValueSetter(dataToSet);

                                        responseId = CndepCommands.RspOK;
                                        responseData =
                                            Encoding.UTF8.GetBytes(ContentParserFactory.GetParser(ContentType).CreateOperationResultResponse(true, String.Empty));
                                    }
                                    else
                                    {
                                        //The value has not been provided in the request
                                        responseData = Encoding.UTF8.GetBytes(
                                                ContentParserFactory.GetParser(ContentType).CreateOperationResultResponse(false,
                                                                                                                          Properties
                                                                                                                              .Resources
                                                                                                                              .ErrorValueNotProvidedInRequest));
                                        responseId = CndepCommands.RspOK;
                                    }
                                }
                            }
                        }
                        catch (Exception exc)
                        {
                            errorMessage = String.Format("Failed setting actuator value. Sensor: {0}; Error: {1}.",
                                                         requestPutActuatorValue.SensorId, exc.Message);

                            responseId = CndepCommands.RspError;
                            responseData = Encoding.UTF8.GetBytes(errorMessage);
                        }
                    }
                    break;
                default:
                    errorMessage = String.Format("Unknown request command. Device address: {0}; Command: {1}.", e.SendingEndpoint.Address, e.Request.CommandId);
                    //Log
                    DeviceServerSimulator.RunningInstance.LogError(null, errorMessage);

                    responseId = CndepCommands.RspError;
                    responseData = UTF8Encoding.UTF8.GetBytes(ContentParserFactory.GetParser(ContentType).CreateOperationResultResponse(false, errorMessage));
                    break;
            }

            //Create and return the response
            CndepMessageResponse response = new CndepMessageResponse(e.Request.SessionId, responseId, responseData);
            e.Response = response;
        }
    }
}
