using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.SPOT;

namespace Gsiot.PachubeClient
{
    public static class HttpClient
    {
        /// <summary>
        /// The method sends an HTTP PUT request.
        /// </summary>
        /// <param name="sample"></param>
        /// <exception cref="Exception"></exception>
        public static void SendPutRequest(string targetUrl, string content, string contentType, int timeout)
        {
            using (var request = CreatePutRequest(targetUrl, content, contentType))
            {
                request.Timeout = timeout;

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    HandleResponse(response);
                }
            }
        }

        private static HttpWebRequest CreatePutRequest(string targetUrl, string content, string contentType)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);

            var request = (HttpWebRequest)WebRequest.Create
                (targetUrl);

            // request line
            request.Method = "PUT";

            // request headers
            request.ContentLength = buffer.Length;
            request.ContentType = contentType;

            // request body
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(buffer, 0, buffer.Length);
            }

            return request;
        }

        public static void HandleResponse(HttpWebResponse response)
        {
            Debug.Print("Status code: " + response.StatusCode);
            Debug.Print("Status description: " + response.StatusDescription);
        }
    }
}
