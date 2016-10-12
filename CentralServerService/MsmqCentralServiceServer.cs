using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Common.Server;
using Common.Server.Msmq;
using System.Configuration;
using GlobalDataContracts;

namespace CentralServerService
{
    /// <summary>
    /// The class implements a MSMQ-based server to accept CentralService-related requests from devices.
    /// </summary>
    public class MsmqCentralServiceServer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constants...
        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgInputQueueAddress = "CentralServerService.Msmq.InputQueueAddress";

        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgOutputQueueAddress = "CentralServerService.Msmq.OutputQueueAddress";

        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgMsmqContentType = "CentralServerService.Msmq.ContentType";

        /// <summary>
        /// Default MSMQ body content type.
        /// </summary>
        public const string DefaultMsmqContentType = "application/json"; 
        #endregion

        private MsmqServer mServer;

        #region Properties...
        /// <summary>
        /// Content type of MSMQ message body.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// MSMQ message queue for receiving messages.
        /// </summary>
        public string InputQueueAddress { get; set; }

        /// <summary>
        /// MSMQ message queue for sending responses.
        /// </summary>
        public string OutputQueueAddress { get; set; }
        #endregion

        /// <summary>
        /// Initializes the server.
        /// </summary>
        public void Init()
        {
            InputQueueAddress = ConfigurationManager.AppSettings[CfgInputQueueAddress];
            OutputQueueAddress = ConfigurationManager.AppSettings[CfgOutputQueueAddress];
            //Figure out the content type
            ContentType = ConfigurationManager.AppSettings[CfgMsmqContentType];
            if (String.IsNullOrEmpty(ContentType)) ContentType = DefaultMsmqContentType;
            ContentParserFactory.GetParser(ContentType);
        }

        /// <summary>
        /// Starts server operations.
        /// </summary>
        public void Start()
        {
            if (mServer != null)
                Stop();

            //Log
            log.Debug("Starting CentralService MSMQ server...");

            if (String.IsNullOrEmpty(InputQueueAddress))
            {
                log.ErrorFormat("CentralService MSMQ server cannot be started, the input queue address ({0})  is not configured.", CfgInputQueueAddress);
                return;
            }

            mServer = new MsmqServer(InputQueueAddress, OutputQueueAddress);
            mServer.RequestReceived += new EventHandler<MsmqRequestReceivedEvArgs>(mServer_RequestReceived);
            mServer.Open();

            //Log
            log.Debug("Started CentralService MSMQ server.");
        }

        /// <summary>
        /// Stops server operations.
        /// </summary>
        public void Stop()
        {
            //Log
            log.Debug("Closing CentralService MSMQ server...");

            if (mServer != null)
            {
                mServer.Close();
            }
            mServer = null;

            //Log
            log.Debug("Closed CentralService MSMQ server.");
        }

        void mServer_RequestReceived(object sender, MsmqRequestReceivedEvArgs e)
        {
            string responseType;
            object responseData;
            string errorMessage;

            IContentParser contentParser = ContentParserFactory.GetParser(ContentType);

            string content = String.IsNullOrEmpty(e.Request.Body) ? String.Empty : e.Request.Body;

            //Log
            log.DebugFormat("=> {0}: {1}", e.Request.RequestType, content);

            switch (e.Request.RequestType)
            {
                case MsmqMessageRequest.RequestTypeStoreSensorData:
                    StoreSensorDataResult resultStore;
                    try
                    {
                        StoreSensorDataRequest requestStore = (StoreSensorDataRequest)contentParser.Decode(content, typeof(StoreSensorDataRequest));

                        Stopwatch watch = new Stopwatch();
                        TrackingPoint.TrackingPoint.CreateTrackingPoint
                            (
                             "Server: MSMQ receive multiple",
                                "before processing: ",
                                requestStore.Data.Length,
                                string.Format
                                    ("{0} - {1}", requestStore.Data[0].Measures[0].CorrelationId, requestStore.Data[requestStore.Data.Length - 1].Measures[0].CorrelationId));
                        watch.Start();
                        responseData = CentralServerServiceImpl.Instance.StoreSensorData(requestStore.DeviceId, new List<MultipleSensorData>(requestStore.Data));
                        watch.Stop();
                        TrackingPoint.TrackingPoint.CreateTrackingPoint
                            (
                             "Server: MSMQ receive multiple",
                                "after processing: ",
                                watch.ElapsedMilliseconds,
                                string.Format
                                    ("{0} - {1}", requestStore.Data[0].Measures[0].CorrelationId, requestStore.Data[requestStore.Data.Length - 1].Measures[0].CorrelationId));
                        responseType = MsmqMessageResponse.ResponseTypeStoreSensorData;
                    }
                    catch (Exception exc)
                    {
                        errorMessage = String.Format("Failed storing sensor data. Error: {0}.", exc.Message);

                        //Log
                        log.Error(errorMessage);

                        resultStore = new StoreSensorDataResult() { Success = false };
                        resultStore.AddError(errorMessage);

                        responseType = MsmqMessageResponse.ResponseTypeError;
                        responseData = resultStore;
                    }
                    break;

                default:
                    errorMessage = String.Format("Unknown request type. Request type: {0}.", e.Request.RequestType);

                    //Log
                    log.Error(errorMessage);

                    OperationResult resultDefault = new OperationResult() { Success = false };
                    resultDefault.AddError(errorMessage);

                    responseType = MsmqMessageResponse.ResponseTypeError;
                    responseData = resultDefault;
                    break;
            }

            //Create and return the response
            e.Response = new MsmqMessageResponse() { CorrelationId = e.Request.CorrelationId, ResponseType = responseType, ResponseData = responseData };
        }
    }
}
