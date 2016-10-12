using System;
using System.Configuration;
using System.Net;

using System.Collections;
using Gsiot.Server.Simulator;

namespace DeviceServer.Simulator
{
    public class HttpRestServerCommunicationHandler:IServerCommunicationHandler
    {
        /// <summary>
        /// Number of a local port to be used to receiving calls from the server.
        /// </summary>
        public const string CfgLocalPort = "HttpRestServerCommunicationHandler.LocalPort";

        /// <summary>
        /// Default port number for the device-side communication server.
        /// </summary>
        public const UInt16 DefaultLocalPort = 41115;

        private DeviceServerSimulator mServer;
        private HttpServer mWebServer;
        private UInt16 _localPort = DefaultLocalPort;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public HttpRestServerCommunicationHandler()
        {
            LocalPort = String.IsNullOrEmpty(ConfigurationManager.AppSettings[CfgLocalPort])
                            ? DefaultLocalPort
                            : UInt16.Parse(ConfigurationManager.AppSettings[CfgLocalPort]);

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="server"></param>
        public HttpRestServerCommunicationHandler(DeviceServerSimulator server):this()
        {
            mServer = server;
        }

        #region IServerCommunicationHandler members...

        /// <summary>
        /// The propery contains the port number to be used by the device for accepting incoming requests.
        /// </summary>
        public UInt16 LocalPort
        {
            get
            {
                return _localPort; }
            set
            {
                if (value <= 0 || value > UInt16.MaxValue)
                    throw new ArgumentOutOfRangeException("value");

                _localPort = value;
            }
        }

        /// <summary>
        /// The property contains reference to the master device server object that processes all received requests.
        /// </summary>
        public DeviceServerSimulator Server {
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                mServer = value;
            }
        }

        public bool StoreSensorData(DeviceConfig config, Device device, Sensor sensor, SensorData data)
        {
            try
            {
                HttpStatusCode responseStatusCode;
                String responseContent;
                //PUT Devices/X/Sensors/Y/data
                HttpClient.SendPostRequest(String.Format(CommunicationConstants.ServerRequestUrlStoreSensorData, sensor.Config.ServerUrl, device.Id, sensor.Id),
                    ContentParserFactory.GetParser(config.ServerContentType).PutSensorDataRequest(new SensorData[] { data }, sensor.SensorValueDataType), 
                    ContentParserFactory.GetParser(config.ServerContentType).MimeContentType, DeviceConfig.DefaultHttpRequestTimeoutInMillis,
                    out responseStatusCode, out responseContent);

                //Check the response
                return HandleOperationResultResponse(config, responseStatusCode, responseContent,
                    Properties.Resources.ErrorFailedSendingSensorDataOverHttp);
            }
            catch (WebException exc)
            {
                DeviceServerSimulator.RunningInstance.LogError(exc,
                    Properties.Resources.ErrorFailedSendingSensorDataOverHttp +
                    Properties.Resources.SocketStatusWithMessage,
                    exc.Status, exc.Message);
                return false;
            }
            catch (Exception exc)
            {
                DeviceServerSimulator.RunningInstance.LogError(exc,
                    Properties.Resources.ErrorFailedSendingSensorDataOverHttp +
                    " ", exc.Message);
                return false;
            }
        }

        public bool RegisterDevice(DeviceConfig config, Device device)
        {
            try
            {
                HttpStatusCode statusCode;
                string content;
                //Refresh the device
                //PUT /Devices/X
                bool result = HttpClient.SendPutRequest(String.Format(CommunicationConstants.ServerRequestUrlRegisterDevice, config.ServerUrl, device.Id),
                    ContentParserFactory.GetParser(config.ServerContentType).CreatePutDeviceRequestContent(device), ContentParserFactory.GetParser(config.ServerContentType).MimeContentType, config.TimeoutInMillis,
                    out statusCode, out content);

                //Check response, log errors
                return HandleOperationResultResponse(config, statusCode, content, Properties.Resources.ErrorFailedRefsreshingDeviceRegistration);
            }
            catch (WebException exc)
            {
                DeviceServerSimulator.RunningInstance.LogError(exc, Properties.Resources.ErrorFailedRefsreshingDeviceRegistration +
                    Properties.Resources.HttpResponseStatusWithMessage, exc.Status, exc.Message);
                return false;
            }
        }

