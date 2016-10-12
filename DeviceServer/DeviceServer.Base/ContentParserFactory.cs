using System;
using Microsoft.SPOT;
using System.Collections;
using NetMf.CommonExtensions;
using Gsiot.Server;

namespace DeviceServer.Base
{
    /// <summary>
    /// The class implements factory for <see cref="IContentParser"/> implementations.
    /// </summary>
    internal sealed class ContentParserFactory
    {
        private static readonly Hashtable mParsers = new Hashtable();
        private static readonly string[][] mParserClassNames = new string[][] { 
            new string[] {CommunicationConstants.ContentTypeApplicationJson, typeof(JsonParser).AssemblyQualifiedName},
            new string[] {CommunicationConstants.ContentTypeApplicationJson, typeof(JsonParser).AssemblyQualifiedName}};

        private ContentParserFactory()
        {
        }

        /// <summary>
        /// The method returns a content parser instance for the passed content type.
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static IContentParser GetParser(string contentType)
        {
            string acceptedContentType = ExtractContentType(contentType);
            IContentParser parser = null;

            //Lazy initialization
            if (mParsers.Contains(acceptedContentType))
            {
                parser = (IContentParser)mParsers[acceptedContentType];
            }
            else
            {
                for (int i = 0; i < mParserClassNames.Length; i++)
                {
                    if (acceptedContentType == mParserClassNames[i][0])
                    {
                        parser = (IContentParser)Utils.CreateObject(mParserClassNames[i][1]);
                        break;
                    }
                }

                //Found it?
                if (parser == null)
                {
                    //Special case
                    if (contentType.IndexOf(CommunicationConstants.ContentTypeTextHtml) > -1)
                    {
                        parser = GetParser(CommunicationConstants.ContentTypeApplicationJson);
                    }
                    else
                    {
                        throw new Exception(StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ExceptionContentTypeNotSupported), contentType));
                    }
                }
            }

            return parser;
        }

        /// <summary>
        /// The method returns if the factory has a parser for the passed accepted content type.
        /// </summary>
        /// <param name="acceptedContentType">Accepted content type from the request content.</param>
        /// <param name="responseContentType">Factory-recognizable content type corresponding to the passed accepted content type.</param>
        /// <returns><value>True</value> if the passed accepted content type is supported.</returns>
        public static bool IsResponseContentTypeSupported(string acceptedContentType, out string responseContentType)
        {
            for (int i = 0; i < mParserClassNames.Length; i++)
            {
                if (acceptedContentType.IndexOf(mParserClassNames[i][0]) > -1)
                {
                    responseContentType = mParserClassNames[i][0];
                    return true;
                }
            }

            //Not supported
            responseContentType = null;
            return false;
        }

        private static string ExtractContentType(string headerAcceptContentType)
        {
            for (int i = 0; i < mParserClassNames.Length; i++)
            {
                if (headerAcceptContentType.IndexOf(mParserClassNames[i][0]) >= 0)
                {
                    return mParserClassNames[i][0];
                }
            }

            throw new Exception(StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ExceptionContentTypeNotSupported), headerAcceptContentType));
        }
    }
}
