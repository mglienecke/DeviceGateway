using System;
using System.Text;

namespace GlobalDataContracts
{
	public class BinaryCommandsDevice
	{
		/// <summary>
		/// get all the non transmitted data of one sensor (defined by its id)
		/// </summary>
		public const byte GetAllSensorDataForSensorId = 1;

		/// <summary>
		/// get the last sensor data reading from a sensor (defined by its id)
		/// </summary>
		public const byte GetLastSensorDataForSensorId = 2;

		/// <summary>
		/// get all the non transmitted data of all sensors
		/// </summary>
		public const byte GetAllNonTransmittedSensorData = 3;

		/// <summary>
		/// the the latest values of all sensors
		/// </summary>
		public const byte GetLatestSensorData = 4;

		/// <summary>
		/// get the sensor definitions from the device
		/// </summary>
		public const byte GetSensors = 5;
	}
}
