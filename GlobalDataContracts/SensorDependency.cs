using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalDataContracts
{
    /// <summary>
    /// The class encapsulates data of a single dependency connection between to sensors.
    /// </summary>
    public class SensorDependency
    {
        /// <summary>
        /// The property contains the internal id of the base sensor.
        /// </summary>
        public int BaseSensorInternalId
        {
            get;
            set;
        }

        /// <summary>
        /// The property contains the internal id of the dependent sensor.
        /// </summary>
        public int DependentSensorInternalId
        {
            get;
            set;
        }
    }
}
