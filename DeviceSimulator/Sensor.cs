using System;

using System.Collections;
using System.Threading;

using System.IO;
using System.Net;
using fastJSON;

namespace DeviceServer.Simulator
{
    /// <summary>
    /// Enumeration for possible sensor data retrieval modes.
    /// </summary>
    [Serializable]
    public enum SensorDataRetrievalMode
    {
        None = 0,

        Pull = 1,

        Push = 2,

        Both = 4,

        PushOnChange = 8
    }

    /// <summary>
    /// Enumeration for possible sensor value data types.
    /// </summary>
    [Serializable]
    public enum SensorValueDataType
    {
        AsIs = TypeCode.Empty, 

        Bit = TypeCode.Boolean,

        Int = TypeCode.Int32,

        Long = TypeCode.Int64,

        Decimal = TypeCode.Decimal,

        String = TypeCode.String
    }

    /// <summary>
    /// Enumeration for possuble pull-mode communication types
    /// </summary>
    [Serializable]
    public enum PullModeCommunicationType
    {
        Undefined = 0,

        REST = 1,

        SOAP = 2,

        DotNetObject = 3,

        CNDEP = 4
    }

    /// <summary>
    /// The class encapsulates data of a single sensor data reading.
    /// </summary>
    [Serializable]
    public class SensorData
    {
        public const string PropertyValue = "Value";

        ///// <summary>
        ///// Constructor. Initializes a new instance of the <see cref="SensorData"/> class.
        ///// </summary>
        ///// <param name="generatedWhen">The generated when.</param>
        ///// <param name="value">The value.</param>
        //public SensorData(DateTime generatedWhen, string value)
        //{
        //    GeneratedWhen = generatedWhen;
        //    Value = value;
        //}

        ///// <summary>
        ///// Constructor. Initializes a new instance of the <see cref="SensorData"/> class.
        ///// </summary>
        ///// <param name="generatedWhenTicks">The generated when.</param>
        ///// <param name="value">The value.</param>
        //public SensorData(long generatedWhenTicks, string value)
        //    : this(new DateTime(generatedWhenTicks), value)
        //{
        //}

        /// <summary>
        /// Constructor. Initializes a new instance of the <see cref="SensorData"/> class.
        /// </summary>
        /// <param name="generatedWhen">The generated when.</param>
        /// <param name="value">The value.</param>
        /// <param name="correlationId"></param>
        public SensorData(DateTime generatedWhen, string value, string correlationId)
        {
            GeneratedWhen = generatedWhen;
            Value = value;
            CorrelationId = correlationId;
        }

        /// <summary>
        /// Constructor. Initializes a new instance of the <see cref="SensorData"/> class.
        /// </summary>
        /// <param name="generatedWhenTicks">The generated when.</param>
        /// <param name="value">The value.</param>
        /// <param name="correlationId"></param>
        public SensorData(long generatedWhenTicks, string value, string correlationId)
            : this(new DateTime(generatedWhenTicks), value, correlationId)
        {
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the created timestamp
        /// </summary>
        /// <value>The created.</value>
        public DateTime GeneratedWhen { get; set; }

        /// <summary>
        /// THe property contains id of the sensor that produced this piece of data.
        /// </summary>
        public string SensorId { get; set;}

        /// <summary>
        /// THe property contains the correlation id of this piece of data.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Constructor. Initializes a new instance of the <see cref="SensorData"/> class.
        /// </summary>
        public SensorData()
        {
            GeneratedWhen = DateTime.Now;
            Value = string.Empty;
        }

        /// <summary>
        /// Constructor. Initializes a new instance of the <see cref="SensorData"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public SensorData(string value)
        {
            Value = value;
            GeneratedWhen = DateTime.Now;
        }
    }

    [Serializable]
    public class MultipleSensorData
    {
        public string SensorId { get; set; }

        public SensorData[] Measures { get; set; }
    }

    /// <summary>
    /// The class encapsulates data of a single sensor.
    /// </summary>
    [Serializable]
    public class Sensor
    {
        private const int DefaultMaxHistoricalRecords = 10;

        private SensorConfig mConfig;
        private ArrayList mPorts;

        #region Delegates...
        /// <summary>
        /// Handler type for getting the sensor's current value.
        /// </summary>
        /// <returns></returns>
        public delegate SensorData GetValueHandler();

        /// <summary>
        /// Handler for setting the sensor's value (if supported).
        /// </summary>
        /// <param name="value"></param>
        public delegate void SetValueHandler(SensorData value);
        #endregion

        #region Properties...
        public string DeviceId { get; set; }

        public string Id { get; set; }

