using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using Microsoft.SPOT;
using NetMf.CommonExtensions;


namespace DeviceServer.Base.Cndep
{
    /// <summary>
    /// The class implements a UDP client for the CNDEP protocol.
    /// </summary>
    public class CndepClient:IDisposable
    {
        /// <summary>
        /// Default property value.
        /// </summary>
        public const int DefaultTimeoutInMillis = 5000;

        /// <summary>
        /// Default property value.
        /// </summary>
        public const int DefaultRequestRetryCount = 1;

        private UdpClient mClient;
        private int mServerPort = Int32.MinValue;
        private string mServerHostName;
        private int mClientReceivePort = Int32.MinValue;
        private Thread mReceiveThread;
        private Thread mReceiveProcessThread;
        private BlockingQueue mReceiveQueue = new BlockingQueue();
        private Hashtable mReceivedResponses = new Hashtable();
        private Hashtable mReceivedResponseLocks = new Hashtable();

        /// <summary>
        /// Event for asynchronously received events.
        /// </summary>
        public event EventHandler ResponseReceived;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serverPort"></param>
        /// <param name="hostName"></param>
        /// <param name="clientReceivePort"></param>
        public CndepClient(int serverPort, string hostName, int clientReceivePort)
        {
            ServerPort = serverPort;
            ServerHostName = hostName;
            ClientReceivePort = clientReceivePort;
            TimeoutInMillis = DefaultTimeoutInMillis;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serverPort"></param>
        /// <param name="hostName"></param>
        /// <param name="clientReceivePort"></param>
        public CndepClient(int serverPort, string hostName)
        {
            ServerPort = serverPort;
            ServerHostName = hostName;
            TimeoutInMillis = DefaultTimeoutInMillis;
        }

        #region Properties...
        /// <summary>
        /// The property contains the target server port number.
        /// </summary>
        public int ServerPort
        {
            get
            {
                return mServerPort;
            }
            private set
            {
                if (value < 0 || value > UInt16.MaxValue){
                    throw new ArgumentOutOfRangeException("value");
                }
                mServerPort = value;
            }
        }

        /// <summary>
        /// The property contains the target server IP address.
        /// </summary>
        public string ServerHostName
        {
            get
            {
                return mServerHostName;
            }
            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                mServerHostName = value;
            }
        }

        /// <summary>
        /// The property contains the client local port number for receiving respones (if set to 0, it 
        /// allows the socket to self-assign the local port number).
        /// </summary>
        public int ClientReceivePort
        {
            get
            {
                return mClientReceivePort;
            }
            private set
            {
                if (value < 0 || value > UInt16.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                mClientReceivePort = value;
            }
        }

        /// <summary>
        /// Timeout to receive a response.
        /// </summary>
        public int TimeoutInMillis
        {
            get;
            set;
        }

        /// <summary>
        /// Request retry count if there is a response timeout.
        /// </summary>
        public int RequestRetryCount
        {
            get;
            set;
        }
        #endregion

        /// <summary>
        /// Connect connection to the server, start receiving messages
        /// </summary>
        public void Connect()
        {
            //Close if opened
            if (mClient != null)
                Close();

            //Connect
            mClient = GetClient();

            //Start the threads
            //Receive UDP datagrams
            mReceiveThread = new Thread(new ThreadStart(RunReceiveMessages));
            mReceiveThread.Start();

            //Process received messages
            mReceiveProcessThread = new Thread(new ThreadStart(RunProcessReceivedMessages));
            mReceiveProcessThread.Start();
        }

        /// <summary>
        /// Close connection to the server, stop receiving messages
        /// </summary>
        public void Close()
        {
            //Just abort it
            if (mReceiveThread != null && mReceiveThread.IsAlive)
            {
                mReceiveThread.Abort();
                mReceiveThread = null;
            }

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
        /// Use the <see cref="Connect"/> method first in order to receive a response.
        /// </summary>
        /// <param name="message"></param>
        public void Send(CndepMessageRequest message)
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
                Debug.Print(StringUtility.Format("Failed sending a UDP message. Error: {0}", exc.Message));
                throw new Exception(StringUtility.Format("Failed sending a UDP message. Error: {0}", exc.Message));
            }
        }

