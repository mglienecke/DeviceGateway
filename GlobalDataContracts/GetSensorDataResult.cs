using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace GlobalDataContracts
{
	[DataContract(Namespace = "http://GatewaysService")]
	public class GetSensorDataResult : OperationResult
	{
        /// <summary>
        /// Default constructor.
        /// </summary>
        public GetSensorDataResult()
            : base()
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="GetSensorDataResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		/// <param name="errorMessages">The error messages.</param>
		/// <param name="data">The data.</param>
		public GetSensorDataResult(bool success, List<string> errorMessages, MultipleSensorData data)
			: base(success, errorMessages)
		{
			Data = data;
		}

        [DataMember]
        public MultipleSensorData Data { get; set; }
	}
}
