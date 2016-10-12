using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

using log4net;
using GlobalDataContracts;

namespace DataStorageAccess
{

	/// <summary>
	/// a delegate to retrieve the sensor for an id
	/// </summary>
	public delegate Sensor GetSensor(string deviceId, string sensorId);

	/// <summary>
	/// a delegate to retrieve the device for an id
	/// </summary>
	public delegate Device GetDevice(string deviceId);

	/// <summary>
	/// the data storage class for the central service. All real methods are declared abstract so that the implementation class has to provide them
	/// </summary>
	public abstract class DataStorageAccessBase
	{
		/// <summary>
		/// the instance we are handling
		/// </summary>
		private static DataStorageAccessBase mInstance;

		/// <summary>
		/// Lock object
		/// </summary>
		private static object mLock = new object();

		/// <summary>
		/// logger access
		/// </summary>
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// the app.config entry for the implementation class
		/// </summary>
		public const string ConfigImplementationClass = "DataStorageImplementation";

		/// <summary>
		/// Gets the instance of the currently loaded data storage access and if not loaded yet performs the instance creation accordingly.
		/// </summary>
		/// <value>The currently loaded instance</value>
		public static DataStorageAccessBase Instance
		{
			get
			{
				lock (mLock)
				{
					// if there is no instance load the storage access class and initialize everything
					if (mInstance == null)
					{
						if (ConfigurationManager.AppSettings[ConfigImplementationClass] == null)
						{
							log.Fatal(Properties.Resources.ErrorNoStorageImplementationClassProvided);
							throw new InvalidOperationException(Properties.Resources.ErrorNoStorageImplementationClassProvided);
						}

						try
						{
							mInstance = (DataStorageAccessBase) Activator.CreateInstance(Type.GetType(ConfigurationManager.AppSettings[ConfigImplementationClass]));
						}
						catch (System.Exception x)
						{
							string message = string.Format(Properties.Resources.ErrorInvalidStorageImplementationClass, ConfigurationManager.AppSettings[ConfigImplementationClass], x.Message);
							log.Fatal(message);
							throw new InvalidOperationException(message, x);
						}
					}
				}
				return (mInstance);

			}
		}

		/// <summary>
		/// stores a device
		/// </summary>
		public abstract void StoreDevice(Device device);

		/// <summary>
		/// Updates the device.
		/// </summary>
		/// <param name="device">The device.</param>
		public abstract void UpdateDevice(Device device);

		/// <summary>
		/// Deletes the device.
		/// </summary>
		/// <param name="deviceId">The device id.</param>
		public abstract void DeleteDevice(string deviceId);

		/// <summary>
		/// Loads the devices.
		/// </summary>
		/// <returns></returns>
		public abstract List<Device> LoadDevices();

		/// <summary>
		/// Stores the sensor.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="sensor">The sensor.</param>
		public abstract void StoreSensor(Sensor sensor);

		/// <summary>
		/// Updates the sensor.
		/// </summary>
		/// <param name="sensor">The sensor.</param>
		public abstract void UpdateSensor(Sensor sensor);

		/// <summary>
		/// Deletes the sensor.
		/// </summary>
		/// <param name="sensorId">The sensor id.</param>
		public abstract void DeleteSensor(string sensorId);

		/// <summary>
		/// Loads the sensors for a given device
		/// </summary>
		/// <param name="device">The device.</param>
		/// <returns>a list of sensors for the device specified</returns>
		public abstract List<Sensor> LoadSensorsForDevice(string deviceId);

		/// <summary>
		/// Gets a single sensor.
		/// </summary>
		/// <param name="deviceId">The device id.</param>
		/// <param name="sensorId">The sensor id.</param>
		/// <returns>the sensor or <c>null</c> if none is found</returns>
		// public abstract Sensor GetSensor(string deviceId, string sensorId);

		/// <summary>
		/// Stores the sensor data.
		/// </summary>
		/// <param name="deviceGetter">The device getter.</param>
		/// <param name="sensorGetter">The sensor getter.</param>
		/// <param name="writeValueList">The write value list.</param>
		/// <returns>the call result</returns>
		//public abstract CallResult StoreSensorData(GetDevice deviceGetter, GetSensor sensorGetter, List<InternalSensorValue> writeValueList);

		/// <summary>
		/// Stores the sensor data from the list to the database
		/// </summary>
		/// <param name="sensorDataList">The sensor data list.</param>
		/// <returns>the call result</returns>
		public abstract CallResult StoreSensorData(List<SensorDataForDevice> sensorDataList);

		/// <summary>
		/// Gets the sensor data.
		/// </summary>
        /// <param name="sensorInternalIdList">The sensor internal id list.</param>
		/// <param name="dataStartDateTime">The data start date time.</param>
		/// <param name="dataEndDateTime">The data end date time.</param>
		/// <param name="maxResults">The max results.</param>
		/// <returns></returns>
        public abstract List<MultipleSensorData> GetSensorData(List<Int32> sensorInternalIdList, DateTime dataStartDateTime, DateTime dataEndDateTime, int maxResults);

        /// <summary>
        /// Gets the latest sensor data.
        /// </summary>
        /// <param name="sensorInternalIdList">The sensor internal id list.</param>
        /// <param name="maxResults">The max results.</param>
        /// <returns></returns>
        public abstract List<MultipleSensorData> GetSensorDataLatest(List<Int32> sensorInternalIdList, int maxResults);

        /// <summary>
        /// The method adds a new dependency between two existing sensors.
        /// </summary>
        /// <param name="baseSensorInternalId"></param>
        /// <param name="dependentSensorInternalId"></param>
        public abstract void AddSensorDependency(int baseSensorInternalId, int dependentSensorInternalId);


        /// <summary>
        /// The method removes an existing dependency between two sensors.
        /// </summary>
        /// <param name="baseSensorInternalId"></param>
        /// <param name="dependentSensorInternalId"></param>
        public abstract void RemoveSensorDependency(int baseSensorInternalId, int dependentSensorInternalId);

        /// <summary>
        /// The method retrieves all sensor dependencies.
        /// </summary>
        /// <returns></returns>
        public abstract List<SensorDependency> GetSensorDependencies();

        /// <summary>
        /// The method retrieves all sensor dependencies where the sensor with the passed internal id is the dependent one.
        /// </summary>
        /// <returns></returns>
        public abstract List<SensorDependency> GetBaseSensorDependencies(int dependentSensorInternalId);

        /// <summary>
        /// The method retrieves all sensor dependencies where the sensor with the passed internal id is the base one.
        /// </summary>
        /// <returns></returns>
        public abstract List<SensorDependency> GetDependentSensorDependencies(int baseSensorInternalId);

        /// <summary>
        /// Get the next available correlation Id
        /// </summary>
        /// <returns>the next available correlation id</returns>
	    public abstract string GetCorrelationId();
	}
}
