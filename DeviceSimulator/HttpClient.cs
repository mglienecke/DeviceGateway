using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;

namespace DeviceServer.Simulator
{
    /// <summary>
    /// The class handles HTTP client communication.
    /// </summary>
    public static class HttpClient
    {
        /// <summary>
        /// The method sends an HTTP PUT request.
        /// </summary>
        /// <param name="sample"></param>
        /// <exception cref="Exception"></exception>
        public static bool SendPutRequest(string targetUrl, string content, string contentType, int timeout, out HttpStatusCode responseCode, out string responseContent)
        {
            var request = CreatePutRequest(targetUrl, content, contentType);
            
                request.Timeout = timeout;

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    return HandleResponse(response, out responseCode, out responseContent);
                }
            
        }

        /// <summary>
        /// The method sends an HTTP POST request.
        /// </summary>
        /// <param name="sample"></param>
        /// <exception cref="Exception"></exception>
        public static bool SendPostRequest(string targetUrl, string content, string contentType, int timeout, out HttpStatusCode responseCode, out string responseContent)
        {
            var request = CreatePostRequest(targetUrl, content, contentType);
                request.Timeout = timeout;

                HttpWebResponse response;
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    return HandleResponse(response, out responseCode, out responseContent);
                }
        }

        /// <summary>
        /// The method sends an HTTP PUT request.
        /// </summary>
        /// <param name="sample"></param>
        /// <exception cref="Exception"></exception>
        public static bool SendGetRequest(string targetUrl, string contentType, int timeout, out HttpStatusCode responseCode, out string responseContent)
        {
            var request = CreateGetRequest(targetUrl, contentType);
            
                request.Timeout = timeout;

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    return HandleResponse(response, out responseCode, out responseContent);
                }
        }

        private static HttpWebRequest CreatePutRequest(string targetUrl, string content, string contentType)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);

            var request = (HttpWebRequest)WebRequest.Create(targetUrl);

            // request line
            request.Method = "PUT";

            // request headers
            request.ContentLength = buffer.Length;
            request.ContentType = contentType;
            request.KeepAlive = false;

            // request body
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(buffer, 0, buffer.Length);
                stream.Close();
            }

            return request;
        }

        private static HttpWebRequest CreatePostRequest(string targetUrl, string content, string contentType)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);

            var request = (HttpWebRequest)HttpWebRequest.Create(targetUrl);

              // request line
            request.Method = "POST";

            // request headers
            request.ContentLength = buffer.Length;
            request.ContentType = contentType;
            request.KeepAlive = false;

            // request body
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(buffer, 0, buffer.Length);
                stream.Close();
            }

            return request;
        }

        private static HttpWebRequest CreateGetRequest(string targetUrl, string contentType)
        {
            var request = (HttpWebRequest)WebRequest.Create(targetUrl);

            // request line
            request.Method = "GET";

            request.ContentType = contentType;
            request.KeepAlive = false;

            return request;
        }

        public static bool HandleResponse(HttpWebResponse response, out HttpStatusCode responseCode, out string responseContent)
        {
            //Get the response
            responseCode = response.StatusCode;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                responseContent = reader.ReadToEnd();
            }

            //Debug output
            Console.WriteLine(response.Method + ": " + response.ResponseUri.OriginalString);
            Console.WriteLine("Status code: " + response.StatusCode);
            Console.WriteLine("Status description: " + response.StatusDescription);
            Console.WriteLine("Content: " + responseContent);

            return responseCode == HttpStatusCode.OK;
        }
    }
}
