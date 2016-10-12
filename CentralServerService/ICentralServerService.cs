using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GlobalDataContracts;

namespace CentralServerService
{
    /// <summary>
    /// The interface declares methods for the service that provides access to the permanent data storage.
    /// </summary>
	public interface ICentralServerService
	{
		#region Registration of sensors, devices, triggers, sinks...

		/// <summary>
		/// Registers the device. If already present the entry is ignored
		/// </summary>
		/// <param name="device">The device.</param>
		/// <returns>the call result</returns>
		OperationResult RegisterDevice(Device device);

		/// <summary>
		/// Updates the device. 
		/// </summary>
		/// <param name="device">The device.</param>
		/// <returns>the call result</returns>
		OperationResult UpdateDevice(Device device);

		/// <summary>
		/// Registers the sensor list (1..n). If the sensor is already present the entry is ignored
		/// </summary>
		/// <param name="sensorList">The sensor list to register.</param>
		/// <returns></returns>
		OperationResult RegisterSensors(IEnumerable<Sensor> sensorList);

		/// <summary>
		/// Updates the single sensor
		/// </summary>
		/// <param name="sensor">The sensor.</param>
		/// <returns>the call result</returns>
		OperationResult UpdateSensor(Sensor sensor);

        /*
		/// <summary>
		/// Registers the data sink handler
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="sensor">The sensor.</param>
		/// <param name="sinkType">Type of the sink.</param>
		/// <param name="sinkHandler">The sink handler.</param>
		void RegisterDataSink(Sensor sensor, DataSinkType sinkType, EventHandler<DataSinkEventArgs> sinkHandler);
         */

		#endregion

		#region Checks..

		/// <summary>
		/// Check if the device id is used already
		/// </summary>
		/// <param name="deviceId">the id to check</param>
		/// <returns>the call result</returns>
		bool IsDeviceIdUsed(string deviceId);

		/// <summary>
		/// determines if the sensor is registered for the given device
		/// </summary>
		/// <param name="deviceId">the device id</param>
		/// <param name="sensorId">the sensor id</param>
		/// <returns>the call result</returns>
		bool IsSensorIdRegisteredForDevice(string deviceId, string sensorId);

		#endregion

        #region Sensors, devices, sensor data...
        /// <summary>
		/// Stores the device data. If a device or a sensor are not registered they will be registered preliminary and can later be updated by a subsequent call to <see cref="RegisterDevice"/> or <see cref="RegisterSensor"/>
		/// </summary>
		/// <param name="deviceId">The device id.</param>
		/// <param name="data">The data.</param>
		/// <returns>the call result</returns>
        StoreSensorDataResult StoreSensorData(string deviceId, List<MultipleSensorData> data);

        /// <summary>
        /// Stores the device data. If a device or a sensor are not registered they will be registered preliminary and can later be updated by a subsequent call to <see cref="RegisterDevice"/> or <see cref="RegisterSensor"/>
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        /// <param name="data">The data.</param>
        /// <returns>the call result</returns>
        StoreSensorDataResult StoreSensorData(string deviceId, MultipleSensorData data);

		/// <summary>
		/// Gets the devices which are registered in the system
		/// </summary>
		/// <returns>the call result</returns>
		GetDevicesResult GetDevices();

        /// <summary>
        /// Gets the device registered in the system for the provided ids.
        /// </summary>
        /// <param name="deviceIds"></param>
        /// <returns>the call result</returns>
        GetDevicesResult GetDevices(IEnumerable<String> deviceIds);

		/// <summary>
		/// Gets the sensors for the specified device. 
		/// </summary>
		/// <param name="deviceId">The device id.</param>
		/// <returns>the call result</returns>
		GetSensorsForDeviceResult GetSensorsForDevice(string deviceId);

		/// <summary>
		/// Gets the sensors with the specified id for the specified device.
		/// </summary>
		/// <param name="deviceId">The device id. If <c>null</c> is passed all sensors are delivered</param>
		/// <param name="sensorId">The sensor id.</param>
		/// <returns>the call result</returns>
		GetSensorResult GetSensor(string deviceId, string sensorId);

		/// <summary>
		/// Gets the sensor data.
		/// </summary>
		/// <param name="deviceId">The device id.</param>
		/// <param name="sensorIdList">The sensor id list.</param>
		/// <param name="dataStartDateTime">The data start date time. <c>DateTime.MinValue</c> indicates no lower bound</param>
		/// <param name="dataEndDateTime">The data end date time. <c>DateTime.MaxValue</c> indicates no upper bound</param>
		/// <param name="maxResults">The max results. 0 indicates no bound</param>
		/// <returns>the sensor data for the period requested</returns>
		GetMultipleSensorDataResult GetSensorData(string deviceId, List<string> sensorIdList, DateTime dataStartDateTime, DateTime dataEndDateTime, int maxResults);

        /// <summary>
        /// Gets the latest sensor data.
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        /// <param name="sensorIdList">The sensor id list.</param>
        /// <param name="maxResults">The max results.</param>
        /// <returns>the sensor data for the period requested</returns>
        GetMultipleSensorDataResult GetSensorDataLatest(string deviceId, List<string> sensorIdList, int maxResults);

        /// <summary>
        /// The method adds a dependency between sensors. The dependent sensor must be a virtual one.
        /// </summary>
        /// <param name="sensorBaseId"></param>
        /// <param name="sensorDependentId"></param>
        /// <exception cref="ArgumentException">The exception thrown if the dependent sensor is not virtual, if creating this dependency will create a circular reference</exception>
        void AddSensorDependency(int sensorBaseInternalId, int sensorDependentInternalId);

        /// <summary>
        /// The method removes an existing dependency between sensors.
        /// </summary>
        /// <param name="sensorBaseId"></param>
        /// <param name="sensorDependentId"></param>
        void RemoveSensorDependency(int sensorBaseInternalId, int sensorDependentInternalId);

        /// <summary>
        /// The method returns all base sensor dependencies for the dependent sensor with the passed id.
        /// </summary>
        /// <returns></returns>
        List<Sensor> GetBaseSensorDependencies(int dependentSensorInternalId);

        /// <summary>
        /// The method returns all dependent sensor dependencies for the base sensor with the passed id.
        /// </summary>
        /// <returns></returns>
        List<Sensor> GetDependentSensorDependencies(int baseSensorInternalId);
        #endregion

        #region Various API

        /// <summary>
        /// Return the next possible correlation id
        /// </summary>
        /// <returns>the next id generated by a central instance which will be always unique</returns>
        GetCorrelationIdResult GetNextCorrelationId();

        #endregion

#if DEBUG
        // the following parts only make sense while debugging

		/// <summary>
		/// Occurs when storing is done.
		/// </summary>
		event EventHandler StoringIsDone;


		/// <summary>
		/// Cancels the service. Just for test purposes
		/// </summary>
		void CancelService();
#endif
    }
}
