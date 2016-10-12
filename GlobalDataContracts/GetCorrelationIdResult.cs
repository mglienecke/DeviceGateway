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
	public class GetCorrelationIdResult : OperationResult
	{
        /// <summary>
        /// Default constructor.
        /// </summary>
        public GetCorrelationIdResult()
            : base()
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="GetSensorDataResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		/// <param name="errorMessages">The error messages.</param>
		/// <param name="correlationId">The id.</param>
		public GetCorrelationIdResult(bool success, List<string> errorMessages, string correlationId)
			: base(success, errorMessages)
		{
		    CorrelationId = correlationId;
		}

        [DataMember]
        public string CorrelationId { get; set; }
	}
}
