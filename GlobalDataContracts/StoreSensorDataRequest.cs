using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalDataContracts
{
    /// <summary>
    /// The class encapsulates data for storing sensor data.
    /// </summary>
    [Serializable]
    public class StoreSensorDataRequest
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public StoreSensorDataRequest()
        {
        }

        /// <summary>
        /// Device id.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Sensor data (with sensor ids, all sensors belong to the same device whose id is specified in the <see cref="DeviceId"/>.
        /// </summary>
        public MultipleSensorData[] Data { get; set; }
    }
}
