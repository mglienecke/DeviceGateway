using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Common.Server.CNDEP
{
    /// <summary>
    /// The class encapsulates data of a request message for the CNDEP protocol.
    /// </summary>
    public class CndepMessageRequest
    {
        private const byte STX = 2;
        private const byte ETX = 3;
        private const int MinimalRequestLength = 5;

        private const int IndSTX = 0;
        private const int IndSessionId = 1;
        private const int IndCommandId = 2;
        private const int IndFunctionId = 3;
        private const int IndData = 4;

        /// <summary>
        /// Check if the byte array is well-formed CNDEP request data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static bool IsWellFormedRequestData(byte[] data)
        {
            return data != null && data.Length >= CndepMessageRequest.MinimalRequestLength
                   && data[0] == CndepMessageRequest.STX && data[data.Length - 1] == CndepMessageRequest.ETX;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="commandId"></param>
        /// <param name="functionId"></param>
        /// <param name="data"></param>
        public CndepMessageRequest(byte sessionId, byte commandId, byte functionId, byte[] data)
        {
            SessionId = sessionId;
            CommandId = commandId;
            FunctionId = functionId;
            Data = data;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messageBytes"></param>
        public CndepMessageRequest(byte[] messageBytes)
        {
            //Checks
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            if (messageBytes.Length < MinimalRequestLength)
            {
                throw new ArgumentException(String.Format("Malformed message: invalid length: {0}", messageBytes.Length), "messageBytes");
            }

            if (messageBytes[IndSTX] != STX)
            {
                throw new ArgumentException("Malformed message: no STX", "messageBytes");
            }

            if (messageBytes[messageBytes.Length-1] != ETX)
            {
                throw new ArgumentException("Malformed message: no ETX", "messageBytes");
            }

            //Parse
            SessionId = messageBytes[IndSessionId];
            CommandId = messageBytes[IndCommandId];
            FunctionId = messageBytes[IndFunctionId];
            int dataLength = messageBytes.Length - 5;
            if (dataLength > 0)
            {
                Data = new byte[dataLength];
                Array.Copy(messageBytes, IndData, Data, 0, dataLength);
            }
        }

        /// <summary>
        /// Session id.
        /// </summary>
        public byte SessionId
        {
            get;
            private set;
        }

        /// <summary>
        /// Command id.
        /// </summary>
        public byte CommandId
        {
            get;
            private set;
        }

        /// <summary>
        /// Function id - modifier of the command id.
        /// </summary>
        public byte FunctionId
        {
            get;
            private set;
        }

        /// <summary>
        /// Data - may be null.
        /// </summary>
        public byte[] Data
        {
            get;
            private set;
        }

        /// <summary>
        /// Create a valid CNDEP datagram.
        /// </summary>
        /// <returns></returns>
        public byte[] GetMessageBytes()
        {
            byte[] messageBytes = new byte[5 + (Data == null ? 0 : Data.Length)];
            messageBytes[IndSTX] = STX;
            messageBytes[messageBytes.Length - 1] = ETX;
            messageBytes[IndSessionId] = SessionId;
            messageBytes[IndCommandId] = CommandId;
            messageBytes[IndFunctionId] = FunctionId;
            if (Data != null)
                Array.Copy(Data, 0, messageBytes, IndData, Data.Length);

            return messageBytes;
        }
    }

    /// <summary>
    /// The class encapsulates data of a request message for the CNDEP protocol.
    /// </summary>
    public class CndepMessageResponse
    {
        private const byte STX = 2;
        private const byte ETX = 3;
        private const int MinimalResponseLength = 4;

        private const int IndSTX = 0;
        private const int IndSessionId = 1;
        private const int IndResponseId = 2;
        private const int IndData = 3;

        /// <summary>
        /// Check if the byte array is well-formed CNDEP request data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static bool IsWellFormedResponseData(byte[] data)
        {
            return data != null && data.Length >= CndepMessageResponse.MinimalResponseLength
                   && data[0] == CndepMessageResponse.STX && data[data.Length - 1] == CndepMessageResponse.ETX;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="responseId"></param>
        /// <param name="data"></param>
        public CndepMessageResponse(byte sessionId, byte responseId, byte[] data)
        {
            SessionId = sessionId;
            ResponseId = responseId;
            Data = data;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messageBytes"></param>
        public CndepMessageResponse(byte[] messageBytes)
        {
            //Checks
            if (messageBytes == null)
            {
                throw new ArgumentNullException("messageBytes");
            }

            if (messageBytes.Length < MinimalResponseLength)
            {
                throw new ArgumentException(String.Format("Malformed message: invalid length: {0}", messageBytes.Length), "messageBytes");
            }

            if (messageBytes[IndSTX] != STX)
            {
                throw new ArgumentException("Malformed message: no STX", "messageBytes");
            }

            if (messageBytes[messageBytes.Length - 1] != ETX)
            {
                throw new ArgumentException("Malformed message: no ETX", "messageBytes");
            }

            //Parse
            SessionId = messageBytes[IndSessionId];
            ResponseId = messageBytes[IndResponseId];
            int dataLength = messageBytes.Length - 4;
            if (dataLength > 0)
            {
                Data = new byte[dataLength];
                Array.Copy(messageBytes, IndData, Data, 0, dataLength);
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messageBytes"></param>
        /// <param name="sendingEndPoint"></param>
        public CndepMessageResponse(byte[] messageBytes, IPEndPoint sendingEndPoint):this(messageBytes)
        {
            if (sendingEndPoint == null)
                throw new ArgumentNullException("sendingEndPoint");

            SendingEndPoint = sendingEndPoint;
        }

        /// <summary>
        /// Session id.
        /// </summary>
        public byte SessionId
        {
            get;
            private set;
        }

        /// <summary>
        /// Response id.
        /// </summary>
        public byte ResponseId
        {
            get;
            private set;
        }

        /// <summary>
        /// Data - may be null.
        /// </summary>
        public byte[] Data
        {
            get;
            private set;
        }

        /// <summary>
        /// Sending IP endpoint.
        /// </summary>
        public IPEndPoint SendingEndPoint
        {
            get;
            private set;
        }

        /// <summary>
        /// Create a valid CNDEP datagram.
        /// </summary>
        /// <returns></returns>
        public byte[] GetMessageBytes()
        {
            byte[] messageBytes = new byte[4 + (Data == null ? 0 : Data.Length)];
            messageBytes[IndSTX] = STX;
            messageBytes[messageBytes.Length - 1] = ETX;
            messageBytes[IndSessionId] = SessionId;
            messageBytes[IndResponseId] = ResponseId;
            if (Data != null)
                Array.Copy(Data, 0, messageBytes, IndData, Data.Length);

            return messageBytes;
        }
    }
}
