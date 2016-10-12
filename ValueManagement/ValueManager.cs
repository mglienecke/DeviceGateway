using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using ValueManagement.DynamicCallback;
using log4net;
using DataStorageAccess;
using GlobalDataContracts;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections;
using System.Configuration;
using Common.Server;

namespace ValueManagement
{
	/// <summary>
	/// The class singleton manages all value definitions and the callbacks that are associated with them. 
	/// 
	/// TODO: For clustered operation any C(R)UD operation must be propagated to all other nodes accordingly
	/// </summary>
	public class ValueManager
	{
        /// <summary>
        /// the time the thread sleeps between iterations
        /// </summary>
        public const int ThreadSleepTime = 250;

        public const string CfgDatabaseConnectionForCallbacksParameters = "ConnectionForCallbacksParameters";

		#region Local variables...

		private static log4net.ILog log = log4net.LogManager.GetLogger(typeof(ValueManager));

        /// <summary>
		/// the dictionary for all call backs
		/// </summary>
		private readonly Dictionary<string, AbstractCallback> mCallbackDict = new Dictionary<string, AbstractCallback>();

		/// <summary>
		/// the dictionary for all value definitions
		/// </summary>
        private readonly Dictionary<int, ValueDefinition> mValueDefinitionDict = new Dictionary<int, ValueDefinition>();

		/// <summary>
		/// dictionary which lists all dependencies a virtual value has in the form virtual value x is dependent on A, Z, U, ...
		/// </summary>
        private readonly Dictionary<ValueDefinition, HashSet<ValueDefinition>> mDependsOnDictionary = new Dictionary<ValueDefinition, HashSet<ValueDefinition>>();

		/// <summary>
		/// dictionary which lists all dependent virtual values of a value (so where the value is the base of the definition of the virtual value) in the form value A is the base of virtual value Z, U, ....
		/// </summary>
        private readonly Dictionary<ValueDefinition, HashSet<ValueDefinition>> mBaseOfDictionary = new Dictionary<ValueDefinition, HashSet<ValueDefinition>>();

		/// <summary>
		/// a dictionary with all unsaved values in it
		/// </summary>
        private static readonly Dictionary<ValueDefinition, List<SensorData>> mUnsavedValueDict = new Dictionary<ValueDefinition, List<SensorData>>();

		/// <summary>
		/// the dictionary to store the associated call backs with value definitions
		/// </summary>
        private readonly Dictionary<ValueDefinition, List<string>> mValueCallbackDictionary = new Dictionary<ValueDefinition, List<string>>();

        /// <summary>
        /// the dictionary to store adjusted sampling rates
        /// </summary>
        private readonly Dictionary<int, TimeSpan> mValueDefinitionSamplingRates = new Dictionary<int,TimeSpan>();

        /// <summary>
        /// the instance of the ValueManager
        /// </summary>
        private static ValueManager mInstance;
        private static Object mLock = new Object();
        /// <summary>
        /// the token source for cancellations
        /// </summary>
        private CancellationTokenSource mCancelTokenCalculation;
        /// <summary>
        /// the synchronization object for the virtual value calculation task
        /// </summary>
        private AutoResetEvent mVirtualValueCalculationTaskSync = new AutoResetEvent(false);
        /// <summary>
        /// the synchronization object for the storage task
        /// </summary>
        private AutoResetEvent mStoreTaskSync = new AutoResetEvent(false);
		#endregion

#if MONITOR_PERFORMANCE
        /*
        private const string PerfMonCentralServerServiceDataStoringCategory = "CentralServerService data storing";
        private const string PerfMonInstanceSensorDataStoringTask = "Data Storing task";
        private const string PerfMonCounterStoredEntriesCount = "StoredEntriesCount";
        private const string PerfMonCounterOperationDuration = "OperationDuration";

        PerformanceCounter perfCounterStoredEntryCount;
        PerformanceCounter perfCounterStoredEntryDuration;
         */

        private const string PerfMonCategoryExperimentValueManager = "Experiment ValueManager";
        private const string PerfMonMethodWriteToStorage = "WriteToStorage";
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueManager"/> class. By hiding the constructor the ValueManager can only be instantiated via the singleton
        /// </summary>
        private ValueManager()
        {
#if MONITOR_PERFORMANCE
            SetupPerformanceMonitoring();
#endif
            WriteRetryCount = DefaultWriteRetryCount;
            DatabaseConnectionSettingsForCallbacks = ConfigurationManager.ConnectionStrings[CfgDatabaseConnectionForCallbacksParameters];
        }

#if MONITOR_PERFORMANCE
        private void SetupPerformanceMonitoring()
        {
            try
            {
                PerformanceMonitoring.SetupPerformanceCounterCategoryForMethods(PerfMonCategoryExperimentValueManager, String.Empty);
                PerformanceMonitoring.SetupPerformanceCounters(PerfMonCategoryExperimentValueManager, PerfMonMethodWriteToStorage);

                /*
                CounterCreationDataCollection counterData = new CounterCreationDataCollection();

                //Entries stored during the last cycle
                CounterCreationData perfCounterStoredEntryCountData = new CounterCreationData("CountEntriesStored",
                    "Count of data entries stored to the permanent data storage.", PerformanceCounterType.NumberOfItems32);
                counterData.Add(perfCounterStoredEntryCountData);

                //Time to store entries during the last cycle
                CounterCreationData perfCounterStoredEntryDurationData = new CounterCreationData("DurationEntriesStored",
                    "Duration of storing data entries to the permanent data storage.", PerformanceCounterType.NumberOfItems32);
                counterData.Add(perfCounterStoredEntryDurationData);

                //Create the category.
                PerformanceMonitoring.SetupPerformanceMonitoringCategory(PerfMonCentralServerServiceDataStoringCategory, "Category for counters related to the data storing task.",
                    counterData);

                perfCounterStoredEntryCount = new PerformanceCounter(PerfMonCentralServerServiceDataStoringCategory, PerfMonCounterStoredEntriesCount, PerfMonInstanceSensorDataStoringTask, false);
                perfCounterStoredEntryDuration = new PerformanceCounter(PerfMonCentralServerServiceDataStoringCategory, PerfMonCounterOperationDuration, PerfMonInstanceSensorDataStoringTask, false);
                 */
            }
            catch (Exception exc)
            {
                log.LogException("ValueManager", "Failed setting up performance counters", exc);
                throw;
            }
        }
#endif


