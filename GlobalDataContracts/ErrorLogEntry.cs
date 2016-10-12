using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalDataContracts
{
    /// <summary>
    /// The class encapsulates data of a single device log entry.
    /// </summary>
    public class ErrorLogEntry
    {
        /// <summary>
        /// The property contains the timestamp of the moment the entry has been logged.
        /// </summary>
        public DateTime Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// The property contains the logged error message.
        /// </summary>
        public string Message
        {
            get;
            set;
        }
    }
}
