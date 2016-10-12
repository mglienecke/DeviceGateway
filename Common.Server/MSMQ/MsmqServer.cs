using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Messaging;
using GlobalDataContracts;


namespace Common.Server.Msmq
{
    /// <summary>
    /// The class implements a server for the MSMQ data exchange protocol.
    /// </summary>
    public class MsmqServer : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Thread mReceiveThread;
        private Thread mSendThread;
        private Thread mReceiveProcessThread;
        private BlockingQueue<MsmqRequestReceivedEvArgs> mReceiveQueue = new BlockingQueue<MsmqRequestReceivedEvArgs>();
        private BlockingQueue<MsmqRequestReceivedEvArgs> mSendQueue = new BlockingQueue<MsmqRequestReceivedEvArgs>();
        private bool mRunServer = false;
        private string _inputMessageQueueAddress;
        private MessageQueue _inputMessageQueue;
        private MessageQueue _outputMessageQueue;

        /// <summary>
        /// Event for asynchronously received events.
        /// </summary>
        public event EventHandler<MsmqRequestReceivedEvArgs> RequestReceived;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inputQueueAddress"></param>
        /// <param name="outputQueueAddress"></param>
        public MsmqServer(string inputQueueAddress, string outputQueueAddress)
        {
            InputQueueAddress = inputQueueAddress;
            OutputQueueAddress = outputQueueAddress;
        }

