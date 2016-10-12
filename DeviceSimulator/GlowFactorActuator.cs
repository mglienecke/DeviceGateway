using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceServer.Simulator
{
    public class GlowFactorActuator : IHardwareSensorSimulator
    {
        private decimal _glowFactor;

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
        public SensorData GetValue() { throw new NotImplementedException(); }

        /// <summary>
        /// The method sets the current value of the sensor.
        /// </summary>
        /// <param name="data"></param>
        public void SetValue(SensorData data)
        {
            _glowFactor = Decimal.Parse(data.Value);
            Console.WriteLine("[{0}] ({1}:{2}) glow factor: {3}", DateTime.Now.ToLongTimeString(), DeviceId, SensorId, _glowFactor);
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
