using System;
using Microsoft.SPOT;
using DeviceServer.Base;
using Microsoft.SPOT.Net.NetworkInformation;

namespace DeviceSever.AUG
{
    public class DeviceServerAug : DeviceServerBase
    {
        private const string ServerUrl = "http://192.168.1.2/GatewayService/";

        private const string SensorIdTemperature = "temperature";
        private const string SensorIdBat = "bat";
        private const string SensorIdAux = "auxil";

        public static void Main()
        {
            try
            {
                DeviceServerBase.Run(new DeviceServerAug());
            }
            catch (Exception exc)
            {
                Debug.Print(exc.ToString());
            }
        }

        /// <summary>
        /// The method returns default device config.
        /// </summary>
        /// <returns></returns>
        protected override DeviceConfig GetDefaultDeviceConfig()
        {
            DeviceConfig config = new DeviceConfig();
            config.ServerUrl = Resources.GetString(Resources.StringResources.DefaultServerUrl);
            config.TimeoutInMillis = 5000;
            config.DefaultContentParserClassName = typeof(JsonParser).AssemblyQualifiedName;
            config.IsRefreshDeviceRegOnStartup = true;
            config.UseServerTime = true;

            return config;
        }

        /// <summary>
        /// The method initializes device for a specific implementation.
        /// </summary>
        protected override void LoadDevice()
        {
            Device device = new Device();
            device.Description = Resources.GetString(Resources.StringResources.DescriptionBoard);
            device.DeviceIpEndPoint =  NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress;
            device.Id = GetDeviceIpAddress();
            device.Location = new Location();
            device.Location.Name = Resources.GetString(Resources.StringResources.DeviceLocationNameDefault);
            Device = device;
        }

        /// <summary>
        /// The method initializes sensors for a specific implementation.
        /// </summary>
        protected override void LoadSensors()
        {
            Sensors.Add(CreateSensorTemperature());
            Sensors.Add(CreateSensorAux());
            Sensors.Add(CreateSensorBat());
        }

        private Sensor CreateSensorTemperature()
        {
            Sensor sensor = new Sensor();
            sensor.Id = SensorIdTemperature;
            sensor.InternalId = 1;
            sensor.DeviceId = Device.Id;
            sensor.Category = Resources.GetString(Resources.StringResources.DefaultSensorCategoryName);
            sensor.Description = Resources.GetString(Resources.StringResources.DescriptionSensorTemperature);
            sensor.PullModeCommunicationType = PullModeCommunicationType.REST;
            sensor.SensorDataRetrievalMode = SensorDataRetrievalMode.Both;
            sensor.UnitSymbol = Resources.GetString(Resources.StringResources.SensorUnitSymbolTemperature);;
            sensor.SensorValueDataType = SensorValueDataType.Decimal;
            sensor.ValueGetter = GetTemperatureSensorCurrentValue;

            //Default config
            SensorConfig config = new SensorConfig();
            config.Id = sensor.Id;
            config.IsCoalesce = false;
            config.KeepHistory = true;
            config.KeepHistoryMaxRecords = 10;
            config.ScanFrequencyInMillis = 5000;
            config.ServerUrl = Resources.GetString(Resources.StringResources.DefaultServerUrl);
            config.SensorDataRetrievalMode = SensorDataRetrievalMode.Push;
            config.UseLocalStore = true;
            sensor.Config = config;

            return sensor;
        }

        private Sensor CreateSensorAux()
        {
            Sensor sensor = new Sensor();
            sensor.Id = SensorIdAux;
            sensor.InternalId = 2;
            sensor.DeviceId = Device.Id;
            sensor.Category = Resources.GetString(Resources.StringResources.DefaultSensorCategoryName);
            sensor.Description = Resources.GetString(Resources.StringResources.DescriptionSensorAux);
            sensor.PullModeCommunicationType = PullModeCommunicationType.REST;
            sensor.SensorDataRetrievalMode = SensorDataRetrievalMode.Both;
            sensor.UnitSymbol = Resources.GetString(Resources.StringResources.SensorUnitSymbolAux);;
            sensor.SensorValueDataType = SensorValueDataType.Decimal;
            sensor.ValueGetter = GetAuxSensorCurrentValue;

            //Default config
            SensorConfig config = new SensorConfig();
            config.Id = sensor.Id;
            config.IsCoalesce = false;
            config.KeepHistory = true;
            config.KeepHistoryMaxRecords = 5;
            config.ScanFrequencyInMillis = 8000;
            config.ServerUrl = Resources.GetString(Resources.StringResources.DefaultServerUrl);
            config.SensorDataRetrievalMode = SensorDataRetrievalMode.Pull;
            config.UseLocalStore = false;
            sensor.Config = config;

            return sensor;
        }

        private Sensor CreateSensorBat()
        {
            Sensor sensor = new Sensor();
            sensor.Id = SensorIdBat;
            sensor.InternalId = 3;
            sensor.DeviceId = Device.Id;
            sensor.Category = Resources.GetString(Resources.StringResources.DefaultSensorCategoryName);
            sensor.Description = Resources.GetString(Resources.StringResources.DescriptionSensorBat);
            sensor.PullModeCommunicationType = PullModeCommunicationType.REST;
            sensor.SensorDataRetrievalMode = SensorDataRetrievalMode.Push;
            sensor.UnitSymbol = Resources.GetString(Resources.StringResources.SensorUnitSymbolBat);;
            sensor.SensorValueDataType = SensorValueDataType.Decimal;
            sensor.ValueGetter = GetBatSensorCurrentValue;

            //Default config
            SensorConfig config = new SensorConfig();
            config.Id = sensor.Id;
            config.IsCoalesce = false;
            config.KeepHistory = true;
            config.KeepHistoryMaxRecords = 10;
            config.ScanFrequencyInMillis = 10000;
            config.ServerUrl = Resources.GetString(Resources.StringResources.DefaultServerUrl);
            config.SensorDataRetrievalMode = SensorDataRetrievalMode.Push;
            config.UseLocalStore = false;
            sensor.Config = config;

            return sensor;
        }

        private SensorData GetTemperatureSensorCurrentValue()
        {
            return new SensorData() { GeneratedWhen = DateTime.Now, Value = TahoeII.Tsc2046.ReadTemperature().ToString() };
        }

        private SensorData GetAuxSensorCurrentValue()
        {
            return new SensorData() { GeneratedWhen = DateTime.Now, Value = TahoeII.Tsc2046.ReadAux().ToString() };
        }

        private SensorData GetBatSensorCurrentValue()
        {
            return new SensorData() { GeneratedWhen = DateTime.Now, Value = TahoeII.Tsc2046.ReadBat().ToString() };
        }
    }
}
