using System;
using System.Text;

#if DEVICE_IMPLEMENTATION
using Ws.ServiceModel;
#else
using System.ServiceModel;
using System.Runtime.Serialization;
#endif

namespace GlobalDataContracts
{
    [Serializable]
	[DataContract(Namespace = "http://GatewaysService")]
	public class MultipleSensorData
	{
		[DataMember]
		public string SensorId { get; set; }

        public int InternalSensorId { get; set; }

		[DataMember]
		public SensorData [] Measures { get; set; }
	}
}
