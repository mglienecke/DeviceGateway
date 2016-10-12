using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;


namespace Common.Server.CNDEP
{
    /// <summary>
    /// The class implements a TCP/IP server for the CNDEP protocol.
    /// </summary>
    public class CndepTcpServer : CndepServer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private TcpListener _tcpListener;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serverPort"></param>
        public CndepTcpServer(int serverPort):base(serverPort)
        {
        }

        #region Properties...
        #endregion

        /// <summary>
        /// Connect connection to the server, start receiving messages
        /// </summary>
        public override void Open()
        {
            //Close if opened
            if (_tcpListener != null)
                Close();

            //Connect
            _tcpListener = GetTcpListener();           

            base.Open();
        }

        /// <summary>
        /// Close connection to the server, stop receiving messages
        /// </summary>
        public override void Close()
        {
            base.Close();

            //Just close it
            if (_tcpListener != null)
            {
                _tcpListener.Stop();
                _tcpListener = null;
            }
        }

        private const int ReceiveBufferSize = UInt16.MaxValue;

        /// <summary>
        /// The method receives a data message from the network and processes it.
        /// </summary>
        protected override void ReceiveDataMessageFromNetwork(ProcessReceivedDataHandler handler)
        {
            try
            {
                TcpClient tcpClient = _tcpListener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(delegate(object state)
                    {
                        var client = (TcpClient) state;
                        IPEndPoint sendingEndpoint = (IPEndPoint)client.Client.RemoteEndPoint;
                        try
                        {
                            //Get the stream
                            var stream = client.GetStream();

                            //Read the bytes
                            byte[] receiveBuffer = new byte[ReceiveBufferSize];
                            int readBytesCount = 0, readDataSize = 0;
                            byte[] data = new byte[0];
      
                            while ((readBytesCount = stream.Read(receiveBuffer, 0, receiveBuffer.Length)) > 0)
                            {
                                Array.Resize(ref data, readDataSize + readBytesCount);
                                Array.Copy(receiveBuffer, 0, data, readDataSize, readBytesCount);
                                readDataSize += readBytesCount;

                                if (IsEndOfTransmission(data, readDataSize))
                                {
                                    break;
                                }
                            }

                            Array.Resize(ref data, data.Length - 3);

                            handler(data, sendingEndpoint, stream);
                        }
                        catch (Exception exc)
                        {
                            log.Error(String.Format("Failed receiving CNDEP message over TCP. Error: {0}; Sending endpoint: {1}",
                                                    exc.Message,
                                                    sendingEndpoint));
                        }
                    }, tcpClient);
            }
            catch (Exception exc)
            {
                log.Error(String.Format("Failed receiving CNDEP message over TCP. Error: {0}", exc.Message));
            }
        }

        private const byte CndepEtx = 3;

        private bool IsEndOfTransmission(byte[] data, int length)
        {
            if (length > 4)
            {
                if (data[length - 1] == CndepTcpClient.EtxSequence[2] 
                    && data[length - 2] == CndepTcpClient.EtxSequence[1]
                    && data[length - 3] == CndepTcpClient.EtxSequence[0]
                    && data[length - 4] == CndepEtx)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// The method sends a response CNDEP message out to the network.
        /// </summary>
        /// <param name="evArgs"></param>
        protected override void SendDataMessageToNetwork(RequestReceivedEvArgs evArgs)
        {
            try
            {
                using (evArgs.Stream)
                {
                    byte[] messageBytes = evArgs.Response.GetMessageBytes();
                    evArgs.Stream.Write(messageBytes, 0, messageBytes.Length);
                    //Write ETX
                    evArgs.Stream.Write(CndepTcpClient.EtxSequence, 0, CndepTcpClient.EtxSequence.Length);
                }

            }
            catch (ThreadAbortException)
            {
                //Ignore, the thread has been interrupted
                log.Debug("Message-sending thread has been aborted.");
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed sending a response message. Endpoint: {0}; Session id: {1}; Error: {2}", evArgs.SendingEndpoint, evArgs.Request.SessionId, exc.Message);
            }
        }

        /// <summary>
        /// The method sends a reply back to the client. 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="targetEndpoint"></param>
        public override void Reply(CndepMessageResponse message, IPEndPoint targetEndpoint)
        {
            throw new NotImplementedException();
        }

        #region Private methods...

        private TcpListener GetTcpListener()
        {
            IPAddress localAddressIPv4 = NetworkUtilities.GetIpV4AddressForDns(Dns.GetHostName());
            var localEndPoint = new IPEndPoint(localAddressIPv4, ServerPort);

            // Create a TCP/IP socket.
            var tcpListener = new TcpListener(localEndPoint);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.
            try
            {
                tcpListener.Start();
                return tcpListener;
            }
            catch (Exception exc)
            {
                log.Error(String.Format("Failed setting up a TCP listener. Server address: {0}; Port: {1}; Error: {2}", localEndPoint.Address, localEndPoint.Port, exc.Message));
                throw new Exception(String.Format("Failed setting up a TCP listener. Server address: {0}; Port: {1}; Error: {2}", localEndPoint.Address, localEndPoint.Port, exc.Message));
            }
        }
        #endregion
    }
}
