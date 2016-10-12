using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalDataContracts;

namespace ValueManagement
{
	/// <summary>
	/// The event in connection with value changes
	/// </summary>
	public class ValueEventArgs : EventArgs
	{
		/// <summary>
		/// Gets or sets the definition.
		/// </summary>
		/// <value>The definition.</value>
		public ValueDefinition Definition { get; set; }

		/// <summary>
		/// Gets or sets the new value.
		/// </summary>
		/// <value>The new value.</value>
		public SensorData NewValue { get; set; }

		/// <summary>
		/// Gets or sets the historic values.
		/// </summary>
		/// <value>The historic values.</value>
		public List<SensorData> HistoricValues { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueChangingEventArgs"/> class.
		/// </summary>
		/// <param name="definition">The definition.</param>
		/// <param name="newValue">The new value.</param>
		/// <param name="historicValues">The historic values.</param>
		public ValueEventArgs(ValueDefinition definition, SensorData newValue, List<SensorData> historicValues)
		{
			Definition = definition;
			NewValue = newValue;
			HistoricValues = historicValues;
		}
	}

	/// <summary>
	/// event for a changing value (cancellable)
	/// </summary>
	public class ValueChangingEventArgs : ValueEventArgs
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
		/// Initializes a new instance of the <see cref="ValueChangingEventArgs"/> class.
		/// </summary>
		/// <param name="definition">The definition.</param>
		/// <param name="newValue">The new value.</param>
		/// <param name="historicValues">The historic values.</param>
		public ValueChangingEventArgs(ValueDefinition definition, SensorData newValue, List<SensorData> historicValues)
			: base(definition, newValue, historicValues)
		{

		}
	}


	/// <summary>
	/// event for a changed value 
	/// </summary>
	public class ValueChangedEventArgs : ValueEventArgs
	{
		/// <summary>
		/// Gets or sets a value indicating whether this instance is cancelled. Setting to <c>true</c> means that no further processing for the value shall take place for whatever reason
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is cancelled; otherwise, <c>false</c>.
		/// </value>
		public bool IsCancelled { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueChangingEventArgs"/> class.
		/// </summary>
		/// <param name="definition">The definition.</param>
		/// <param name="newValue">The new value.</param>
		/// <param name="historicValues">The historic values.</param>
		public ValueChangedEventArgs(ValueDefinition definition, SensorData newValue, List<SensorData> historicValues)
			: base(definition, newValue, historicValues)
		{

		}
	}


}
