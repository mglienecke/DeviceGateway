using System;
using Microsoft.SPOT;

namespace DeviceServer.Base
{
    /// <summary>
    /// Class encapsulates data for a CNDEP request to store sensor data.
    /// </summary>
    [Serializable]
    public class StoreSensorDataRequest
    {
        /// <summary>
        /// Device id.
        /// </summary>
        public string DeviceId { get; set;}

        /// <summary>
        /// Sensor data.
        /// </summary>
        public MultipleSensorData[] Data { get; set; }

    }

    /// <summary>
    /// The class encapsulates multiple sensor data (with all sensors belonging to the same device)
    /// </summary>
    [Serializable]

    public class MultipleSensorData
    {
        /// <summary>
        /// Sensor id.
        /// </summary>
        public string SensorId { get; set; }

        /// <summary>
        /// Sensor's value data type.
        /// </summary>
        public SensorValueDataType DataType { get; set; }

        /// <summary>
        /// Sensor data.
        /// </summary>
        public SensorData[] Measures { get; set; }
    }
}