        #region Properties...
        /// <summary>
        /// The property contains the address of the input message queue.
        /// </summary>
        public string InputQueueAddress
        {
            get
            {
                return _inputMessageQueueAddress;
            }
            private set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                _inputMessageQueueAddress = value;
            }
        }

        /// <summary>
        /// The property contains the address of the output message queue.
        /// </summary>
        public string OutputQueueAddress
        {
            get;
            private set;
        }

        #endregion

        /// <summary>
        /// Connect connection to the server, start receiving messages
        /// </summary>
        public void Open()
        {
            //Close if opened
            if (_inputMessageQueue != null)
                Close();

            mRunServer = true;

            //Connect
            _inputMessageQueue = GetMessageQueue(_inputMessageQueueAddress);
            if (OutputQueueAddress != null)
            {
                _outputMessageQueue = GetMessageQueue(OutputQueueAddress);
            }

            //Start the threads
            //Receive queue messages
            mReceiveThread = new Thread(new ParameterizedThreadStart(RunReceiveMessages));
            mReceiveThread.IsBackground = true;
            mReceiveThread.Start(_inputMessageQueue);

            //Process received messages
            mReceiveProcessThread = new Thread(new ThreadStart(RunProcessReceivedMessages));
            mReceiveProcessThread.IsBackground = true;
            mReceiveProcessThread.Start();

            //Send responses
            mSendThread = new Thread(new ThreadStart(RunSendMessages));
            mSendThread.IsBackground = true;
            mSendThread.Start();
        }

        /// <summary>
        /// Close connection to the server, stop receiving messages
        /// </summary>
        public void Close()
        {
            mRunServer = false;

            //Wait a magic number of milliseconds to let the threads finish gracefully
            Thread.Sleep(2000);

            //Just abort it
            if (mReceiveThread != null && mReceiveThread.IsAlive)
                mReceiveThread.Abort();

            if (mReceiveProcessThread != null && mReceiveProcessThread.IsAlive)
                mReceiveProcessThread.Abort();

            if (mSendThread != null && mSendThread.IsAlive)
                mSendThread.Abort();

            //Just close it
            if (_inputMessageQueue != null)
            {
                _inputMessageQueue.Close();
                _inputMessageQueue = null;
            }

            if (_outputMessageQueue != null)
            {
                _outputMessageQueue.Close();
                _outputMessageQueue = null;
            }
        }

        /// <summary>
        /// The method sends a reply back to the client. 
        /// </summary>
        /// <param name="response"></param>
        public void Reply(MsmqMessageResponse response)
        {
            try
            {
                if (_outputMessageQueue == null)
                    throw new InvalidOperationException("Use the Open() method to initialize the server.");

                var contentType = JsonContentParser.ContentType;
                var messageBody = ContentParserFactory.GetParser(contentType).Encode(response.ResponseData);

                var message = new Message();
                message.Formatter = new XmlMessageFormatter(new[] { "System.String,mscorlib" });
                message.Body = messageBody;
                message.CorrelationId = response.CorrelationId;

                _outputMessageQueue.Send(message);
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed sending a MSMQ message. Error: {0}", exc.Message);
                throw new Exception(String.Format("Failed sending a MSMQ message. Error: {0}", exc.Message));
            }
        }

        #region IDisposable members...
        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
            Close();
        }
        #endregion

        #region Private methods...

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

        private void RunReceiveMessages(object client)
        {
            try
            {
                while (mRunServer)
                {
                    try
                    {
                        var requestMessage = _inputMessageQueue.Receive();
                        if (requestMessage != null)
                        {
                            requestMessage.Formatter = new XmlMessageFormatter(new[] { "System.String,mscorlib" });
                            ReceiveMsmqMessage(requestMessage);
                        }
                    }
                    catch (Exception exc)
                    {
                        log.Error(String.Format("Failed receiving MSMQ message. Error: {0}; Message queue: {1}", exc.Message, _inputMessageQueue.Path));
                    }
                }
            }
            catch (ThreadAbortException)
            {
                //Ignore, the thread has been interrupted
                log.Debug("Message-receiving thread has been aborted.");
            }
            finally
            {
                log.Debug("MSMQ message-receiving thread exited.");
            }
        }

        private void ReceiveMsmqMessage(Message message)
        {
            try
            {
                //Enqueue
                mReceiveQueue.Enqueue(new MsmqRequestReceivedEvArgs(new MsmqMessageRequest(message)));
            }
            catch (Exception exc)
            {
                log.DebugFormat("Failed parsing a received message. Error: {0}", exc.Message);
            }
        }

        private void RunProcessReceivedMessages()
        {
            while (true)
            {
                try
                {
                    //Process the received request in a separate thread
                    ThreadPool.QueueUserWorkItem(new WaitCallback(x => 
                    {
                        MsmqRequestReceivedEvArgs evArgs = (MsmqRequestReceivedEvArgs)x;
                        RaiseRequestReceived(evArgs);
                        if (evArgs.Response == null)
                        {
                            evArgs.Response = new MsmqMessageResponse(){CorrelationId = evArgs.Request.CorrelationId, ResponseType = MsmqMessageResponse.ResponseTypeOk, 
                                ResponseData = String.Empty};
                        }
                        mSendQueue.Enqueue(evArgs);
                    }),
                    mReceiveQueue.Dequeue());
                }
                catch (ThreadAbortException)
                {
                    //Ignore, the thread has been interrupted
                    log.Debug("Message-processing thread has been aborted.");
                    break;
                }
                catch (Exception exc)
                {
                    log.ErrorFormat("Failed processing a received message. Error: {0}", exc.Message);
                }
            }
        }

        private void RunSendMessages()
        {
            while (true)
            {
                try
                {
                    MsmqRequestReceivedEvArgs evArgs = mSendQueue.Dequeue();

                    if (_outputMessageQueue != null)
                    {
                        var contentType = JsonContentParser.ContentType;
                        var messageBody = ContentParserFactory.GetParser(contentType).Encode(evArgs.Response.ResponseData);

                        var message = new Message();
                        message.Formatter = new XmlMessageFormatter(new[] {"System.String,mscorlib"});
                        message.Body = messageBody;
                        message.Label = evArgs.Response.ResponseType;
                        message.CorrelationId = evArgs.Response.CorrelationId;

                        _outputMessageQueue.Send(message);
                    }
                }
                catch (ThreadAbortException)
                {
                    //Ignore, the thread has been interrupted
                    log.Debug("Message-sending thread has been aborted.");
                    break;
                }
                catch (Exception exc)
                {
                    log.ErrorFormat("Failed sending a response message. Message queue: {0}; Error: {1}", _outputMessageQueue.Path, exc.Message);
                }
            }
        }

        private void RaiseRequestReceived(MsmqRequestReceivedEvArgs args)
        {
            if (RequestReceived != null)
            {
                RequestReceived(this, args);
            }
        }
        #endregion
    }

    /// <summary>
    /// The class encapsulates raw data of an incoming request stored in a MSMQ message.
    /// </summary>
    public class MsmqMessageRequest
    {
        /// <summary>
        /// Constant - request type for store data requests.
        /// </summary>
        public const string RequestTypeStoreSensorData = "STORE_DATA";

        /// <summary>
        /// Constant - request type for set data requests to device sensors.
        /// </summary>
        public const string RequestTypeSetActuatorDataPrefix = "SET_DATA:";

        /// <summary>
        /// Constant - request type for set data requests to device sensors.
        /// </summary>
        public const string RequestTypeGetSensorCurrentDataPrefix = "GET_DATA:";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        public MsmqMessageRequest(Message message)
        {
            CorrelationId = message.Id;
            Body = Convert.ToString(message.Body);
            RequestType = message.Label;
        }

        /// <summary>
        /// MSMQ message correlation id.
        /// </summary>
        public string CorrelationId { get; set; }
        /// <summary>
        /// The request body - e.g., a JSON-ized <see cref="StoreSensorDataRequest"/> object.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Request types that says the type of the object that is stored in the <see cref="Body"/>.
        /// </summary>
        public string RequestType { get; set; }
    }

    /// <summary>
    /// The class encapsulates data of a response to be sent to the MSMQ output queue.
    /// </summary>
    public class MsmqMessageResponse
    {
        /// <summary>
        /// Constant: default response type. for a no-content OK response.
        /// </summary>
        public const string ResponseTypeOk = "OK";

        /// <summary>
        /// Constant: response type for an error response.
        /// </summary>
        public const string ResponseTypeError = "ERR";

        /// <summary>
        /// Constant: default response type. for a no-content OK response.
        /// </summary>
        public const string ResponseTypeStoreSensorData = "RESP_STORE_DATA";

        /// <summary>
        /// Constant: response type for get data requests.
        /// </summary>
        public const string ResponseTypeGetSensorData = "RESP_GET_DATA:";

        /// <summary>
        /// The MSMQ correlation id of the corresponding MSQM request message.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// The actual response data to be converted to MSMQ message body.
        /// </summary>
        public object ResponseData { get; set; }

        /// <summary>
        /// Response types that says the type of the object that is stored in the <see cref="Body"/>.
        /// </summary>
        public string ResponseType { get; set; }
    }

    /// <summary>
    /// Event argument class for the RequestReceived events.
    /// </summary>
    public class MsmqRequestReceivedEvArgs : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="request"></param>
        public MsmqRequestReceivedEvArgs(MsmqMessageRequest request)
        {
            Request = request;
            Success = true;
            ErrorMessage = null;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="success"></param>
        /// <param name="errorMessage"></param>
        public MsmqRequestReceivedEvArgs(MsmqMessageRequest request, bool success, string errorMessage)
        {
            Request = request;
            Success = success;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Flag showing of the receiving has been successful.
        /// </summary>
        public bool Success
        {
            get;
            private set;
        }

        /// <summary>
        /// Error message if there was an error receiving the response
        /// </summary>
        public string ErrorMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// Received request.
        /// </summary>
        public MsmqMessageRequest Request
        {
            get;
            private set;
        }

        /// <summary>
        /// Response to the request.
        /// </summary>
        public MsmqMessageResponse Response
        {
            get;
            set;
        }
    }
}
