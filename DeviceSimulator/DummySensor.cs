using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviceServer.Simulator
{
    public class DummySensor:IHardwareSensorSimulator
    {
        int mCurrentValue = DateTime.Now.Millisecond;

        /// <summary>
        /// Id of the sensor.
        /// </summary>
        public string SensorId { get; set; }

        /// <summary>
        /// Id of the master device.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// The method gets the current value of the sensor.
        /// </summary>
        /// <returns></returns>
        public SensorData GetValue() { return new SensorData(DateTime.Now, mCurrentValue.ToString(), DateTime.Now.Ticks.ToString()); }

        /// <summary>
        /// The method sets the current value of the sensor.
        /// </summary>
        /// <param name="data"></param>
        public void SetValue(SensorData data) { mCurrentValue = Convert.ToInt32(data.Value); }

        /// <summary>
        /// The flag specifies if the GetValue method is enabled.
        /// </summary>
        public bool GetValueEnabled { get { return true; } }

        /// <summary>
        /// The flag specifies if the SetValue method is enabled.
        /// </summary>
        public bool SetValueEnabled { get { return true; } }
    }

    public class CurrentTimeTicksSensor : IHardwareSensorSimulator
    {
        /// <summary>
        /// Id of the sensor.
        /// </summary>
        public string SensorId { get; set; }

        /// <summary>
        /// Id of the master device.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// The method gets the current value of the sensor.
        /// </summary>
        /// <returns></returns>
        public SensorData GetValue() { return new SensorData(DateTime.Now, DateTime.Now.Ticks.ToString(), DateTime.Now.Ticks.ToString()); }

        /// <summary>
        /// The method sets the current value of the sensor.
        /// </summary>
        /// <param name="data"></param>
        public void SetValue(SensorData data) {  }

        /// <summary>
        /// The flag specifies if the GetValue method is enabled.
        /// </summary>
        public bool GetValueEnabled { get { return true; } }

        /// <summary>
        /// The flag specifies if the SetValue method is enabled.
        /// </summary>
        public bool SetValueEnabled { get { return false; } }
    }

    public class ConsoleWritingSensor : IHardwareSensorSimulator
    {
        /// <summary>
        /// Id of the sensor.
        /// </summary>
        public string SensorId { get; set; }

        /// <summary>
        /// Id of the master device.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// The method gets the current value of the sensor.
        /// </summary>
        /// <returns></returns>
        public SensorData GetValue() { return new SensorData(0, DateTime.Now.Ticks.ToString(), DateTime.Now.Ticks.ToString()); }

        /// <summary>
        /// The method sets the current value of the sensor.
        /// </summary>
        /// <param name="data"></param>
        public void SetValue(SensorData data)
        {
            TrackingPoint.TrackingPoint.CreateTrackingPoint("DeviceSimulator: ActuatorReceive", String.Format("Sensor: {0}", data.SensorId), 0, data.CorrelationId);
            Console.WriteLine("[{0}] Sensor {1} value set: {2}. Correlation id: {3}", DateTime.Now.ToLongTimeString(), "ConsoleWritingSensor", data.Value, data.CorrelationId);
        }

        /// <summary>
        /// The flag specifies if the GetValue method is enabled.
        /// </summary>
        public bool GetValueEnabled { get { return true; } }

        /// <summary>
        /// The flag specifies if the SetValue method is enabled.
        /// </summary>
        public bool SetValueEnabled { get { return true; } }
    }
}
