using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Runtime.Serialization;

namespace CentralServerService
{
    /// <summary>
    /// Exception of this class gets thrown when the Central Server fails to setup one of its devices for scanning.
    /// </summary>
    [Serializable]
    public class ScanningSetupException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ScanningSetupException()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The exception describing message.</param>
        public ScanningSetupException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Constructor
        /// <param name="ex">The nested exception.</param>
        public ScanningSetupException(string message, Exception ex)
            : base(message, ex)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message formatter.</param>
        /// <param name="parameters">The format elements.</param>
        public ScanningSetupException(string message, params object[] parameters)
            : base(String.Format(CultureInfo.InvariantCulture, message, parameters))
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message formatter.</param>
        /// <param name="ex">The nested exception.</param>
        /// <param name="parameters">The format elements.</param>
        public ScanningSetupException(string message, Exception ex, params object[] parameters)
            : base(String.Format(CultureInfo.InvariantCulture, message, parameters), ex)
        {
        }


        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="c"></param>
        protected ScanningSetupException(SerializationInfo info, StreamingContext c) :
            base(info, c)
        {
        }

        /// <summary>
        /// Returns the deepest contained exception that is not ScanningSetupException.
        /// </summary>
        public Exception DeepestException
        {
            get
            {
                if (InnerException is ScanningSetupException)
                {
                    return ((ScanningSetupException)InnerException).DeepestException;
                }
                else
                    if (InnerException == null)
                    {
                        return this;
                    }
                    else
                    {
                        return InnerException;
                    }
            }
        }
    }
}

