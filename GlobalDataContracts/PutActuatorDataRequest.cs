using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalDataContracts
{
    /// <summary>
    /// The class encapsulates data for setting actuator data.
    /// </summary>
    [Serializable]
    public class PutActuatorDataRequest
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public PutActuatorDataRequest()
        {
        }

        /// <summary>
        /// Sensor id.
        /// </summary>
        public string SensorId { get; set; }

        /// <summary>
        /// Sensor data to be passed to the actuator.
        /// </summary>
        public SensorData Data { get; set; }
    }
}
