//Copyright 2011 Oberon microsystems, Inc.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

//Developed for the book
//  "Getting Started with the Internet of Things", by Cuno Pfister.
//  Copyright 2011 Cuno Pfister, Inc., 978-1-4493-9357-1.
//
//Version 0.9 (beta release)

// This is a variant of the asynchronous Yaler client library published at
//    http://hg.yaler.org/yalercontrib/src/a8d40553a4d9/CSharp/Library/

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Gsiot.Contracts;

namespace Gsiot.Yaler
{
    /// <summary>
    /// Object that emulates a listener socket (incoming) by opening an
    /// outgoing socket to a relay server. Once the initialization protocol
    /// has completed (upgrade from HTTP to PTTH protocol for reverse HTTP)
    /// then the socket is returned to the server application, which
    /// awaits incoming requests on this socket.
    /// </summary>
    public sealed class YalerListener
    {
        readonly string host, relayDomain;
        readonly int port;
        bool aborted;
        readonly object abortedLock = new object();
        Socket listener;

        /// <summary>
        /// Constructor for opening an emulated listener socket using
        /// the Yaler reverse HTTP protocol.
        /// Preconditions
        ///     host != null
        ///     host.Length > 0
        ///     port > 0
        ///     port leq 65536
        ///     relayDomain != null
        ///     relayDomain.Length >= 11
        /// </summary>
        /// <param name="host">Internet address or domain name of the
        /// relay server.</param>
        /// <param name="port">Port number of the relay service.</param>
        /// <param name="relayDomain">Yaler relay domain.</param>
        public YalerListener(string host, int port, string relayDomain)
        {
            Contract.Requires(host != null);
            Contract.Requires(host.Length > 0);
            Contract.Requires(port > 0);
            Contract.Requires(port <= 65535);
            Contract.Requires(relayDomain != null);
            Contract.Requires(relayDomain.Length >= 11);
            this.host = host;
            this.port = port;
            this.relayDomain = relayDomain;
        }

        /// <summary>
        /// Close the emulated listener.
        /// </summary>
        public void Abort()
        {
            lock (abortedLock)
            {
                aborted = true;
            }
            try
            {
                listener.Close();
            }
            catch { }
        }

        void Find(string pattern, Socket s, out bool found)
        {
            int[] x = new int[pattern.Length];
            byte[] b = new byte[1];
            int i = 0, j = 0, t = 0;
            do
            {
                found = true;
                for (int k = 0; (k != pattern.Length) && found; k++)
                {
                    if (i + k == j)
                    {
                        int n = s.Receive(b);
                        x[j % x.Length] = n != 0 ? b[0] : -1;
                        j = j + 1;
                    }
                    t = x[(i + k) % x.Length];
                    found = pattern[k] == t;
                }
                i = i + 1;
            } while (!found && (t != -1));
        }

        void FindLocation(Socket s, out string host, out int port)
        {
            host = null;
            port = 80;
            bool found;
            Find("\r\nLocation: http://", s, out found);
            if (found)
            {
                char[] stringChars = new char[40];
                var stringIndex = 0;
                byte[] x = new byte[1];
                int n = s.Receive(x);
                while ((n != 0) && (x[0] != ':') && (x[0] != '/'))
                {
                    stringChars[stringIndex] = (char)x[0];
                    stringIndex = stringIndex + 1;
                    n = s.Receive(x);
                }
                if (x[0] == ':')
                {
                    port = 0;
                    n = s.Receive(x);
                    while ((n != 0) && (x[0] != '/'))
                    {
                        port = 10 * port + x[0] - '0';
                        n = s.Receive(x);
                    }
                }
                host = new string(stringChars, 0, stringIndex);
            }
        }

        /// <summary>
        /// Register this device at the relay service and return a socket
        /// for upcoming requests (and their responses).
        /// </summary>
        /// <returns>Socket on which a request can be awaited.</returns>
        public Socket Accept()
        {
            bool a;
            lock (abortedLock)
            {
                a = aborted;
            }
            if (a)
            {
                throw new InvalidOperationException();
            }
            else
            {
                string host = this.host;
                int port = this.port;
                Socket s;
                bool acceptable;
                int[] x = new int[3];
                byte[] b = new byte[1];
                do
                {
                    // look up relay host's domain name,
                    // to find IP address(es)
                    IPHostEntry hostEntry = Dns.GetHostEntry(host);
                    // extract a returned address
                    IPAddress hostAddress = hostEntry.AddressList[0];
                    IPEndPoint remoteEndPoint = new IPEndPoint(hostAddress,
                        port);

                    listener = new Socket(
                        AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    listener.Connect(remoteEndPoint);
                    listener.SetSocketOption(SocketOptionLevel.Tcp,
                        SocketOptionName.NoDelay, true);
                    s = listener;
                    do
                    {
                        s.Send(Encoding.UTF8.GetBytes(
                            "POST /" + relayDomain + " HTTP/1.1\r\n" +
                            "Upgrade: PTTH/1.0\r\n" +
                            "Connection: Upgrade\r\n" +
                            "Host: " + host + "\r\n\r\n"));
                        for (int j = 0; j != 12; j = j + 1)
                        {
                            int n = s.Receive(b);
                            x[j % 3] = n != 0 ? b[0] : -1;
                        }
                        if ((x[0] == '3') && (x[1] == '0') && (x[2] == '7'))
                        {
                            FindLocation(s, out host, out port);
                        }
                        Find("\r\n\r\n", s, out acceptable);
                    } while (acceptable && ((x[0] == '2') && (x[1] == '0') && (x[2] == '4')));
                    if (!acceptable || (x[0] != '1') || (x[1] != '0') || (x[2] != '1'))
                    {
                        s.Close();
                        s = null;
                    }
                } while (acceptable && ((x[0] == '3') && (x[1] == '0') && (x[2] == '7')));
                listener = null;
                return s;
            }
        }
    }
}