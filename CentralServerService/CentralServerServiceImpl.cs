using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Configuration;
using System.Threading;
using System.Diagnostics;

using log4net;
using System.Threading.Tasks;

using GlobalDataContracts;
using DataStorageAccess;
using ValueManagement;
using ValueManagement.DynamicCallback;
using System.Globalization;
using Common.Server;
using CentralServerService.Cndep;

namespace CentralServerService
{
	/// <summary>
	/// implementation class for the central remoting sFervice
	/// </summary>
	public class CentralServerServiceImpl : MarshalByRefObject, ICentralServerService
	{
		private static log4net.ILog log = log4net.LogManager.GetLogger(typeof(CentralServerServiceImpl));

#if MONITOR_PERFORMANCE
        private const string PerfMonCategoryMethods         = "Experiment CentralServerService methods";
        private const string PerfMonCategoryService         = "Experiment CentralServerService service";
        private const string PerfMonStoreMultipleSensorData = "StoreMultipleSensorData";
        private const string PerfMonStoreSingleSensorData   = "StoreSingleSensorData";
        private const string PerfMonGetSensorData           = "GetSensorData";
        private const string PerfMonGetSensorDataLatest     = "GetSensorDataLatest";
        private const string PerfMonGetNextCorrelationId    = "GetNextCorrelationId";
#endif

        #region Constants...
        /// <summary>
		/// the configuration string for the duration
		/// </summary>
		public const string CFG_DURATION_BETWEEN_WRITES = "DurationBetweenWrites";
        /// <summary>
        /// Configuration property name. Value: boolean.
        /// </summary>
	    public const string CFG_USE_CNDEP_SERVER = "UseCndepServer";
        /// <summary>
        /// Configuration property name. Value: boolean.
        /// </summary>
        public const string CFG_USE_MSMQ_SERVER = "UseMsmqServer";

		/// <summary>
		/// the time the thread sleeps between iterations
		/// </summary>
		public const int ThreadSleepTime = 250;

		/// <summary>
		/// Writes every second
		/// </summary>
		public const int DefaultDuration = 1000;

        /// <summary>
        /// the separator between the device name and the sensor
        /// </summary>
        public const string DeviceSensorSeparator = "=>";
        #endregion

#if DEBUG
		/// <summary>
		/// Gets or sets the storing is done handler
		/// </summary>
		/// <value>The storing is done handler.</value>
		public event EventHandler StoringIsDone;
#endif

#if MONITOR_PERFORMANCE
        private Thread mPerfCountersThread;
        private PerformanceCounter mPerfCounterProcessHeapSizeKBytes;
#endif 

        #region Fields...
        /// <summary>
		/// the dictionary to store the devices in the system
		/// </summary>
		private Dictionary<string, Device> mDeviceDict = new Dictionary<string, Device>();

        /// <summary>
        /// the dictionary to store the devices in the system
        /// </summary>
        private Dictionary<Int32, Sensor> mSensorDict = new Dictionary<Int32, Sensor>();

		/// <summary>
		/// the dictionary with the sensors for a device
		/// </summary>
		private Dictionary<string, Dictionary<string, Sensor>> mDeviceSensorDict = new Dictionary<string, Dictionary<string, Sensor>>();

        /// <summary>
        /// Log of actuator calls. 
        /// Key: internal sensor id
        /// Value: timestamp of the last call error ; number of failed retries.
        /// </summary>
        private Dictionary<int, Tuple<DateTime,int>> _logActuatorCalls = new Dictionary<int, Tuple<DateTime, int>>();
	    private int _maxActuatorCallRetries = 15;
        private TimeSpan _actuatorCallRetryInterval = new TimeSpan(0, 0, 10);

		/// <summary>
		/// sensor values
		/// </summary>
		//private InternalSensorValueDictionary mSensorValues = new InternalSensorValueDictionary();

        private DeviceScanningTask mScanningTask;

        private CndepCentralServiceServer mCndepServer;
	    private MsmqCentralServiceServer _msmqServer;
        #endregion

        #region Properties...
        /// <summary>
        /// Gets or sets the duration between writes to the storage
        /// </summary>
        /// <value>The duration between writes.</value>
        public int DurationBetweenWrites { get; set; }

		/// <summary>
		/// Gets or sets the instance.
		/// </summary>
		/// <value>The instance.</value>
        public static CentralServerServiceImpl Instance { get; private set;  }
        #endregion

        /// <summary>
        /// The method returns an instance of the class.
        /// </summary>
        /// <returns></returns>
        public static ICentralServerService GetInstance()
        {
            if (Instance == null)
            {
                lock (typeof(CentralServerServiceImpl))
                {
                    if (Instance == null)
                    {
                        Instance = new CentralServerServiceImpl();
                    }
                }
            }

            return Instance;
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="CentralServerServiceImpl"/> class.
		/// </summary>
		internal CentralServerServiceImpl()
		{
            if (Instance != null)
            {
                throw new InvalidOperationException();
            }

            Instance = this;

#if MONITOR_PERFORMANCE
            try
            {
                PerformanceMonitoring.SetupPerformanceCounterCategoryForMethods(PerfMonCategoryMethods, String.Empty);

                PerformanceMonitoring.SetupPerformanceCounters(PerfMonCategoryMethods, PerfMonStoreMultipleSensorData);
                PerformanceMonitoring.SetupPerformanceCounters(PerfMonCategoryMethods, PerfMonStoreSingleSensorData);
                PerformanceMonitoring.SetupPerformanceCounters(PerfMonCategoryMethods, PerfMonGetSensorData);
                PerformanceMonitoring.SetupPerformanceCounters(PerfMonCategoryMethods, PerfMonGetSensorDataLatest);
                PerformanceMonitoring.SetupPerformanceCounters(PerfMonCategoryMethods, PerfMonGetNextCorrelationId);

                if (PerformanceCounterCategory.Exists(PerformanceMonitoring.PerfMonCentralServerServiceCategoryName) == true)
                    PerformanceCounterCategory.Delete(PerformanceMonitoring.PerfMonCentralServerServiceCategoryName);

                CounterCreationDataCollection counterData = new CounterCreationDataCollection();

                //Process heap size 
                CounterCreationData perfCounterDataHeapSizeKbytes = new CounterCreationData("HeapSize KBytes",
                    "Process heap size in KBytes.", PerformanceCounterType.NumberOfItems64);
                counterData.Add(perfCounterDataHeapSizeKbytes);

                PerformanceMonitoring.SetupPerformanceMonitoringCategory(PerfMonCategoryService, "Category for the CentralSeverService general parameters.", counterData);

                mPerfCounterProcessHeapSizeKBytes = new PerformanceCounter(PerfMonCategoryService, "HeapSize KBytes", "CentraServiceServer", false);

            }
            catch (Exception exc)
            {
                LogException("CentralServerServiceImpl", "Failed setting up performance counters", exc);
                throw;
            }

            mPerfCountersThread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    mPerfCounterProcessHeapSizeKBytes.RawValue = GC.GetTotalMemory(false) / 1024;
                }
            }
            ));
            mPerfCountersThread.IsBackground = true;
            mPerfCountersThread.Start();
