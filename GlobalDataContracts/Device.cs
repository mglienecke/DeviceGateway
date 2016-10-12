using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Net;

namespace GlobalDataContracts
{
    [Serializable]
	[DataContract(Namespace = "http://GatewaysService")]
	public class Location
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public double Latitude { get; set; }

		[DataMember]
		public double Longitude { get; set; }

		[DataMember]
		public double Elevation { get; set; }
	}

    [Serializable]
	[DataContract(Namespace = "http://GatewaysService")]
	public class Device
	{
		[DataMember]
		public string Id { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public Location Location { get; set; }

		[DataMember]
		public string DeviceIpEndPoint { get; set; }
	}
}
