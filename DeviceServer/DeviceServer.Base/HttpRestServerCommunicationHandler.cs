using System;
using Microsoft.SPOT;
using System.Net;
using NetMf.CommonExtensions;
using System.Collections;
using Gsiot.Server;
using System.Threading;

namespace DeviceServer.Base
{
    public class HttpRestServerCommunicationHandler:IServerCommunicationHandler
    {
        #region Server request URLs...
        internal const string ServerRequestUrlRegisterDeviceSensors = "{0}/Devices/{1}/Sensors";
        internal const string ServerRequestUrlRegisterDevice = "{0}/Devices/{1}/";
        internal const string ServerRequestUrlStoreSensorData = "{0}/Devices/{1}/Sensors/{2}/data";
        #endregion

        #region Device server request URLs...
        internal const string RequestGetSensorCurrentValue = "GET /Sensors/*/currentValue";
        internal const string RequestGetSensorValues = "GET /Sensors/*/values";
        internal const string RequestGetSensors = "GET /Sensors";
        internal const string RequestPutDeviceConfig = "PUT /DeviceConfig";
        internal const string RequestGetDeviceConfig = "GET /DeviceConfig";
        internal const string RequestGetSensorsValues = "GET /Sensors/values";
        internal const string RequestPutSensorCurrentValue = "PUT /Sensors/*/currentValue";
        internal const string RequestGetErrorLog = "GET /ErrorLog";
        #endregion

        private DeviceServerBase mServer;
        private HttpServer mWebServer;
        private Thread mRequestHandlingThread;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="server"></param>
        public HttpRestServerCommunicationHandler(DeviceServerBase server)
        {
            mServer = server;
        }

