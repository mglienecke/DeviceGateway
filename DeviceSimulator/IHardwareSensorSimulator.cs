using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviceServer.Simulator
{
    /// <summary>
    /// The interface for all sensor simulators, which can produce and consume sensor values.
    /// </summary>
    public interface IHardwareSensorSimulator
    {
        /// <summary>
        /// Id of the sensor.
        /// </summary>
        string SensorId { get; set; }

        /// <summary>
        /// Id of the master device.
        /// </summary>
        string DeviceId { get; set; }

        /// <summary>
        /// The method gets the current value of the sensor.
        /// </summary>
        /// <returns></returns>
        SensorData GetValue();

        /// <summary>
        /// The method sets the current value of the sensor.
        /// </summary>
        /// <param name="data"></param>
        void SetValue(SensorData data);

        /// <summary>
        /// The flag specifies if the GetValue method is enabled.
        /// </summary>
        bool GetValueEnabled { get; }

        /// <summary>
        /// The flag specifies if the SetValue method is enabled.
        /// </summary>
        bool SetValueEnabled { get; }
    }
}
