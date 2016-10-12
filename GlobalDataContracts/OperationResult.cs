using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace GlobalDataContracts
{
    [Serializable]
    [DataContract(Namespace = "http://GatewaysService")]
	public class OperationResult
	{
        public static readonly long TimeZeroTicks = new DateTime(1700, 1, 1).Ticks;

        /// <summary>
        /// The property contains the local time of the server.
        /// </summary>
        [DataMember]
        public long Timestamp { get; set; }

		[DataMember]
		public bool Success { get; set; }

		[DataMember]
		public string ErrorMessages { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="OperationResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		/// <param name="errorMessage">The error message.</param>
		public OperationResult(bool success, List<string> errorMessages)
            :this()
		{
			Success = success;
			if (errorMessages != null)
			{
				ErrorMessages = String.Empty;

				foreach (string message in errorMessages)
				{
                    AddError(message);
				}
			}
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationResult"/> class.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="errorMessage">The error message.</param>
        public OperationResult(string errorMessage)
            : this()
        {
            if (errorMessage != null)
            {
                AddError(errorMessage);
            }
        }


		/// <summary>
		/// Initializes a new instance of the <see cref="OperationResult"/> class.
		/// </summary>
		public OperationResult()
		{
			Success = true;
			ErrorMessages = null;
            Timestamp = DateTime.Now.Ticks - TimeZeroTicks;
		}

        /// <summary>
        /// Adds the error message to the list and sets the status to false
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>the original message</returns>
        public string AddError(string message)
        {
            Success = false;

            if (ErrorMessages != null)
            {
                ErrorMessages += "\n";
            }
            else
            {
                ErrorMessages = String.Empty;
            }
            ErrorMessages += message;

            return (message);
        }

        /// <summary>
        /// Adds an error with formatting option
        /// </summary>
        /// <param name="formatString">The format string.</param>
        /// <param name="data">The data.</param>
        /// <returns>the formatted string</returns>
        public string AddErrorFormat(string formatString, params object[] data)
        {
            return (AddError(string.Format(formatString, data)));
        }
	}

	[DataContract(Namespace = "http://GatewaysService")]
	public class TestResult
	{
		[DataMember]
		public string ErrorMessage { get; set; }

		[DataMember]
		public bool IsUsed { get; set; }

		[DataMember]
		public string Data { get; set; }
		
		[DataMember]
		public Sensor SensorValues { get; set; }

		public TestResult()
		{
			
		}
	}
}
