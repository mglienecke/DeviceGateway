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
	public class GetSensorResult : OperationResult
	{
		[DataMember]
		public Sensor Sensor { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GetSensorResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		/// <param name="errorMessages">The error messages.</param>
		/// <param name="sensor">The sensor.</param>
		public GetSensorResult(bool success, List<string> errorMessages, Sensor sensor)
			: base(success, errorMessages)
		{
			Sensor = sensor;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GetSensorResult"/> class.
		/// </summary>
		public GetSensorResult()
			: base(false, null)
		{

		}
	}
}
