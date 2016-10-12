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
    public class CndepServer : IDisposable
    {
        private static readonly byte[] NoData = new byte[0];

        private UdpClient mClient;
        private int mLocalPort;
        private string mRemoteHostName;
        private Thread mReceiveThread;
        private Thread mSendThread;
        private Thread mReceiveProcessThread;
        private BlockingQueue mReceiveQueue = new BlockingQueue();
        private BlockingQueue mSendQueue = new BlockingQueue();
        private bool mRunServer = false;

        /// <summary>
        /// Event for asynchronously received events.
        /// </summary>
        public event EventHandler RequestReceived;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="port"></param>
        public CndepServer(int port)
        {
            LocalPort = port;
        }

        #region Properties...
        /// <summary>
        /// The property contains the local server port number.
        /// </summary>
        public int LocalPort
        {
            get
            {
                return mLocalPort;
            }
            private set
            {
                if (value < 0 || value > UInt16.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                mLocalPort = value;
            }
        }

        /// <summary>
        /// The property contains the hostname of the remote server.
        /// </summary>
        public string RemoteHostName
        {
            get
            {
                return mRemoteHostName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                mRemoteHostName = value;
            }
        }
        #endregion

        /// <summary>
        /// Connect connection to the server, start receiving messages
        /// </summary>
        public void Open()
        {
            //Close if opened
            if (mClient != null)
                Close();

            mRunServer = true;

            //Connect
            mClient = GetClient();

            //Start the threads
            //Receive UDP datagrams
            mReceiveThread = new Thread(new ThreadStart(RunReceiveMessages));
            mReceiveThread.Start();

            //Process received messages
            mReceiveProcessThread = new Thread(new ThreadStart(RunProcessReceivedMessages));
            mReceiveProcessThread.Start();

            //Send UDP datagrams
            mSendThread = new Thread(new ThreadStart(RunSendMessages));
            mSendThread.Start();
        }

        /// <summary>
        /// Close connection to the server, stop receiving messages
        /// </summary>
        public void Close()
        {
            mRunServer = false;

            //Wait for a magic number of seconds to let the processing and the sending threads to finish 
            //processing all messages in the queues.
            Thread.Sleep(2000);

            //Just abort it
            if (mReceiveThread != null && mReceiveThread.IsAlive)
                try { mReceiveThread.Abort(); } catch { }

            if (mReceiveProcessThread != null && mReceiveProcessThread.IsAlive)
                try { mReceiveProcessThread.Abort(); } catch { }

            if (mSendThread != null && mSendThread.IsAlive)
                try { mSendThread.Abort(); } catch { }

            //Just close it
            if (mClient != null)
            {
                try { mClient.Close(); } catch { }
                mClient = null;
            }
        }

        /// <summary>
        /// The property returns if the server is in the request-handling state now.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return (mReceiveThread != null && mReceiveThread.IsAlive);
            }
        }

        /// <summary>
        /// The method sends a reply back to the client. 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="targetEndpoint"></param>
        public void Reply(CndepMessageResponse message, IPEndPoint targetEndpoint)
        {
            try
            {
                if (mClient == null)
                    throw new InvalidOperationException("Use the Open() method to initialize the server.");

                byte[] bytes = message.GetMessageBytes();
                mClient.Send(bytes, bytes.Length, targetEndpoint);
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
                UdpClient client = new UdpClient(LocalPort);
                return client;
            }
            catch (Exception exc)
            {
                Debug.Print(StringUtility.Format("Failed setting up a UDP client. Port: {0}; Error: {1}", mLocalPort, exc.Message));
                throw new Exception(StringUtility.Format("Failed setting up a UDP client. Port: {0}; Error: {1}", mLocalPort, exc.Message), exc);
            }
        }

        private void RunReceiveMessages()
        {
            try
            {
                //mClient.Connect();

                while (mRunServer)
                {
                    //Receive and Process
                    IPEndPoint sendingEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = mClient.Receive(ref sendingEndPoint);

                    //if it is a valid request
                    if (CndepMessageRequest.IsWellFormedRequestData(data))
                    {
                        try
                        {
                            CndepMessageRequest request = new CndepMessageRequest(data);
                            //Enqueue
                            mReceiveQueue.Enqueue(new RequestReceivedEvArgs(request, sendingEndPoint));
                        }
                        catch (Exception exc)
                        {
                            Debug.Print(StringUtility.Format("Failed parsing a received message. Error: {0}", exc.Message));
                        }
                    }
                    //Else ignore
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
                RaiseRequestReceived(new RequestReceivedEvArgs(null, null, false, StringUtility.Format("Failed receiving UDP messages. Error: {0}", exc.Message)));
            }
            finally
            {
                Debug.Print("UDP message-receiving thread exited.");
            }
        }

        private void RunSendMessages()
        {
            while (true)
            {
                try
                {
                    RequestReceivedEvArgs evArgs = (RequestReceivedEvArgs)mSendQueue.Dequeue();
                    Reply(evArgs.Response, evArgs.SendingEndpoint);
                }
                catch (ThreadAbortException)
                {
                    //Ignore, the thread has been interrupted
                    Debug.Print("Message-sending thread has been aborted.");
                    break;
                }
                catch (Exception exc)
                {
                    Debug.Print(StringUtility.Format("Failed sending a response message. Error: {0}", exc.Message));
                }
            }
        }

        private void RunProcessReceivedMessages()
        {
            while (true)
            {
                try
                {
                    //Get event
                    RequestReceivedEvArgs evArgs = (RequestReceivedEvArgs)mReceiveQueue.Dequeue();
                    
                    //Raise
                    RaiseRequestReceived(evArgs);
                    
                    //Create the response if needed
                    if (evArgs.Response == null){
                        evArgs.Response = new CndepMessageResponse(evArgs.Request.SessionId, CndepConstants.RspOK, NoData);
                    }

                    //Put the response to the send queue
                    mSendQueue.Enqueue(evArgs);
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
        /// <param name="response"></param>
        /// <param name="sendingEndpoint"></param>
        public RequestReceivedEvArgs(CndepMessageRequest request, IPEndPoint sendingEndpoint)
        {
            Request = request;
            SendingEndpoint = sendingEndpoint;
            Success = true;
            ErrorMessage = null;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="sendingEndpoint"></param>
        /// <param name="success"></param>
        /// <param name="errorMessage"></param>
        public RequestReceivedEvArgs(CndepMessageRequest request, IPEndPoint sendingEndpoint, bool success, string errorMessage)
        {
            Request = request;
            SendingEndpoint = sendingEndpoint;
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
        /// The endpoint that has sent the message.
        /// </summary>
        public IPEndPoint SendingEndpoint
        {
            get;
            private set;
        }

        /// <summary>
        /// The response to the request that may be set by event listeners.
        /// </summary>
        public CndepMessageResponse Response
        {
            get;
            set;
        }
    }
}
