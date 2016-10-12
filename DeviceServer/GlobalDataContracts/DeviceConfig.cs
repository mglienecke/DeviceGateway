using System;
using System.Collections;
using Microsoft.SPOT;

namespace GlobalDataContracts
{
    /// <summary>
    /// The class encapsulates config data for a single device;
    /// </summary>
    [Serializable]
    public class DeviceConfig
    {
        /// <summary>
        /// Default id
        /// </summary>
        public const uint DefaultId = 1;

        /// <summary>
        /// THe property contains configuration data for the device's sensors (<see cref="Sensor"/>).
        /// </summary>
        public ArrayList Sensors { get; set; }
    }
}
