using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.SPOT;
using System;

namespace DeviceServer.Base
{
    public class EfficientPostRequest
    {
        public static void SendPostRequest(Uri targetUri, string contentType, string content)
        {
            //byte[] contentBuffer = Encoding.UTF8.GetBytes(content);

            using (Socket connection = Connect(targetUri.Host, 5000))
            {
                SendRequest(connection, targetUri, contentType, content);
                connection.Close();
            }
        }

        static Socket Connect(string host, int timeout)
        {
            // look up host's domain name to find IP address(es)
            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            // extract a returned address
            IPAddress hostAddress = hostEntry.AddressList[0];
            IPEndPoint remoteEndPoint = new IPEndPoint(hostAddress, 80);

            var connection = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            connection.Connect(remoteEndPoint);
            connection.SetSocketOption(SocketOptionLevel.Tcp,
                SocketOptionName.NoDelay, true);
            connection.SetSocketOption(SocketOptionLevel.Tcp
                , SocketOptionName.Linger, new byte[] { 0, 0, 0, 0 });
            connection.SendTimeout = timeout;
            return connection;
        }

        static void SendRequest(Socket s, Uri uri, string contentType, string content)
        {
            byte[] contentBuffer = Encoding.UTF8.GetBytes(content);
            const string CRLF = "\r\n";
            var requestLine =
                "POST " + uri.AbsolutePath + " HTTP/1.1" + CRLF;
            byte[] requestLineBuffer = Encoding.UTF8.GetBytes(requestLine);
            var headers =
                "Host: " + uri.Host + CRLF +
                "Content-Type: " + contentType + CRLF +
                "Content-Length: " + contentBuffer.Length + CRLF +
                CRLF;
            byte[] headersBuffer = Encoding.UTF8.GetBytes(headers);
            s.Send(requestLineBuffer);
            s.Send(headersBuffer);
            s.Send(contentBuffer);

            byte[] buffer = new byte[256];
            s.Receive(buffer);
            String result = new String(Encoding.UTF8.GetChars(buffer));
            Debug.Print(result);
            
        }
    }
}

