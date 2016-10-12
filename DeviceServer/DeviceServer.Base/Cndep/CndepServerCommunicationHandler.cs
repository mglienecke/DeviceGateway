 using System;
using Microsoft.SPOT;
using System.Text;
using NetMf.CommonExtensions;
using System.Collections;

namespace DeviceServer.Base.Cndep
{
    /// <summary>
    /// UDP_based CNDEP implementation of the <see cref="IServerCommunicationHandler"/> interface.
    /// </summary>
    public class CndepServerCommunicationHandler:ISensorDataExchangeCommunicationHandler
    {
        private const string GetStoreSensorDataRequestTemplate =
@"{{
    ""DeviceId"": ""{0}"",
    ""SensorId"": ""{1}"",
    ""Data"": [{2}]
}}";

        private const string HostNamePortSeparator = ":";
        /// <summary>
        /// Default CNDEP remote server port.
        /// </summary>
        public const int DefaultCndepRemoteServerPort = 41120;
        /// <summary>
        /// Default CNDEP local server port.
        /// </summary>
        public const int DefaultCndepLocalServerPort = 41120;
        /// <summary>
        /// Default CNDEP local client port.
        /// </summary>
        public const int DefaultCndepLocalClientPort = 41121;

        private static readonly object SessionIdLock = new object();
        private static byte SessionId = 0;

        private CndepClient mCndepClient;
        private CndepServer mCndepServer;
        private DeviceServerBase mServer;

        private static byte GetSessionId()
        {
            lock (SessionIdLock)
            {
                return ++SessionId;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="server"></param>
        public CndepServerCommunicationHandler(DeviceServerBase server)
        {
            mServer = server;

            if (mServer.Config.CndepConfig == null)
            {
                throw new InvalidOperationException("The server configuration doesn't have data for the CNDEP configuration.");
            }
        }

        #region IServerCommunicationHandler members...
        public bool StoreSensorData(DeviceConfig config, Device device, Sensor sensor, SensorData data)
        {
            try
            {
                using (CndepClient client = GetClient())
                {
                    StoreSensorDataRequest requestStoreData = new StoreSensorDataRequest()
                    {
                        DeviceId = device.Id,
                        Data = new MultipleSensorData[]{new MultipleSensorData(){SensorId = sensor.Id, DataType = sensor.SensorValueDataType, 
                            Measures = new SensorData[]{data}}}
                    };
                    string content = ContentParserFactory.GetParser(config.CndepConfig.ServerContentType).StoreSensorDataRequest(requestStoreData);
                    byte[] dataBytes = Encoding.UTF8.GetBytes(content);

                    CndepMessageRequest request = new CndepMessageRequest(GetSessionId(), CndepConstants.CmdStoreSensorData, 0, dataBytes);
                    client.Connect();
                    CndepMessageResponse response = client.SendWithResponse(request);

                    //Check the response
                    return HandleOperationResultResponse(config, response,
                        Properties.Resources.GetString(Properties.Resources.StringResources.ErrorFailedSendingSensorDataOverCndep));
                }
            }
            catch (Exception exc)
            {
                DeviceServerBase.RunningInstance.LogError(exc,
                    Properties.Resources.GetString(Properties.Resources.StringResources.ErrorFailedSendingSensorDataOverCndep) +
                    " ", exc.Message);
                Debug.Print(exc.ToString());
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
                //Create the CNDEP-server
                mCndepServer = new CndepServer(mServer.Config.CndepConfig.LocalServerPort);
                mCndepServer.RequestReceived += new EventHandler(mCndepServer_RequestReceived);
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
        public bool Initialized { get { return mCndepServer != null; } }


        /// <summary>
        /// The method starts handling incoming requests.
        /// </summary>
        /// <returns><c>True</c> if the start has been successful.</returns>
        public bool StartRequestHandling()
        {
            try
            {
                mCndepServer.Open();
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
        /// The method stops the process of handling incoming requests.
        /// </summary>
        /// <returns></returns>
        public void StopRequestHandling()
        {
            mCndepServer.Close();
        }

        /// <summary>
        /// The property returns if the comm handler is in the request-handling state.
        /// </summary>
        public bool IsHandlingRequests
        {
            get
            {
                return mCndepServer != null && mCndepServer.IsRunning;
            }
        }

        #endregion

        #region CNDEP request handlers...

        void mCndepServer_RequestReceived(object sender, EventArgs e)
        {
            string errorMessage = null;
            RequestReceivedEvArgs requestArgs = (RequestReceivedEvArgs)e;

            if (requestArgs.Success == false)
            {
                //Log
                mServer.LogError(null, requestArgs.ErrorMessage, errorMessage);
            }
            else
            {
                switch (requestArgs.Request.CommandId)
                {
                    case CndepConstants.CmdGetSensorData:
                        switch (requestArgs.Request.FunctionId)
                        {
                            case CndepConstants.FncGetSensorDataCurrentValue:
                                requestArgs.Response = HandleGetSensorCurrentValueRequest(requestArgs.Request);
                                return;
                            case CndepConstants.FncGetSensorDataAllValues:
                                requestArgs.Response = HandleGetSensorValuesRequest(requestArgs.Request);
                                return;
                            default:
                                errorMessage = StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ErrorUnknownCndepCommandFunction),
                                    requestArgs.Request.CommandId, requestArgs.Request.FunctionId);
                                break;
                        }
                        break;

                    default:
                        errorMessage = StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ErrorUnknownCndepCommand), requestArgs.Request.CommandId);
                        break;
                }

                //Log
                mServer.LogError(null, errorMessage);
                //Set response
                requestArgs.Response = new CndepMessageResponse(requestArgs.Request.SessionId, CndepConstants.RspError,
                    Encoding.UTF8.GetBytes(ContentParserFactory.GetParser(mServer.Config.ServerContentType).CreateOperationResultResponse(false, errorMessage)));
            }
         }

        public CndepMessageResponse HandleGetSensorCurrentValueRequest(CndepMessageRequest request)
        {
            string responseContent;
            byte responseId;
            try
            {
                if (request.Data == null || request.Data.Length == 0)
                {
                    responseContent = ContentParserFactory.GetParser(mServer.Config.ServerContentType).CreateOperationResultResponse(false,
                        StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ErrorMissingRequestContent)));
                    responseId = CndepConstants.RspError;
                }
                else
                {
                    string content = new string(Encoding.UTF8.GetChars(request.Data));

                    mServer.DisplayText(Properties.Resources.GetString(Properties.Resources.StringResources.TextCndepRequestArrived), request.CommandId, request.FunctionId, content);

                    if (content == null)
                        throw new Exception(Properties.Resources.GetString(Properties.Resources.StringResources.ErrorNoSensorIdInRequest));
                    Sensor sensor = mServer.FindSensor(content);

                    if (sensor == null)
                    {
                        //Sensor with this id is not found
                        responseContent = ContentParserFactory.GetParser(mServer.Config.ServerContentType).CreateOperationResultResponse(false,
                            StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ErrorSensorNotFound), content));
                        responseId = CndepConstants.RspError;
                    }
                    else
                    {
                        if (sensor.SensorDataRetrievalMode != SensorDataRetrievalMode.Both &&
                            sensor.SensorDataRetrievalMode != SensorDataRetrievalMode.Pull)
                        {
                            responseContent = ContentParserFactory.GetParser(mServer.Config.ServerContentType).CreateOperationResultResponse(false,
                                Properties.Resources.GetString(Properties.Resources.StringResources.ErrorPullModeNotSupported));
                            responseId = CndepConstants.RspError;
                        }
                        else
                        {
                            SensorData sensorValue = sensor.ReadSensorValue();
                            responseContent = ContentParserFactory.GetParser(mServer.Config.ServerContentType).CreateGetCurrentSensorDataResponseContent(sensorValue, sensor.SensorValueDataType);
                            responseId = CndepConstants.RspOK;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                //Internal server error
                responseContent = ContentParserFactory.GetParser(mServer.Config.ServerContentType).CreateOperationResultResponse(false, exc.ToString());
                responseId = CndepConstants.RspError;
            }

            //Return
            return new CndepMessageResponse(request.SessionId, responseId, Encoding.UTF8.GetBytes(responseContent));
        }

        public CndepMessageResponse HandleGetSensorValuesRequest(CndepMessageRequest request)
        {
            string responseContent;
            byte responseId;
            try
            {
                if (request.Data == null || request.Data.Length == 0)
                {
                    responseContent = ContentParserFactory.GetParser(mServer.Config.ServerContentType).CreateOperationResultResponse(false,
                        StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ErrorMissingRequestContent)));
                    responseId = CndepConstants.RspError;
                }
                else
                {
                    string content = new string(Encoding.UTF8.GetChars(request.Data));
                    mServer.DisplayText(Properties.Resources.GetString(Properties.Resources.StringResources.TextCndepRequestArrived), request.CommandId, request.FunctionId, content);

                    if (content == null)
                        throw new Exception(Properties.Resources.GetString(Properties.Resources.StringResources.ErrorNoSensorIdInRequest));
                    Sensor sensor = mServer.FindSensor(content);

                    if (sensor == null)
                    {
                        //Sensor with this id is not found
                        responseContent = ContentParserFactory.GetParser(mServer.Config.ServerContentType).CreateOperationResultResponse(false,
                            StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ErrorSensorNotFound), content));
                        responseId = CndepConstants.RspError;
                    }
                    else
                    {
                        if (sensor.SensorDataRetrievalMode != SensorDataRetrievalMode.Both &&
                            sensor.SensorDataRetrievalMode != SensorDataRetrievalMode.Pull)
                        {
                            responseContent = ContentParserFactory.GetParser(mServer.Config.ServerContentType).CreateOperationResultResponse(false,
                                Properties.Resources.GetString(Properties.Resources.StringResources.ErrorPullModeNotSupported));
                            responseId = CndepConstants.RspError;
                        }
                        else
                        {
                            SensorData sensorValue = sensor.ReadSensorValue();
                            responseContent = ContentParserFactory.GetParser(mServer.Config.ServerContentType).CreateGetLastSensorValuesResponse(sensor);
                            responseId = CndepConstants.RspOK;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                //Internal server error
                responseContent = ContentParserFactory.GetParser(mServer.Config.ServerContentType).CreateOperationResultResponse(false, exc.ToString());
                responseId = CndepConstants.RspError;
            }

            //Return
            return new CndepMessageResponse(request.SessionId, responseId, Encoding.UTF8.GetBytes(responseContent));
        }
        #endregion

        private CndepClient GetClient()
        {
            return new CndepClient(mServer.Config.CndepConfig.RemoteServerPort, mServer.Config.CndepConfig.ServerAddress) 
            { TimeoutInMillis = mServer.Config.CndepConfig.TimeoutInMillis, RequestRetryCount = mServer.Config.CndepConfig.RequestRetryCount};

            //return new CndepClient(mServer.Config.CndepConfig.RemoteServerPort, mServer.Config.CndepConfig.ServerAddress, mServer.Config.CndepConfig.LocalClientPort) { TimeoutInMillis = mServer.Config.CndepConfig.TimeoutInMillis, RequestRetryCount = mServer.Config.CndepConfig.RequestRetryCount };
        }

        private bool HandleOperationResultResponse(DeviceConfig config, CndepMessageResponse  response, string baseErrorMessage)
        {
            if (response == null)
            {
                mServer.LogError(null, baseErrorMessage, "response timeout");
                return false;
            }
            else
            {
                OperationResult opResult = ContentParserFactory.GetParser(config.CndepConfig.ServerContentType).ParseOperationResult(new string(Encoding.UTF8.GetChars(response.Data)));
                if (opResult != null)
                {
                    //Read the server time.
                    if (opResult.Timestamp != 0)
                    {
                        mServer.ServerTimeMark = DeviceServerBase.TimeZero.AddTicks(opResult.Timestamp);
                        mServer.ServerTimeOffset = mServer.ServerTimeMark.Ticks - DateTime.Now.Ticks;
                    }

                    //Success or not?
                    if (response.ResponseId != CndepConstants.RspError && opResult.Success)
                    {
                        //OK
                        return true;
                    }
                    else
                    {
                        mServer.LogError(null, baseErrorMessage +
                            Properties.Resources.GetString(Properties.Resources.StringResources.CndepResponseStatusWithMessage), response.ResponseId, opResult.ErrorMessages);
                    }
                }
                else
                {
                    mServer.LogError(null, baseErrorMessage +
                        Properties.Resources.GetString(Properties.Resources.StringResources.CndepResponseId), response.ResponseId);
                }

                return false;
            }
        }
    }
}