        /// <summary>
        /// Gets the instance as a singleton
        /// </summary>
        /// <value>The instance.</value>
        public static ValueManager Instance
        {
            get
            {
                lock (mLock)
                {
                    if (mInstance == null)
                    {
                        mInstance = new ValueManager();
                    }

                    return (mInstance);
                }
            }
        }

		#region Properties...

		/// <summary>
		/// Defines the order in which call backs are called 
		/// </summary>
		public readonly CallbackType[] CallbackOrder = new CallbackType[] 
			{ 
				CallbackType.DetectAnomaly, 
				CallbackType.AdjustSampleRate, 
				CallbackType.GeneralCheck, 
				CallbackType.BeforeStore, 
				CallbackType.AfterStore
			};

		/// <summary>
		/// Phase 1 call backs
		/// </summary>
		public readonly CallbackType[] PhaseOne = new CallbackType[] 
			{ 
				CallbackType.DetectAnomaly, 
				CallbackType.AdjustSampleRate, 
				CallbackType.GeneralCheck
			};


		/// <summary>
		/// Phase 2 call backs
		/// </summary>
		public readonly CallbackType[] PhaseTwo = new CallbackType[] 
			{ 
				CallbackType.AfterChangeCallback
			};

		/// <summary>
		/// Gets the callback dictionary.
		/// </summary>
		/// <value>The callback dictionary.</value>
		public Dictionary<string, AbstractCallback> CallbackDictionary { get { return (mCallbackDict); } }

		/// <summary>
		/// Gets the value callback dictionary which defines which call backs are called for which value definition
		/// </summary>
		/// <value>The value callback dictionary.</value>
		public Dictionary<ValueDefinition, List<string>> ValueCallbackDictionary { get { return (mValueCallbackDictionary); } }

		/// <summary>
		/// Gets the value definition dictionary.
		/// </summary>
		/// <value>The value definition dictionary.</value>
		public Dictionary<int, ValueDefinition> ValueDefinitionDictionary { get { return (mValueDefinitionDict); } }

        /// <summary>
        /// The property getter returns a reference to the adjusted sampling rates dictionary.
        /// Mapping: ValueDefinition.InternalId -> TimeSpan
        /// </summary>
        public Dictionary<int, TimeSpan> AdjustedSamplingRates { get{return mValueDefinitionSamplingRates;} }

        /// <summary>
        /// The property contains internal database connection string for SQL callbacks.
        /// </summary>
        public ConnectionStringSettings DatabaseConnectionSettingsForCallbacks
        {
            get;
            set;
        }
		#endregion

		#region Callback for value definitions...

		/// <summary>
		/// Adds a callback for a value definition. The call back is registered in the call back dictionary (if not present) and then added for the definition itself. 
		/// 
		/// A call back can be added several times for the same definition
		/// </summary>
		/// <param name="definition">The definition.</param>
		/// <param name="callback">The callback.</param>
		public void AddCallbackForValueDefinition(ValueDefinition definition, AbstractCallback callback)
		{
            //Checks
			if (definition == null)
			{
				throw new ArgumentNullException("definition");
			}

			if (callback == null)
			{
				throw new ArgumentNullException("callback");
			}

			// register the callback, if not registered
            if (mCallbackDict.ContainsKey(callback.SymbolicName) == false)
            {
                lock (((ICollection)mCallbackDict).SyncRoot)
                {
                    if (mCallbackDict.ContainsKey(callback.SymbolicName) == false)
                    {
                        mCallbackDict.Add(callback.SymbolicName, callback);
                    }
                }
            }

            AddCallbackForValueDefinition(definition, callback.SymbolicName);
		}

        /// <summary>
        /// Adds a callback for a value definition. The call back is registered in the call back dictionary (if not present) and then added for the definition itself. 
        /// 
        /// A call back can be added several times for the same definition
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="callbackSymbolicName">The callback's name.</param>
        public void AddCallbackForValueDefinition(ValueDefinition definition, string callbackSymbolicName)
        {
            //Checks
            if (definition == null)
            {
                throw new ArgumentNullException("definition");
            }

            if (callbackSymbolicName == null)
            {
                throw new ArgumentNullException("callbackSymbolicName");
            }

            // set the callback for the definition register the value definition if needed
            List<string> nameList;
            if (mValueCallbackDictionary.TryGetValue(definition, out nameList) == false)
            {
                lock (((ICollection)mValueCallbackDictionary).SyncRoot)
                {
                    if (mValueCallbackDictionary.ContainsKey(definition) == false)
                    {
                        mValueCallbackDictionary.Add(definition, nameList = new List<string>());
                    }
                    else
                    {
                        nameList = mValueCallbackDictionary[definition];
                    }
                }
            }
            nameList.Add(callbackSymbolicName);

            // TODO: to store in a sorted fashion is problematic as external changes (circumventing the access methods) might cause havoc with the ordering. A solution would be to make the entries immutable
            // store as an ordered list already as this saves
            //ValueCallbackDictionary[definition] = CallbackDictionary
            //            .Where((KeyValuePair<string, AbstractCallback> x) => ValueCallbackDictionary[definition].Contains(x.Key))
            //            .Select((KeyValuePair<string, AbstractCallback> x) => x.Value)
            //            .OrderBy((AbstractCallback c) => c.CallbackType)
            //            .Select((AbstractCallback n) => n.SymbolicName)
            //            .ToList();
        }

		/// <summary>
		/// Removes the callback for a value definition and clears the dictionary entry if this was the last association
		/// </summary>
		/// <param name="definition">The definition.</param>
		/// <param name="callback">The callback.</param>
		public void RemoveCallbackForValueDefinition(ValueDefinition definition, AbstractCallback callback)
		{
            //Checks
			if (definition == null)
			{
				throw new ArgumentNullException("definition");
			}

			if (callback == null)
			{
				throw new ArgumentNullException("callback");
			}

			// only if the definition is present (otherwise the callback remains in the dict as it is)
            List<string> nameList;
			if (mValueCallbackDictionary.TryGetValue(definition, out nameList) == true)
			{
                // doesn't matter if found or not
                nameList.Remove(callback.SymbolicName);

				// if this was the last entry remove the slot as well
                if (nameList.Count == 0)
				{
                    lock (((ICollection)mValueCallbackDictionary).SyncRoot)
                    {
                        mValueCallbackDictionary.Remove(definition);
                    }
				}

				// the callback remains registered as such
			}
		}

