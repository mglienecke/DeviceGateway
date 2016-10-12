using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalDataContracts;

namespace ValueManagement
{
	/// <summary>
	/// defines a new value
	/// 
	/// TODO: For clustering operations the List<> must be replaced by an implementation which notifies other members in the system
	/// </summary>
	[Serializable]
	public class ValueDefinition
	{
        /// <summary>
        /// the default amount of historic values (before a drop occurs)
        /// </summary>
        public const int MaxHistoricValuesDefault = 20;

        /// <summary>
		/// shared among all classes for an entry list
		/// </summary>
		protected static List<SensorData> emptyList = new List<SensorData>();

		protected string mName;
		protected int mInternalId;
		protected List<SensorData> mHistoricValueList = emptyList;
        private SensorData mCurrentValue;
        private string mDefaultValue;

        #region Events...
        /// <summary>
        /// Occurs when [value is changing].
        /// </summary>
        public event EventHandler<ValueChangingEventArgs> ValueIsChanging;

        /// <summary>
        /// Occurs when [value has changed].
        /// </summary>
        public event EventHandler<ValueChangedEventArgs> ValueHasChanged;
        #endregion

        #region Constructors...
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueDefinition"/> class. Only for derived classes
        /// </summary>
        protected ValueDefinition()
        {
            // at least have a basic value
            mHistoricValueList = new List<SensorData>();

            // set the 2 default values properly
            MaxHistoricValues = MaxHistoricValuesDefault;
            KeepHistoricValues = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueDefinition"/> class with the name
        /// </summary>
        /// <param name="internalId">The internal id.</param>
        /// <param name="name">The name.</param>
        public ValueDefinition(int internalId, string name)
            : this()
        {
            mInternalId = internalId;
            mName = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueDefinition"/> class with the name and an initial set of values to be loaded
        /// </summary>
        /// <param name="interalId">The interal id.</param>
        /// <param name="name">The name.</param>
        /// <param name="dataValueList">The data value list.</param>
        public ValueDefinition(int internalId, string name, List<SensorData> dataValueList)
            : this(internalId, name)
        {
            if (dataValueList == null)
            {
                throw new ArgumentNullException("dataValueList");
            }

            // create a copy so that the original object has no effect on the representation
            mHistoricValueList = dataValueList.ToList();

            // set the last historic value as the current one
            mCurrentValue = mHistoricValueList.LastOrDefault();
        }
        #endregion

        #region Properties...
        /// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get { return (mName); } }

		/// <summary>
		/// Gets the internal id.
		/// </summary>
		/// <value>The internal id.</value>
		public int InternalId { get { return (mInternalId); } }

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		public string Description { get; set; }

		/// <summary>
		/// Gets the historic values which are associated with the value definition
		/// </summary>
		public SensorData[] GetHistoricValues() { return (mHistoricValueList.ToArray()); }

	    /// <summary>
	    /// Sets the historic values which are associated with the value definition
	    /// </summary>
	    /// <value>The values.</value>
	    public void SetHistoricValues(IEnumerable<SensorData> values)
	    {
	        lock (this)
	        {
	            mHistoricValueList.Clear();
	            mHistoricValueList.AddRange(values);
	        }
	    }

	    /// <summary>
		/// Gets or sets the max historic values.
		/// </summary>
		/// <value>The max historic values.</value>
		public int MaxHistoricValues { get; set; }

        /// <summary>
        /// Gets or sets the flag defining if run-time historic values are to be saved.
        /// </summary>
        public bool KeepHistoricValues { get; set; }

		/// <summary>
		/// Gets or sets the value type code. This can be useful as a consumer might then convert according to the type code
		/// </summary>
		/// <value>The intended value type code.</value>
		public TypeCode ValueTypeCode { get; set; }

		/// <summary>
		/// Gets or sets the value unit of measure. This is only really needed if the type code for the value has a custom format (and is <see cref="TypeCode.Object"/>). 
		/// In any other case it might be a simple representation like K(elvin) or LUX
		/// </summary>
		/// <value>The value unit of measure.</value>
		public string ValueUnitOfMeasure { get; set; }

        /// <summary>
        /// Gets or sets the flag defining if the definition's values are to be persisted.
        /// </summary>
        public bool ShallSensorDataBePersisted { get; set; }

        /// <summary>
        /// Gets or sets the default value for the sensor to be returned, when the sensor's current value has not been yet set.
        /// </summary>
        public string DefaultValue { 
            get { return mDefaultValue; } 
            set { 
                mDefaultValue = value; 
            } 
        }

        /// <summary>
        /// Gets or sets the current value. Getting just delivers the currently defined value whereas setting involving calling back all the defined call backs as well as perhaps launching the 
        /// evaluation process for dependent virtual values
        /// </summary>
        /// <value>The current value.</value>
        public virtual SensorData CurrentValue
        {
            set
            {
                //Null-check
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                // just one caller at a time as otherwise chaos might happen
                lock (this)
                {
                    bool historicValueHasBeenSaved = false;

                    // Call back in the various stages
                    if (ValueIsChanging != null)
                    {
                        // add the current value to the historic value as this might be important
                        if (KeepHistoricValues && mCurrentValue != null)
                        {
                            mHistoricValueList.Add(mCurrentValue);
                            historicValueHasBeenSaved = true;
                        }

                        // and now call the listeners so that the final call queue can be produced
                        if (ValueIsChanging != null)
                        {
                            // create a new instance for the event
                            ValueChangingEventArgs args = new ValueChangingEventArgs(this, value, mHistoricValueList);

                            ValueIsChanging(this, args);

                            if (args.IsCancelled)
                            {
                                // remove the added item in case a cancellation happened
                                mHistoricValueList.Remove(mCurrentValue);

                                // and get out directly
                                return;
                            }

                            if (args.IsValueModified)
                            {
                                value = args.NewValue;
                            }
                        }
                    }

                    // if not yet done then now and there was a value
                    if (KeepHistoricValues && (historicValueHasBeenSaved == false) && (mCurrentValue != null))
                    {
                        mHistoricValueList.Add(mCurrentValue);
                    }

                    // if the maximum is reached truncate one element - this avoids the problem to "delete" an element and then need it again because of cancel
                    TruncateListOnDemand(mHistoricValueList, MaxHistoricValues);


                    // as the value has been remembered in the history already, we can just set the new value (which might have been modified as well)
                    mCurrentValue = value;

                    // call any registered call back after assigning the value
                    if (ValueHasChanged != null)
                    {
                        ValueHasChanged(this, new ValueChangedEventArgs(this, value, mHistoricValueList));
                    }
                }
            }

            get
            {
                //If it's not set - return the default
                if (mCurrentValue == null) 
                    return DefaultValue;
                
                return (mCurrentValue);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a virtual value. Standard values are not
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a virtual value; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsVirtualValue { get { return (false); } }

        /// <summary>
        /// The property contains a reference to the sensor object, from which this value definition has been created.
        /// </summary>
        public Sensor SensorCreatedFrom
        {
            get;
            internal set;
        }
        #endregion

        /// <summary>
		/// Truncates the list on demand (if the current size exceeds the maximum amount)
		/// </summary>
		/// <param name="list">The list.</param>
		/// <param name="maxCount">The max count.</param>
		protected void TruncateListOnDemand(IList list, int maxCount)
		{
			// if unlimited maximum is set get out
			if (maxCount == -1)
			{
				return;
			}

			while (list.Count > maxCount)
			{
				list.RemoveAt(0);
			}
		}

        /// <summary>
        /// The method makes a deep copy of the definition's data.
        /// </summary>
        /// <returns></returns>
        public ValueDefinition CopyData()
        {
            //lock (this){
                ValueDefinition copy = new ValueDefinition(InternalId, Name);

                SensorData currentValue = this.CurrentValue;
                if (currentValue == null)
                {
                    copy.CurrentValue = null;
                }
                else
                {
                    copy.CurrentValue = new SensorData();
                    copy.CurrentValue.CopyData(currentValue);
                }

                copy.mHistoricValueList.AddRange(this.mHistoricValueList.ToArray());

                return copy;
            //}
        }

        #region Overridden base methods...
        /// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return mInternalId;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			return ((obj is ValueDefinition) && ((obj as ValueDefinition).InternalId == mInternalId));
        }
        #endregion
    }
}