        /// <summary>
        /// The method sends a request to the server and blocks until a response is received.
        /// Use the <see cref="Connect"/> method first.
        /// </summary>
        /// <param name="request"></param>
        /// <returns><c>null</c> if exit by timeout</returns>
        public CndepMessageResponse SendWithResponse(CndepMessageRequest request)
        {
            try
            {
                //Check the state
                if (mClient == null)
                    throw new InvalidOperationException("Use the Connect() method to open the connection to the server.");

                int retries = 0;
                do
                {
                    //Prepare receiving
                    ManualResetEvent receiveEvent = new ManualResetEvent(false);
                    mReceivedResponseLocks[request.SessionId] = receiveEvent;
                    mReceivedResponses.Remove(request.SessionId);

                    //Send
                    byte[] bytes = request.GetMessageBytes();

                    lock (mClient)
                    {
                        mClient.Send(bytes, bytes.Length);
                    }

                    //Wait to receive
                    if (receiveEvent.WaitOne(TimeoutInMillis, true))
                    {
                        CndepMessageResponse response = (CndepMessageResponse)mReceivedResponses[request.SessionId];
                        //Cleanup
                        mReceivedResponses.Remove(request.SessionId);
                        mReceivedResponseLocks.Remove(request.SessionId);
                        return response;
                    }
                }
                while (retries++ < RequestRetryCount);

                //There is no response
                //Cleanup
                mReceivedResponseLocks.Remove(request.SessionId);
                //Timeout exit
                return null;
            }
            catch (Exception exc)
            {
                Debug.Print(StringUtility.Format("Failed sending a UDP message. Error: {0}", exc.Message));
                throw new Exception(StringUtility.Format("Failed sending a UDP message. Error: {0}", exc.Message));
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

        private UdpClient GetClient()
        {
            try
            {
                //UdpClient client = (mClientReceivePort == 0) ? new UdpClient() : new UdpClient(mClientReceivePort, mServerHostName, mServerPort);
                UdpClient client = (mClientReceivePort == 0) ? new UdpClient() : new UdpClient(mServerHostName, mServerPort);
                client.Connect();
                return client;
            }
            catch (Exception exc)
            {
                Debug.Print(StringUtility.Format("Failed setting up a UDP client. Server address: {0}; Port: {1}; Error: {2}", mServerHostName, mServerPort, exc.Message));
                throw new Exception(StringUtility.Format("Failed setting up a UDP client. Server address: {0}; Port: {1}; Error: {2}", mServerHostName, mServerPort, exc.Message), exc);
            }
        }

        private void RunReceiveMessages()
        {
            try
            {
                IPHostEntry address = Dns.GetHostEntry(ServerHostName);
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, ServerPort);
                while (true)
                {
                    //Receive and Process
                    ReceiveDatagram(mClient.Receive(ref serverEndPoint));
                }
            }
            catch (ThreadAbortException)
            {
                //Ignore, the thread has been interrupted
                Debug.Print("Datagram-receiving thread has been aborted.");
            }
            catch (Exception exc)
            {
                Debug.Print(StringUtility.Format("Failed receiving UDP messages. Error: {0}", exc.Message));
                Debug.Print(StringUtility.Format("UDP message-receiving thread exiting due to an exception..."));
                RaiseResponseReceived(new ResponseReceivedEvArgs(null, false, StringUtility.Format("Failed receiving UDP messages. Error: {0}", exc.Message)));
            }
            finally{
                Debug.Print("UDP message-receiving thread exited.");
            }
        }

        private void ReceiveDatagram(byte[] data)
        {
            if (CndepMessageResponse.IsWellFormedResponseData(data))
            {
                try
                {
                    CndepMessageResponse response = new CndepMessageResponse(data);
                    if (mReceivedResponseLocks.Contains(response.SessionId))
                    {
                        //Keep the response and notify the waiting thread to retrieve it
                        mReceivedResponses[response.SessionId] = response;
                        ((ManualResetEvent)mReceivedResponseLocks[response.SessionId]).Set();
                    }
                    else
                    {
                        //Enqueue
                        mReceiveQueue.Enqueue(response);
                    }
                }
                catch (Exception exc)
                {
                    Debug.Print(StringUtility.Format("Failed parsing a received message. Error: {0}", exc.Message));
                }
            }
        }

        private void RunProcessReceivedMessages()
        {
            while (true)
            {
                try
                {
                    RaiseResponseReceived(new ResponseReceivedEvArgs((CndepMessageResponse)mReceiveQueue.Dequeue()));
                }
                catch (ThreadAbortException)
                {
                    //Ignore, the thread has been interrupted
                    Debug.Print("Message-processing thread has been aborted.");
                    break;
                }
                catch (Exception exc)
                {
                    Debug.Print(StringUtility.Format("Failed processing a received message. Error: {0}", exc.Message));
                }
            }
        }

        private void RaiseResponseReceived(ResponseReceivedEvArgs args)
        {
            if (ResponseReceived != null)
            {
                ResponseReceived(this, args);
            }
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
        public ResponseReceivedEvArgs(CndepMessageResponse response)
        {
            Response = response;
            Success = true;
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
    }
}