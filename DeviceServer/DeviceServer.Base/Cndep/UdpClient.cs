using System;
using Microsoft.SPOT;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT.Net.NetworkInformation;

namespace DeviceServer.Base
{
    /// <summary>
    /// The class implements a UDP client (since the expected System.Ext.Net.UdpClient is not present in the MFDpwsExtensions.dll for whatever reason, which I deem quite unproper.)
    /// </summary>
    public class UdpClient:IDisposable
    {
        private Socket mSocket;
        //private Socket mSocket1;
        private string mRemoteHostName;
        private int mRemotePort = Int32.MinValue;
        private int mLocalPort = Int32.MinValue;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UdpClient()
        {
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //mSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment, true);
            mSocket.SetSocketOption(SocketOptionLevel.Udp, SocketOptionName.SendBuffer, 10240);
            //mSocket1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="localPort"></param>
        public UdpClient(int localPort):this()
        {
            LocalPort = localPort;
            mSocket.Bind(GetLocalEndPoint());
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="localPort"></param>
        /// <param name="hostName"></param>
        /// <param name="serverPort"></param>
        public UdpClient(int localPort, string hostName, int serverPort):this(localPort)
        {
            RemoteHostName = hostName;
            RemotePort = serverPort;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="serverPort"></param>
        public UdpClient(string hostName, int serverPort)
            : this()
        {
            RemoteHostName = hostName;
            RemotePort = serverPort;
        }

        #region Properties...
        /// <summary>
        /// The property contains the target server port number.
        /// </summary>
        public int RemotePort
        {
            get
            {
                return mRemotePort;
            }
            private set
            {
                if (value < 0 || value > UInt16.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                mRemotePort = value;
            }
        }

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
        /// The property contains the target server IP address.
        /// </summary>
        public string RemoteHostName
        {
            get
            {
                return mRemoteHostName;
            }
            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                mRemoteHostName = value;
            }
        }
        #endregion

        #region IDisposable members...
        /// <summary>
        /// Dispose the object, free the resources.
        /// </summary>
        public void Dispose()
        {
            Close();
        }
        #endregion

        /// <summary>
        /// Connects to the default address.
        /// </summary>
        public void Connect()
        {
            if (mSocket == null)
                throw new InvalidOperationException("Socket == null");

            //mSocket.Connect(GetRemotePoint());
        }
 
        /// <summary>
        /// Connects to the specified address.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        public void Connect(string hostname, int port)
        {
            if (mSocket == null)
                throw new InvalidOperationException("Socket == null");

            RemoteHostName = hostname;
            RemotePort = port;

            //mSocket.Connect(GetRemotePoint());
        }

        /// <summary>
        /// Closes the current connection/
        /// </summary>
        public void Close()
        {
            if (mSocket != null) mSocket.Close();
            mSocket = null;

            //if (mSocket1 != null) mSocket1.Close();
            //mSocket1 = null;
        }

        /// <summary>
        /// Sends the specified number of bytes from the provided byte array.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public void Send(byte[] data, int length)
        {
            if (mSocket == null)
                throw new InvalidOperationException("Socket == null");

            mSocket.SendTo(data, length, SocketFlags.None, GetRemotePoint());
        }

        /// <summary>
        /// Sends the specified number of bytes from the provided byte array.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public void Send(byte[] data, int length, IPEndPoint targetEndPoint)
        {
            if (mSocket == null)
                throw new InvalidOperationException("Socket == null");

            mSocket.SendTo(data, length, SocketFlags.None, targetEndPoint);
        }

        /// <summary>
        /// Receive a datagram.
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <returns></returns>
        public byte[] Receive(ref IPEndPoint remoteEndPoint)
        {
            if (mSocket == null)
                throw new InvalidOperationException("Socket == null");

            try
            {
                EndPoint endpoint = remoteEndPoint;
                while (true)
                {
                    if (mSocket.Poll(-1, SelectMode.SelectRead))
                    {
                        byte[] data = new byte[mSocket.Available];
                        int length = mSocket.ReceiveFrom(data, ref endpoint);
                        remoteEndPoint = (IPEndPoint)endpoint;
                        return data;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                //Ignore
                return null;
            }
            catch (SocketException exc2)
            {
                Debug.Print(exc2.ToString());
                throw;
            }
        }

        private EndPoint GetLocalEndPoint()
        {
            if (mLocalPort == Int32.MinValue)
                throw new InvalidOperationException("LocalPort == null");


            return new IPEndPoint(IPAddress.Any, LocalPort);
        }

        private EndPoint GetRemotePoint()
        {
            if (mRemotePort == Int32.MinValue)
                throw new InvalidOperationException("RemotePort == null");

            if (mRemoteHostName == null)
                throw new InvalidOperationException("RemoteHostName == null");

            return new IPEndPoint(Dns.GetHostEntry(mRemoteHostName).AddressList[0], mRemotePort);
        }
    }
}
