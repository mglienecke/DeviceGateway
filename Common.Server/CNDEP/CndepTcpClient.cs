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
    /// The class implements a TCP/IP client for the CNDEP protocol.
    /// </summary>
    public class CndepTcpClient:CndepClient
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serverPort"></param>
        /// <param name="serverAddress"></param>
        public CndepTcpClient(int serverPort, IPAddress serverAddress):base(serverPort, serverAddress)
        {
        }

        #region Properties...
        #endregion

        /// <summary>
        /// The method sends a CNDEP request message to the server. The response gets received asynchronously.
        /// Use the <see cref="Open"/> method first in order to receive a response.
        /// </summary>
        /// <param name="message"></param>
        public override void Send(CndepMessageRequest message)
        {
            try
            {
                var client = GetClient();

                //Use the single client
                byte[] bytes = message.GetMessageBytes();
                var stream = client.GetStream();
                stream.Write(bytes, 0, bytes.Length);
                ThreadPool.QueueUserWorkItem(ReceiveResponse, client);
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed sending a UDP message. Error: {0}", exc.Message);
                throw new Exception(String.Format("Failed sending a UDP message. Error: {0}", exc.Message), exc);
            }
        }

        private void ReceiveResponse(object state)
        {
            //Receive the resoponse asynchronously
            CndepMessageResponse response;
            EndPoint sendingEndpoint;
            using (var client = (TcpClient) state)
            {
                response = ReceiveResponse(client, client.GetStream());
                sendingEndpoint = client.Client.RemoteEndPoint;
            }

            RaiseResponseReceived(new ResponseReceivedEvArgs(response, (IPEndPoint)sendingEndpoint));
        }

        private const int ReceiveBufferSize = UInt16.MaxValue;

        private CndepMessageResponse ReceiveResponse(TcpClient client, NetworkStream stream)
        {
            //Read the bytes
            byte[] receiveBuffer = new byte[ReceiveBufferSize];
            int readBytesCount, readDataSize = 0;
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

            //Cut the last three bytes - 4,4,4 - ETX of the message
            Array.Resize(ref data, data.Length - 3);

            if (CndepMessageResponse.IsWellFormedResponseData(data))
            {
                try
                {
                    return new CndepMessageResponse(data);
                }
                catch (Exception exc)
                {
                    log.ErrorFormat("Failed parsing a received message. Error: {0}", exc.Message);
                    throw new Exception(String.Format("Failed parsing a received message. Error: {0}", exc.Message));
                }
            }

            throw new Exception("Malformed CNDEP response received.");
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
        /// Byte sequence to mark end of transmission.
        /// </summary>
        internal static readonly byte[] EtxSequence = new byte[] {4, 4, 4};

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
                using (var client = GetClient())
                {

                    //Send
                    byte[] bytes = request.GetMessageBytes();

                    //Send
                    var stream = client.GetStream();
                    stream.Write(bytes, 0, bytes.Length);
                    //Write ETX
                    stream.Write(EtxSequence, 0, EtxSequence.Length);

                    return ReceiveResponse(client, stream);
                }
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed performing a TCP message exchange. Error: {0}", exc.Message);
                throw new Exception(String.Format("Failed performing a TCP message exchange. Error: {0}", exc.Message), exc);
            }
        }

        #region Private methods...

        private TcpClient GetClient()
        {
            try
            {
                return new TcpClient(ServerAddress.ToString(), ServerPort);
            }
            catch (Exception exc)
            {
                log.Error(String.Format("Failed setting up a TCP client. Error: {0}", exc.Message));
                throw new Exception(String.Format("Failed setting up a TCP client. Error: {0}", exc.Message), exc);
            }
        }
        #endregion
    }
}