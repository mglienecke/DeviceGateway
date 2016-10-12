using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Common.Server
{
    /// <summary>
    /// The interface declares methods for parsing and encoding string content on the server side.
    /// </summary>
    public interface IContentParser
    {
        /// <summary>
        /// The method encodes the passed data object to a string.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="output">Text writer to write the encoded data to.</param>
        /// <returns></returns>
        void Encode(object dataObject, TextWriter output);

        /// <summary>
        /// The method encodes the passed data object to a string.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <returns></returns>
        string Encode(object dataObject);

        /// <summary>
        /// The method parses the passed string and populates the specified object with data.
        /// The data object must be of the type that corresponds the data passed in the content string.
        /// </summary>
        /// <param name="content">The parameter value may not be <c>null</c>.</param>
        /// <param name="resultType">The parameter value may not be <c>null</c>.</param>
        /// <returns></returns>
        object Decode(TextReader input, Type resultType);

        /// <summary>
        /// The method parses the passed string and populates the specified object with data.
        /// The data object must be of the type that corresponds the data passed in the content string.
        /// </summary>
        /// <param name="content">The parameter value may not be <c>null</c>.</param>
        /// <param name="resultType">The parameter value may not be <c>null</c>.</param>
        /// <returns></returns>
        object Decode(string input, Type resultType);
        
        /// <summary>
        /// The method returns the name of the MIME content type this parser implementation supports.
        /// </summary>
        /// <returns></returns>
        string GetSupportedMimeContentType();

        /// <summary>
        /// The property getter returns a flag that defines if the objects of the implementation class are reusable and thread-safe
        /// (or should be instantiated every time before use)
        /// </summary>
        bool IsReusableAndThreadSafe { get; }

    }
}
