using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;


namespace Common.Server.CNDEP
{
    /// <summary>
    /// The class implements an abstract client for the CNDEP protocol.
    /// </summary>
    public abstract class CndepClient:IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private int mServerPort = Int32.MinValue;
        private IPAddress mServerAddress;

        /// <summary>
        /// Event for asynchronously received events.
        /// </summary>
        public event EventHandler<ResponseReceivedEvArgs> ResponseReceived;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serverPort"></param>
        /// <param name="serverAddress"></param>
        public CndepClient(int serverPort, IPAddress serverAddress)
        {
            ServerPort = serverPort;
            ServerAddress = serverAddress;
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
        public IPAddress ServerAddress
        {
            get
            {
                return mServerAddress;
            }
            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                mServerAddress = value;
            }
        }
        #endregion

        /// <summary>
        /// Open the client, start receiving messages
        /// </summary>
        public virtual void Open()
        {
        }

        /// <summary>
        /// Close connection to the server, stop receiving messages
        /// </summary>
        public virtual void Close()
        {
        }

        /// <summary>
        /// The method sends a CNDEP request message to the server. The response gets received asynchronously.
        /// Use the <see cref="Open"/> method first in order to receive a response.
        /// </summary>
        /// <param name="message"></param>
        public abstract void Send(CndepMessageRequest message);

        /// <summary>
        /// The method sends a request to the server and blocks until a response is received.
        /// Use the <see cref="Open"/> method first.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="timeout">Timeout in millis to wait for the response.</param>
        /// <returns><c>null</c> if exit by timeout</returns>
        public abstract CndepMessageResponse Send(CndepMessageRequest request, int timeout);

        #region IDisposable members...
        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
            Close();
        }
        #endregion

        /// <summary>
        /// The method raises a ResponseReceived event using the provided arguments.
        /// </summary>
        /// <param name="args"></param>
        protected void RaiseResponseReceived(ResponseReceivedEvArgs args)
        {
            if (ResponseReceived != null)
            {
                ResponseReceived(this, args);
            }
        }
    }
}