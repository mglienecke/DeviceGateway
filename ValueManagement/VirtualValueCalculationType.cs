using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValueManagement
{
	/// <summary>
	/// defines how a virtual value is calculated
	/// </summary>
	public enum VirtualValueCalculationType
	{
		/// <summary>
		/// not defined
		/// </summary>
		Undefined,

		/// <summary>
		/// cycle and calculate the value
		/// </summary>
		Cyclic,

		/// <summary>
		/// when the underlying reference value changes
		/// </summary>
		OnChange,

		/// <summary>
		/// when the value is requested
		/// </summary>
		OnRequest
	}
}
