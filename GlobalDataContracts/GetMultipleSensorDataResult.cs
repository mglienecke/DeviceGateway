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
	public class GetMultipleSensorDataResult : OperationResult
	{
		[DataMember]
		public List<MultipleSensorData> SensorDataList { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GetMultipleSensorDataResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		/// <param name="errorMessages">The error messages.</param>
		/// <param name="data">The data.</param>
		public GetMultipleSensorDataResult(bool success, List<string> errorMessages, List<MultipleSensorData> data)
			: base(success, errorMessages)
		{
			SensorDataList = data;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GetMultipleSensorDataResult"/> class.
		/// </summary>
		public GetMultipleSensorDataResult()
			: base(false, null)
		{
			SensorDataList = null;
		}
	}
}
