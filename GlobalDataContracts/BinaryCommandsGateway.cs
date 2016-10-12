using System;
using System.Text;

namespace GlobalDataContracts
{
	public class BinaryCommandsGateway
	{
		/// <summary>
		/// register a device
		/// </summary>
		public const byte RegisterDevice = 1;

		/// <summary>
		/// updates a device registration
		/// </summary>
		public const byte UpdateDevice = 2;

		/// <summary>
		/// register a sensor
		/// </summary>
		public const byte RegisterSensor = 3;

		/// <summary>
		/// update a sensor registration
		/// </summary>
		public const byte UpdateSensor = 4;

		/// <summary>
		/// check if the sensor is already defined 
		/// </summary>
		public const byte IsSensorIdUsedForDevice = 5;

		/// <summary>
		/// store a single data item for a sensor
		/// </summary>
		public const byte StoreSensorData = 6;

		/// <summary>
		/// store several data items for (potentially several) sensor(s)
		/// </summary>
		public const byte StoreData = 7;
	}
}
