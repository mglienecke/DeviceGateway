using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Server
{
    /// <summary>
    /// The class contains link patterns for the server RESTful HTTP Gateway service.
    /// </summary>
    public sealed class ServerHttpRestUrls
    {
        /// <summary>
        /// No instances allowed.
        /// </summary>
        private ServerHttpRestUrls()
        {
        }

        /// <summary>
        /// all registered devices.
        /// </summary>
        public const string MultipleDevices = "/MultipleDevices";
        /// <summary>
        /// single registered device.
        /// </summary>
        public const string SingleDeviceWithId = "/SingleDevice/{0}/";
        /// <summary>
        /// flag to check if the device is used/
        /// </summary>
        public const string DeviceIsUsed = SingleDeviceWithId + "/isUsed";
        /// <summary>
        /// all sensors registered for a device.
        /// </summary>
        public const string DeviceWithIdMultipleSensors = SingleDeviceWithId + "/MultipleSensors";
        /// <summary>
        /// all sensors registered for a device.
        /// </summary>
        public const string DeviceWithIdSingleSensor = SingleDeviceWithId + "/SingleSensor";
        /// <summary>
        /// sensor registered for a device.
        /// </summary>
        public const string DeviceWithIdSingleSensorWithId = DeviceWithIdSingleSensor + "/{1}";
        /// <summary>
        /// if sensor registered for a device.
        /// </summary>
        public const string DeviceSensorIsRegistered = DeviceWithIdSingleSensorWithId + "/isRegistered";
        /// <summary>
        /// Data from multiple sensors of a device
        /// </summary>
        public const string DeviceWithIdSensorsSensorData = DeviceWithIdMultipleSensors + "/SensorData";
        /// <summary>
        /// Latest data from multiple sensors of a device
        /// </summary>
        public const string DeviceWithIdSensorsSensorDataLatest = DeviceWithIdSensorsSensorData + "/latest";
        /// <summary>
        /// Sensor data for a single sensor of a device.
        /// </summary>
        public const string DeviceWithIdSensorWithIdSensorData = DeviceWithIdSingleSensorWithId + "/data";

        /// <summary>
        /// Get the next correlation id
        /// </summary>
        public const string GetNextCorrelationId = "/GetNextCorrelationId";
    }
}
