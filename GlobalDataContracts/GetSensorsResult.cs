using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace GlobalDataContracts
{
	[DataContract(Namespace = "http://GatewaysService")]
	public class GetSensorsResult : OperationResult
	{
		[DataMember]
		public List<Sensor> SensorList { get; set; }
	}
}
