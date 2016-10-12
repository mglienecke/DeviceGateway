using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValueManagement.DynamicCallback
{
	/// <summary>
	/// defines the types of call back which are defined
	/// </summary>
	public enum CallbackType
	{
		/// <summary>
		/// undefined
		/// </summary>
		Undefined = 0,

		/// <summary>
		/// anomaly detection
		/// </summary>
		DetectAnomaly = 1,

		/// <summary>
		/// sample rate adjustment
		/// </summary>
		AdjustSampleRate = 2,

		/// <summary>
		/// general check of the value
		/// </summary>
		GeneralCheck = 3,

		/// <summary>
		/// before storing has been done
		/// </summary>
		BeforeStore = 4,

		/// <summary>
		/// after storing has been done
		/// </summary>
		AfterStore = 5,

		/// <summary>
		/// the virtual value shall be calculated
		/// </summary>
		VirtualValueCalculation,

        /// <summary>
        /// A callback after a value change has happened
        /// </summary>
        AfterChangeCallback
	}
}
