using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Common.Server
{
    public class NetworkUtilities
    {
        /// <summary>
        /// Return the IPv4 address of the DNS entry
        /// </summary>
        /// <param name="dnsEntry">name of the host e.g. localhost</param>
        /// <returns>IPv4 address or <c>null</c></returns>
        public static IPAddress GetIpV4AddressForDns(string dnsEntry)
        {
            string resolvedAddress = string.Empty;
            foreach (IPAddress ip in Dns.GetHostEntry(dnsEntry).AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return (ip);
                }
            }
            return (null);
        }
    }
}
