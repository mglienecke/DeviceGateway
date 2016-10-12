using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Common.Server
{
    /// <summary>
    /// This is a factory class for <see cref="IContentParser"/> implementations.
    /// </summary>
    public class ContentParserFactory
    {
        /// <summary>
        /// appSettings configuration property name prefix.
        /// </summary>
        public const string CfgPropertyNamePrefixContentParserType = "ContentParserTypeName.";

        private static readonly Dictionary<string, string> mParserTypes = new Dictionary<string, string>();
        private static readonly Dictionary<string, IContentParser> mParsers = new Dictionary<string, IContentParser>();

        static ContentParserFactory()
        {
            foreach (string key in ConfigurationManager.AppSettings.AllKeys)
            {
                if (key.StartsWith(CfgPropertyNamePrefixContentParserType))
                {
                    mParserTypes[key.Substring(CfgPropertyNamePrefixContentParserType.Length)] = ConfigurationManager.AppSettings[key];
                }
            }
        }

        /// <summary>
        /// The method returns a content parser for the specified content type. 
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static IContentParser GetParser(string contentType)
        {
            IContentParser parser;

            if (mParsers.TryGetValue(contentType, out parser) == false)
            {
                string parserType;
                if (mParserTypes.TryGetValue(contentType, out parserType))
                {
                    try
                    {
                        parser = Utilities.CreateObjectByTypeName<IContentParser>(parserType);
                    }
                    catch (Exception exc)
                    {
                        throw new Exception(String.Format(Properties.Resources.ExceptionFailedCreatingContentParser , contentType, exc.Message), exc);
                    }

                    //Keep if reusable
                    if (parser.IsReusableAndThreadSafe)
                    {
                        mParsers[contentType] = parser;
                    }
                }
                else
                {
                    throw new Exception(String.Format(Properties.Resources.ExceptionNoContentParserImplementationFound, contentType));
                }
            }

            return parser;
        }

        /// <summary>
        /// The method checks if the passed content type contains one of the supported content types and returns the name of the supported one.
        /// </summary>
        /// <param name="headerAcceptContentType"></param>
        /// <returns></returns>
        public static string ExtractContentType(string headerAcceptContentType)
        {
            foreach (var next in mParserTypes){
                if (headerAcceptContentType.IndexOf(next.Key) == 0)
                {
                    return next.Key;
                }
            }

            throw new Exception(String.Format(Properties.Resources.ExceptionContentTypeNotSupported, headerAcceptContentType));
        }
    }
}