        #region IServerCommunicationHandler members...
        public bool StoreSensorData(DeviceConfig config, Device device, Sensor sensor, SensorData data)
        {
            try
            {
                HttpStatusCode responseStatusCode;
                String responseContent;
                //PUT Devices/X/Sensors/Y/data
                HttpClient.SendPostRequest(StringUtility.Format(ServerRequestUrlStoreSensorData, sensor.Config.ServerUrl, device.Id, sensor.Id),
                    ContentParserFactory.GetParser(config.ServerContentType).PutSensorDataRequest(new SensorData[] { data }, sensor.SensorValueDataType), 
                    ContentParserFactory.GetParser(config.ServerContentType).MimeContentType, DeviceConfig.DefaultHttpRequestTimeoutInMillis,
                    out responseStatusCode, out responseContent);

                //Check the response
                return HandleOperationResultResponse(config, responseStatusCode, responseContent,
                    Properties.Resources.GetString(Properties.Resources.StringResources.ErrorFailedSendingSensorDataOverHttp));
            }
            catch (WebException exc)
            {
                DeviceServerBase.RunningInstance.LogError(exc,
                    Properties.Resources.GetString(Properties.Resources.StringResources.ErrorFailedSendingSensorDataOverHttp) +
                    Properties.Resources.GetString(Properties.Resources.StringResources.SocketStatusWithMessage),
                    exc.Status, exc.Message);
                Debug.Print(exc.ToString());
                return false;
            }
            catch (Exception exc)
            {
                DeviceServerBase.RunningInstance.LogError(exc,
                    Properties.Resources.GetString(Properties.Resources.StringResources.ErrorFailedSendingSensorDataOverHttp) +
                    " ", exc.Message);
                Debug.Print(exc.ToString());
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
                bool result = HttpClient.SendPutRequest(StringUtility.Format(ServerRequestUrlRegisterDevice, config.ServerUrl, device.Id),
                    ContentParserFactory.GetParser(config.ServerContentType).CreatePutDeviceRequestContent(device), ContentParserFactory.GetParser(config.ServerContentType).MimeContentType, config.TimeoutInMillis,
                    out statusCode, out content);

                //Check response, log errors
                return HandleOperationResultResponse(config, statusCode, content, Properties.Resources.GetString(Properties.Resources.StringResources.ErrorFailedRefsreshingDeviceRegistration));
            }
            catch (WebException exc)
            {
                DeviceServerBase.RunningInstance.LogError(exc, Properties.Resources.GetString(Properties.Resources.StringResources.ErrorFailedRefsreshingDeviceRegistration) +
                    Properties.Resources.GetString(Properties.Resources.StringResources.HttpResponseStatusWithMessage), exc.Status, exc.Message);
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
                bool result = HttpClient.SendPutRequest(StringUtility.Format(ServerRequestUrlRegisterDeviceSensors, config.ServerUrl, device.Id),
                    ContentParserFactory.GetParser(config.ServerContentType).CreatePutSensorsRequestContent(sensors), ContentParserFactory.GetParser(config.ServerContentType).MimeContentType, config.TimeoutInMillis,
                    out statusCode, out content);

                //Check response, log errors
                return HandleOperationResultResponse(config, statusCode, content, Properties.Resources.GetString(Properties.Resources.StringResources.ErrorFailedRefsreshingDeviceSensorsRegistration));
            }
            catch (WebException exc)
            {
                DeviceServerBase.RunningInstance.LogError(exc, Properties.Resources.GetString(Properties.Resources.StringResources.ErrorFailedRefsreshingDeviceRegistration) +
                    Properties.Resources.GetString(Properties.Resources.StringResources.HttpResponseStatusWithMessage), exc.Status, exc.Message);
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
                    RequestGetSensorCurrentValue,
                    HandleGetSensorCurrentValueRequest
                },
                {
                    RequestGetSensorValues,
                    HandleGetSensorValuesRequest
                },
                {
                    RequestGetSensors,
                    HandleGetSensorsRequest
                },
                {
                    RequestPutDeviceConfig,
                    HandlePutDeviceConfigRequest
                },
                {
                    RequestGetDeviceConfig,
                    HandleGetDeviceConfigRequest
                },
                {
                    RequestGetSensorsValues,
                    HandleGetSensorsValuesRequest
                },
                {
                    RequestPutSensorCurrentValue,
                    HandlePutSensorValueRequest
                },
                {
                    RequestGetErrorLog,
                    HandleGetErrorLogRequest
                }
            }
                };
                return true;
            }
            catch (Exception exc)
            {
                Debug.Print(exc.ToString());
                mServer.DisplayText(exc.ToString());
                return false;
            }
        }

        /// <summary>
        /// The property contains a flag showing if the handler is initialized.
        /// </summary>
        public bool Initialized { get { return mWebServer != null; } }

        /// <summary>
        /// The method starts handling incoming requests.
        /// </summary>
        /// <returns><c>True</c> if the start has been successful.</returns>
        public bool StartRequestHandling()
        {
            //First time or not?
            if (mRequestHandlingThread == null)
            {
                mRequestHandlingThread = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        mWebServer.Run();
                    }
                    catch (ThreadAbortException)
                    {
                        Debug.Print("Request handling thread has been aborted.");
                    }
                    catch (Exception exc)
                    {
                        Debug.Print(exc.ToString());
                        mServer.DisplayText(exc.ToString());
                    }
                    finally
                    {
                        Debug.Print("Request handling thread has exited.");
                    }
                }));

                mRequestHandlingThread.Start();
            }
            else
            {
                mWebServer.Resume();
            }

            return true;
        }

        /// <summary>
        /// The method stops the process of handling incoming requests.
        /// </summary>
        /// <returns></returns>
        public void StopRequestHandling()
        {
            //Close the server
            try { mWebServer.Pause(); }
            catch (Exception exc) { Debug.Print(exc.ToString());}
        }

        /// <summary>
        /// The property returns if the comm handler is in the request-handling state.
        /// </summary>
        public bool IsHandlingRequests {
            get
            {
                return (mRequestHandlingThread != null && mRequestHandlingThread.IsAlive);
            }
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
                    context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateErrorResponseContent(StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ErrorSensorNotFound), parts[2]));
                    //Not found
                    context.ResponseStatusCode = (Int32)HttpStatusCode.NotFound;
                }
                else
                {
                    if (sensor.SensorDataRetrievalMode != SensorDataRetrievalMode.Both &&
                        sensor.SensorDataRetrievalMode != SensorDataRetrievalMode.Pull)
                    {
                        context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateErrorResponseContent(Properties.Resources.GetString(Properties.Resources.StringResources.ErrorPullModeNotSupported));
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
                        StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ErrorSensorNotFound), parts[2]));
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
                        StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ErrorSensorNotFound), parts[2]));
                    context.ResponseStatusCode = (Int32)HttpStatusCode.OK;
                }
                else
                {
                    if (sensor.ValueSetter == null)
                    {
                        //No value setter for this sensor
                        context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(false,
                            StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ErrorSettingSensorValueNotSupported), parts[2]));
                        context.ResponseStatusCode = (Int32)HttpStatusCode.OK;
                    }
                    else
                    {

                        
                        SensorData dataToSet = ContentParserFactory.GetParser(context.ResponseContentType).ParseSensorData(context.RequestContent);
                        
                        if (dataToSet != null)
                        {
                            //Set the value
                            sensor.ValueSetter(dataToSet.Value);

                            context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(true, String.Empty);
                            context.ResponseStatusCode = (Int32)HttpStatusCode.OK;
                        }
                        else
                        {
                            //The value has not been provided in the request
                            context.ResponseContent = ContentParserFactory.GetParser(context.ResponseContentType).CreateOperationResultResponse(false,
                                Properties.Resources.GetString(Properties.Resources.StringResources.ErrorValueNotProvidedInRequest));
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
                DeviceConfig newConfig = ContentParserFactory.GetParser(context.ResponseContentType).ParseDeviceConfig(context.RequestContent);

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
            context.ResponseContent = StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ErrorUnsupportedResponseContentType), context.AcceptedContentType);
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
                    mServer.ServerTimeMark = DeviceServerBase.TimeZero.AddTicks(opResult.Timestamp);
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
                        Properties.Resources.GetString(Properties.Resources.StringResources.HttpResponseStatusWithMessage), responseStatusCode, opResult.ErrorMessages);
                }
            }
            else
            {
                mServer.LogError(null, baseErrorMessage +
                    Properties.Resources.GetString(Properties.Resources.StringResources.HttpResponseStatus), responseStatusCode);
            }

            return false;
        }
    }
}