        public bool RegisterSensors(DeviceConfig config, Device device, ArrayList sensors)
        {
            try
            {
                HttpStatusCode statusCode;
                string content;

                //Refresh the sensors
                //PUT /Devices/X/Sensors/Y
                bool result = HttpClient.SendPutRequest(String.Format(CommunicationConstants.ServerRequestUrlRegisterDeviceSensors, config.ServerUrl, device.Id),
                    ContentParserFactory.GetParser(config.ServerContentType).CreatePutSensorsRequestContent(sensors), ContentParserFactory.GetParser(config.ServerContentType).MimeContentType, config.TimeoutInMillis,
                    out statusCode, out content);

                //Check response, log errors
                return HandleOperationResultResponse(config, statusCode, content, Properties.Resources.ErrorFailedRefsreshingDeviceSensorsRegistration);
            }
            catch (WebException exc)
            {
                DeviceServerSimulator.RunningInstance.LogError(exc, Properties.Resources.ErrorFailedRefsreshingDeviceRegistration +
                    Properties.Resources.HttpResponseStatusWithMessage, exc.Status, exc.Message);
                return false;
            }
        }

        /// <summary>
        /// The method initializes the device server for handling incoming requests.
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {
            try
            {
                //Create the HTTP-server
                mWebServer = new HttpServer
                {
                    RequestRouting =
            {
                {
                    CommunicationConstants.RequestGetSensorCurrentValue,
                    HandleGetSensorCurrentValueRequest
                },
                {
                    CommunicationConstants.RequestGetSensorValues,
                    HandleGetSensorValuesRequest
                },
                {
                    CommunicationConstants.RequestGetSensors,
                    HandleGetSensorsRequest
                },
                {
                    CommunicationConstants.RequestPutDeviceConfig,
                    HandlePutDeviceConfigRequest
                },
                {
                    CommunicationConstants.RequestGetDeviceConfig,
                    HandleGetDeviceConfigRequest
                },
                {
                    CommunicationConstants.RequestGetSensorsValues,
                    HandleGetSensorsValuesRequest
                },
                {
                    CommunicationConstants.RequestPutSensorCurrentValue,
                    HandlePutSensorValueRequest
                },
                {
                    CommunicationConstants.RequestGetErrorLog,
                    HandleGetErrorLogRequest
                }
            }
                };

                //Port
                mWebServer.Port = _localPort;

                return true;
            }
            catch (Exception exc)
            {
                DeviceServerSimulator.RunningInstance.LogError(exc, "Failed initializing Device HTTP server.");
                return false;
            }
        }

        /// <summary>
        /// The method starts handling incoming requests.
        /// </summary>
        /// <returns><c>True</c> if the start has been successful.</returns>
        public bool StartRequestHandling()
        {
            try
            {
                mWebServer.Run();
                return true;
            }
            catch (Exception exc)
            {
                DeviceServerSimulator.RunningInstance.LogError(exc, "Failed starting Device HTTP server.");
                return false;
            }
        }

        /// <summary>
        /// The method stops the process of handling incoming requests.
        /// </summary>
        /// <returns></returns>
        public bool StopRequestHandling()
        {
            return true;
        }

        #endregion

