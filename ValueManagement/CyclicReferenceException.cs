using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ValueManagement
{
	/// <summary>
	/// Used if a reference would cause a cycle
	/// </summary>
    [Serializable]
	public class CyclicReferenceException : ApplicationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CyclicReferenceException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public CyclicReferenceException(string message)
			: base(message)
		{
			;
		}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="c"></param>
        public CyclicReferenceException(SerializationInfo info, StreamingContext c) :
            base(info, c)
        {
        }
	}
}
