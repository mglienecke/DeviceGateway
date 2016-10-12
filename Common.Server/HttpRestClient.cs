using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Web;

namespace Common.Server{
    /// <summary>
    /// The class handles HTTP client communication.
    /// </summary>
    public static class HttpClient
    {
        private static readonly byte[] NoBytes = new byte[0];

        #region HTTP communication methods...
        /// <summary>
        /// The method sends an HTTP PUT request.
        /// </summary>
        /// <param name="sample"></param>
        /// <exception cref="Exception"></exception>
        public static bool SendPutRequest(string targetUrl, string content, string contentType, int timeout, out HttpStatusCode responseCode, out string responseContentType, out string responseContent)
        {
            var request = CreatePutRequest(targetUrl, contentType, content);
            request.Timeout = timeout;

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                return HandleResponse(response, out responseCode, out responseContentType, out responseContent);
            }
        }

        /// <summary>
        /// The method sends an HTTP POST request.
        /// </summary>
        /// <param name="sample"></param>
        /// <exception cref="Exception"></exception>
        public static bool SendPostRequest(string targetUrl, string content, string contentType, int timeout, out HttpStatusCode responseCode, out string responseContentType, out string responseContent)
        {
            var request = CreatePostRequest(targetUrl, contentType, content);
            request.Timeout = timeout;

            HttpWebResponse response;
            using (response = (HttpWebResponse)request.GetResponse())
            {
                return HandleResponse(response, out responseCode, out responseContentType, out responseContent);
            }
        }

        /// <summary>
        /// The method sends an HTTP PUT request.
        /// </summary>
        /// <param name="sample"></param>
        /// <exception cref="Exception"></exception>
        public static bool SendGetRequest(string targetUrl, string contentType, int timeout, out HttpStatusCode responseCode, out string responseContentType, out string responseContent)
        {
            var request = CreateGetRequest(targetUrl, contentType);
            request.Timeout = timeout;

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                return HandleResponse(response, out responseCode, out responseContentType, out responseContent);
            }
        }
        #endregion

        public static Dictionary<string, object> ParseResponse(string responseContentType, string responseContent)
        {
            return (Dictionary<string, object>)ContentParserFactory.GetParser(responseContentType).Decode(new StringReader(responseContent), typeof(Dictionary<string, object>));
        }

        /// <summary>
        /// The method sends an HTTP PUT request.
        /// </summary>
        /// <param name="sample"></param>
        /// <exception cref="Exception"></exception>
        public static bool SendPutRequest(string targetUrl, string content, string contentType, int timeout, Type responseObjectType,  out object responseObject)
        {
            var request = CreatePutRequest(targetUrl, contentType, content);
            request.Timeout = timeout;

            responseObject = null;
            bool result = false;
 
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                HttpStatusCode responseCode = HttpStatusCode.NoContent;
                string responseContentType = null;
                string responseContent = null;

                if (HandleResponse(response, out responseCode, out responseContentType, out responseContent))
                {
                    if (responseContent.Length != 0)
                    {
                        // only here the result is fine
                        responseObject = ContentParserFactory.GetParser(ContentParserFactory.ExtractContentType(responseContentType))
                                                .Decode(responseContent, responseObjectType);
                        result = true;
                    }
                }
            }

            return (result);
        }

        /// <summary>
        /// The method sends an HTTP POST request.
        /// </summary>
        /// <param name="sample"></param>
        /// <exception cref="Exception"></exception>
        public static bool SendPostRequest(string targetUrl, string content, string contentType, int timeout, Type responseObjectType, out object responseObject)
        {
            var request = CreatePostRequest(targetUrl, contentType, content);
            request.Timeout = timeout;

            HttpWebResponse response;
            using (response = (HttpWebResponse)request.GetResponse())
            {
                HttpStatusCode responseCode = HttpStatusCode.NoContent;
                string responseContentType = null;
                string responseContent = null;

                if (HandleResponse(response, out responseCode, out responseContentType, out responseContent))
                {
                    responseObject = ContentParserFactory.GetParser(ContentParserFactory.ExtractContentType(responseContentType)).Decode(responseContent, responseObjectType);
                    return true;
                }
                else
                {
                    responseObject = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// The method sends an HTTP GET request.
        /// </summary>
        /// <param name="sample"></param>
        /// <exception cref="Exception"></exception>
        public static bool SendGetRequest(string targetUrl, string contentType, int timeout, Type responseObjectType, out object responseObject)
        {
            string responseContentType, responseContent;
            HttpStatusCode responseCode;
            if (SendGetRequest(targetUrl, contentType, timeout, out responseCode, out responseContentType, out responseContent))
            {
                responseObject = ContentParserFactory.GetParser(ContentParserFactory.ExtractContentType(responseContentType)).Decode(responseContent, responseObjectType);
                return true;
            }
            else
            {
                responseObject = null;
                return false;
            }
        }

        #region Private methods...
        private static HttpWebRequest CreatePutRequest(string targetUrl, string contentType, string content)
        {
            byte[] buffer = content != null ? Encoding.UTF8.GetBytes(content) : NoBytes;

            int index = targetUrl.LastIndexOf("/");
            string queryString = targetUrl.Substring(index + 1, targetUrl.Length - (index + 1));
            targetUrl = String.Format("{0}/{1}", targetUrl.Substring(0, index), HttpUtility.UrlEncode(queryString));
            var request = (HttpWebRequest)WebRequest.Create(targetUrl);
            request.Method = WebRequestMethods.Http.Put;

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

        private static HttpWebRequest CreatePostRequest(string targetUrl, string contentType, string content)
        {
            byte[] buffer = content != null ? Encoding.UTF8.GetBytes(content) : NoBytes;

            var request = (HttpWebRequest)HttpWebRequest.Create(targetUrl);

              // request line
            request.Method = WebRequestMethods.Http.Post;

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
            request.Method = WebRequestMethods.Http.Get;

            request.ContentType = contentType;
            request.KeepAlive = false;

            return request;
        }

        public static bool HandleResponse(HttpWebResponse response, out HttpStatusCode responseCode, out string responseContentType, out string responseContent)
        {
            //Get the response
            responseCode = response.StatusCode;
            responseContentType = response.ContentType;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                responseContent = reader.ReadToEnd();
            }

            return responseCode == HttpStatusCode.OK;
        }
        #endregion
    }
}
