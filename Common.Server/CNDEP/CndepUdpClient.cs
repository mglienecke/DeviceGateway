using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Common.Server.CNDEP
{
    /// <summary>
    /// The class implements a UDP client for the CNDEP protocol.
    /// </summary>
    public class CndepUdpClient:CndepClient
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private UdpClientCustom mClient;
        private Thread mReceiveThread;
        private Thread mReceiveProcessThread;
        private BlockingQueue<CndepMessageResponse> mReceiveQueue = new BlockingQueue<CndepMessageResponse>();
        private Dictionary<byte, CndepMessageResponse> mReceivedResponses = new Dictionary<byte, CndepMessageResponse>();
        private Dictionary<byte, ManualResetEvent> mReceivedResponseLocks = new Dictionary<byte, ManualResetEvent>();

        /// <summary>
        /// Event for asynchronously received events.
        /// </summary>
        public event EventHandler<ResponseReceivedEvArgs> ResponseReceived;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serverPort"></param>
        /// <param name="serverAddress"></param>
        public CndepUdpClient(int serverPort, IPAddress serverAddress):base(serverPort, serverAddress)
        {
        }

        #region Properties...
        #endregion

        /// <summary>
        /// Open the client, start receiving messages
        /// </summary>
        public override void Open()
        {
            //Close if opened
            if (mClient != null)
                Close();

            //Connect
            mClient = GetClient();
            mClient.Connect(ServerAddress, ServerPort);

            //Start the threads
            //Process received messages
            mReceiveProcessThread = new Thread(new ThreadStart(RunProcessReceivedMessages));
            mReceiveProcessThread.IsBackground = true;
            mReceiveProcessThread.Start();
        }

        /// <summary>
        /// Close connection to the server, stop receiving messages
        /// </summary>
        public override void Close()
        {
            //Just abort it
            if (mReceiveThread != null && mReceiveThread.IsAlive)
            {
                mReceiveThread.Abort();
                mReceiveThread = null;
            }

            //Just abort it
            if (mReceiveProcessThread != null && mReceiveProcessThread.IsAlive)
            {
                mReceiveProcessThread.Abort();
                mReceiveProcessThread = null;
            }

            //Just close it
            if (mClient != null)
            {
                mClient.Close();
                mClient = null;
            }
        }

        /// <summary>
        /// The method sends a CNDEP request message to the server. The response gets received asynchronously.
        /// Use the <see cref="Open"/> method first in order to receive a response.
        /// </summary>
        /// <param name="message"></param>
        public override void Send(CndepMessageRequest message)
        {
            try
            {
                if (mClient == null)
                    throw new InvalidOperationException("Use the Connect() method to open the connection to the server.");

                //Use the single client
                byte[] bytes = message.GetMessageBytes();
                mClient.Send(bytes, bytes.Length);
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed sending a UDP message. Error: {0}", exc.Message);
                throw new Exception(String.Format("Failed sending a UDP message. Error: {0}", exc.Message), exc);
            }
        }

        /// <summary>
        /// The method sends a request to the server and blocks until a response is received.
        /// Use the <see cref="Open"/> method first.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="timeout">Timeout in millis to wait for the response.</param>
        /// <returns><c>null</c> if exit by timeout</returns>
        public override CndepMessageResponse Send(CndepMessageRequest request, int timeout)
        {
            Thread receiveThread = null;
            try
            {
                //Check the state
                if (mClient == null)
                    throw new InvalidOperationException("Use the Connect() method to open the connection to the server.");

                //Prepare receiving
                ManualResetEvent receiveEvent = new ManualResetEvent(false);
                mReceivedResponseLocks[request.SessionId] = receiveEvent;
                mReceivedResponses.Remove(request.SessionId);

                //Send
                byte[] bytes = request.GetMessageBytes();
                // byte[] bytes = new byte[] { 0xFF, 0xFF };

                //Start the receive thread if the socked is already bound
                if (mReceiveThread == null && mClient.LocalEndPoint != null)
                    StartReceiveThread();
                    

                //Send
                mClient.Send(bytes, bytes.Length);    
                

                //Start the receive thread if it is not started yet (since the socket is surely bound now)
                if (mReceiveThread == null)
                    StartReceiveThread();


                //Wait to receive
                if (receiveEvent.WaitOne(timeout))
                {
                    CndepMessageResponse response = mReceivedResponses[request.SessionId];
                    //Cleanup
                    mReceivedResponses.Remove(request.SessionId);
                    mReceivedResponseLocks.Remove(request.SessionId);
                    return response;
                }
                else
                {
                    //Cleanup
                    mReceivedResponseLocks.Remove(request.SessionId);
                    //Timeout
                    return null;
                }
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed sending a UDP message. Error: {0}", exc.Message);
                throw new Exception(String.Format("Failed sending a UDP message. Error: {0}", exc.Message), exc);
            }
            finally
            {
                if (receiveThread != null && receiveThread.IsAlive)
                    receiveThread.Abort();
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

        private void StartReceiveThread()
        {
            //Start the receive thread
            mReceiveThread = new Thread(new ParameterizedThreadStart(RunReceiveMessages));
            mReceiveThread.IsBackground = true;
            mReceiveThread.Start(mClient);
        }

        private UdpClientCustom GetClient()
        {
            try
            {
                UdpClientCustom client = new UdpClientCustom();
                client.DontFragment = false;
                return client;
            }
            catch (Exception exc)
            {
                log.Error(String.Format("Failed setting up a UDP client. Server address: {0}; Port: {1}; Error: {2}", exc.Message));
                throw new Exception(String.Format("Failed setting up a UDP client. Server address: {0}; Port: {1}; Error: {2}", ServerAddress, ServerPort, exc.Message), exc);
            }
        }

        private void RunReceiveMessages(object client)
        {
            try
            {
                IPEndPoint serverEndPoint = new IPEndPoint(ServerAddress, ServerPort);
                while (true)
                {
                    try
                    {
                        //Receive and Process
                        ReceiveDatagram(((UdpClientCustom)client).Receive(ref serverEndPoint), serverEndPoint);
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception exc)
                    {
                        log.Error(String.Format("Failed receiving UDP messages. Error: {0}", exc.Message));
                        log.Error(String.Format("UDP message-receiving thread exiting due to an exception..."));
                        RaiseResponseReceived(new ResponseReceivedEvArgs(null, false, String.Format("Failed receiving UDP messages. Error: {0}", exc.Message)));
                    }
                }
            }
            catch (ThreadAbortException)
            {
                //Ignore, the thread has been interrupted
                log.Debug("UDP message-receiving thread exited.");
            }
            finally
            {
                log.Debug("UDP message-receiving thread exited.");
            }
        }

        private void ReceiveDatagram(byte[] data, IPEndPoint sendingEndPoint)
        {
            if (CndepMessageResponse.IsWellFormedResponseData(data))
            {
                try
                {
                    CndepMessageResponse response = new CndepMessageResponse(data, sendingEndPoint);
                    if (mReceivedResponseLocks.ContainsKey(response.SessionId))
                    {
                        //Keep the response and notify the waiting thread to retrieve it
                        mReceivedResponses[response.SessionId] = response;
                        mReceivedResponseLocks[response.SessionId].Set();
                    }
                    else
                    {
                        //Enqueue
                        mReceiveQueue.Enqueue(response);
                    }
                }
                catch (Exception exc)
                {
                    log.ErrorFormat("Failed parsing a received message. Error: {0}", exc.Message);
                }
            }
        }

        private void RunProcessReceivedMessages()
        {
            while (true)
            {
                try
                {
                    CndepMessageResponse response = (CndepMessageResponse)mReceiveQueue.Dequeue();
                    RaiseResponseReceived(new ResponseReceivedEvArgs(response, response.SendingEndPoint));
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

        private EndPoint GetRemotePoint()
        {
            if (ServerAddress == null)
                throw new InvalidOperationException("ServerAddress == null");

            if (ServerPort == Int32.MinValue)
                throw new InvalidOperationException("InputQueueAddress == null");

            return new IPEndPoint(Dns.GetHostEntry(ServerAddress).AddressList[0], ServerPort);
        }
        #endregion
    }

    /// <summary>
    /// Event argument class for the ResponseReceived events.
    /// </summary>
    public class ResponseReceivedEvArgs : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="sendingEndpoint"></param>
        public ResponseReceivedEvArgs(CndepMessageResponse response, IPEndPoint sendingEndpoint)
        {
            Response = response;
            Success = true;
            SendingEndpoint = sendingEndpoint;
            ErrorMessage = null;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="success"></param>
        /// <param name="errorMessage"></param>
        public ResponseReceivedEvArgs(CndepMessageResponse response, bool success, string errorMessage)
        {
            Response = response;
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
        /// Received response.
        /// </summary>
        public CndepMessageResponse Response
        {
            get;
            private set;
        }

        /// <summary>
        /// The endpoint that has sent the message.
        /// </summary>
        public IPEndPoint SendingEndpoint
        {
            get;
            private set;
        }
    }
}