using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalDataContracts;

namespace ValueManagement.DynamicCallback
{
	/// <summary>
	/// Any callback result has to be passed in this class
	/// </summary>
	[Serializable]
	public class CallbackResultData
	{
		/// <summary>
		/// Gets or sets a value indicating whether this instance is cancelled. Setting to <c>true</c> means that no further processing for the value shall take place for whatever reason
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is cancelled; otherwise, <c>false</c>.
		/// </value>
		public bool IsCancelled { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance's value is modified.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance's value is modified; otherwise, <c>false</c>.
		/// </value>
		public bool IsValueModified { get; set; }

		/// <summary>
		/// Gets or sets the new value.
		/// </summary>
		/// <value>The new value.</value>
		public SensorData NewValue { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [sample rate needs adjustment]. This is only relevant for the <see cref="CallbackType.SampleRateAdjustment"/> callback
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [sample rate needs adjustment]; otherwise, <c>false</c>.
		/// </value>
		public bool SampleRateNeedsAdjustment { get; set; }

		/// <summary>
		/// Gets or sets the new sample rate. This is only relevant for the <see cref="CallbackType.SampleRateAdjustment"/> callback
		/// </summary>
		/// <value>The new sample rate.</value>
		public TimeSpan NewSampleRate { get; set; }
	}
}
