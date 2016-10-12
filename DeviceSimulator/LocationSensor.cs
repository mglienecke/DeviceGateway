using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeviceServer.Simulator;

namespace DeviceServer.Simulator
{
    /// <summary>
    /// The class implements a dummy location sensor.
    /// </summary>
    public class LocationSensor : IHardwareSensorSimulator
    {
        /// <summary>
        /// Minimum value for the X coordinate.
        /// </summary>
        public const int MinX = 0;
        /// <summary>
        /// Minimum value for the Y coordinate.
        /// </summary>
        public const int MinY = 0;
        /// <summary>
        /// Minimum value for the Z coordinate.
        /// </summary>
        public const int MinZ = 0;
        /// <summary>
        /// Maximum value for the X coordinate.
        /// </summary>
        public const int MaxX = 100;

        /// <summary>
        /// Maximum value for the Y coordinate.
        /// </summary>
        public const int MaxY = 100;
        /// <summary>
        /// Maximum value for the Z coordinate.
        /// </summary>
        public const int MaxZ  = 100;

        /// <summary>
        /// Max step change for the X coordinate (positive or negative)
        /// </summary>
        public const int MaxChangeX = 2;
        /// <summary>
        /// Max step change for the Y coordinate (positive or negative)
        /// </summary>
        public const int MaxChangeY = 2;
        /// <summary>
        /// Max step change for the Z coordinate (positive or negative)
        /// </summary>
        public const int MaxChangeZ = 2;

        private static readonly Random _random = new Random();
        private int _x, _y, _z;
        private int _stepX;
        private int _stepY;
        private int _stepZ;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LocationSensor()
        {
            //Set the initial position
            _x = _random.Next(MinX, MaxX);
            _y = _random.Next(MinY, MaxY);
            _z = _random.Next(MinZ, MaxZ);

            _stepX = _stepY = _stepZ = 1;
        }

        #region Properies...

        /// <summary>
        /// Id of the sensor.
        /// </summary>
        public string SensorId { get; set; }

        /// <summary>
        /// Id of the master device.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// The X coordinate value.
        /// </summary>
        public int X
        {
            get { return _x; }
            private set
            {
                if (value < MinX || value > MaxX)
                    throw new ArgumentOutOfRangeException("value");
                _x = value;
            }
        }

        /// <summary>
        /// The Y coordinate value.
        /// </summary>
        public int Y
        {
            get { return _y; }
            private set
            {
                if (value < MinY || value > MaxY)
                    throw new ArgumentOutOfRangeException("value");
                _y = value;
            }
        }
        /// <summary>
        /// The Z coordinate value.
        /// </summary>
        public int Z
        {
            get { return _z; }
            private set
            {
                if (value < MinZ || value > MaxZ)
                    throw new ArgumentOutOfRangeException("value");
                _z = value;
            }
        }
        
        #endregion

        /// <summary>
        /// The method gets the current value of the sensor.
        /// </summary>
        /// <returns></returns>
        public SensorData GetValue()
        {
            //Check the boundary conditions, reverse, if going out of bounds
            if (X + _stepX > MaxX || X + _stepX < MinX)
                _stepX = -_stepX;

            if (Y + _stepY > MaxY || Y + _stepY < MinY)
                _stepY = -_stepY;

            if (Z + _stepZ > MaxZ || Z + _stepZ < MinZ)
                _stepZ = -_stepZ;

            X += _stepX;
            Y += _stepY;
            Z += _stepZ;

            //Return the value
            var value = new StringBuilder();
            value.AppendFormat("{0}/{1}/{2}", X, Y, Z);
            return new SensorData(DateTime.Now, value.ToString(), DateTime.Now.Ticks.ToString());
        }

        /// <summary>
        /// The method sets the current value of the sensor.
        /// </summary>
        /// <param name="data"></param>
        public void SetValue(SensorData data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The flag specifies if the GetValue method is enabled.
        /// </summary>
        public bool GetValueEnabled
        {
            get { return true; }
        }

        /// <summary>
        /// The flag specifies if the SetValue method is enabled.
        /// </summary>
        public bool SetValueEnabled
        {
            get { return false; }
        }
    }
}
