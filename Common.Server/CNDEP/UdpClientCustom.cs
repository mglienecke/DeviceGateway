using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Common.Server.CNDEP
{
    /// <summary>
    /// The class extends the regular UdpClient in order to get access to the LocalEndPoint property.
    /// </summary>
    internal class UdpClientCustom:UdpClient
    {
        #region Constructors...
        /// <summary>
        /// Constructor.
        /// </summary>
        public UdpClientCustom() : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="family"></param>
        public UdpClientCustom(AddressFamily family) : base(family) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="port"></param>
        public UdpClientCustom(int port) : base(port) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="localEP"></param>
        public UdpClientCustom(IPEndPoint localEP) : base(localEP) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="family"></param>
        public UdpClientCustom(int port, AddressFamily family) : base(port, family) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        public UdpClientCustom(string hostname, int port) : base(hostname, port) { }
        #endregion

        /// <summary>
        /// The method returns the current local endpoint.
        /// </summary>
        public EndPoint LocalEndPoint
        {
            get
            {
                return base.Client.LocalEndPoint;
            }
        }
    }
}