		/// <summary>
		/// Removes the callback for value definition.
		/// </summary>
		/// <param name="definition">The definition.</param>
		/// <param name="callbackName">Name of the callback.</param>
		public void RemoveCallbackForValueDefinition(ValueDefinition definition, string callbackName)
		{
            //Checks
			if (definition == null)
			{
				throw new ArgumentNullException("definition");
			}

			if (callbackName == null)
			{
				throw new ArgumentNullException("callbackName");
			}

            //Find the callback
            AbstractCallback callback;
            if (mCallbackDict.TryGetValue(callbackName, out callback) == false)
			{
				throw new KeyNotFoundException(string.Format(Properties.Resources.ErrorCallbackNameNotFound, callbackName));
			}

			// call the original function
			RemoveCallbackForValueDefinition(definition, callback);
		}

        /// <summary>
        /// The method registers a callback. If a callback with its name already exists, it gets replaced.
        /// </summary>
        /// <param name="callback"></param>
        public void RegisterCallback(AbstractCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            lock (((ICollection)mCallbackDict).SyncRoot)
            {
                mCallbackDict[callback.SymbolicName] = callback;
            }
        }

		/// <summary>
		/// Gets the callback list for a value definition in a sorted fashion (according to the order specified in the global structure)
		/// </summary>
		/// <param name="definition">The definition.</param>
		/// <returns>a list (can be empty) of callbacks</returns>
		protected List<AbstractCallback> GetCallbackListForValueDefinition(ValueDefinition definition)
		{
			// the result is an empty list if the key is not included
			if (mValueCallbackDictionary.ContainsKey(definition) == false)
			{
				return (new List<AbstractCallback>());
			}

			// get the callback list
			List<string> callbackNamesList = mValueCallbackDictionary[definition];

			// get the associated callbacks for the list and order them according to the call order
            return (mCallbackDict
					.Where((KeyValuePair<string, AbstractCallback> x) => callbackNamesList.Contains(x.Key))
					.Select((KeyValuePair<string, AbstractCallback> x) => x.Value)
					.OrderBy((AbstractCallback c) => c.CallbackType)
					.ToList());
		}


		#endregion 
		
		#region ValueDefinition methods...

		/// <summary>
		/// provide a indexer with the key for the manager class
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ValueDefinition this[int key] { get { 
            return (mValueDefinitionDict[key]); } 
        }

