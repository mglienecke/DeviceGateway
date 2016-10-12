//Copyright 2011 Oberon microsystems, Inc.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

//Developed for the book
//  "Getting Started with the Internet of Things", by Cuno Pfister.
//  Copyright 2011 Cuno Pfister, Inc., 978-1-4493-9357-1.
//
//Version 0.9 (beta release)

// These types provide support for serializing and deserializing objects
// of the types used in GSIoT, i.e., for the conversion between C#
// objects and HTTP message bodies.

namespace Gsiot.Server.Simulator
{
    /// <summary>
    /// Converts an object to an HTTP response message body.
    /// content may be null.
    /// Preconditions
    ///     context != null
    /// 
    /// Postconditions
    ///     "context.ResponseContent contains serialized representation of
    ///      content object"
    ///     "context.ResponseContentType contains the MIME type of the
    ///      representation"
    /// </summary>
    public delegate void Serializer(RequestHandlerContext context,
        object content);

    /// <summary>
    /// Converts an HTTP request message body to an object.
    /// content may be null.
    /// The return value indicates whether conversion was successful.
    /// Preconditions
    ///     context != null
    /// Postconditions
    ///     Result =>
    ///         content != null
    ///         "content contains deserialized representation of
    ///          context.RequestContent"
    ///     !Result =>
    ///         content == null
    ///         "context.RequestContent could not be converted to a
    ///          C# object"
    /// </summary>
    public delegate bool Deserializer(RequestHandlerContext context,
        out object content);

    /// <summary>
    /// Class that supports serialization of all C# objects, and
    /// deserialization of bool and int values.
    /// </summary>
    public static class CSharpRepresentation
    {
        /// <summary>
        /// Converts an object to an HTTP response message body.
        /// content may be null.
        /// If the content is null, the string "null" is produced as
        /// its representation. Otherwise, the object's ToString
        /// method is called for producing the representation.
        /// Preconditions
        ///     context != null
        /// Postconditions
        ///     "context.RequestContent contains serialized representation of
        ///      content object"
        ///     context.ResponseContentType == "text/plain"
        /// </summary>
        public static void Serialize(RequestHandlerContext context,
            object content)
        {
            string s = (content != null) ? content.ToString() : "null";
            context.SetResponse(s, "text/plain");
        }

        /// <summary>
        /// Converts an HTTP request message body to a bool object.
        /// content is not null if conversion was successful.
        /// The return value indicates whether conversion was successful.
        /// Preconditions
        ///     context != null
        /// Postconditions
        ///     Result =>
        ///         content != null
        ///         "content contains deserialized representation of
        ///          context.RequestContent"
        ///     !Result =>
        ///         content == null
        ///         "context.RequestContent could not be converted to a
        ///          C# object"
        /// </summary>
        public static bool TryDeserializeBool(
            RequestHandlerContext context, out object content)
        {
            string s = context.RequestContent;
            if (s == "true") { content = true; return true; }
            if (s == "false") { content = false; return true; }
            content = null;
            return false;
        }

        /// <summary>
        /// Converts an HTTP request message body to an int object.
        /// content is not null if conversion was successful.
        /// The return value indicates whether conversion was successful.
        /// Preconditions
        ///     context != null
        /// Postconditions
        ///     Result =>
        ///         content != null
        ///         "content contains deserialized representation of
        ///          context.RequestContent"
        ///     !Result =>
        ///         content == null
        ///         "context.RequestContent could not be converted to a
        ///          C# object"
        /// </summary>
        public static bool TryDeserializeInt(RequestHandlerContext context,
            out object content)
        {
            string s = context.RequestContent;
            int i;
            if (Utilities.TryParseUInt32(s, out i))
            {
                content = i;
                return true;
            }
            content = null;
            return false;
        }
    }
}
