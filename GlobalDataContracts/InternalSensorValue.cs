using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS;

namespace GlobalDataContracts
{
    /// <summary>
    /// The class encapsulates data of sensor values collected over some period time.
    /// </summary>
    [Serializable]
	public class InternalSensorValue
    {
        #region Constants...
        /// <summary>
		/// the divider between the two strings
		/// </summary>
		public const string DeviceSensorDivider = "=>";

		/// <summary>
		/// 10 records depth for the history
		/// </summary>
		public const int ValueHistoryDepth = 10;
        #endregion

        /// <summary>
		/// a lock for access
		/// </summary>
		private readonly object accessLock = new object();

		/// <summary>
		/// Gets or sets the unique key.
		/// </summary>
		/// <value>The unique key.</value>
		public string UniqueKey { get; set; }

		/// <summary>
		/// old historical values ordered by the date when they were collected
		/// </summary>
		private SortedList<Int64, object> historicValues = new SortedList<Int64, object>();

        /// <summary>
        /// this is a HashSet of value timestamps which have not yet been saved
        /// </summary>
        private HashSet<Int64> unsavedValues = new HashSet<Int64>();

        private List<long> currentUnsavedValueMarks = new List<long>();

        #region Properties...

        /// <summary>
        /// The property contains the internal id of the associated sensor.
        /// </summary>
        public int InternalSensorId
        {
            get;
            set;
        }

        /// <summary>
		/// Gets the historic values.
		/// </summary>
		/// <value>The historic values.</value>
		public SortedList<Int64, object> HistoricValues
		{
			get { return (historicValues); }
		}

