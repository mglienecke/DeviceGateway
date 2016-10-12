using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using GlobalDataContracts;

namespace Common.Server.Msmq
{
    /// <summary>
    /// The class implements device communication handler that uses the MSMQ communication channel.
    /// </summary>
    public class MsmqDeviceCommunicationHandler:IDeviceCommunicationHandler
    {
        #region Constants...
        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgInputQueueAddress = "MsmqDeviceCommunicationHandler.InputQueueAddress";

        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgOutputQueueAddress = "MsmqDeviceCommunicationHandler.OutputQueueAddress";

        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgMsmqContentType = "MsmqDeviceCommunicationHandler.ContentType";

        public static readonly string[] XmlFormatterTypeNames = new[] {"System.String,mscorlib"};

        /// <summary>
        /// Default MSMQ content type
        /// </summary>
        public const string DefaultMsmqContentType = JsonContentParser.ContentType;
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constant: default value.
        /// </summary>
        public static readonly TimeSpan DefaultMessageReceiveTimeout = new TimeSpan(0, 0, 10);

        private string _messageQueueNameInput;
        private string _messageQueueNameOutput;
        private string _contentType;
        private bool _isCreateMessageQueueIfNotFound;
        private TimeSpan _messageReceiveTimeout = DefaultMessageReceiveTimeout;

        private List<SensorData> _listSensorData = new List<SensorData>(10);

        /// <summary>
        /// Constructor.
        /// </summary>
        public MsmqDeviceCommunicationHandler()
        {
            try
            {
                //Figure out the input queue
                _messageQueueNameInput = ConfigurationManager.AppSettings[CfgInputQueueAddress];
                if (String.IsNullOrEmpty(_messageQueueNameInput))
                {
                    throw new Exception("Value not set.");
                }
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed obtaining settings from the app.config. Key: {0}; Error: {1}.}",
                                CfgInputQueueAddress, exc.Message);
                throw;
            }

            try
            {
                //Figure out the output queue
                _messageQueueNameOutput = ConfigurationManager.AppSettings[CfgOutputQueueAddress];
                if (String.IsNullOrEmpty(_messageQueueNameOutput))
                {
                    throw new Exception("Value not set.");
                }
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed obtaining settings from the app.config. Key: {0}; Error: {1}.}",
                                CfgOutputQueueAddress, exc.Message);
                throw;
            }

            //Figure out the content type
            _contentType = ConfigurationManager.AppSettings[CfgMsmqContentType];
            if (_contentType == null) _contentType = DefaultMsmqContentType; 
        }

        #region IDeviceCommunicationHandler members...

        SensorData IDeviceCommunicationHandler.GetSensorCurrentData(Device device, Sensor sensor)
        {
            //Send request
            var outputMessageQueue = GetMessageQueue(_messageQueueNameOutput);

            var requestMessage = new Message();
            requestMessage.Body = "OK";
            requestMessage.Label = GetMessageLabel(MsmqMessageRequest.RequestTypeGetSensorCurrentDataPrefix, device, sensor).ToString();
            requestMessage.Formatter = new XmlMessageFormatter(XmlFormatterTypeNames);
            string correlationId;
            try
            {
                outputMessageQueue.Send(requestMessage);
                correlationId = requestMessage.Id;
            }
            catch (Exception exc)
            {
                throw new Exception(String.Format(Properties.Resources.ExceptionFailedSendingRequestToMessageQueue, outputMessageQueue.QueueName), exc);
            }

            //Get response
            var inputMessageQueue = GetMessageQueue(_messageQueueNameInput);

            try
            {
                SensorData result = null;
                var responseMessage = inputMessageQueue.ReceiveByCorrelationId(correlationId, _messageReceiveTimeout);
                if (responseMessage != null)
                {
                    responseMessage.Formatter = new XmlMessageFormatter(XmlFormatterTypeNames);
                    result = (SensorData)ContentParserFactory.GetParser(_contentType).Decode(responseMessage.Body.ToString(), typeof(SensorData));
                }

                return result;
            }
            catch (Exception exc)
            {
                throw new Exception(String.Format(Properties.Resources.ExceptionFailedRetrievingSensorDataFromMessageQueue, _messageQueueNameInput), exc);
            }
        }

        OperationResult IDeviceCommunicationHandler.PutSensorCurrentData(Device device, Sensor sensor, SensorData data)
        {
            var messageQueue = GetMessageQueue(_messageQueueNameOutput);

            var messageBody = ContentParserFactory.GetParser(_contentType).Encode(data);

            var message = new Message();
            message.Formatter = new XmlMessageFormatter(XmlFormatterTypeNames);
            message.Body = messageBody;
            message.Label = GetMessageLabel(MsmqMessageRequest.RequestTypeSetActuatorDataPrefix, device, sensor).ToString();

            try
            {
                messageQueue.Send(message);
                return new OperationResult(true, null);
            }
            catch (Exception exc)
            {
                throw new Exception(String.Format(Properties.Resources.ExceptionFailedSendingMessageToMessageQueue, messageQueue.QueueName), exc);
            }
        }

        bool IDeviceCommunicationHandler.IsReusableAndThreadSafe
        {
            get { return true; }
        }
        #endregion

        /// <summary>
        /// Get formatted label for MSMQ requests.
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="device"></param>
        /// <param name="sensor"></param>
        /// <returns></returns>
        public static StringBuilder GetMessageLabel(string prefix, Device device, Sensor sensor)
        {
            return new StringBuilder(prefix).Append(device.Id).Append("=>").Append(sensor.Id);
        }

        private MessageQueue GetMessageQueue(string messageQueueAddress)
        {
            MessageQueue mq;
            if (!messageQueueAddress.Contains("private$"))
            {

                if (MessageQueue.Exists(messageQueueAddress))
                {
                    //creates an instance MessageQueue, which points 
                    //to the already existing MyQueue
                    mq = new MessageQueue(messageQueueAddress);
                }
                else
                {
                    throw new Exception(String.Format(Properties.Resources.ExceptionMessageQueueIsNotFound, messageQueueAddress));
                }
            }
            else
            {
                //creates an instance MessageQueue, which points 
                //to the already existing MyQueue
                mq = new MessageQueue(messageQueueAddress);
            }

            return mq;
        }
    }
}
