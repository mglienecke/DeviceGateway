using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;


namespace Common.Server.CNDEP
{
    /// <summary>
    /// The class implements a UDP server for the CNDEP protocol.
    /// </summary>
    public class CndepUdpServer : CndepServer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private UdpClientCustom _client;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <paparam name="serverPort"></paparam>
        public CndepUdpServer(int serverPort):base(serverPort)
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
            if (_client != null)
                Close();

            //Connect
            _client = GetClient();

            base.Open();
        }

        /// <summary>
        /// Close connection to the server, stop receiving messages
        /// </summary>
        public override void Close()
        {
            base.Close();

            //Just close it
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }

        /// <summary>
        /// The method sends a reply back to the client. 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="targetEndpoint"></param>
        public override void Reply(CndepMessageResponse message, IPEndPoint targetEndpoint)
        {
            try
            {
                if (_client == null)
                    throw new InvalidOperationException("Use the Opem() method to initialize the server.");

                //Get the IP endpoint
                byte[] bytes = message.GetMessageBytes();
                _client.Send(bytes, bytes.Length, targetEndpoint);
            }
            catch (Exception exc)
            {
                log.ErrorFormat("Failed sending a UDP message. Error: {0}", exc.Message);
                throw new Exception(String.Format("Failed sending a UDP message. Error: {0}", exc.Message));
            }
        }

        #region Private methods...

        private UdpClientCustom GetClient()
        {
            try
            {
                UdpClientCustom client = new UdpClientCustom(ServerPort);
                client.DontFragment = true;
                return client;
            }
            catch (Exception exc)
            {
                log.Error(String.Format("Failed setting up a UDP client. Server port: {0}; Error: {1}", ServerPort, exc.Message));
                throw new Exception(String.Format("Failed setting up a UDP client. Port: {0}; Error: {1}", ServerPort, exc.Message), exc);
            }
        }

        /// <summary>
        /// The method receives a data message from the network and processes it.
        /// </summary>
        protected override void ReceiveDataMessageFromNetwork(ProcessReceivedDataHandler handler)
        {
            IPEndPoint sendingEndpoint = new IPEndPoint(0, 0);
            try
            {
                handler(_client.Receive(ref sendingEndpoint), sendingEndpoint, null);
            }
            catch (Exception exc)
            {
                log.Error(String.Format("Failed receiving UDP messages. Error: {0}; Sending endpoint: {1}", exc.Message, sendingEndpoint));
            }
        }

        /// <summary>
        /// The method sends a response CNDEP message out to the network.
        /// </summary>
        /// <param name="evArgs"></param>
        protected override void SendDataMessageToNetwork(RequestReceivedEvArgs evArgs)
        {
            try
            {
                byte[] messageBytes = evArgs.Response.GetMessageBytes();
                _client.Send(messageBytes, messageBytes.Length, evArgs.SendingEndpoint);
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
        #endregion
    }
}