		/// <summary>
		/// Gets the unsaved values.
		/// </summary>
		/// <value>The unsafed values.</value>
		public HashSet<Int64> UnsavedValues
		{
			get { return (unsavedValues); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether values shall be persisted
		/// </summary>
		/// <value><c>true</c> if [persist values]; otherwise, <c>false</c>.</value>
		public bool PersistValues { get; set; }

		/// <summary>
		/// Gets the device id.
		/// </summary>
		/// <value>The device id.</value>
		public string DeviceId
		{
			get { return (UniqueKey.Split(new string [] {DeviceSensorDivider}, StringSplitOptions.None)[0]); }
		}

		/// <summary>
		/// Gets the sensor id.
		/// </summary>
		/// <value>The sensor id.</value>
		public string SensorId
		{
			get { return (UniqueKey.Split(new string[] { DeviceSensorDivider }, StringSplitOptions.None)[1]); }
		}

		/// <summary>
		/// Gets or sets the most recent value. This takes the first entry from the list of historic values (if present)
		/// When set the last changed count is incremented and the timestamp stored as well as the change counter incremented and the persisted flag reset
		/// </summary>
		/// <value>The value.</value>
		public object NewestValue
		{
			get { return (AreValuesPresent ? historicValues.Last().Value : null); }
		}

		/// <summary>
		/// Gets or sets the type of the value data.
		/// </summary>
		/// <value>The type of the value data.</value>
		public SensorValueDataType ValueDataType { get; set; }
        #endregion

        /// <summary>
        /// Gets the values to save by finding all historic values which have not yet been saved
        /// </summary>
        /// <returns>an enumeration of values to save</returns>
        public IEnumerable<KeyValuePair<Int64, object>> GetValuesToSave()
        {
            lock (accessLock)
            {
                //Keep marks for values to be given away, so we could clear there marks later
                currentUnsavedValueMarks.Clear();
                if (ValueCount > 0)
                {
                    currentUnsavedValueMarks.AddRange(unsavedValues);
                }

                return (from entry in historicValues where unsavedValues.Contains(entry.Key) select entry);
            }
        }

        /// <summary>
        /// The method removes from the unsaved value marks storage all marks that are in the snapshot of unsaved value marks.
        /// </summary>
        public void ClearMarksForSavedValues()
        {
            lock (accessLock)
            {
                int count = currentUnsavedValueMarks.Count();
                for (int i = 0; i < count; i++)
                {
                    unsavedValues.Remove(currentUnsavedValueMarks.ElementAt(i));
                }

                currentUnsavedValueMarks.Clear();
            }
        }

		/// <summary>
		/// Determines whether the specified value is a valid sensor value (for the given type).
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// 	<c>true</c> if the specified value is a valid sensor value (for the given type); otherwise, <c>false</c>.
		/// </returns>
		public bool IsValidSensorValue(object value)
		{
			bool isValidFormat = false;

			switch (ValueDataType)
			{
				case SensorValueDataType.String:
					// everything can be represented as a string
					isValidFormat = true;
					break;

				case SensorValueDataType.Int:
					int resInt;
					isValidFormat = Int32.TryParse(value.ToString(), out resInt);
					break;

				case SensorValueDataType.Long:
					long resLong;
					isValidFormat = Int64.TryParse(value.ToString(), out resLong);
					break;

				case SensorValueDataType.Bit:
					bool resBool;
					isValidFormat = Boolean.TryParse(value.ToString(), out resBool);
					break;

				case SensorValueDataType.Decimal:
					Decimal resDec;
					isValidFormat = Decimal.TryParse(value.ToString(), out resDec);
					break;

			}

			return (isValidFormat);
		}

		/// <summary>
		/// Sets the value and checks if the value has the proper type and format
		/// </summary>
		/// <param name="when">When was the value gathered.</param>
		/// <param name="value">The value.</param>
		/// <returns><c>true</c> if the value was set, otherwise <c>false</c> which usually indicates a format error for the data</returns>
		public bool SetValue(DateTime when, object value)
		{
            //Checks
			if (IsValidSensorValue(value) == false)
			{
				return (false);
			}

			lock (accessLock)
			{
                //Store
				historicValues.Add(when.Ticks, value);

                if (PersistValues)
                {
                    //Keep a ref
                    unsavedValues.Add(when.Ticks);

                    //Clean up historical values
                    // a removal of values exceeding the history length only takes place after the values have been written 
                    // as the unsaved set only remembers the timestamp of the value
                    while (historicValues.Count > ValueHistoryDepth)
                    {
                        //If the first element has been saved already or doesn't need to be saved
                        if (unsavedValues.Contains(historicValues.ElementAt(0).Key) == false)
                        {
                            historicValues.RemoveAt(0);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
			}

			LastModified = DateTime.Now;
			NumChanges++;

			return (true);
		}


		/// <summary>
		/// Gets a value indicating whether values are present 
		/// </summary>
		/// <value><c>true</c> if values are present; otherwise, <c>false</c>.</value>
		public bool AreValuesPresent
		{
			get { return (ValueCount > 0); }
		}

		/// <summary>
		/// Gets the amount of values present.
		/// </summary>
		/// <value>The amount of values present.</value>
		public int ValueCount
		{
			get { return (historicValues.Count); }
		}

		/// <summary>
		/// Gets or sets the last changed timestamp which represents when the entry has been modified
		/// </summary>
		/// <value>The last changed.</value>
		public DateTime LastModified { get; set; }

		/// <summary>
		/// Gets or sets the number of changes.
		/// </summary>
		/// <value>The number of changes.</value>
		public int NumChanges { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="InternalSensorValue&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="deviceId">The device id.</param>
		/// <param name="sensorId">The sensor id.</param>
		/// <param name="persistValues">if set to <c>true</c> [persist values].</param>
		/// <param name="dataType">Type of the data.</param>
		/// <param name="when">When was the value gathered.</param>
		/// <param name="value">The value.</param>
		public InternalSensorValue(string deviceId, string sensorId, bool persistValues, SensorValueDataType dataType, DateTime when, object value)
			: this(deviceId, sensorId, persistValues, dataType)
		{
			SetValue(when, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InternalSensorValue&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="deviceId">The device id.</param>
		/// <param name="sensorId">The sensor id.</param>
		/// <param name="persistValues">if set to <c>true</c> [persist values].</param>
		/// <param name="dataType">Type of the data.</param>
		public InternalSensorValue(string deviceId, string sensorId, bool persistValues, SensorValueDataType dataType)
		{
			UniqueKey = GetUniqueKey(deviceId, sensorId);
			PersistValues = persistValues;
			ValueDataType = dataType;
		}

		/// <summary>
		/// Gets the unique key.
		/// </summary>
		/// <param name="deviceId">The device id.</param>
		/// <param name="sensorId">The sensor id.</param>
		/// <returns>a unique combined key</returns>
		public static string GetUniqueKey(string deviceId, string sensorId)
		{
			return (deviceId + DeviceSensorDivider + sensorId);
		}

		
		#region ToXXX conversion functions

		/// <summary>
		/// Converts to Int32
		/// </summary>
		/// <returns>the <c>Int32</c> representation of the newest value</returns>
		public Int32 ToInt32()
		{
			return (Convert.ToInt32(NewestValue));
		}

		/// <summary>
		/// Converts to Int64
		/// </summary>
		/// <returns>the <c>Int64</c> representation of the newest value</returns>
		public Int64 ToInt64()
		{
			return (Convert.ToInt64(NewestValue));
		}

		/// <summary>
		/// Converts to Boolean
		/// </summary>
		/// <returns>the <c>Boolean</c> representation of the newest value</returns>
		public Boolean ToBoolean()
		{
			return (Convert.ToBoolean(NewestValue));
		}


		/// <summary>
		/// Converts to Decimal
		/// </summary>
		/// <returns>the <c>Decimal</c> representation of the newest value</returns>
		public Decimal ToDecimal()
		{
			return (Convert.ToDecimal(NewestValue));
		}

		/// <summary>
		/// Converts to String
		/// </summary>
		/// <returns>the <c>String</c> representation of the newest value</returns>
		public override String ToString()
		{
			return (Convert.ToString(NewestValue));
		}

		#endregion

		#region explicit conversion operators

		/// <summary>
		/// Performs an implicit conversion from <see cref="System.Object"/> to <see cref="System.Int32"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator Int32(InternalSensorValue value)
		{
			return (Convert.ToInt32(value.NewestValue));
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="System.Object"/> to <see cref="System.Int64"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator Int64(InternalSensorValue value)
		{
			return (Convert.ToInt64(value.NewestValue));
		}
		/// <summary>
		/// Performs an implicit conversion from <see cref="System.Object"/> to <see cref="System.Decimal"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator Decimal(InternalSensorValue value)
		{
			return (Convert.ToDecimal(value.NewestValue));
		}
		/// <summary>
		/// Performs an implicit conversion from <see cref="System.Object"/> to <see cref="System.Boolean"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator Boolean(InternalSensorValue value)
		{
			return (Convert.ToBoolean(value.NewestValue));
		}
		/// <summary>
		/// Performs an implicit conversion from <see cref="System.Object"/> to <see cref="System.String"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator String(InternalSensorValue value)
		{
			return (Convert.ToString(value.NewestValue));
		}

		#endregion

	}

	/// <summary>
	/// a dictionary of internal sensor values
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class InternalSensorValueDictionary
	{
        /// <summary>
        /// a lock for access
        /// </summary>
        private readonly object accessLock = new object();

		/// <summary>
		/// the dictionary where the values are stored
		/// </summary>
		private Dictionary<string, InternalSensorValue> valueDict = new Dictionary<string, InternalSensorValue>();

		/// <summary>
		/// Gets the sensor value.
		/// </summary>
		/// <param name="deviceId">The device id.</param>
		/// <param name="sensorId">The sensor id.</param>
		/// <returns>the value or <c>null</c> if not present</returns>
		public InternalSensorValue GetSensorValue(string deviceId, string sensorId)
		{
			string key = InternalSensorValue.GetUniqueKey(deviceId, sensorId);
			return (valueDict.ContainsKey(key) ? valueDict[key] : null);
		}

		/// <summary>
		/// Determines whether a value is present for the specified device and sensor id.
		/// </summary>
		/// <param name="deviceId">The device id.</param>
		/// <param name="sensorId">The sensor id.</param>
		/// <returns>
		/// 	<c>true</c> if a value is present for the specified device and sensor id; otherwise, <c>false</c>.
		/// </returns>
		public bool IsValuePresent(string deviceId, string sensorId)
		{
			return (valueDict.ContainsKey(InternalSensorValue.GetUniqueKey(deviceId, sensorId)));
		}

		/// <summary>
		/// Sets the sensor value.
		/// </summary>
		/// <param name="deviceId">The device id.</param>
		/// <param name="sensorId">The sensor id.</param>
		/// <param name="persistValues">if set to <c>true</c> [persist values].</param>
		/// <param name="valueType">Type of the value.</param>
		/// <param name="when">when was the value gathered.</param>
		/// <param name="value">The value.</param>
		public bool SetSensorValue(string deviceId, string sensorId, bool persistValues, SensorValueDataType valueType, DateTime when, object value)
		{
			InternalSensorValue val = GetSensorValue(deviceId, sensorId);

			// if not present add an object
			if (val == null)
			{
				val = new InternalSensorValue(deviceId, sensorId, persistValues, valueType);
                
                lock (accessLock)
                {
                    valueDict.Add(val.UniqueKey, val);
                }
			}

			// and store the value accordingly
			return (val.SetValue(when, value));
		}

		/// <summary>
		/// Gets the sensor values to write.
		/// </summary>
		/// <returns></returns>
		public List<InternalSensorValue> GetSensorValuesToWrite()
		{
            lock (accessLock)
            {
                return (valueDict.Values.Where(sensVal => sensVal.PersistValues && (sensVal.UnsavedValues.Count > 0)).ToList());
            }
		}
	}
}
