using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net;
using System.IO;
using fastJSON;

namespace Common.Server
{
    /// <summary>
    /// The class implements various utility functionality.
    /// </summary>
    public class Utilities
    {
        private readonly static Type[] NoType = new Type[0];
        private readonly static Object[] NoObject = new Object[0];

        /// <summary>
        /// The method creates a new object of the type with the specified name using the type's default constructor.
        /// </summary>
        /// <param name="typeName">The parameter value may not be <c>null</c>./></param>
        /// <returns></returns>
        public static object CreateObjectByTypeName(string typeName)
        {
            try
            {
                Type type = Type.GetType(typeName);
                if (type == null)
                {
                    throw new ArgumentException(String.Format("Type is not found. Type name: {0}", typeName), "typeName");
                }

                return type.GetConstructor(NoType).Invoke(NoObject);
            }
            catch (Exception exc)
            {
                throw new Exception(String.Format("Failed creating type instance. Type name: {0}; Error: {1}", typeName, exc.Message), exc);
            }
        }

        /// <summary>
        /// The method creates a new object of the type with the specified name using the type's default constructor.
        /// </summary>
        /// <param name="typeName">The parameter value may not be <c>null</c>./></param>
        /// <returns></returns>
        public static T CreateObjectByTypeName<T>(string typeName)
        {
            return (T)CreateObjectByTypeName(typeName);
        }

        /// <summary>
        /// The method searches for an entry in the appSettings configuration section containing a type name and creates an object of this type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appSettingsKeyName"></param>
        /// <param name="defaultTypeName"></param>
        /// <returns></returns>
        public static T CreateObjectByAppSettingsKeyName<T>(string appSettingsKeyName)
        {
            return CreateObjectByAppSettingsKeyName<T>(appSettingsKeyName, null);
        }

        /// <summary>
        /// The method searches for an entry in the appSettings configuration section containing a type name and creates an object of this type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appSettingsKeyName"></param>
        /// <param name="defaultTypeName"></param>
        /// <returns></returns>
        public static T CreateObjectByAppSettingsKeyName<T>(string appSettingsKeyName, string defaultTypeName)
        {
            string typeName = ConfigurationManager.AppSettings[appSettingsKeyName];
            if (typeName == null)
            {
                if (defaultTypeName == null)
                {
                    throw new Exception(String.Format(Properties.Resources.ExceptionAppSettingsKeyNotFound, appSettingsKeyName));
                }
                else
                {
                    typeName = defaultTypeName;
                }
            }

            return Utilities.CreateObjectByTypeName<T>(typeName);
        }

        /// <summary>
        /// The method sends an HTTP request to the specified URI. 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"><c>GET</c>, <c>PUT</c>, <c>POST</c>, etc,</param>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        /// <param name="resultObjectType"></param>
        /// <param name="result"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public bool SendHttpRequest(string uri, string method, string content, string contentType, Type resultObjectType, out object result, params object[] parameters)
        {
            try
            {
                String uriString = String.Format("http://" + uri, parameters);

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uriString);
                request.Method = method;
                request.Accept = contentType;
                request.KeepAlive = false;
                request.ContentType = contentType;

                //Put the content
                if (content != null && content.Length > 0)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(content);

                    // request body if needed
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Close();
                    }
                }

                //Get the response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonContentParser serializer = new JsonContentParser();
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = serializer.Decode(reader, resultObjectType);
                    }

                    return true;
                }
                else
                {
                    result = String.Format(Properties.Resources.ExceptionHttpRestPutRequestFailedDetails,
                        response.StatusCode, response.StatusDescription);
                    return false;
                }

            }
            catch (Exception exc)
            {
                result = String.Format(Properties.Resources.ExceptionHttpRestPutRequestFailed, exc.Message);
                return false;
            }
        }
    }
}
