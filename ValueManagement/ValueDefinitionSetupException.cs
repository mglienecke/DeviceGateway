using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Runtime.Serialization;

namespace ValueManagement
{
    /// <summary>
    /// Exception of this class gets thrown when there is a setup failure for one of the value definitions.
    /// </summary>
    [Serializable]
    public class ValueDefinitionSetupException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ValueDefinitionSetupException()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The exception describing message.</param>
        public ValueDefinitionSetupException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Constructor
        /// <param name="ex">The nested exception.</param>
        public ValueDefinitionSetupException(string message, Exception ex)
            : base(message, ex)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message formatter.</param>
        /// <param name="parameters">The format elements.</param>
        public ValueDefinitionSetupException(string message, params object[] parameters)
            : base(String.Format(CultureInfo.InvariantCulture, message, parameters))
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message formatter.</param>
        /// <param name="ex">The nested exception.</param>
        /// <param name="parameters">The format elements.</param>
        public ValueDefinitionSetupException(string message, Exception ex, params object[] parameters)
            : base(String.Format(CultureInfo.InvariantCulture, message, parameters), ex)
        {
        }


        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="c"></param>
        protected ValueDefinitionSetupException(SerializationInfo info, StreamingContext c) :
            base(info, c)
        {
        }

        /// <summary>
        /// Returns the deepest contained exception that is not ValueDefinitionSetupException.
        /// </summary>
        public Exception DeepestException
        {
            get
            {
                if (InnerException is ValueDefinitionSetupException)
                {
                    return ((ValueDefinitionSetupException)InnerException).DeepestException;
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