		/// <summary>
		/// Adds the value definition.
		/// </summary>
		/// <param name="definition">The definition.</param>
		public void AddValueDefinition(ValueDefinition definition)
		{
            //Checks
            if (definition == null)
            {
                throw new ArgumentNullException("definition");
            }

            //Add if not added already
            if (mValueDefinitionDict.ContainsKey(definition.InternalId) == false)
            {
                lock (((ICollection)mValueDefinitionDict).SyncRoot)
                {
                    if (mValueDefinitionDict.ContainsKey(definition.InternalId) == false)
                    {
                        mValueDefinitionDict.Add(definition.InternalId, definition);

                        // register the handlers
                        definition.ValueHasChanged += ValueHasChangedHandler;
                        definition.ValueIsChanging += ValueIsChangingHandler;
                    }
                    else
                    {
                        log.ErrorFormat(CultureInfo.InvariantCulture, Properties.Resources.ErrorAddingValueDefinition, definition.InternalId);
                        throw new ArgumentException(String.Format(Properties.Resources.ExceptionValueDefinitionAlreadyRegistered, definition.InternalId));
                    }
                }
            }
            else
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, Properties.Resources.ErrorAddingValueDefinition, definition.InternalId);
                throw new ArgumentException(String.Format(Properties.Resources.ExceptionValueDefinitionAlreadyRegistered, definition.InternalId));
            }
		}

		/// <summary>
		/// Removes the value definition and any references towards it in case there are. Only first level references are removed (so A -> B, if B is removed then A -> null) as deeper levels are removed 
		/// indirectly due to removal of the dependency entry as such (so A -> B -> C, if B is removed it will be A -> null as B->C is removed from the dictionary as well)
		/// </summary>
		/// <param name="internalId">The internal id.</param>
		public void RemoveValueDefinition(ValueDefinition definition)
		{
			if (definition == null)
			{
				throw new ArgumentNullException("definition");
			}

			if (IsValueDefinitionPresent(definition.InternalId) == false)
			{
				throw new ArgumentOutOfRangeException(string.Format(Properties.Resources.InternalIdNotFound, definition.InternalId));
			}

            //Remove listeners
            definition.ValueIsChanging -= ValueIsChangingHandler;
            definition.ValueHasChanged -= ValueHasChangedHandler;

            // should any dependency be registered remove
            if (mDependsOnDictionary.ContainsKey(definition))
            {
                mDependsOnDictionary.Remove(definition);
            }

            // now check that all references to the base are removed as well 
            mDependsOnDictionary.Values.Where(valueSet => valueSet.Contains(definition)).All(set => set.Remove(definition));

            // should this be the base for any value remove it
            if (mBaseOfDictionary.ContainsKey(definition))
            {
                mBaseOfDictionary.Remove(definition);
            }

            //Remove from the dictionary
            lock (((ICollection)mValueDefinitionDict).SyncRoot)
            {
                mValueDefinitionDict.Remove(definition.InternalId);
            }

            //Remove associated callbacks
            if (mValueCallbackDictionary.ContainsKey(definition))
            {
                lock (((ICollection)mValueCallbackDictionary).SyncRoot)
                {
                    mValueCallbackDictionary.Remove(definition);
                }
            }
		}

        /// <summary>
        /// The method builds a value definition instance based on the passed sensor data and registers the definition.
        /// This registeration replaces the existing one (if any).
        /// </summary>
        /// <param name="sensor"></param>
        /// <returns>The created and registered definition</returns>
        public ValueDefinition RegisterSensorAsValueDefinition(Sensor sensor)
        {
            if (sensor == null)
            {
                throw new ArgumentNullException("sensor");
            }

            //Create callbacks if the sensor is virtual
            AbstractCallback callback = null;
            if (sensor.IsVirtualSensor && sensor.VirtualSensorDefinition != null)
            {
                try
                {
                    callback = CreateDynamicCallback(sensor.DeviceId + ";" + sensor.Id, sensor.VirtualSensorDefinition);
                }
                catch (ArgumentException exc)
                {
                    log.Error(String.Format(CultureInfo.InvariantCulture, Properties.Resources.ExceptionFailedCreatingCallbackForSensor, sensor.DeviceId, sensor.Id), exc);
                    throw new ValueDefinitionSetupException(String.Format(CultureInfo.InvariantCulture, Properties.Resources.ExceptionFailedCreatingCallbackForSensor, sensor.DeviceId, sensor.Id), exc);
                }
            }

            //Add sensor
            ValueDefinition valueDef = ValueManager.ConvertToValueDefinition(sensor);
            //Remove the definition in case it is already registered
            if (IsValueDefinitionPresent(valueDef.InternalId) == true)
            {
                RemoveValueDefinition(valueDef);
            }
            //Register the definition
            AddValueDefinition(valueDef);

            //Callback may be of the legal type but unhandled
            if (callback != null)
            {
                //Register the callback
                RegisterCallback(callback);

                //Add it to the definition's callback list
                AddCallbackForValueDefinition(valueDef, callback);

                ((VirtualValueDefinition)valueDef).VirtualValueEvaluationCallback = callback.SymbolicName;
                ((VirtualValueDefinition)valueDef).VirtualValueCalculationType = sensor.VirtualSensorDefinition.VirtualSensorCalculationType;
            }


            return valueDef;
        }

        /// <summary>
        /// The method creates a new <see cref="ValueDefinition"/> object based on the passed sensor data.
        /// </summary>
        /// <param name="sensor"></param>
        /// <returns></returns>
        private static ValueDefinition ConvertToValueDefinition(Sensor sensor)
        {
            ValueDefinition result;

            if (sensor.IsVirtualSensor)
            {
                if (sensor.VirtualSensorDefinition == null)
                    throw new Exception(String.Format(Properties.Resources.ExceptionVirtualSensorHasNoVirtualDefinition, sensor.DeviceId, sensor.Id));

                VirtualValueDefinition resultTemp = new VirtualValueDefinition(sensor.InternalSensorId, sensor.Id, sensor.VirtualSensorDefinition.VirtualSensorCalculationType);
                resultTemp.Description = sensor.Description;
                resultTemp.ValueTypeCode = (TypeCode)sensor.SensorValueDataType;
                resultTemp.ValueUnitOfMeasure = sensor.UnitSymbol;
                resultTemp.CycleTime = new TimeSpan(0, 0, sensor.PullFrequencyInSeconds);
                resultTemp.ShallSensorDataBePersisted = sensor.ShallSensorDataBePersisted;
                resultTemp.DefaultValue = sensor.DefaultValue;

                result = resultTemp;
            }
            else
            {
                result = new ValueDefinition(sensor.InternalSensorId, sensor.Id);
                result.Description = sensor.Description;
                result.ValueTypeCode = (TypeCode)sensor.SensorValueDataType;
                result.ValueUnitOfMeasure = sensor.UnitSymbol;
                result.ShallSensorDataBePersisted = sensor.ShallSensorDataBePersisted;
                result.DefaultValue = sensor.DefaultValue;
            }

            result.SensorCreatedFrom = sensor;
            return result;
        }

        private AbstractCallback CreateDynamicCallback(string symbolicName, VirtualSensorDefinition definition)
        {
            switch (definition.VirtualSensorDefinitionType)
            {
                case VirtualSensorDefinitionType.DotNetObject:
                    return new CompiledMethodCallback(symbolicName, CallbackType.VirtualValueCalculation, Type.GetType(definition.Definition));
                case VirtualSensorDefinitionType.CSharpExpression:
                    return new CSharpInteractiveCallback(symbolicName, CallbackType.VirtualValueCalculation, definition.Definition);
                case VirtualSensorDefinitionType.FSharpExpression:
                    return new FSharpInteractiveCallback(symbolicName, CallbackType.VirtualValueCalculation, definition.Definition);
                case VirtualSensorDefinitionType.IronPhyton:
                    return new PythonCallback(symbolicName, CallbackType.VirtualValueCalculation, definition.Definition);
                case VirtualSensorDefinitionType.IronRuby:
                    return new RubyCallback(symbolicName, CallbackType.VirtualValueCalculation, definition.Definition);

                case VirtualSensorDefinitionType.SQL:
                    return new SqlInteractiveCallback(symbolicName, CallbackType.VirtualValueCalculation, DatabaseConnectionSettingsForCallbacks, definition.Definition);
                //case VirtualSensorDefinitionType.SQLProcedure:
                //    return new SqlProcedureCallback(symbolicName, CallbackType.VirtualValueCalculation, DatabaseConnectionStringForCallbacks, definition.Definition);
                case VirtualSensorDefinitionType.WWF:
                    return new SoftwareAgentCallback(symbolicName, CallbackType.VirtualValueCalculation, Type.GetType(definition.Definition, true));

                case VirtualSensorDefinitionType.Formula:
                case VirtualSensorDefinitionType.Undefined:
                    return null;
                default:
                    throw new ArgumentException(String.Format(Properties.Resources.ExceptionUnknownVirtualSensorDefinitionType, definition.VirtualSensorDefinitionType), "definition.VirtualSensorDefinitionType");
            }
        }
		
		#endregion

		#region Storing & Data Access...

        #region Constants...
        /// <summary>
        /// the default retry count for write operations
        /// </summary>
        public const int DefaultWriteRetryCount = 4;

        /// <summary>
        /// default sleep time for the write thread
        /// </summary>
        public const int DefaultWriteThreadSleepTime = 500;

        /// <summary>
        /// the amount of waits before writing takes place. This results to 10 * 500 msec = 5 sec of duration
        /// </summary>
        public const int WriteThreadSleepWaitCounter = 10;
        #endregion

        #region Fields...
        /// <summary>
        /// Occurs when [data was written].
        /// </summary>
        public static event EventHandler DataWasWritten;

        /// <summary>
        /// flag if writing shall be suspended
        /// </summary>
        private static bool mStopWritingFlag;

        /// <summary>
        /// semaphore that the thread gracefully terminated
        /// </summary>
        private static ManualResetEvent writeThreadSuspendedSem = new ManualResetEvent(false);

        /// <summary>
        /// semaphore that the thread has started
        /// </summary>
        private static ManualResetEvent writeThreadStartedSem = new ManualResetEvent(false);

        /// <summary>
        /// a simple lock
        /// </summary>
        private static object mLockObject = new object();

        /// <summary>
        /// the thread instance
        /// </summary>
        private Thread writeStorageThread;
        #endregion

        /// <summary>
		/// Starts the write to storage thread. After starting a small pause is made to check if the thread came up properly or there were any problems
		/// </summary>
		/// <returns><c>true</c> if the start was successful, otherwise <c>false</c></returns>
		public bool StartWriteToStorageThread()
		{
			bool result = false;

			lock (mLockObject)
			{
				// if writing is active and now set -> get out
				if (writeStorageThread != null)
				{
					return (result);
				}

				// writing is not active and shall be started - but as a background thread
				writeStorageThread = new Thread(new ThreadStart(WriteStorageThread));
				writeStorageThread.IsBackground = true;
				writeStorageThread.Start();

				try
				{
					writeThreadStartedSem.WaitOne();

					// if no thread suspension happened then the thread started OK, otherwise the suspension was set -> error
					result = (writeThreadSuspendedSem.WaitOne(1) == false);
				}
				catch (System.Exception ex)
				{
					log.LogException("StartWriteToStorageThread", Properties.Resources.ErrorStartingWriteToStorageThread, ex);
				}

				return (result);
			}
		}

		/// <summary>
		/// Stops the write to storage thread.
		/// </summary>
		public AutoResetEvent StopWriteToStorageThread()
		{
			lock (mLockObject)
			{
				// if writing is not active and now not set -> get out
				if (writeStorageThread == null)
				{
					return new AutoResetEvent(true);
				}

				// if writing is active and shall be suspended -> set the flag and wait for clearance of the 2nd semaphore to indicate the thread is really gone
				mStopWritingFlag = true;
				writeThreadSuspendedSem.WaitOne();
				writeStorageThread = null;
                return mStoreTaskSync;
			}
		}

        /// <summary>
        /// The method starts the task calculating virtual values registered by the ValueManager instance.
        /// </summary>
        public void StartCalculatingVirtualValues()
        {
            lock (mLockObject)
            {
                if (mCancelTokenCalculation != null && mCancelTokenCalculation.IsCancellationRequested == false)
                {
                    throw new ValueDefinitionSetupException(Properties.Resources.ExceptionCalculationTaskIsAlreadyStarted);
                }

                mCancelTokenCalculation = new CancellationTokenSource();
                // create the task to compute the virtual values
                Task virtualValueTask = Task.Factory.StartNew(() => VirtualValueCalculationTask(mCancelTokenCalculation.Token), mCancelTokenCalculation.Token);
            }
        }

        /// <summary>
        /// The method stops the task that calculates virtual values registered by the ValueManager instance.
        /// </summary>
        public AutoResetEvent StopCalculatingVirtualValues()
        {
            lock (mLockObject)
            {
                if (mCancelTokenCalculation != null)
                {
                    mCancelTokenCalculation.Cancel();
                    mCancelTokenCalculation = null;
                }
                return mVirtualValueCalculationTaskSync;
            }
        }

		/// <summary>
		/// Gets a value indicating whether write to storage is active.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is write to storage active; otherwise, <c>false</c>.
		/// </value>
		public bool IsWriteToStorageActive { get { return (writeStorageThread != null); } }

		/// <summary>
		/// Gets or sets the write retry count.
		/// </summary>
		/// <value>The write retry count.</value>
		public static int WriteRetryCount { get; set; }

		/// <summary>
		/// Removes the written values.
		/// </summary>
		private static void RemoveWrittenValues()
		{
			lock (mUnsavedValueDict)
			{
				mUnsavedValueDict.Clear();
			}
		}

		/// <summary>
		/// Gets the unsaved value dict. 
		/// </summary>
		/// <value>The unsaved value dict.</value>
		public static Dictionary<ValueDefinition, List<SensorData>> UnsavedValueDict 
		{ 
			get 
			{
				lock (mUnsavedValueDict)
				{
					return (mUnsavedValueDict); 
				}
			} 
		}

		/// <summary>
		/// Write data to storage. In the current simplistic approach all values to be written are gathered once every iteration (with a default wait time between iterations of x seconds)
		/// </summary>
		private void WriteStorageThread()
		{
			DataStorageAccessBase storage = null;
			bool abortThread = false;

			try
			{
				storage = DataStorageAccessBase.Instance;
			}
			catch (System.Exception ex)
			{
				log.LogException("WriteStorageThread", Properties.Resources.ErrorNoProperStorageToWriteTo, ex);
				writeStorageThread = null;

				// the thread has been suspended
				writeThreadSuspendedSem.Set();
				abortThread = true;
			}

			// a start happened as well
			writeThreadStartedSem.Set();

			// but was potentially followed directly by an abort
			if (abortThread)
			{
				return;
			}

			// so that we start writing immediately
			int iterationCounter = WriteThreadSleepWaitCounter + 1;
			int retryCount = 0;
            Stopwatch stopwatch = new Stopwatch();

			// as long as we are not done writing check the flag every iteration (500 msec)
			while (mStopWritingFlag != true)
			{
				if (iterationCounter > WriteThreadSleepWaitCounter)
				{
					try
					{
#if MONITOR_PERFORMANCE
                        //Monitor performance
                        long startTimeTicks = PerformanceMonitoring.IncrementRequestMade(PerfMonCategoryExperimentValueManager, PerfMonMethodWriteToStorage);
#endif

						// get the list of objects to write to the database
						List<SensorDataForDevice> data = new List<SensorDataForDevice>();

						lock (mUnsavedValueDict)
						{
							// traverse the dictionary
							foreach (KeyValuePair<ValueDefinition, List<SensorData>> pair in mUnsavedValueDict)
							{
								List<AbstractCallback> callbackList = ValueManager.Instance.GetCallbackListForValueDefinition(pair.Key);
								AbstractCallback beforeStoreCallback = callbackList.FirstOrDefault(callback => callback.CallbackType == CallbackType.BeforeStore);

                                //BEFORE-STORE CALLBACK
								if (beforeStoreCallback != null)
								{
									// if there is a callback traverse over all the points and call back the method to get the result
									foreach (SensorData dataPoint in pair.Value)
									{
										SensorData storeData = dataPoint;

										// Pass parameters and get the result
										try
										{
											CallbackResultData result = beforeStoreCallback.ExecuteCallback(new CallbackPassInData(CallbackType.BeforeStore, pair.Key, dataPoint));
											if (result != null)
											{
												// canceling means a skip of the value
												if (result.IsCancelled)
												{
													continue;
												}

												// if a modify occurred then use the new value
												if (result.IsValueModified)
												{
													storeData = result.NewValue;
												}
											}
										}
										catch (System.Exception ex)
										{
											log.LogException("WriteStorageThread", Properties.Resources.ErrorExecuteCallback, ex);
										}

										// and add the values to write
										data.Add(new SensorDataForDevice(pair.Key.InternalId, storeData.GeneratedWhen, 
                                            storeData.Value ?? pair.Key.DefaultValue, storeData.CorrelationId));
									}
								}
								else
								{
									// in case no callback is defined make the quick version
									data.AddRange(pair.Value.Select(entry => new SensorDataForDevice(pair.Key.InternalId, entry.GeneratedWhen, 
                                        entry.Value ??pair.Key.DefaultValue, entry.CorrelationId)).ToList());
								}
							}

                            //STORE
                            if (data.Count > 0)
                            {
//#if MONITOR_PERFORMANCE
//                                stopwatch.Restart();
//#endif


                                // write to the database
                                storage.StoreSensorData(data);

//#if MONITOR_PERFORMANCE
//                                perfCounterStoredEntryCount.RawValue = data.Count;
//                                perfCounterStoredEntryDuration.RawValue = stopwatch.ElapsedMilliseconds;
//                                stopwatch.Stop();
//#endif
                            }

                            //AFTER-STORE CALLBACK
							// traverse the dictionary
							foreach (KeyValuePair<ValueDefinition, List<SensorData>> pair in mUnsavedValueDict)
							{
								List<AbstractCallback> callbackList = ValueManager.Instance.GetCallbackListForValueDefinition(pair.Key);
								AbstractCallback afterStoreCallback = callbackList.FirstOrDefault(callback => callback.CallbackType == CallbackType.AfterStore);

								if (afterStoreCallback != null)
								{
									foreach (SensorData dataPoint in pair.Value)
									{
										try
										{
											// just call the method - no result is processed
											afterStoreCallback.ExecuteCallback(new CallbackPassInData(CallbackType.AfterStore, pair.Key, dataPoint));
										}
										catch (System.Exception ex)
										{
											log.LogException("WriteStorageThread", Properties.Resources.ErrorExecuteCallback, ex);
										}
									}
								}
							}

							// no retries up to now (or not any more) as everything was written properly
							retryCount = 0;

							// mark the values as written
							RemoveWrittenValues();
						}

#if MONITOR_PERFORMANCE
                        PerformanceMonitoring.IncrementRequestsCompleted(PerfMonCategoryExperimentValueManager, PerfMonMethodWriteToStorage, startTimeTicks);
#endif

						// call any registered listener 
						if (DataWasWritten != null)
						{
							DataWasWritten(null, new EventArgs());
						}
					}
					catch (System.Exception ex)
					{
						log.LogException("WriteStorageThread", Properties.Resources.ErrorWritingValues, ex);
						retryCount++;

						// purge any data in the dictionary if the retry is getting problematic
						if (retryCount > ValueManager.WriteRetryCount)
						{
							log.Fatal(Properties.Resources.SaveObjectsRemovedDueToRetry);
							RemoveWrittenValues();

							// try it again
							retryCount = 0;
						}
					}

					// reset the counter
					iterationCounter = 0;
				}
				else
				{
					iterationCounter++;
				}

				// sleep the default time
				Thread.Sleep(DefaultWriteThreadSleepTime);
			}

			// signal that we are done
			writeThreadSuspendedSem.Set();
            mStoreTaskSync.Set();
		}

		#endregion

		#region Handlers for callbacks...

		/// <summary>
		/// Call any registered handlers - the values for the input event are defined from the caller
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void ValueIsChangingHandler(object sender, ValueChangingEventArgs args)
		{
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}

			// only makes sense if there are any callbacks associated
			if (mValueCallbackDictionary.ContainsKey(args.Definition))
			{
				// get the associated callbacks for the list and order them according to the call order
				List<AbstractCallback> callbackList = GetCallbackListForValueDefinition(args.Definition);

				// Call the proper callbacks now
				foreach (AbstractCallback callback in callbackList)
				{
					// if this is not a phase 1 call back skip it
					if (PhaseOne.Contains(callback.CallbackType) == false)
					{
						continue;
					}

					// Pass parameters and get the result
					try
					{
						CallbackResultData result = callback.ExecuteCallback(new CallbackPassInData(callback.CallbackType, args.Definition, args.NewValue));
						if (result != null)
						{
							args.IsCancelled = result.IsCancelled;

							// canceling means a direct exit
							if (result.IsCancelled)
							{
								return;
							}

							// copy the modified flag and if a modify occurred take the new value as the base for the next iteration
							args.IsValueModified = result.IsValueModified;
							if (result.IsValueModified)
							{
								args.NewValue = result.NewValue;
							}

                            //Updatet the sampling rate if needed
                            if (callback.CallbackType ==  CallbackType.AdjustSampleRate && result.SampleRateNeedsAdjustment)
                            {
                                AdjustedSamplingRates[args.Definition.InternalId] = result.NewSampleRate;
                            }
						}
					}
					catch (System.Exception ex)
					{
                        //There is an error, so the change should be cancelled
                        args.IsCancelled = true;
                        args.IsValueModified = false;
						log.LogException("ValueIsChangingHandler", Properties.Resources.ErrorExecuteCallback, ex);
					}
				}
			}
		}

		/// <summary>
		/// A value has changed and now the dependent (if any) values are calculated
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="ValueManagement.ValueChangedEventArgs"/> instance containing the event data.</param>
		private void ValueHasChangedHandler(object sender, ValueChangedEventArgs args)
		{
			if (args == null)
			{
				throw new ArgumentNullException("args");
			}

            //Do we have to persist the new value in the storage?
            if (args.Definition.ShallSensorDataBePersisted)
            {
                lock (mUnsavedValueDict)
                {
                    // add the value to the dictionary of values to be saved
                    List<SensorData> sensorDataList;
                    if (mUnsavedValueDict.TryGetValue(args.Definition, out sensorDataList) == false)
                    {
                        mUnsavedValueDict.Add(args.Definition, sensorDataList = new List<SensorData>());
                    }
                    sensorDataList.Add(args.NewValue);
                }
            }


			// are any virtual values depending on this value?
			if (IsBaseDefinition(args.Definition))
			{
				// Get the list and then traverse perhaps executing the calculation methods
				HashSet<ValueDefinition> dependentValueSet = GetDependentValueDefinitions(args.Definition);

				foreach (ValueDefinition definition in dependentValueSet)
				{
					if (definition.IsVirtualValue)
					{
						// always tag the value as a recalculation is needed
						(definition as VirtualValueDefinition).IsRecalculationNeeded = true;

						if (((definition as VirtualValueDefinition).VirtualValueCalculationType == VirtualSensorCalculationType.OnChange)
							&& (definition as VirtualValueDefinition).IsVirtualEvaluationCallbackDefined)
						{
							(definition as VirtualValueDefinition).CalculateVirtualValue();
						}
					}
				}
			}


            // only makes sense if there are any callbacks associated
            if (mValueCallbackDictionary.ContainsKey(args.Definition))
            {
                // get the associated callbacks for the list and order them according to the call order
                List<AbstractCallback> callbackList = GetCallbackListForValueDefinition(args.Definition);

                // Call the proper callbacks now
                foreach (AbstractCallback callback in callbackList)
                {
                    // if this is not a phase 2 call back skip it
                    if (PhaseTwo.Contains(callback.CallbackType) == false)
                    {
                        continue;
                    }

                    // Pass parameters -> no result can be taken as the value was changed already
                    try
                    {
                        callback.ExecuteCallback(new CallbackPassInData(callback.CallbackType, args.Definition, args.NewValue));
                    }
                    catch (System.Exception ex)
                    {
                        //There is an error, so the change should be cancelled
                        log.LogException("ValueHasChangedHandler", Properties.Resources.ErrorExecuteCallback, ex);
                    }
                }
            }
		}


		#endregion

		#region Dependencies & Test methods...

		/// <summary>
		/// Determines whether the specified internal id is contained in the dictionary
		/// </summary>
		/// <param name="internalId">The internal id.</param>
		/// <returns>
		/// <c>true</c> if the specified internal id is present; otherwise, <c>false</c>.
		/// </returns>
		public bool IsValueDefinitionPresent(int internalId)
		{
			return (mValueDefinitionDict.ContainsKey(internalId));
		}


		/// <summary>
		/// Determines whether the specified dependency to check is dependent on other values
		/// </summary>
		/// <param name="definitionToCheck">The definition to check.</param>
		/// <returns>
		/// 	<c>true</c> if the specified value definition is a dependent one; otherwise, <c>false</c>.
		/// </returns>
		public bool IsDependentDefinition(ValueDefinition definitionToCheck)
		{
			return (mDependsOnDictionary.ContainsKey(definitionToCheck) && (mDependsOnDictionary[definitionToCheck].Count > 0));
		}

		/// <summary>
		/// Determines whether the specified value definition is a base for other value definitions
		/// </summary>
		/// <param name="baseToCheck">The base to check.</param>
		/// <returns>
		/// 	<c>true</c> if the specified value definition is a base definition; otherwise, <c>false</c>.
		/// </returns>
		public bool IsBaseDefinition(ValueDefinition baseToCheck)
		{
			return (mBaseOfDictionary.ContainsKey(baseToCheck) && (mBaseOfDictionary[baseToCheck].Count > 0));
		}

		/// <summary>
		/// Gets the dependent value definitions for the given base definition
		/// </summary>
		/// <param name="baseDefintion">The base defintion.</param> 
		/// <returns>a copy of the set of dependent definitions or an empty set</returns>
		public HashSet<ValueDefinition> GetDependentValueDefinitions(ValueDefinition baseDefintion)
		{
			return (mBaseOfDictionary.ContainsKey(baseDefintion) ? new HashSet<ValueDefinition>(mBaseOfDictionary[baseDefintion]) : new HashSet<ValueDefinition>());
		}

		/// <summary>
		/// Gets the base value definitions for the given dependent definition
		/// </summary>
		/// <param name="dependentDefinition">The dependent definition.</param>
		/// <returns>a copy of the set of base definitions or an empty set</returns>
		public HashSet<ValueDefinition> GetBaseValueDefinitions(ValueDefinition dependentDefinition)
		{
			return (mDependsOnDictionary.ContainsKey(dependentDefinition) ? new HashSet<ValueDefinition>(mDependsOnDictionary[dependentDefinition]) : new HashSet<ValueDefinition>());
		}


		/// <summary>
		/// Determines whether a circular reference is present by using a very simple approach assuming set R = {reachable} and CR {children of reachable}
		/// 
		/// while CR != Empty
		///		R += CR
		///		Test for dependent value in CR
		///		CR += Children of CR
		///		CR -= R
		///		
		/// In the end R is the complete set of reachable definitions of the base value definition	
		/// </summary>
		/// <param name="baseValueDefinition">The base value definition.</param>
		/// <param name="dependentValueDefinition">The dependent value definition.</param>
		/// <returns>
		/// 	<c>true</c> if a circular is reference present; otherwise, <c>false</c>.
		/// </returns>
		protected bool IsCircularReferencePresent(ValueDefinition baseValueDefinition, ValueDefinition dependentValueDefinition)
		{
			if (baseValueDefinition == null)
			{
				throw new ArgumentNullException("baseValueDefinition");
			}

			if (dependentValueDefinition == null)
			{
				throw new ArgumentNullException("dependentValueDefinition");
			}

			// self references are anyhow not valid
			if (baseValueDefinition == dependentValueDefinition)
			{
                throw new ValueDefinitionSetupException(String.Format(Properties.Resources.ErrorNoDependencyToSelf, baseValueDefinition.InternalId));
			}

			HashSet<ValueDefinition> baseSet = new HashSet<ValueDefinition>(new [] {baseValueDefinition});
			HashSet<ValueDefinition> parentsOfBase = GetBaseValueDefinitions(baseValueDefinition);

			bool isCircular = false;

			while (parentsOfBase.Count > 0)
			{
				// add the children to the base
				baseSet.UnionWith(parentsOfBase);

				// test for the dependent object
				if (parentsOfBase.Contains(dependentValueDefinition))
				{
					isCircular = true;
					break;
				}

				// now traverse all children and their bases add to the set
				foreach (ValueDefinition valDef in baseSet)
				{
					parentsOfBase.UnionWith(GetBaseValueDefinitions(valDef));
				}

				// finally remove the base from the children
				parentsOfBase.ExceptWith(baseSet);
			}

			return (isCircular);
		}

		/// <summary>
		/// Adds the dependency (assuming it is a virtual value definition, otherwise an exception is thrown) to both lists. 
		/// The base of list gets an entry for the virtual value and the dependency list one for the base entry.
		/// 
		/// Before all the adding happens a check is made if the dependent value is already in the list of dependencies 
		/// </summary>
		/// <param name="baseValueDefinition">The base value definition.</param>
		/// <param name="dependentValueDefinition">The dependent value definition.</param>
		public void AddDependency(ValueDefinition baseValueDefinition, ValueDefinition dependentValueDefinition)
		{
            //Checks
            if (baseValueDefinition == null)
            {
                throw new ArgumentNullException("baseValueDefinition");
            }
            if (dependentValueDefinition == null)
            {
                throw new ArgumentNullException("dependentValueDefinition");
            }

            if (IsValueDefinitionPresent(baseValueDefinition.InternalId) == false)
            {
                throw new ArgumentOutOfRangeException("baseValueDefinition", String.Format(Properties.Resources.ExceptionValueDefinitionNotFound, baseValueDefinition.InternalId));
            }

            if (IsValueDefinitionPresent(dependentValueDefinition.InternalId) == false)
            {
                throw new ArgumentOutOfRangeException("dependentValueDefinition", String.Format(Properties.Resources.ExceptionValueDefinitionNotFound, baseValueDefinition.InternalId));
            }

			// this currently makes only sense for virtual values
			if (dependentValueDefinition.IsVirtualValue)
			{
				// test for self references
				if (baseValueDefinition == dependentValueDefinition)
				{
                    throw new ValueDefinitionSetupException(String.Format(Properties.Resources.ErrorNoDependencyToSelf, baseValueDefinition.InternalId));
				}

				// now check if the dependent value is already used as a base value once
				if (IsCircularReferencePresent(baseValueDefinition, dependentValueDefinition))
				{
					throw new CyclicReferenceException(String.Format(Properties.Resources.ErrorCyclicReference, baseValueDefinition.InternalId, dependentValueDefinition.InternalId));
				}

				// add the dependency to the list for the base value
				if (mBaseOfDictionary.ContainsKey(baseValueDefinition) == false)
				{
					mBaseOfDictionary.Add(baseValueDefinition, new HashSet<ValueDefinition>());
				}

				mBaseOfDictionary[baseValueDefinition].Add(dependentValueDefinition);

				// add the base value to the list of dependencies
				if (mDependsOnDictionary.ContainsKey(dependentValueDefinition) == false)
				{
					mDependsOnDictionary.Add(dependentValueDefinition, new HashSet<ValueDefinition>());
				}

				mDependsOnDictionary[dependentValueDefinition].Add(baseValueDefinition);
			}
			else
			{
                throw new ValueDefinitionSetupException(String.Format(Properties.Resources.ExceptionDependentValueDefinitionIsNotVirtual, dependentValueDefinition.InternalId), "dependentValueDefinition");
			}
		}

		/// <summary>
		/// Removes the dependency from both lists and clears the slot if the entries are empty now
		/// </summary>
		/// <param name="baseValueDefinition">The base value definition.</param>
		/// <param name="dependentValueDefinition">The dependent value definition.</param>
		public void RemoveDependency(ValueDefinition baseValueDefinition, ValueDefinition dependentValueDefinition)
		{
            //Checks
            if (baseValueDefinition == null)
            {
                throw new ArgumentNullException("baseValueDefinition");
            }
            if (dependentValueDefinition == null)
            {
                throw new ArgumentNullException("dependentValueDefinition");
            }

			// if the element is present in the list
			if (mDependsOnDictionary.ContainsKey(dependentValueDefinition))
			{
				// then remove the base (on which the value is dependent upon) -> ignore if nothing is found
				mDependsOnDictionary[dependentValueDefinition].Remove(baseValueDefinition);

				// remove the slot if it was the last dependency
				if (mDependsOnDictionary[dependentValueDefinition].Count == 0)
				{
					mDependsOnDictionary.Remove(dependentValueDefinition);
				}

				// now remove the depend upon entry in the base list as well
				if (mBaseOfDictionary.ContainsKey(baseValueDefinition))
				{
					// remove the slot and ignore not found situations
					mBaseOfDictionary[baseValueDefinition].Remove(dependentValueDefinition);

					// if the slot is now empty remove it as well
					if (mBaseOfDictionary[baseValueDefinition].Count == 0)
					{
						mBaseOfDictionary.Remove(baseValueDefinition);
					}
				}
			}
		}

		#endregion

        /// <summary>
        /// the storage task which iterates all 
        /// </summary>
        /// <param name="token">The token.</param>
        private void VirtualValueCalculationTask(CancellationToken token)
        {
            while (token.IsCancellationRequested == false)
            {
                Thread.Sleep(ThreadSleepTime);

                //Get all virtual values that are to be calculated cyclically
                List<ValueDefinition> virtualValDefsForCyclicCalculation;
                lock (((ICollection)ValueDefinitionDictionary).SyncRoot)
                {
                    virtualValDefsForCyclicCalculation = new List<ValueDefinition>(
                        ValueManager.Instance.ValueDefinitionDictionary.Values.
                        Where(x => x.IsVirtualValue
                            && ((VirtualValueDefinition)x).VirtualValueCalculationType == VirtualSensorCalculationType.Cyclic
                            && ((VirtualValueDefinition)x).IsVirtualEvaluationCallbackDefined));
                }

                //Calculate the values
                //TODO: provide for parallel computations here
                foreach (VirtualValueDefinition definition in virtualValDefsForCyclicCalculation)
                {
                    //Is this value definition to be recalculated because of its cycle time? 
                    if (definition.LastValueCalculation.Add(definition.CycleTime) <= DateTime.Now)
                    {
                        //Data sinks before the value is calculated
                        //The data sink is able to cancel the calculation
                        //if (ActivateDataSink(definition.InternalId, DataSinkType.BeforeVirtualValueComputed, new SensorData(definition.LastValueCalculation, definition.CurrentValue)))
                        //{
                        // always tag the value as a recalculation is needed
                        definition.IsRecalculationNeeded = true;
                        definition.CalculateVirtualValue();

                        //Data sinks after the calculation is completed
                        //ActivateDataSink(definition.InternalId, DataSinkType.BeforeVirtualValueComputed, new SensorData(definition.LastValueCalculation, definition.CurrentValue));
                        //}
                    }
                }
            }

            // tell the controller we are finished
            mVirtualValueCalculationTaskSync.Set();
        }
	}
}
