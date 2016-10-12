using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GlobalDataContracts;

namespace CentralServerService
{
	/// <summary>
	/// Basic data for an operation which can be cancelled and modified
	/// </summary>
	public class DataSinkEventArgs : EventArgs
	{
		private SensorData originalValue;
		private bool hasValueChanged;
		private SensorData changedValue;
		private bool isModifiable;
		private bool isCancellable;
		private bool cancel;

		/// <summary>
		/// Gets the original value.
		/// </summary>
		/// <value>The original value.</value>
		public SensorData OriginalValue { get { return (originalValue); } }

		/// <summary>
		/// Gets or sets a value indicating whether the operation shall be cancelled 
		/// </summary>
		/// <value><c>true</c> if cancel; otherwise, <c>false</c>.</value>
		public bool Cancel
		{
			get { return (cancel); }
			set
			{
				if (isCancellable)
				{
					cancel = value;
				}
				else
				{
					throw new InvalidOperationException(Properties.Resources.CancelNotAllowed);
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the value has changed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the value has changed; otherwise, <c>false</c>.
		/// </value>
		public bool HasValueChanged { get { return (hasValueChanged); } }

		/// <summary>
		/// Gets or sets the changed value. When set the changed flag is modified as well
		/// </summary>
		/// <value>The changed value.</value>
		public SensorData ChangedValue
		{
			get { return (changedValue); }
			set
			{
				if (isModifiable)
				{
					changedValue = value;
					hasValueChanged = true;
				}
				else
				{
					throw new InvalidOperationException(Properties.Resources.ChangeValueNotAllowed);
				}
			}
		}


		/// <summary>
		/// Gets a value indicating whether this instance is modifiable.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is modifyable; otherwise, <c>false</c>.
		/// </value>
		public bool IsModifiable { get { return (isModifiable); } }

		/// <summary>
		/// Gets a value indicating whether this instance is cancellable.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is cancellable; otherwise, <c>false</c>.
		/// </value>
		public bool IsCancellable { get { return (isCancellable); } }

		/// <summary>
		/// Initializes a new instance of the <see cref="DataSinkEventArgs"/> class.
		/// </summary>
		/// <param name="dataValue">The data value.</param>
		public DataSinkEventArgs(SensorData dataValue)
		{
			originalValue = dataValue;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CancellableDataSinkEventArgs"/> class.
		/// </summary>
		/// <param name="dataValue">The data value.</param>
		/// <param name="isCancellable">if set to <c>true</c> this instance [is cancellable].</param>
		/// <param name="isModifiable">if set to <c>true</c> [is modifiable].</param>
		public DataSinkEventArgs(SensorData dataValue, bool isCancellable, bool isModifiable)
			: this(dataValue)
		{
			this.isCancellable = isCancellable;
			this.isModifiable = isModifiable;
		}
	}
}
