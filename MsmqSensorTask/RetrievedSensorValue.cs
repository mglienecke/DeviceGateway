using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsmqSensorTask
{
    /// <summary>
    /// The class encapsulates sensor value data for displaying them in a data grid.
    /// </summary>
    public class RetrievedSensorValue
    {
        /// <summary>
        /// The property contains the sensor id
        /// </summary>
        public string SensorId { get; set; }

        /// <summary>
        /// The property contains the description of the sensor.
        /// </summary>
        public string SensorDescription { get; set; }

        /// <summary>
        /// The property contains the sensor value.
        /// </summary>
        public string SensorValue { get; set; }

        /// <summary>
        /// The property contains the timestamp of the sensor value.
        /// </summary>
        public string Timestamp { get; set; }
    }
}
