using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace GlobalDataContracts
{
	[DataContract(Namespace = "http://GatewaysService")]
	public class IsSensorIdRegisteredForDeviceResult : OperationResult
	{
		[DataMember]
		public bool IsRegistered { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="IsSensorIdRegisteredForDeviceResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		/// <param name="errorMessages">The error messages.</param>
		/// <param name="isRegistered">if set to <c>true</c> [is registered].</param>
		public IsSensorIdRegisteredForDeviceResult(bool success, List<string> errorMessages, bool isRegistered)
			: base(success, errorMessages)
		{
			IsRegistered = isRegistered;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IsSensorIdRegisteredForDeviceResult"/> class.
		/// </summary>
		public IsSensorIdRegisteredForDeviceResult()
			: base(true, null)
		{
			IsRegistered = false;
		}
	}
}
