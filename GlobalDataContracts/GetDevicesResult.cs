using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace GlobalDataContracts
{
    [Serializable]
	[DataContract(Namespace="http://GatewaysService")]
	public class GetDevicesResult : OperationResult
	{
		[DataMember]
		public List<Device> Devices { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GetDevicesResult()
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="GetDevicesResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		/// <param name="errorMessages">The error messages.</param>
		/// <param name="devices">The devices.</param>
		public GetDevicesResult(bool success, List<string> errorMessages, List<Device> devices)
			: base(success, errorMessages)
		{
			Devices = devices;
		}
	}
}