#endif

            try
			{
                //LOAD DATABASE DATA
                //Load device data
				mDeviceDict = DataStorageAccessBase.Instance.LoadDevices().ToDictionary(device => device.Id);
				foreach (Device device in mDeviceDict.Values)
				{
                    //Get sensors for the device
					List<Sensor> sensorList = DataStorageAccessBase.Instance.LoadSensorsForDevice(device.Id);

                    //Store sensors
                    sensorList.ForEach(x => mSensorDict[x.InternalSensorId] = x);
					mDeviceSensorDict.Add(device.Id, sensorList.ToDictionary(sensor => sensor.Id));
				}

                //Register sensors as value definitions
                foreach (Dictionary<string, Sensor> sensors in mDeviceSensorDict.Values)
                {
                    foreach (Sensor sensor in sensors.Values)
                    {
                        try
                        {
                            var valueDef = ValueManager.Instance.RegisterSensorAsValueDefinition(sensor);
                            if (sensor.IsActuator && !sensor.IsSynchronousPushToActuator)
                                valueDef.ValueHasChanged += SendValueDefinitionValueToActuator;
                        }
                        catch (ValueDefinitionSetupException exc)
                        {
                            LogException("CentralServerServiceImpl", String.Format(Properties.Resources.ErrorRegisteringSensor, sensor.Id, sensor.DeviceId, exc.Message), exc);
                        }
                    }
                }

                //Load sensor historical data
                List<Int32> sensorIds = new List<Int32>();
                foreach (ValueDefinition valueDef in ValueManager.Instance.ValueDefinitionDictionary.Values)
                {
                    //Only those whose values have been persisted
                    if (valueDef.ShallSensorDataBePersisted)
                        sensorIds.Add(valueDef.InternalId);
                }

                List<MultipleSensorData> sensorData = DataStorageAccessBase.Instance.GetSensorDataLatest(sensorIds, ValueDefinition.MaxHistoricValuesDefault);
                foreach (MultipleSensorData data in sensorData)
                {
                    ValueManager.Instance.ValueDefinitionDictionary[data.InternalSensorId].SetHistoricValues(data.Measures);
                }


                //Load sensor dependencies
                List<SensorDependency> dependencies = DataStorageAccessBase.Instance.GetSensorDependencies();
                foreach (SensorDependency next in dependencies)
                {
                    try
                    {
                        ValueManager.Instance.AddDependency(
                            ValueManager.Instance.ValueDefinitionDictionary[next.BaseSensorInternalId],
                            ValueManager.Instance.ValueDefinitionDictionary[next.DependentSensorInternalId]
                            );
                    }
                    catch (Exception exc)
                    {
                        LogException("CentralServerServiceImpl", String.Format(Properties.Resources.ErrorFailedAddingSensorDependency, 
                            next.BaseSensorInternalId, next.DependentSensorInternalId), exc);
                    }
                }

                //LOAD CONFIGURATION DATA
				int duration;
				if ((ConfigurationManager.AppSettings[CFG_DURATION_BETWEEN_WRITES] == null) ||
					(Int32.TryParse(ConfigurationManager.AppSettings[CFG_DURATION_BETWEEN_WRITES].ToString(), out duration) == false))
				{
					duration = DefaultDuration;
				}
				DurationBetweenWrites = duration;

                //START TASKS
				// create the task to store the data properly
				//Task storeTask = Task.Factory.StartNew(() => StoreTask(cts.Token), cts.Token);
                ValueManager.Instance.StartWriteToStorageThread();
                ValueManager.DataWasWritten += new EventHandler(ValueManager_DataWasWritten);

                //Start calculating virtual values
                ValueManager.Instance.StartCalculatingVirtualValues();

				// set the instance
				Instance = this;

                //Start the CNDEP server
			    if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[CFG_USE_CNDEP_SERVER]) == false &&
			        Convert.ToBoolean(ConfigurationManager.AppSettings[CFG_USE_CNDEP_SERVER]))
			    {
			        mCndepServer = new CndepCentralServiceServer();
			        mCndepServer.Init();
			        mCndepServer.Start();
			    }

			    //Start the MSMQ server
			    if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[CFG_USE_MSMQ_SERVER]) == false &&
			        Convert.ToBoolean(ConfigurationManager.AppSettings[CFG_USE_MSMQ_SERVER]))
			    {
			        _msmqServer = new MsmqCentralServiceServer();
			        _msmqServer.Init();
			        _msmqServer.Start();
			    }

			    //Start device scanning
                mScanningTask = new DeviceScanningTask(this);
                foreach (Device device in mDeviceDict.Values)
                {
                    try
                    {
                        mScanningTask.AddDeviceToScanning(device);
                    }
                    catch (ScanningSetupException exc)
                    {
                        LogException("CentralServerServiceImpl", String.Format(CultureInfo.InvariantCulture, Properties.Resources.ErrorSettingUpDeviceScanning, device.Id), exc);
                    }
                }
			}
			catch (System.Exception ex)
			{
				LogException("CentralServerServiceImpl", Properties.Resources.ErrorInitializingCentralService, ex);
				throw;
			}
		}

        void ValueManager_DataWasWritten(object sender, EventArgs e)
        {
            if (StoringIsDone != null)
            {
                StoringIsDone(this, new EventArgs());
            }
        }

#if DEBUG
		/// <summary>
		/// Cancels the service. Just for test purposes -> after canceling waiting is done for the two tasks
		/// </summary>
		public void CancelService()
		{
            //Stop scanning sensors
            if (mScanningTask != null)
            {
                mScanningTask.Stop();
            }

            //Stop the CNDEP server
            if (mCndepServer != null)
            {
                mCndepServer.Stop();
            }

            //Stop the MSMQ server
            if (_msmqServer != null)
            {
                _msmqServer.Stop();
            }

            //Stop storing sensor data to the database
            ValueManager.Instance.StopWriteToStorageThread();
            AutoResetEvent stopEventCalculation = ValueManager.Instance.StopCalculatingVirtualValues();
            

			// wait for the tasks to finish
			// WaitHandle.WaitAll(new WaitHandle[] { mStoreTaskSync, mVirtualValueCalculationTaskSync });

            //Stop calculation virtual sensor values
			//mStoreTaskSync.WaitOne();
            stopEventCalculation.WaitOne();
		}
