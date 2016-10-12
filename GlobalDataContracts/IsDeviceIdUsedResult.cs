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
	public class IsDeviceIdUsedResult : OperationResult
	{
		[DataMember]
		public bool IsUsed { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="IsDeviceIdUsedResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		/// <param name="errorMessages">The error messages.</param>
		/// <param name="isUsed">if set to <c>true</c> [is used].</param>
		public IsDeviceIdUsedResult(bool success, List<string> errorMessages, bool isUsed)
			: base(success, errorMessages)
		{
			IsUsed = isUsed;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IsDeviceIdUsedResult"/> class.
		/// </summary>
		public IsDeviceIdUsedResult()
			: base(false, null)
		{
			IsUsed = false;
		}
	}
}
