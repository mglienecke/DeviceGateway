using System;
using System.Collections.Generic;
using System.Linq;

using GlobalDataContracts;

namespace DataStorageAccess
{
	partial class DbSensorData
	{
	}

	partial class SqlStorageLinqDataClassesDataContext
	{
	}

	public partial class DbDevice
	{
		/// <summary>
		/// Fills the device data
		/// </summary>
		/// <param name="device">The device.</param>
		public void FillDeviceData(Device device)
		{
			this.Id = device.Id;
			this.Description = device.Description;
            this.IpEndPoint = device.DeviceIpEndPoint;
			this.Elevation = device.Location != null ? (decimal?)device.Location.Elevation : (decimal?)null;
			this.Latitude = device.Location != null ? (decimal?)device.Location.Latitude : (decimal?)null;
			this.Longitude = device.Location != null ? (decimal?)device.Location.Longitude : (decimal?)null;
			this.LocationName = device.Location != null ? device.Location.Name : string.Empty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DbDevice"/> class.
		/// </summary>
		/// <param name="device">The device.</param>
		public DbDevice(Device device)
			: this()
		{
			FillDeviceData(device);
		}
	}

	public partial class DbSensor
	{
		/// <summary>
		/// Fills the sensor data.
		/// </summary>
		/// <param name="sensor">The sensor.</param>
		public void FillSensorData(Sensor sensor)
		{
			this.Description = sensor.Description;
			this.Id = sensor.Id;
			this.DeviceId = sensor.DeviceId;
			this.IsVirtualSensor = sensor.IsVirtualSensor;
			this.SensorDataRetrievalMode = (int)sensor.SensorDataRetrievalMode;
			this.ShallSensorDataBePersisted = sensor.ShallSensorDataBePersisted;
			this.UnitSymbol = sensor.UnitSymbol;
            this.SensorDataCalculationMode = (int)(sensor.IsVirtualSensor ? sensor.VirtualSensorDefinition.VirtualSensorCalculationType : GlobalDataContracts.VirtualSensorCalculationType.Undefined);
            this.VirtualSensorDefinitionType = (int)(sensor.IsVirtualSensor ? sensor.VirtualSensorDefinition.VirtualSensorDefinitionType : GlobalDataContracts.VirtualSensorDefinitionType.Undefined);
			this.VirtualSensorDefininition = sensor.IsVirtualSensor ? sensor.VirtualSensorDefinition.Definition : null;
			this.PullFrequencyInSec = sensor.PullFrequencyInSeconds;
            this.PullModeCommunicationType = (int)sensor.PullModeCommunicationType;
			this.PullModeDotNetType = sensor.PullModeDotNetObjectType;
			this.SensorCategory = sensor.Category;
            this.SensorValueDataType = (int)sensor.SensorValueDataType;
            this.DefaultValue = sensor.DefaultValue;
		    this.IsSynchronousPushToActuator = sensor.IsSynchronousPushToActuator;
		    this.IsActuator = sensor.IsActuator;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DbSensor"/> class.
		/// </summary>
		/// <param name="sensor">The sensor.</param>
		public DbSensor(Sensor sensor)
			: this()
		{
			FillSensorData(sensor);
		}
	}
}
