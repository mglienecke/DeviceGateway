using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Newtonsoft.Json;
using fastJSON;
using System.IO;

namespace Common.Server
{
    public class JsonContentParser:IContentParser
    {
        /// <summary>
        /// Content type MIME name for this content parser implementation.
        /// </summary>
        public const string ContentType = "application/json";

        #region IContentParser members...

        public JsonContentParser()
        {
            fastJSON.JSON.Instance.SerializeNullValues = true;
            fastJSON.JSON.Instance.ShowReadOnlyProperties = true;
            fastJSON.JSON.Instance.UseUTCDateTime = false;
            fastJSON.JSON.Instance.IndentOutput = false;
            fastJSON.JSON.Instance.UsingGlobalTypes = false;
        }

        /// <summary>
        /// The method encodes the passed data object to a string.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="output">Text writer to write the encoded data to.</param>
        /// <returns></returns>
        public void Encode(object dataObject, TextWriter output)
        {
            output.Write(JSON.Instance.ToJSON(dataObject, false));
            //JsonSerializer serializer = new JsonSerializer();
            //serializer.Serialize(output, dataObject);
        }

        /// <summary>
        /// The method encodes the passed data object to a string.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <returns></returns>
        public string Encode(object dataObject)
        {
           return JSON.Instance.ToJSON(dataObject, false);
        }

        /// <summary>
        /// The method parses the passed string and populates the specified object with data.
        /// The data object must be of the type that corresponds the data passed in the content string.
        /// </summary>
        /// <param name="content">The parameter value may not be <c>null</c>.</param>
        /// <param name="resultType">The parameter value may not be <c>null</c>.</param>
        /// <returns></returns>
        public object Decode(TextReader input, Type resultType)
        {
            return JSON.Instance.ToObject(input.ReadToEnd(), resultType);
            //JsonSerializer serializer = new JsonSerializer();
            //serializer.Populate(input, dataObjectToPopulate);
            //return dataObjectToPopulate;
        }

        /// <summary>
        /// The method parses the passed string and populates the specified object with data.
        /// The data object must be of the type that corresponds the data passed in the content string.
        /// </summary>
        /// <param name="content">The parameter value may not be <c>null</c>.</param>
        /// <param name="resultType">The parameter value may not be <c>null</c>.</param>
        /// <returns></returns>
        public object Decode(string input, Type resultType)
        {
            return JSON.Instance.ToObject(input, resultType);
        }

        /// <summary>
        /// The method returns the name of the MIME type this parser implementation supports.
        /// </summary>
        /// <returns></returns>
        public string GetSupportedMimeContentType()
        {
            return ContentType;
        }

        /// <summary>
        /// The property getter returns a flag that defines if the objects of the implementation class are reusable and thread-safe
        /// (or should be instantiated every time before use)
        /// </summary>
        public bool IsReusableAndThreadSafe { get { return true; } }

        #endregion
    }
}