        public uint InternalId { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        public string UnitSymbol { get; set; }

        public SensorDataRetrievalMode SensorDataRetrievalMode { get; set; }

        public SensorValueDataType SensorValueDataType { get; set; }

        public PullModeCommunicationType PullModeCommunicationType { get; set; }

        public PullModeCommunicationType PushModeCommunicationType { get; set; }

        public SensorData LastReadValue {get; set;}

        public bool ShallSensorDataBePersisted { get; set; }

        public bool PersistDirectlyAfterChange { get; set; }

        public string PullModeDotNetObjectType { get; set; }

        public string HardwareSensorSimulatorTypeName { get; set; }

        public bool IsSynchronousPushToActuator { get; set; }

        public bool IsActuator { get; set; }

        /// <summary>
        /// The property contains a reference to the sensor's runtime config data.
        /// </summary>
        public SensorConfig Config
        {
            get
            {
                return mConfig;
            }
            set
            {
                mConfig = value;
            }
        }

        public SetValueHandler ValueSetter { get; set; }

        public GetValueHandler ValueGetter { get; set; }

        /// <summary>
        /// The property contains historical sensor values
        /// </summary>
        public LimitedQueue StoredValues
        {
            get;
            private set;
        }

        /// <summary>
        /// The property contains a flag defining if the sensor is turned on.
        /// </summary>
        internal protected bool IsActive { get; set; }
    
        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Sensor()
        {
        }

        internal void Init()
        {
            InitStoredValues();

			if (Config == null)
            {
                IsActive = false;
            }
            else
            if (mConfig.UseLocalStore)
            {
                SaveStoredValues();
            }
        }


        internal SensorData ScanSensorValue(out bool isCoalesced)
        {
            //Read the current value
            //Coalesce it if configured
            isCoalesced = false;
            if (Config.IsCoalesce)
            {
                SensorData previousValue = LastReadValue;
                LastReadValue = ReadSensorValue(); 

                //If the values are the same
                if (previousValue.Value == LastReadValue.Value)
                {
                    isCoalesced = true;
                }
            }
            else
            {
                //Just read
                LastReadValue = ReadSensorValue(); 
            }

            //Keep the value
            StoreCurrentValueToHistory();

            LastReadValue.SensorId = Id;
            return LastReadValue;
        }

        internal void StoreCurrentValueToHistory()
        {
            //Keep the value
            if (StoredValues != null)
            {
                StoredValues.Enqueue(LastReadValue);

                //Save if needed
                if (Config.UseLocalStore)
                {
                    SaveStoredValues();
                }
            }
        }

        internal SensorData ReadSensorValue()
        {
            SensorData newValue = ValueGetter();
            //Apply time correction if needed
            if ( DeviceServerSimulator.RunningInstance.UseServerTime) {
                newValue.GeneratedWhen = DeviceServerSimulator.RunningInstance.GetServerTime(newValue.GeneratedWhen); 
            }
            return newValue;
        }

        #region Storing sensor historical values persistently in a local storage...
        protected virtual LimitedQueue TryRecoverStoredValues()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.PathSeparator + "StoredValues.json";
            string jsonStored = null;
            LimitedQueue storedValues;
            if (File.Exists(path))
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (TextReader reader = new StreamReader(stream))
                    {
                        jsonStored = reader.ReadToEnd();
                    }
                }
            }

            if (jsonStored == null)
            {
                storedValues = new LimitedQueue();
            }
            else
            {
                storedValues = new LimitedQueue((SensorData[])JSON.Instance.ToObject(jsonStored, typeof(SensorData[])));
            }

            return storedValues;
        }

        protected virtual void SaveStoredValues()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.PathSeparator + "StoredValues.json";
            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
            {
                using (TextWriter writer = new StreamWriter(stream))
                {
                    writer.Write(JSON.Instance.ToJSON(StoredValues));
                }
            }
        }

        private void InitStoredValues()
        {
            if (Config != null)
            {
                if (Config.KeepHistory)
                {
                    //Are the values stored somewhere?
                    if (Config.UseLocalStore)
                    {
                        StoredValues = TryRecoverStoredValues();
                        //Not recovered?
                        if (StoredValues == null)
                        {
                            StoredValues = new LimitedQueue(Config.KeepHistoryMaxRecords);
                        }
                        else
                        {
                            StoredValues.MaxCount = mConfig.KeepHistoryMaxRecords;
                        }
                    }
                    else
                    {
                        //Just create
                        StoredValues = new LimitedQueue(Config.KeepHistoryMaxRecords);
                    }
                }
                else
                {
                    StoredValues = null;
                }
            }
            else
            {
                StoredValues = null;
            }

            //Placeholder
            LastReadValue = new SensorData();
        }
        #endregion
    }
}
