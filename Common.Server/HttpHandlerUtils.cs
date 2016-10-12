using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Common.Server;
using GlobalDataContracts;
using System.Net;
using System.IO;
using System.Collections.Specialized;

namespace Common.Server
{
    /// <summary>
    /// The class contains various methods and constants for the Http Handlers.
    /// </summary>
    public sealed class HttpHandlerUtils
    {
        public const string CfgContentParserTypeNamePrefix = "ContentParserTypeName.";

        public const char PathDelimiter = '/';

        public const string HttpMethodGet = "GET";
        public const string HttpMethodPut = "PUT";
        public const string HttpMethodPost = "POST";

        public const string ContentTypeJson = JsonContentParser.ContentType;
        public const string ContentTypeXml =  "text/xml";
        public const string ContentTypeTextPlain = "text/plain";

        public const string QueryStringParamGeneratedBefore = "generatedBefore";
        public const string QueryStringParamGeneratedAfter = "generatedAfter";
        public const string QueryStringParamMaxValuesPerSensor = "maxValuesPerSensor";
        public const string QueryStringParamIdPrefix = "id";

        private static IContentParser mJsonContentParser = new JsonContentParser();
        private static readonly Dictionary<string, IContentParser> mParsers = new Dictionary<string, IContentParser>();

        private static readonly string[] SupportedContentTypes = new string[] { ContentTypeJson };

        private HttpHandlerUtils()
        {
        }

        /// <summary>
        /// Gets the error description.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        public static List<string> GetErrorDescription(string method, Exception x)
        {
            List<string> result = new List<string>();
            result.Add(String.Format(Properties.Resources.ExceptionOccured, method, x.Message, string.Empty));
            return (result);
        }

        public static void WriteResponseOperationFailed(HttpContext context, HttpStatusCode status, string errorMessage)
        {
            context.Response.ContentType = GetResponseContentType(context.Request.AcceptTypes);
            context.Response.StatusCode = (Int32)status;

            OperationResult result = new OperationResult() { Success = false };
            result.AddError(errorMessage);

            WriteResponse(context, result);
        }

        public static void WriteResponse(HttpContext context, object result)
        {
            string[] acceptedContentTypes = context.Request.AcceptTypes;
            IContentParser parser = FindParser(acceptedContentTypes);
            if (parser == null)
            {
                WriteResponseUnsupportedContentTypes(context, acceptedContentTypes);
            }
            else
            {
                parser.Encode(result, context.Response.Output);
                context.Response.ContentType = parser.GetSupportedMimeContentType();
                context.Response.StatusCode = (Int32)HttpStatusCode.OK;
            }
        }

        public static void WriteResponseUnsupportedContentTypes(HttpContext context, string[] acceptedContentTypes)
        {
            StringBuilder builder = new StringBuilder();
            if (acceptedContentTypes.Length > 0)
            {
                builder.Append(acceptedContentTypes[0]);
            }

            if (acceptedContentTypes.Length > 1)
            {
                for (int i = 1; i < acceptedContentTypes.Length; i++)
                {
                    builder.Append(", ");
                    builder.Append(acceptedContentTypes[i]);
                }
            }

            context.Response.Output.Write(Properties.Resources.ErrorRequestedContentTypesNotSupported, builder.ToString());
            context.Response.StatusCode = (Int32)HttpStatusCode.NotAcceptable;
        }

        public static object DecodeDataObject(HttpContext context, StreamReader input, Type dataObjectType)
        {
            string[] acceptedContentTypes = context.Request.AcceptTypes;
            IContentParser parser = FindParser(acceptedContentTypes);
            if (parser == null)
            {
                WriteResponseUnsupportedContentTypes(context, acceptedContentTypes);
            }

            return parser.Decode(input, dataObjectType);
        }

        /// <summary>
        /// Finds content parser appropriate for the specified content types. Returns a JSON parser if no content types are specified.
        /// </summary>
        /// <param name="acceptedContentTypes"></param>
        /// <returns></returns>
        public static IContentParser FindParser(string[] acceptedContentTypes)
        {
            if (acceptedContentTypes == null || acceptedContentTypes.Length == 0){
                //Let it be the default
                return mJsonContentParser;
            }

            foreach (string contentType in acceptedContentTypes)
            {
                foreach (string supportedContentType in SupportedContentTypes)
                {
                    if (contentType.Contains(supportedContentType))
                    {
                        return GetParser(supportedContentType);
                    }
                }
            }

            return null;
        }

        public static string GetResponseContentType(string[] acceptedTypes){
            if (acceptedTypes == null || acceptedTypes.Length == 0)
            {
                //Let it be the default
                return ContentTypeJson;
            }

            foreach (string contentType in acceptedTypes)
            {
                foreach (string supportedContentType in SupportedContentTypes)
                {
                    if (contentType.Contains(supportedContentType))
                    {
                        return supportedContentType;
                    }
                }
            }

            //Let it be the default
            return ContentTypeJson;
        }

        public static IContentParser GetParser(string contentType)
        {
            IContentParser parser;
            if (mParsers.TryGetValue(contentType, out parser) == false)
            {
                parser = Utilities.CreateObjectByAppSettingsKeyName<IContentParser>(CfgContentParserTypeNamePrefix + contentType);
                mParsers[contentType] = parser;
            }

            return parser;
        }

        /// <summary>
        /// The query string is supposed to contains keys named "id*" where * ranges from 0 up.
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static string[] GetIds(NameValueCollection queryString)
        {
            List<string> result = new List<string>();
            foreach (string key in queryString.AllKeys){
                if (key.StartsWith(QueryStringParamIdPrefix))
                    result.Add(queryString[key]);
            }
            return result.ToArray();
        }
    }
}
