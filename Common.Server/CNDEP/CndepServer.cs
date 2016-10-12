using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;


namespace Common.Server.CNDEP
{
    /// <summary>
    /// The class implements an abstract server for the CNDEP protocol.
    /// </summary>
    public abstract class CndepServer : IDisposable
    {
        private static readonly byte[] NoData = new byte[0];
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private int mServerPort;
        private Thread mReceiveThread;
        private Thread mSendThread;
        private Thread mReceiveProcessThread;
        private BlockingQueue<RequestReceivedEvArgs> mReceiveQueue = new BlockingQueue<RequestReceivedEvArgs>();
        private BlockingQueue<RequestReceivedEvArgs> mSendQueue = new BlockingQueue<RequestReceivedEvArgs>();
        private bool mRunServer = false;

        /// <summary>
        /// Event for asynchronously received events.
        /// </summary>
        public event EventHandler<RequestReceivedEvArgs> RequestReceived;

        /// <summary>
        /// Handler for processing received data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sendingEndpoint"></param>
        public delegate void ProcessReceivedDataHandler(byte[] data, IPEndPoint sendingEndpoint, NetworkStream stream);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="port"></param>
        /// <paparam name="clientPort"></paparam>
        public CndepServer(int serverPort)
        {
            ServerPort = serverPort;
        }

        #region Properties...
        /// <summary>
        /// The property contains the server port number.
        /// </summary>
        public int ServerPort
        {
            get
            {
                return mServerPort;
            }
            private set
            {
                if (value < 0 || value > UInt16.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                mServerPort = value;
            }
        }

        #endregion

        /// <summary>
        /// Connect connection to the server, start receiving messages
        /// </summary>
        public virtual void Open()
        {
            mRunServer = true;

            //Start the threads
            //Receive UDP datagrams
            mReceiveThread = new Thread(new ParameterizedThreadStart(RunReceiveMessages));
            mReceiveThread.IsBackground = true;
            mReceiveThread.Start();

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
        public virtual void Close()
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
        }

        /// <summary>
        /// The method sends a reply back to the client. 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="targetEndpoint"></param>
        public abstract void Reply(CndepMessageResponse message, IPEndPoint targetEndpoint);

        /// <summary>
        /// The method receives a data message from the network and processes it.
        /// </summary>
        protected abstract void ReceiveDataMessageFromNetwork(ProcessReceivedDataHandler handler);

        /// <summary>
        /// The method sends a response CNDEP message out to the network.
        /// </summary>
        /// <param name="evArgs"></param>
        protected abstract void SendDataMessageToNetwork(RequestReceivedEvArgs evArgs);

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

        private void RunReceiveMessages(object client)
        {
            try
            {
                while (mRunServer)
                {
                    Thread.Yield();
                    //Receive and Process
                    ReceiveDataMessageFromNetwork(ProcessReceivedData);
                }
            }
            catch (ThreadAbortException)
            {
                //Ignore, the thread has been interrupted
                log.Debug("CNDEP message-receiving thread has been aborted.");
            }
            finally
            {
                log.Debug("CNDEP message-receiving thread exited.");
            }
        }

        private void ProcessReceivedData(byte[] data, IPEndPoint sendingEndpoint, NetworkStream stream)
        {
            if (data != null)
            {
                if (CndepMessageRequest.IsWellFormedRequestData(data))
                {
                    try
                    {
                        CndepMessageRequest response = new CndepMessageRequest(data);
                        //Enqueue
                        mReceiveQueue.Enqueue(new RequestReceivedEvArgs(response, sendingEndpoint, stream));
                    }
                    catch (Exception exc)
                    {
                        log.DebugFormat("Failed parsing a received message. Error: {0}", exc.Message);
                    }
                }
                else
                {
                    log.DebugFormat("Malformed message received. Sending endpoint: {0}", sendingEndpoint.ToString());
                    RequestReceivedEvArgs evArgs = new RequestReceivedEvArgs(null, sendingEndpoint, stream);
                    evArgs.Response = new CndepMessageResponse(0, CndepCommands.RspError, NoData);

                    mSendQueue.Enqueue(evArgs);
                }
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
                        RequestReceivedEvArgs evArgs = (RequestReceivedEvArgs)x;
                        RaiseRequestReceived(evArgs);
                        if (evArgs.Response == null)
                        {
                            evArgs.Response = new CndepMessageResponse(evArgs.Request.SessionId, CndepCommands.RspOK, NoData);
                        }
                        mSendQueue.Enqueue(evArgs);
                    }),
                    (RequestReceivedEvArgs)mReceiveQueue.Dequeue());
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
                RequestReceivedEvArgs evArgs = (RequestReceivedEvArgs)mSendQueue.Dequeue();

                try
                {
                    SendDataMessageToNetwork(evArgs);
                }
                catch (ThreadAbortException)
                {
                    //Ignore, the thread has been interrupted
                    log.Debug("Message-sending thread has been aborted.");
                    break;
                }
                catch (Exception exc)
                {
                    log.ErrorFormat("Failed sending a response message. Endpoint: {0}; Session id: {1}; Error: {2}", evArgs.SendingEndpoint, evArgs.Request.SessionId, exc.Message);
                }
            }
        }

        private void RaiseRequestReceived(RequestReceivedEvArgs args)
        {
            if (RequestReceived != null)
            {
                RequestReceived(this, args);
            }
        }
        #endregion
    }

    /// <summary>
    /// Event argument class for the RequestReceived events.
    /// </summary>
    public class RequestReceivedEvArgs : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="sendingEndpoint"></param>
        /// <param name="stream"></param>
        public RequestReceivedEvArgs(CndepMessageRequest request, IPEndPoint sendingEndpoint, NetworkStream stream)
        {
            Request = request;
            SendingEndpoint = sendingEndpoint;
            Stream = stream;
            Success = true;
            ErrorMessage = null;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="sendingEndpoint"></param>
        /// <param name="stream"></param>
        /// <param name="success"></param>
        /// <param name="errorMessage"></param>
        public RequestReceivedEvArgs(CndepMessageRequest request, IPEndPoint sendingEndpoint, NetworkStream stream, bool success, string errorMessage)
        {
            Request = request;
            SendingEndpoint = sendingEndpoint;
            Stream = stream;
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
        public CndepMessageRequest Request
        {
            get;
            private set;
        }

        /// <summary>
        /// The endpoint that has sent the message (UDP mode).
        /// </summary>
        public IPEndPoint SendingEndpoint
        {
            get;
            private set;
        }

        /// <summary>
        /// The communication stream to use for sending the response (TCP mode)
        /// </summary>
        public NetworkStream Stream { get; private set; }

        /// <summary>
        /// Response to the request.
        /// </summary>
        public CndepMessageResponse Response
        {
            get;
            set;
        }
    }

    /// <summary>
    /// The enumeration declares constants for possible communication protocols.
    /// </summary>
    public enum CommunicationProtocol
    {
        /// <summary>
        /// UDP
        /// </summary>
        UDP = 0,

        /// <summary>
        /// TCP
        /// </summary>
        TCP = 1
    }
}