        #region HTTP request handlers...
        public void HandleGetSensorCurrentValueRequest(RequestHandlerContext context)
        {
            //Check the content type
            string responseContentType;
            if (ContentParserFactory.IsResponseContentTypeSupported(context.AcceptedContentType, out responseContentType))
            {
                context.ResponseContentType = responseContentType;
            }
            else
            {
                CreateUnsupportedAcceptedContentTypeResponse(context);
                return;
            }

            try
            {
                mServer.DisplayText(context.RequestUri);

                string[] parts = context.RequestUri.Split(CommunicationConstants.PathSplitChar);
                Sensor sensor = mServer.FindSensor(parts[CommunicationConstants.SensorIdPos]);



                if (sensor == null)
                {
                    //Sensor with this id is not found
                    context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateErrorResponseContent(String.Format(Properties.Resources.ErrorSensorNotFound, parts[CommunicationConstants.SensorIdPos]));
                    //Not found
                    context.ResponseStatusCode = (Int32)HttpStatusCode.NotFound;
                }
                else
                {
                    if (!sensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.Both) &&
                        !sensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.Pull))
                    {
                        context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateErrorResponseContent(Properties.Resources.ErrorPullModeNotSupported);
                        //Not supported
                        context.ResponseStatusCode = (Int32)HttpStatusCode.MethodNotAllowed;
                    }
                    else
                    {
                        SensorData sensorValue = sensor.ReadSensorValue();
                        context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateGetCurrentSensorDataResponseContent(sensorValue, sensor.SensorValueDataType);
                        //OK
                        context.ResponseStatusCode = (Int32)HttpStatusCode.OK;
                    }
                }
            }
            catch (Exception exc)
            {
                //Internal server error
                context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(false, exc.ToString());
                context.ResponseStatusCode = (Int32)HttpStatusCode.InternalServerError;
            }
        }

        public void HandleGetSensorValuesRequest(RequestHandlerContext context)
        {
            //Check the requested content type
            string responseContentType;
            if (ContentParserFactory.IsResponseContentTypeSupported(context.AcceptedContentType, out responseContentType))
            {
                context.ResponseContentType = responseContentType;
            }
            else
            {
                CreateUnsupportedAcceptedContentTypeResponse(context);
                return;
            }

            try
            {
                context.ResponseContentType = responseContentType;

                string[] parts = context.RequestUri.Split(CommunicationConstants.PathSplitChar);

                Sensor sensor = mServer.FindSensor(parts[CommunicationConstants.SensorIdPos]);
                if (sensor == null)
                {
                    //Sensor with this is not found
                    context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(false,
                        String.Format(Properties.Resources.ErrorSensorNotFound, parts[2]));
                    //context.ResponseStatusCode = (Int32)HttpStatusCode.NotFound;
                    context.ResponseStatusCode = (Int32)HttpStatusCode.OK;
                }
                else
                {
                    context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateGetLastSensorValuesResponse(sensor);
                    context.ResponseStatusCode = (Int32)HttpStatusCode.OK;
                }
            }
            catch (Exception exc)
            {
                //Internal server error
                context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(false, exc.ToString());
                context.ResponseStatusCode = (Int32)HttpStatusCode.InternalServerError;
            }
        }

        public void HandleGetSensorsValuesRequest(RequestHandlerContext context)
        {
            //Check the requested content type
            string responseContentType;
            if (ContentParserFactory.IsResponseContentTypeSupported(context.AcceptedContentType, out responseContentType))
            {
                context.ResponseContentType = responseContentType;
            }
            else
            {
                CreateUnsupportedAcceptedContentTypeResponse(context);
                return;
            }

            try
            {
                context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateGetLastSensorsValuesResponse(mServer.Sensors);
                context.ResponseStatusCode = (Int32)HttpStatusCode.OK;
            }
            catch (Exception exc)
            {
                //Internal server error
                context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(false, exc.ToString());
                context.ResponseStatusCode = (Int32)HttpStatusCode.InternalServerError;
            }
        }

        public void HandleGetErrorLogRequest(RequestHandlerContext context)
        {
            //Check the requested content type
            string responseContentType;
            if (ContentParserFactory.IsResponseContentTypeSupported(context.AcceptedContentType, out responseContentType))
            {
                context.ResponseContentType = responseContentType;
            }
            else
            {
                CreateUnsupportedAcceptedContentTypeResponse(context);
                return;
            }

            try
            {
                context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateGetErrorLogResponseContent(mServer.ErrorLog);
                context.ResponseStatusCode = (Int32)HttpStatusCode.OK;
            }
            catch (Exception exc)
            {
                //Internal server error
                context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(false, exc.ToString());
                context.ResponseStatusCode = (Int32)HttpStatusCode.InternalServerError;
            }
        }

        public void HandleGetSensorsRequest(RequestHandlerContext context)
        {
            //Check the requested content type
            string responseContentType;
            if (ContentParserFactory.IsResponseContentTypeSupported(context.AcceptedContentType, out responseContentType))
            {
                context.ResponseContentType = responseContentType;
            }
            else
            {
                CreateUnsupportedAcceptedContentTypeResponse(context);
                return;
            }

            try
            {
                context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateGetSensorsResponseContent(mServer.Device.Id, mServer.Sensors);
                context.ResponseStatusCode = (Int32)HttpStatusCode.OK;
            }
            catch (Exception exc)
            {
                //Internal server error
                context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(false, exc.ToString());
                context.ResponseStatusCode = (Int32)HttpStatusCode.InternalServerError;
            }
        }

        public void HandlePutSensorValueRequest(RequestHandlerContext context)
        {
            //Check the requested content type
            string responseContentType;
            if (ContentParserFactory.IsResponseContentTypeSupported(context.AcceptedContentType, out responseContentType))
            {
                context.ResponseContentType = responseContentType;
            }
            else
            {
                CreateUnsupportedAcceptedContentTypeResponse(context);
                return;
            }

            try
            {
                context.ResponseContentType = responseContentType;

                string[] parts = context.RequestUri.Split(CommunicationConstants.PathSplitChar);

                Sensor sensor = mServer.FindSensor(parts[2]);
                if (sensor == null)
                {
                    //Sensor with this is not found
                    context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(false,
                        String.Format(Properties.Resources.ErrorSensorNotFound, parts[2]));
                    context.ResponseStatusCode = (Int32)HttpStatusCode.OK;
                }
                else
                {
                    if (sensor.ValueSetter == null)
                    {
                        //No value setter for this sensor
                        context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(false,
                            String.Format(Properties.Resources.ErrorSettingSensorValueNotSupported, parts[2]));
                        context.ResponseStatusCode = (Int32)HttpStatusCode.OK;
                    }
                    else
                    {
                        SensorData dataToSet = ContentParserFactory.GetParser(context.RequestContentType).ParseSensorData(context.RequestContent);
                        
                        if (dataToSet != null)
                        {
                            dataToSet.SensorId = sensor.Id;
                            //Set the value
                            sensor.ValueSetter(dataToSet);

                            context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(true, String.Empty);
                            context.ResponseStatusCode = (Int32)HttpStatusCode.OK;
                        }
                        else
                        {
                            //The value has not been provided in the request
                            context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(false,
                                Properties.Resources.ErrorValueNotProvidedInRequest);
                            //Bad request
                            context.ResponseStatusCode = (Int32)HttpStatusCode.OK;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                //Internal server error
                context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(false, exc.ToString());
                context.ResponseStatusCode = (Int32)HttpStatusCode.InternalServerError;
            }
        }

        public void HandlePutDeviceConfigRequest(RequestHandlerContext context)
        {
            //Check the requested content type
            string responseContentType;
            if (ContentParserFactory.IsResponseContentTypeSupported(context.AcceptedContentType, out responseContentType))
            {
                context.ResponseContentType = responseContentType;
            }
            else
            {
                CreateUnsupportedAcceptedContentTypeResponse(context);
                return;
            }

            try
            {
                //Parse the input
                DeviceConfig newConfig = ContentParserFactory.GetParser(context.RequestContentType).ParseDeviceConfig(context.RequestContent);

                mServer.ApplyDeviceConfig(newConfig);

                //Response
                context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(true, String.Empty);
                context.ResponseStatusCode = (Int32)HttpStatusCode.OK;
            }
            catch (Exception exc)
            {
                //Internal server error
                context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(false, exc.ToString());
                context.ResponseStatusCode = (Int32)HttpStatusCode.InternalServerError;
            }
        }

        public void HandleGetDeviceConfigRequest(RequestHandlerContext context)
        {
            //Check the requested content type
            string responseContentType;
            if (ContentParserFactory.IsResponseContentTypeSupported(context.AcceptedContentType, out responseContentType))
            {
                context.ResponseContentType = responseContentType;
            }
            else
            {
                CreateUnsupportedAcceptedContentTypeResponse(context);
                return;
            }

            try
            {
                context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateGetDeviceConfigResponseContent(mServer.Config);
                context.ResponseStatusCode = (Int32)HttpStatusCode.OK;
            }
            catch (Exception exc)
            {
                //Internal server error
                context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(false, exc.ToString());
                context.ResponseStatusCode = (Int32)HttpStatusCode.InternalServerError;
            }
        }
        #endregion

        private void CreateUnsupportedAcceptedContentTypeResponse(RequestHandlerContext context)
        {
            context.ResponseContent = String.Format(Properties.Resources.ErrorUnsupportedResponseContentType, context.AcceptedContentType);
            context.ResponseStatusCode = (Int32)HttpStatusCode.UnsupportedMediaType;
        }

        private bool HandleOperationResultResponse(DeviceConfig config, HttpStatusCode responseStatusCode, string responseContent, string baseErrorMessage)
        {
            OperationResult opResult = ContentParserFactory.GetParser(config.ServerContentType).ParseOperationResult(responseContent);
            if (opResult != null)
            {
                //Read the server time.
                if (opResult.Timestamp != 0)
                {
                    mServer.ServerTimeMark = DeviceServerSimulator.TimeZero.AddTicks(opResult.Timestamp);
                    mServer.ServerTimeOffset = mServer.ServerTimeMark.Ticks - DateTime.Now.Ticks;
                }

                //Success or not?
                if (opResult.Success)
                {
                    //OK
                    return true;
                }
                else
                {
                    mServer.LogError(null, baseErrorMessage +
                        Properties.Resources.HttpResponseStatusWithMessage, responseStatusCode, opResult.ErrorMessages);
                }
            }
            else
            {
                mServer.LogError(null, baseErrorMessage +
                    Properties.Resources.HttpResponseStatus, responseStatusCode);
            }

            return false;
        }
    }
}