#endif

        #region ICentralServerService members...

        #region Registration of sensors, devices, triggers, sinks...
        /// <summary>
        /// Registers the device. If already present the entry is ignored
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns>the call result</returns>
        public OperationResult RegisterDevice(Device device)
        {
            if (device == null)
            {
                log.Error(Properties.Resources.ErrorDeviceIsNull);
                throw new ArgumentNullException("device");
            }

            OperationResult result = new OperationResult();

            if (IsValidIdentifier(device.Id) == false)
            {
                string message = string.Format(Properties.Resources.ErrorIdentifierIsInvalid, device.Id);
                log.Error(message);
                result.AddError(message);

                return (result);
            }

            try
            {
                // ignore any existing device				
                if (IsDeviceIdUsed(device.Id) == false)
                {
                    DataStorageAccessBase.Instance.StoreDevice(device);

                    // already add the sensor dictionary entry 
                    mDeviceSensorDict.Add(device.Id, new Dictionary<string, Sensor>());
                    mDeviceDict[device.Id] = device;

                    //Start scanning sensors of the device
                    mScanningTask.AddDeviceToScanning(device);
                }
                else
                {
                    result.AddError(string.Format(Properties.Resources.ErrorDeviceAlreadyRegistered, device.Id));
                }
            }
            catch (System.Exception ex)
            {
                LogException("RegisterDevice", string.Format(Properties.Resources.ErrorRegisteringDevice, device.Id, ex.Message), ex);
                result.AddError(string.Format(Properties.Resources.ErrorRegisteringDevice, device.Id, ex.Message));
            }

            return (result);
        }


        /// <summary>
        /// Updates the device.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <returns>the call result</returns>
        public OperationResult UpdateDevice(Device device)
        {
            if (device == null)
            {
                log.Error(Properties.Resources.ErrorDeviceIsNull);
                throw new ArgumentNullException("device");
            }

            if (IsValidIdentifier(device.Id) == false)
            {
                string message = string.Format(Properties.Resources.ErrorIdentifierIsInvalid, device.Id);
                log.Error(message);
                return (new OperationResult(message));
            }

            OperationResult result = new OperationResult();

            try
            {
                if (IsDeviceIdUsed(device.Id))
                {
                    DataStorageAccessBase.Instance.UpdateDevice(device);
                    mDeviceDict[device.Id] = device;
                }
                else
                {
                    result.AddError(string.Format(Properties.Resources.ErrorDeviceNotRegistered, device.Id));
                }

            }
            catch (System.Exception ex)
            {
                LogException("UpdateDevice", string.Format(Properties.Resources.ErrorUpdatingDevice, device.Id, ex.Message), ex);
                result.AddError(string.Format(Properties.Resources.ErrorUpdatingDevice, device.Id, ex.Message));
            }

            return (result);
        }

        /// <summary>
        /// Registers the sensor list (1..n). If the sensor is already present the entry is ignored
        /// </summary>
        /// <param name="sensorList">The sensor list to register.</param>
        /// <returns></returns>
        public OperationResult RegisterSensors(IEnumerable<Sensor> sensorList)
        {
            OperationResult result = new OperationResult();

            if (sensorList == null)
            {
                log.Error(Properties.Resources.ErrorSensorListIsNull);
                throw new ArgumentNullException("sensorList");
            }

            // traverse all sensors
            foreach (Sensor s in sensorList)
            {
                if (s == null)
                {
                    log.Error(Properties.Resources.ErrorSensorListContainsNullElement);
                    throw new ArgumentNullException("sensorList[i]");
                }

                // if the device is not registered
                if (IsDeviceIdUsed(s.DeviceId) == false)
                {
                    string message = string.Format(Properties.Resources.ErrorDeviceNotRegistered, s.DeviceId);
                    log.Error(message);
                    result.AddError(message);
                    continue;
                }

                // or the id is not valid
                if (IsValidIdentifier(s.Id) == false)
                {
                    string message = string.Format(Properties.Resources.ErrorIdentifierIsInvalid, s.Id);
                    log.Error(message);
                    result.AddError(message);
                    continue;
                }

                try
                {
                    // if not present yet -> store, else update
                    if (IsSensorIdRegisteredForDevice(s.DeviceId, s.Id) == false)
                    {
                        //Testing the sensor data validity
                        ValueDefinition newValueDefinition = ValueManager.Instance.RegisterSensorAsValueDefinition(s);
                        ValueManager.Instance.RemoveValueDefinition(newValueDefinition);

                        DataStorageAccessBase.Instance.StoreSensor(s);

                        newValueDefinition = ValueManager.Instance.RegisterSensorAsValueDefinition(s);
                        if (s.IsActuator && !s.IsSynchronousPushToActuator)
                            newValueDefinition.ValueHasChanged += SendValueDefinitionValueToActuator;

                        // get a new copy so that changes in the passed in class do not affect our copy
                        mDeviceSensorDict[s.DeviceId][s.Id] = s;
                        mSensorDict[s.InternalSensorId] = s;

                        //Put the sensor to scanning
                        mScanningTask.PutSensorToScanning(s, s.PullFrequencyInSeconds * 1000);
                    }
                    else
                    {
                        result.AddError(string.Format(Properties.Resources.ErrorSensorAlreadyRegistered, s.Id, s.DeviceId));
                    }
                }
                catch (System.Exception ex)
                {
                    string message = string.Format(Properties.Resources.ErrorRegisteringSensor, s.Id, s.DeviceId, ex.Message);
                    LogException("RegisterSensor", message, ex);
                    result.AddError(message);

                    // no re-throw as we want to traverse the entire list before yielding a final result
                }
            }

            return (result);
        }

	    private void SendValueDefinitionValueToActuator(object sender, ValueChangedEventArgs e)
	    {
	        //Use the value from the value definition, it may be changed by a callback.
	        if (e.Definition.CurrentValue != null)
	        {
	            try
	            {
	                //Quick-and-dirty passing of parameters...
	                var sensor = e.Definition.SensorCreatedFrom;

                    //Check if the actuator call has failed during the last call and how many failures in a row
	                if (IsActuatorCallAllowed(sensor))
	                {
                        TrackingPoint.TrackingPoint.CreateTrackingPoint("Server: Actuator_Send",
	                                                                    String.Format("Device id: {0}; Sensor id: {1}", sensor.DeviceId,
	                                                                                    sensor.Id),
	                                                                    0,
	                                                                    e.Definition.CurrentValue.CorrelationId);

	                    ThreadPool.QueueUserWorkItem(SendDataToSensor, new object[] {sensor, e.Definition.CurrentValue});
	                }
	            }
	            catch (Exception exc)
	            {
	                log.Error("Sensor data sending initiation failed.", exc);
	            }
	        }
	    }


        /// <summary>
        /// The method checks if the actuator call can be performed.
        /// </summary>
        /// <param name="actuator"></param>
        /// <returns></returns>
        private bool IsActuatorCallAllowed(Sensor actuator)
        {
            Tuple<DateTime, int> entry;
            if (_logActuatorCalls.TryGetValue(actuator.InternalSensorId, out entry))
            {
                //Check if the retry call period since the last failed call has expired and the number of retries has not been exceeded.
                return DateTime.Now.Subtract(entry.Item1).CompareTo(_actuatorCallRetryInterval)  >= 0 && entry.Item2 <= _maxActuatorCallRetries;
            }

            _logActuatorCalls[actuator.InternalSensorId] = new Tuple<DateTime, int>(DateTime.MinValue, 0);
            return true;
        }

	    /// <summary>
        /// Updates the single sensor
        /// </summary>
        /// <param name="sensor">The sensor.</param>
        /// <returns>the call result</returns>
        public OperationResult UpdateSensor(Sensor sensor)
        {
            OperationResult result = new OperationResult();

            if (sensor == null)
            {
                log.Error(Properties.Resources.ErrorSensorIsNull);
                throw new ArgumentNullException("sensor");
            }

            // if the device is not registered
            if (IsDeviceIdUsed(sensor.DeviceId) == false)
            {
                string message = string.Format(Properties.Resources.ErrorDeviceNotRegistered, sensor.DeviceId);
                log.Error(message);
                result.AddError(message);
                return (result);
            }

            try
            {
                // if not present ignore but log an error
                if (IsSensorIdRegisteredForDevice(sensor.DeviceId, sensor.Id))
                {
                    sensor.InternalSensorId = mDeviceSensorDict[sensor.DeviceId][sensor.Id].InternalSensorId;
                    //Clean up the actuator call log
                    if (sensor.IsActuator)
                    {
                        _logActuatorCalls.Remove(sensor.InternalSensorId);
                    }

                    //Reregister with the Value Manager
                    var valueDefinition = ValueManager.Instance.RegisterSensorAsValueDefinition(sensor);
                    if (sensor.IsActuator && !sensor.IsSynchronousPushToActuator)
                        valueDefinition.ValueHasChanged += SendValueDefinitionValueToActuator;

                    //Store
                    DataStorageAccessBase.Instance.UpdateSensor(sensor);

                    mSensorDict[sensor.InternalSensorId] = sensor;
                    mDeviceSensorDict[sensor.DeviceId][sensor.Id] = sensor;

                    //Put the sensor to scanning
                    mScanningTask.PutSensorToScanning(sensor, sensor.PullFrequencyInSeconds * 1000);
                }
                else
                {
                    string message = string.Format(Properties.Resources.ErrorSensorIsNotRegistered, sensor.Id, sensor.DeviceId);
                    log.Error(message);
                    result.AddError(message);
                }
            }
            catch (System.Exception ex)
            {
                string message = string.Format(Properties.Resources.ErrorUpdatingSensor, sensor.Id, sensor.DeviceId, ex.Message);
                LogException("UpdateSensor", message, ex);
                result.AddError(message);
                // no re-throw 
            }

            return (result);
        }

        #region Data sinks...
        /*
        /// <summary>
        /// All registered data sinks are stored here.
        /// InternalSensorId => ArrayOfListOfDataSinks[DataSinkType]
        /// </summary>
        private readonly Dictionary<int, List<RegisteredDataSink>[]> mDataSinks = new Dictionary<int, List<RegisteredDataSink>[]>();

        public void RegisterDataSink(Sensor sensor, DataSinkType sinkType, EventHandler<DataSinkEventArgs> sinkHandler)
        {
            //Checks
            if (sensor == null)
            {
                throw new ArgumentNullException("sensor");
            }
            if (sinkHandler == null)
            {
                throw new ArgumentNullException("sinkHandler");
            }

            if (ValueManager.Instance.IsValueDefinitionPresent(sensor.InternalSensorId) == false)
            {
                throw new ArgumentException(String.Format(Properties.Resources.ErrorSensorIsNotRegistered, sensor.Id, sensor.DeviceId));
            }

            //Get or create the list
            List<RegisteredDataSink> list = GetDataSinkList(sensor.InternalSensorId, sinkType, true);
            //Add the new sink
            list.Add(new RegisteredDataSink() { SensorId = sensor.Id, SinkType = sinkType, Handler = sinkHandler });
        }

        private void ActivateDataSink(string deviceId, string sensorId, DataSinkType sinkType, SensorData data)
        {
            ActivateDataSink(mDeviceSensorDict[deviceId][sensorId].InternalSensorId, sinkType, data);
        }

        private bool ActivateDataSink(int internalSensorId, DataSinkType sinkType, SensorData data)
        {
            //Get the data sink list
            List<RegisteredDataSink> list = GetDataSinkList(internalSensorId, sinkType, false);

            if (list != null)
            {
                foreach (RegisteredDataSink sink in list)
                {
                    //Call the handler
                    try
                    {
                        //DataSinkEventArgs args = new DataSinkEventArgs(data, true, true);
                        DataSinkEventArgs args = new DataSinkEventArgs(data);
                        sink.Handler(this, args);

                        //If cancelled - notify the caller
                        if (args.Cancel)
                        {
                            return false;
                        }

                        // if a modify occurred then use the new value
                        if (args.HasValueChanged)
                        {
                            data.CopyData(args.ChangedValue);
                        }
                    }
                    catch (Exception exc)
                    {
                        log.LogException("ActivateDataSink", String.Format(Properties.Resources.ErrorActivatingDataSink, internalSensorId, sinkType), exc);
                        return false;
                    }
                }
            }

            return true;
        }

        private void ActivateDataSinks(DataSinkType sinkType, List<InternalSensorValue> values)
        {
            foreach (InternalSensorValue val in values)
            {
                var unsavedValues = val.GetValuesToSave();
                foreach (KeyValuePair<long, object> pair in unsavedValues)
                {
                    ActivateDataSink(val.DeviceId, val.SensorId, sinkType, new SensorData(pair.Key, Convert.ToString(pair.Value)));
                }
            }
        }

        private List<RegisteredDataSink> GetDataSinkList(int internalSensorId, DataSinkType sinkType, bool createIfNotExists)
        {
            //Get all sink lists for the sensor
            List<RegisteredDataSink>[] listSinks;
            if (mDataSinks.TryGetValue(internalSensorId, out listSinks) == false)
            {
                if (createIfNotExists)
                {
                    //Put a new list
                    listSinks = new List<RegisteredDataSink>[Enum.GetValues(typeof(DataSinkType)).Length];
                    mDataSinks[internalSensorId] = listSinks;
                }
                else
                {
                    //Not found
                    return null;
                }
            }

            //Get the sink list for the sink type
            int index = (int)sinkType;
            if (listSinks[index] == null)
            {
                if (createIfNotExists)
                {
                    listSinks[index] = new List<RegisteredDataSink>();
                }
                else
                {
                    //Not found
                    return null;
                }
            }

            return listSinks[index];
        }
         */
        #endregion
        #endregion

		#region Checks...

		/// <summary>
		/// Check if the device id is used already
		/// </summary>
		/// <param name="deviceId">the id to check</param>
		/// <returns><c>true</c> if yes, otherwise <c>false</c></returns>
		public bool IsDeviceIdUsed(string deviceId)
		{
            if (deviceId == null)
            {
                throw new ArgumentNullException("deviceId");
            }

			return (mDeviceDict.ContainsKey(deviceId));
		}

		/// <summary>
		/// determines if the sensor is registered for the given device
		/// </summary>
		/// <param name="deviceId">the device id</param>
		/// <param name="sensorId">the sensor id</param>
		/// <returns><c>true</c> if the sensor is registered for the device, otherwise <c>false</c></returns>
		public bool IsSensorIdRegisteredForDevice(string deviceId, string sensorId)
		{
            //Checks
            if (deviceId == null)
            {
                throw new ArgumentNullException("deviceId");
            }

            if (sensorId == null)
            {
                throw new ArgumentNullException("sensorId");
            }

            Dictionary<string, Sensor> sensorsDict;
			return  
                mDeviceSensorDict.TryGetValue(deviceId, out sensorsDict) && 
                sensorsDict.ContainsKey(sensorId);
		}

		#endregion

        #region Sensors, devices, sensor data...
        /// <summary>
        /// Stores the device data. If a device or a sensor are not registered they will be registered preliminary and can later be updated by a subsequent call to <see cref="RegisterDevice"/> or <see cref="RegisterSensor"/>
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        /// <param name="data">The data.</param>
        /// <returns>the call result</returns>
        public StoreSensorDataResult StoreSensorData(string deviceId, List<MultipleSensorData> data)
        {
            //Check arguments
            if (data == null)
            {
                log.Error(Properties.Resources.ErrorSensorDataIsNull);
                throw new ArgumentNullException("data");
            }

            if (deviceId == null)
            {
                log.Error(Properties.Resources.ErrorDeviceIsNull);
                throw new ArgumentNullException("deviceId");
            }

#if MONITOR_PERFORMANCE
            long methodStartTimestamp = PerformanceMonitoring.IncrementRequestMade(PerfMonCategoryMethods, PerfMonStoreMultipleSensorData);
#endif
                StoreSensorDataResult result = new StoreSensorDataResult();
                //Will overwritn later if fails
                result.Success = true;

                //Check if the device is valid
                if (IsDeviceIdUsed(deviceId) == false)
                {
                    string message = string.Format(Properties.Resources.ErrorDeviceNotRegistered, deviceId);
                    log.Error(message);
                    result.AddError(message);
                }
                else
                {
                    try
                    {
                        // filter out the entries which can be inserted because the sensor is registered for the device
                        List<MultipleSensorData> storeList = (from entry in data where IsSensorIdRegisteredForDevice(deviceId, entry.SensorId) select entry).ToList();

                        // TODO: Sinks for "after value arrived"

                        foreach (MultipleSensorData entry in storeList)
                        {
                            StoreSensorData(deviceId, entry, result);
                        }

                        // TODO: "before virtual value computed"
                        //ActivateDataSink(, DataSinkType.BeforeVirtualValueComputed, );

                        // TODO: "after virtual value computed"
                        //ActivateDataSink(, DataSinkType.AfterVirtualValueComputed, );

                        // for the remaining ones create error messages
                        List<MultipleSensorData> noStoreList = (from entry in data where IsSensorIdRegisteredForDevice(deviceId, entry.SensorId) == false select entry).ToList();
                        noStoreList.ForEach(entry => result.AddError(string.Format(Properties.Resources.ErrorStoringDataForSensor, entry.SensorId, deviceId)));
#if MONITOR_PERFORMANCE
                        PerformanceMonitoring.IncrementRequestsCompleted(PerfMonCategoryMethods, PerfMonStoreMultipleSensorData, methodStartTimestamp);
#endif
                    }
                    catch (System.Exception ex)
                    {
                        LogException("StoreSensorData", Properties.Resources.ErrorStoringSensorData, ex);
                        throw;
                    }
                }

                return (result);
        }

        public StoreSensorDataResult StoreSensorData(string deviceId, MultipleSensorData data)
        {
            //Check arguments
            if (data == null)
            {
                log.Error(Properties.Resources.ErrorSensorDataIsNull);
                throw new ArgumentNullException("data");
            }

            if (deviceId == null)
            {
                log.Error(Properties.Resources.ErrorDeviceIsNull);
                throw new ArgumentNullException("deviceId");
            }

#if MONITOR_PERFORMANCE
            long methodStartTimestamp = PerformanceMonitoring.IncrementRequestMade(PerfMonCategoryMethods, PerfMonStoreSingleSensorData);
#endif
                StoreSensorDataResult result = new StoreSensorDataResult();

                //Check if the device is valid
                if (IsDeviceIdUsed(deviceId) == false)
                {
                    string message = string.Format(Properties.Resources.ErrorDeviceNotRegistered, deviceId);
                    log.Error(message);
                    result.AddError(message);
                }
                else
                {
                    try
                    {
                        //Check if the sensor is ok
                        if (IsSensorIdRegisteredForDevice(deviceId, data.SensorId))
                        {
                            //Store
                            StoreSensorData(deviceId, data, result);
                            //Ok, success
                            result.Success = true;

#if MONITOR_PERFORMANCE
                            PerformanceMonitoring.IncrementRequestsCompleted(PerfMonCategoryMethods, PerfMonStoreSingleSensorData, methodStartTimestamp);
#endif
                        }
                        else
                        {
                            // for the remaining ones create error messages
                            result.AddError(string.Format(Properties.Resources.ErrorStoringDataForSensor, data.SensorId, deviceId));
                        }
                    }
                    catch (System.Exception ex)
                    {
                        LogException("StoreSensorData", Properties.Resources.ErrorStoringSensorData, ex);
                        throw;
                    }
                }

                return (result);
        }

        private void StoreSensorData(string deviceId, MultipleSensorData data, StoreSensorDataResult result)
        {
            // TODO: Sinks for "after value arrived"

            Sensor s = mDeviceSensorDict[deviceId][data.SensorId];
            //Pass the value to the internal store
            if (ValueManager.Instance.IsValueDefinitionPresent(s.InternalSensorId))
            {
                IEnumerable<SensorData> sortedMeasures = data.Measures.OrderBy(x => x.GeneratedWhen);
                          
                foreach (SensorData sd in sortedMeasures)
                {
                    string errorMessage;

                    //Store
                    StoreSensorData(deviceId, s, s.InternalSensorId, sd, out errorMessage);

                    //Check the result
                    if (errorMessage != null)
                    {
                        result.AddError(errorMessage);
                    }
                }

                //Do we have an adjusted sampling rate?
                TimeSpan adjustedSamplingRate;               
                if (ValueManager.Instance.AdjustedSamplingRates.TryGetValue(s.InternalSensorId, out adjustedSamplingRate))
                {
                    //Keep the adjusted rate
                    result.AddAdjustedSamplingRate(data.SensorId, (Int32)adjustedSamplingRate.TotalMilliseconds);
                    //Clean up
                    ValueManager.Instance.AdjustedSamplingRates.Remove(s.InternalSensorId);
                }
            }
            else
            {
                //No such value definition registered
                result.AddErrorFormat(Properties.Resources.ErrorNoValueDefinitionForSensor, s.DeviceId, s.Id);
            }
        }

	    private void StoreSensorData(string deviceId, Sensor sensor, Int32 internalSensorId, SensorData data, out string errorMessage)
	    {
	        //Pass the value to the internal store
	        if (data != null)
	        {
	            try
	            {
	                //Activate the data sink for the new arrived value
	                //ActivateDataSink(s.DeviceId, s.Id, DataSinkType.AfterValueArrived, sd);
	                SensorData passedData = new SensorData(data.GeneratedWhen, data.Value, data.CorrelationId);

	                //We do not accept setting values for virtual value definitions
	                ValueDefinition valueDef = ValueManager.Instance[sensor.InternalSensorId];
	                //if (valueDef.IsVirtualValue == false)
	                    valueDef.CurrentValue = passedData;

	                if (valueDef.CurrentValue != null)
	                {
                        //Tracking point for receiving sensor value
	                    TrackingPoint.TrackingPoint.CreateTrackingPoint("Server: Sensor_Receive", String.Format("Device id: {0}; Sensor id: {1}", sensor.DeviceId, sensor.Id), 0, valueDef.CurrentValue.CorrelationId);

	                    //Push the data to the sensor, if it allows it.
	                    if (sensor.IsActuator && sensor.IsSynchronousPushToActuator)
	                    {
	                        //Use the value from the value definition, it may be changed by a callback.
	                        try
	                        {
	                            //Quick-and-dirty passing of parameters...
	                            TrackingPoint.TrackingPoint.CreateTrackingPoint("Server: Actuator_Send", String.Format("Device id: {0}; Sensor id: {1}",sensor.DeviceId, sensor.Id), 0, valueDef.CurrentValue.CorrelationId);

                                SendDataToSensor(new object[] { sensor, valueDef.CurrentValue });
	                        }
	                        catch (Exception exc)
	                        {
	                            log.Error("Sensor data sending initiation failed.", exc);
	                        }
	                    }
	                }

	                errorMessage = null;
	            }
	            catch (System.Exception ex)
	            {
	                errorMessage = String.Format(Properties.Resources.ErrorStoringData, data.Value, sensor.Id, sensor.DeviceId, ex);
	            }
	        }
	        else
	        {
	            errorMessage = String.Format(Properties.Resources.ErrorNullSensorDataPassedInRequest, sensor.DeviceId, sensor.Id);
	        }
	    }

	    /// <summary>
		/// Gets the devices.
		/// </summary>
		/// <returns>the call result</returns>
		public GetDevicesResult GetDevices()
		{
			return (new GetDevicesResult(true, null, mDeviceDict.Values.ToList()));
		}

        /// <summary>
        /// Gets the devices.
        /// </summary>
        /// <returns>the call result</returns>
        public GetDevicesResult GetDevices(IEnumerable<String> deviceIds)
        {
            List<Device> devices;
            List<string> errors = new List<string>();
            Device device;

            if (deviceIds == null)
            {
                devices = mDeviceDict.Values.ToList();
            }
            else
            {
                devices = new List<Device>();

                foreach (string id in deviceIds)
                {
                    if (mDeviceDict.TryGetValue(id, out device))
                    {
                        devices.Add(device);
                    }
                    else
                    {
                        errors.Add(String.Format(Properties.Resources.ErrorDeviceNotRegistered, id));
                    }
                }
            }

            return (new GetDevicesResult(errors.Count() == 0, errors.Count() == 0 ? null : errors, devices));
        }

		/// <summary>
		/// Gets the sensors with the specified id for the specified device.
		/// </summary>
		/// <param name="deviceId">The device id.</param>
		/// <param name="sensorId">The sensor id.</param>
		/// <returns>the call result</returns>
		public GetSensorResult GetSensor(string deviceId, string sensorId)
		{
            if (IsSensorIdRegisteredForDevice(deviceId, sensorId))
            {
                return (new GetSensorResult(true, null, mDeviceSensorDict[deviceId][sensorId]));
            }
            else
            {
                List<string> errorMessages = new List<string>();
                errorMessages.Add(String.Format(Properties.Resources.ErrorSensorNotRegisteredForDevice, deviceId, sensorId));
                return (new GetSensorResult(false, errorMessages , null));
            }
		}

		/// <summary>
		/// Gets the sensors.
		/// </summary>
		/// <param name="deviceId">The device id.</param>
		/// <returns>the call result</returns>
		public GetSensorsForDeviceResult GetSensorsForDevice(string deviceId)
		{
            if (IsDeviceIdUsed(deviceId))
            {
                return (new GetSensorsForDeviceResult(true, null, mDeviceSensorDict[deviceId].Values.ToList()));
            }
            else
            {
                List<string> errorMessages = new List<string>();
                errorMessages.Add(String.Format(Properties.Resources.ErrorDeviceNotRegistered, deviceId));
                return (new GetSensorsForDeviceResult(false, errorMessages, null));
            }
		}

		/// <summary>
		/// Gets the sensor data.
		/// </summary>
		/// <param name="deviceId">The device id.</param>
		/// <param name="sensorIdList">The sensor id list.</param>
		/// <param name="dataStartDateTime">The data start date time.</param>
		/// <param name="dataEndDateTime">The data end date time.</param>
		/// <param name="maxResults">The max results.</param>
		/// <returns>the sensor data for the period requested</returns>
        public GetMultipleSensorDataResult GetSensorData(string deviceId, List<string> sensorIdList, DateTime dataStartDateTime, DateTime dataEndDateTime, int maxResults)
        {
            //Checks
            if (deviceId == null)
            {
                throw new ArgumentNullException("deviceId");
            }

            if (sensorIdList == null || sensorIdList.Count == 0)
            {
                throw new ArgumentNullException("sensorIdList");
            }

            if (dataStartDateTime > dataEndDateTime)
            {
                throw new ArgumentOutOfRangeException("dataEndDateTime");
            }

            if (maxResults < 0)
            {
                throw new ArgumentOutOfRangeException("maxResults");
            }

#if MONITOR_PERFORMANCE
            long methodStartTimestamp = PerformanceMonitoring.IncrementRequestMade(PerfMonCategoryMethods, PerfMonGetSensorData);
#endif
            GetMultipleSensorDataResult result = new GetMultipleSensorDataResult();

            //Check if the device is used
            if (IsDeviceIdUsed(deviceId) == false)
            {
                result.Success = false;
                result.AddError(String.Format(Properties.Resources.ErrorDeviceNotRegistered, deviceId));
                return result;
            }

            //Find all internal ids corresponding to the passed sensor ids.
            List<int> sensorInternalIdList = new List<int>();
            Dictionary<string, Sensor> deviceSensors = mDeviceSensorDict[deviceId];
            foreach (string sensorId in sensorIdList)
            {
                Sensor sensor;
                if (deviceSensors.TryGetValue(sensorId, out sensor))
                    sensorInternalIdList.Add(sensor.InternalSensorId);
            }

            try
            {
                result.SensorDataList = DataStorageAccessBase.Instance.GetSensorData(sensorInternalIdList, dataStartDateTime, dataEndDateTime, maxResults);
                result.Success = true;

#if MONITOR_PERFORMANCE
                PerformanceMonitoring.IncrementRequestsCompleted(PerfMonCategoryMethods, PerfMonGetSensorData, methodStartTimestamp);
#endif

            }
            catch (System.Exception ex)
            {
                StringBuilder sensorListBuilder = new StringBuilder();
                sensorIdList.ForEach(id => sensorListBuilder.Append(id + ", "));

                String message = string.Format(Properties.Resources.ErrorGetSensorData,
                        deviceId,
                        sensorListBuilder.ToString(),
                        dataStartDateTime.ToString(),
                        dataEndDateTime.ToString(),
                        maxResults,
                        ex.Message);

                result.Success = false;
                result.ErrorMessages = message;

                LogException("GetSensorData", message, ex);
            }

            return result;
        }

        /// <summary>
        /// Gets the latest sensor data.
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        /// <param name="sensorIdList">The sensor id list.</param>
        /// <param name="maxResults">The max results.</param>
        /// <returns>the sensor data for the period requested</returns>
        public GetMultipleSensorDataResult GetSensorDataLatest(string deviceId, List<string> sensorIdList, int maxResults)
        {
            //Checks
            if (deviceId == null)
            {
                throw new ArgumentNullException("deviceId");
            }

            if (sensorIdList == null)
            {
                throw new ArgumentNullException("sensorIdList");
            }

            if (maxResults < 0)
            {
                throw new ArgumentOutOfRangeException("maxResults");
            }

#if MONITOR_PERFORMANCE
            long methodStartTimestamp = PerformanceMonitoring.IncrementRequestMade(PerfMonCategoryMethods, PerfMonGetSensorDataLatest);
#endif
            GetMultipleSensorDataResult result = new GetMultipleSensorDataResult();
            result.Success = true;
            result.SensorDataList = new List<MultipleSensorData>();

            //Check if the device is used
            if (IsDeviceIdUsed(deviceId) == false)
            {
                result.Success = false;
                result.AddError(String.Format(Properties.Resources.ErrorDeviceNotRegistered, deviceId));
                return result;
            }

            //If no sensor id provided - get them all
            if (sensorIdList.Count == 0)
            {
                mDeviceSensorDict[deviceId].Values.ToList().ForEach(x => sensorIdList.Add(x.Id));
            }

            //Collect historical values that are in the cache already
            foreach (string nextSensorId in sensorIdList)
            {
                if (IsSensorIdRegisteredForDevice(deviceId, nextSensorId))
                {
                    ValueDefinition nextValue = ValueManager.Instance[mDeviceSensorDict[deviceId][nextSensorId].InternalSensorId].CopyData();

                    //Get data from the cache
                    MultipleSensorData nextSensorData = new MultipleSensorData();
                    nextSensorData.SensorId = nextSensorId;
                    SensorData[] latestCachedData;
                    SensorData[] historicalValues = nextValue.GetHistoricValues();
                    if (maxResults == 0)
                    {
                        latestCachedData = historicalValues.Reverse().ToArray();
                    }
                    else
                    {
                        if (maxResults > 1)
                        {
                            if (historicalValues.Length > (maxResults - 1))
                            {
                                //Take the last maxResults-1
                                latestCachedData = historicalValues.Skip(historicalValues.Length - maxResults + 1).Reverse().ToArray();
                            }
                            else
                            {
                                //Just take all
                                latestCachedData = historicalValues.Reverse().ToArray();
                            }
                        }
                        else
                        {
                            latestCachedData = new SensorData[0];
                        }
                    }
                    nextSensorData.Measures = new SensorData[1 + latestCachedData.Length];
                    nextSensorData.Measures[0] = nextValue.CurrentValue;
                    Array.Copy(latestCachedData, 0, nextSensorData.Measures, 1, latestCachedData.Length);

                    result.SensorDataList.Insert(0, nextSensorData);
                }
            }


#if MONITOR_PERFORMANCE
                PerformanceMonitoring.IncrementRequestsCompleted(PerfMonCategoryMethods, PerfMonGetSensorDataLatest, methodStartTimestamp);
#endif
            return result;
        }

        /// <summary>
        /// The method adds a dependency between sensors. The dependent sensor must be a virtual one.
        /// </summary>
        /// <param name="baseSensorInternalId"></param>
        /// <param name="dependentSensorInternalId"></param>
        /// <exception cref="ArgumentException">The exception thrown if the dependent sensor is not virtual, if creating this dependency will create a circular reference</exception>
        public void AddSensorDependency(int baseSensorInternalId, int dependentSensorInternalId)
        {
            //Checks
            //Base exists?
            if (!ValueManager.Instance.IsValueDefinitionPresent(baseSensorInternalId))
            {
                throw new ArgumentException(String.Format(Properties.Resources.ExceptionSensorNotFound, baseSensorInternalId), "baseSensorInternalId");
            }

            //Dependent exists?
            if (!ValueManager.Instance.IsValueDefinitionPresent(dependentSensorInternalId))
            {
                throw new ArgumentException(String.Format(Properties.Resources.ExceptionSensorNotFound, dependentSensorInternalId), "dependentSensorInternalId");
            }

            //Add the dependency to the runtime storage, it makes all the checks as well
            ValueManager.Instance.AddDependency(
                ValueManager.Instance.ValueDefinitionDictionary[baseSensorInternalId],
                ValueManager.Instance.ValueDefinitionDictionary[dependentSensorInternalId]);

            try
            {
                DataStorageAccessBase.Instance.AddSensorDependency(baseSensorInternalId, dependentSensorInternalId);
            }
            catch (Exception exc)
            {
                //Rollback
                //Remove from the runtime storage
                ValueManager.Instance.RemoveDependency(
                    ValueManager.Instance.ValueDefinitionDictionary[baseSensorInternalId],
                    ValueManager.Instance.ValueDefinitionDictionary[dependentSensorInternalId]
                    );

                throw new Exception(String.Format(Properties.Resources.ExceptionFailedAddingSensorDependency, baseSensorInternalId, dependentSensorInternalId), exc);
            }
        }

        /// <summary>
        /// The method removes an existing dependency between sensors.
        /// </summary>
        /// <param name="baseSensorInternalId"></param>
        /// <param name="dependentSensorInternalId"></param>
        public void RemoveSensorDependency(int baseSensorInternalId, int dependentSensorInternalId)
        {
            //Checks
            //Base exists?
            if (!ValueManager.Instance.IsValueDefinitionPresent(baseSensorInternalId))
            {
                throw new ArgumentException(String.Format(Properties.Resources.ExceptionSensorNotFound, baseSensorInternalId), "baseSensorInternalId");
            }

            //Dependent exists?
            if (!ValueManager.Instance.IsValueDefinitionPresent(dependentSensorInternalId))
            {
                throw new ArgumentException(String.Format(Properties.Resources.ExceptionSensorNotFound, dependentSensorInternalId), "dependentSensorInternalId");
            }

            //Remove from the persistent storage
            try
            {
                DataStorageAccessBase.Instance.RemoveSensorDependency(baseSensorInternalId, dependentSensorInternalId);
            }
            catch (Exception exc)
            {
                throw new Exception(String.Format(Properties.Resources.ExceptionFailedRemovingSensorDependency, baseSensorInternalId, dependentSensorInternalId), exc);
            }

            //Remove from the runtime storage
            ValueManager.Instance.RemoveDependency(
                ValueManager.Instance.ValueDefinitionDictionary[baseSensorInternalId],
                ValueManager.Instance.ValueDefinitionDictionary[dependentSensorInternalId]
                );
        }

        /// <summary>
        /// The method returns all base sensor dependencies for the dependent sensor with the passed id.
        /// </summary>
        /// <returns></returns>
        public List<Sensor> GetBaseSensorDependencies(int dependentSensorInternalId)
        {
            //Checks
            //Dependent exists?
            if (!ValueManager.Instance.IsValueDefinitionPresent(dependentSensorInternalId))
            {
                throw new ArgumentException(String.Format(Properties.Resources.ExceptionSensorNotFound, dependentSensorInternalId), "dependentSensorInternalId");
            }

            return ValueManager.Instance.GetBaseValueDefinitions(ValueManager.Instance.ValueDefinitionDictionary[dependentSensorInternalId]).Select(x => x.SensorCreatedFrom).ToList();
        }

        /// <summary>
        /// The method returns all dependent sensor dependencies for the base sensor with the passed id.
        /// </summary>
        /// <returns></returns>
        public List<Sensor> GetDependentSensorDependencies(int baseSensorInternalId)
        {
            //Checks
            //Base exists?
            if (!ValueManager.Instance.IsValueDefinitionPresent(baseSensorInternalId))
            {
                throw new ArgumentException(String.Format(Properties.Resources.ExceptionSensorNotFound, baseSensorInternalId), "baseSensorInternalId");
            }

            return ValueManager.Instance.GetDependentValueDefinitions(ValueManager.Instance.ValueDefinitionDictionary[baseSensorInternalId]).Select(x => x.SensorCreatedFrom).ToList();
        }
        #endregion

        #endregion

        internal Device GetDevice(string deviceId)
        {
            return mDeviceDict[deviceId];
        }

        internal Sensor GetSensor(Int32 sensorInternalId)
        {
            return mSensorDict[sensorInternalId];
        }

	    private void SendDataToSensor(object state)
	    {
            //Quick-and-dirty passing of parameters...
	        object[] parameters = (object[])state;
	        Sensor sensor = (Sensor) parameters[0];
	        SensorData data = (SensorData) parameters[1];

	        //Get the device
	        Device device = GetDevice(sensor.DeviceId);

	        //Get the device communication handler
	        IDeviceCommunicationHandler handler;
	        try
	        {
	            //Check for undefined
	            if (sensor.PushModeCommunicationType == PullModeCommunicationType.Undefined)
	            {
	                handler = null;
	            }
	            else
	            {
	                handler = DeviceCommunicationHandlerFactory.GetDeviceCommunicationHandler(sensor.PushModeCommunicationType.ToString());
	            }
	        }
	        catch (Exception exc)
	        {
	            log.Error(String.Format(Properties.Resources.ErrorFailedGettingCommunicationHandlerForSensor, sensor.DeviceId, sensor.Id), exc);
	            return;
	        }

	        //Proceed if the hander is defined
	        if (handler == null)
	        {
	            log.Info(String.Format("SENDING DATA TO SENSOR: {0}===>{1}. DEVICE COMMUNICATION HANDLER IS UNDEFINED ", device.Id, sensor.Id));
	        }
	        else
	        {
	            try
	            {
	                log.Info(String.Format("SENDING DATA TO SENSOR: {0}===>{1}", device.Id, sensor.Id));

	                OperationResult result = handler.PutSensorCurrentData(device, sensor, data);
	                if (result.Success == false)
	                {
	                    MarkActuatorCallFailure(sensor);

	                    log.Error(String.Format(Properties.Resources.ErrorFailedSettingSensorDataWithDetails, sensor.DeviceId, sensor.Id,
	                                            data.Value, result.ErrorMessages));
	                }
	                else
	                {
                        //Clear the log entry
	                    ClearActuatorCallFailureEntry(sensor);
	                }
	            }
	            catch (Exception exc)
	            {
                    MarkActuatorCallFailure(sensor);

	                log.Error(String.Format(Properties.Resources.ErrorFailedSettingSensorData, sensor.DeviceId, sensor.Id), exc);
	            }
	        }
	    }

        private void MarkActuatorCallFailure(Sensor actuator)
        {
            Tuple<DateTime, int> entry;
            if (_logActuatorCalls.TryGetValue(actuator.InternalSensorId, out entry))
            {
                _logActuatorCalls[actuator.InternalSensorId] = new Tuple<DateTime, int>(DateTime.Now, entry.Item2 + 1);
            }
        }

        private void ClearActuatorCallFailureEntry(Sensor actuator)
        {
            _logActuatorCalls[actuator.InternalSensorId] = new Tuple<DateTime, int>(DateTime.MinValue, 0);
        }

        /// <summary>
        /// Return the next possible correlation id
        /// </summary>
        /// <returns>the next id generated by a central instance which will be always unique</returns>
        public GetCorrelationIdResult GetNextCorrelationId()
        {
        
#if MONITOR_PERFORMANCE
            long methodStartTimestamp = PerformanceMonitoring.IncrementRequestMade(PerfMonCategoryMethods, PerfMonGetNextCorrelationId);
#endif
            GetCorrelationIdResult result = new GetCorrelationIdResult();
            result.Success = true;
            result.CorrelationId = DataStorageAccessBase.Instance.GetCorrelationId();

#if MONITOR_PERFORMANCE
            PerformanceMonitoring.IncrementRequestsCompleted(PerfMonCategoryMethods, PerfMonGetNextCorrelationId, methodStartTimestamp);
#endif
            return result;
        }


	    #region Utility methods...

        /// <summary>
        /// Determines whether the identifier is valid and does not contain an invalid character sequence
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <returns>
        /// 	<c>true</c> if the identifier is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidIdentifier(string identifier)
        {
            return (identifier != null && identifier.Length > 0 && identifier.Contains(DeviceSensorSeparator) == false);
        }

        /// <summary>
        /// The method merges two lists containing MultipleSensorData entries.
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <param name="maxResultsPerSensor"></param>
        /// <returns></returns>
        private List<MultipleSensorData> MergeMultipleSensorDataLists(List<MultipleSensorData> list1, List<MultipleSensorData> list2, int maxResultsPerSensor)
        {
            List<MultipleSensorData> result = new List<MultipleSensorData>();
            result.AddRange(list1);
            Dictionary<string, MultipleSensorData> list1Dict = new Dictionary<string, MultipleSensorData>();
            list1.ForEach(x => list1Dict[x.SensorId] = x);
            MultipleSensorData nextList1Data;
            foreach (MultipleSensorData nextList2Data in list2)
            {
                if (list1Dict.TryGetValue(nextList2Data.SensorId, out nextList1Data))
                {
                    nextList1Data.Measures = MergeSensorDataArrays(nextList1Data.Measures, nextList2Data.Measures, maxResultsPerSensor);
                }
                else
                {
                    result.Add(nextList2Data);
                }
            }

            return result;
        }

        /// <summary>
        /// The method merges two SensorData arrays. The arrays are supposed to contains sorted (by the GeneratedWhen property values, descending)
        /// entries. The method merges the two arrays, so that the elements in the resulting array are still sorted in descending order.
        /// </summary>
        /// <param name="cachedData">Not null</param>
        /// <param name="retrievedData">Not null</param>
        /// <param name="maxResultsPerSensor"></param>
        /// <returns></returns>
        private SensorData[] MergeSensorDataArrays(SensorData[] cachedData, SensorData[] retrievedData, int maxResultsPerSensor)
        {
            if (retrievedData.Length == 0){
                return cachedData;
            }

            List<SensorData> resultRaw = new List<SensorData>();
            for (int i = 0; i < cachedData.Length; i++)
            {
                if (cachedData[i] != null && 
                    cachedData[i].GeneratedWhen > retrievedData[0].GeneratedWhen && 
                    (cachedData[i].GeneratedWhen.Ticks - retrievedData[0].GeneratedWhen.Ticks > 10000))
                {
                    resultRaw.Add(cachedData[i]);
                }
                else
                {
                    resultRaw.AddRange(retrievedData);
                    break;
                }
            }

            //Check if the second array is still to be added
            if (resultRaw.Count == cachedData.Length)
                resultRaw.AddRange(retrievedData);

            //Truncate the array if it is too long
            if (resultRaw.Count > maxResultsPerSensor)
                resultRaw.RemoveRange(maxResultsPerSensor, resultRaw.Count-maxResultsPerSensor);
            
            return resultRaw.ToArray();
        }

        /// <summary>
		/// Get the lease object
		/// </summary>
		/// <returns>
		/// <c>null</c> to indicate that the object never expires
		/// </returns>
		public override object InitializeLifetimeService()
		{
			return (null);
		}

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="message">The message.</param>
        /// <param name="exc">The exc.</param>
        private void LogException(string methodName, string message, Exception exc)
        {
            // log the information
            log.ErrorFormat(Properties.Resources.ErrorExceptionOccured, methodName, message, exc.Message, exc.StackTrace);
            if (exc.InnerException != null)
            {
                LogException(methodName, message, exc.InnerException);
            }
        }
        #endregion
	}
}
