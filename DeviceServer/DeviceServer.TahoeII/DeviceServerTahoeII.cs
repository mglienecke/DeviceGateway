using System;
using Microsoft.SPOT;
using DeviceSolutions.SPOT.Hardware;
using DeviceServer.Base;
using Microsoft.SPOT.Net.NetworkInformation;
using Microsoft.SPOT.Hardware;
using DeviceServer.vTahoeII.Properties;
using Microsoft.SPOT.IO;
using System.IO;

namespace DeviceSever.vTahoeII
{
    public class DeviceServerTahoeII:DeviceServerBase
    {
        private const string SensorIdTemperature = "temperature";
        private const string SensorIdBat = "bat";
        private const string SensorIdAux = "auxil";
        private const string SensorIdSW7 = "SW7";

        private Sensor sensorSW7;

        public static void Main()
        {
            try
            {
                DeviceServerBase server = new DeviceServerTahoeII();
                DeviceServerBase.Run(server);

                if (server.Config.UseDeviceDisplay)
                {
                    DeviceServerBase.StartTextDisplayApplication("DeviceSolutions Tahoe II device server is running.");
                }
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
            config.ServerUrl = GatewayServiceUrl;
            config.TimeoutInMillis = 5000;
            config.DefaultContentType = DeviceConfig.DefaultDefaultContentType;
            config.IsRefreshDeviceRegOnStartup = true;
            config.UseServerTime = true;
            config.UseDeviceDisplay = true;
            config.ServerCommType = CommunicationConstants.ServerCommTypeHttpRest;

            return config;
        }

        /*
        /// <summary>
        /// The method loads device configuration stored on a removable media (SD card).
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected override string GetConfigContentFromRemovableMedia(string fileName)
        {
            //Find the SD card volume
            var volumes = VolumeInfo.GetVolumes();
            VolumeInfo sdCardVolume = null;
            foreach (VolumeInfo volume in volumes)
            {
                if (volume.Name == "SD1")
                {
                    sdCardVolume = volume;
                    break;
                }
            }

            //Check if the config file is there
            var configFileName = Path.Combine(sdCardVolume.RootDirectory, fileName);
            if (sdCardVolume != null && File.Exists(configFileName))
            {
                using (StreamReader reader = new StreamReader(new FileStream(configFileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    return reader.ReadToEnd();
                }
            }
            else
            {
                return null;
            }
        }
         */

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
            //for (uint i = 0; i < 50; i++)
            //    Sensors.Add(CreateSensorTemperature(100 + i));

            Sensors.Add(CreateSensorTemperature(1));
            Sensors.Add(CreateSensorAux());
            Sensors.Add(CreateSensorBat());
            Sensors.Add(CreateSensorButtonSW7());
        }

        private Sensor CreateSensorTemperature(uint id)
        {
            Sensor sensor = new Sensor();
            sensor.Id = SensorIdTemperature + id.ToString();
            sensor.InternalId = id;
            sensor.DeviceId = Device.Id;
            sensor.Category = Resources.GetString(Resources.StringResources.DefaultSensorCategoryName);
            sensor.Description = Resources.GetString(Resources.StringResources.DescriptionSensorTemperature);
            sensor.PullModeCommunicationType = PullModeCommunicationType.REST;
            sensor.SensorDataRetrievalMode = SensorDataRetrievalMode.Pull;
            sensor.UnitSymbol = Resources.GetString(Resources.StringResources.SensorUnitSymbolTemperature);;
            sensor.SensorValueDataType = SensorValueDataType.Decimal;
            sensor.ValueGetter = GetTemperatureSensorCurrentValue;
            sensor.ShallSensorDataBePersisted = true;

            //Default config
            SensorConfig config = new SensorConfig();
            config.Id = sensor.Id;
            config.IsCoalesce = false;
            config.KeepHistory = true;
            config.KeepHistoryMaxRecords = 10;
            config.ScanFrequencyInMillis = 5000;
            config.ServerUrl = GatewayServiceUrl;
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
            sensor.ShallSensorDataBePersisted = true;

            //Default config
            SensorConfig config = new SensorConfig();
            config.Id = sensor.Id;
            config.IsCoalesce = false;
            config.KeepHistory = true;
            config.KeepHistoryMaxRecords = 5;
            config.ScanFrequencyInMillis = 8000;
            config.ServerUrl = GatewayServiceUrl;
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
            sensor.SensorDataRetrievalMode = SensorDataRetrievalMode.Both;
            sensor.UnitSymbol = Resources.GetString(Resources.StringResources.SensorUnitSymbolBat);;
            sensor.SensorValueDataType = SensorValueDataType.Decimal;
            sensor.ValueGetter = GetBatSensorCurrentValue;
            sensor.ShallSensorDataBePersisted = false;

            //Default config
            SensorConfig config = new SensorConfig();
            config.Id = sensor.Id;
            config.IsCoalesce = false;
            config.KeepHistory = true;
            config.KeepHistoryMaxRecords = 10;
            config.ScanFrequencyInMillis = 10000;
            config.ServerUrl = GatewayServiceUrl;
            config.UseLocalStore = false;
            sensor.Config = config;

            return sensor;
        }

        private Sensor CreateSensorButtonSW7()
        {
            sensorSW7 = new Sensor();
            sensorSW7.Id = SensorIdSW7;
            sensorSW7.InternalId = 4;
            sensorSW7.DeviceId = Device.Id;
            sensorSW7.Category = Resources.GetString(Resources.StringResources.DefaultSensorCategoryName);
            sensorSW7.Description = Resources.GetString(Resources.StringResources.DescriptionSensorButtonSW7);
            sensorSW7.PullModeCommunicationType = PullModeCommunicationType.REST;
            sensorSW7.SensorDataRetrievalMode = SensorDataRetrievalMode.PushOnChange;
            sensorSW7.UnitSymbol = Resources.GetString(Resources.StringResources.SensorUnitSymbolButtonSW7); ;
            sensorSW7.SensorValueDataType = SensorValueDataType.Bit;
            sensorSW7.ValueGetter = GetButtonSW7SensorCurrentValue;
            sensorSW7.ShallSensorDataBePersisted = true;

            InterruptPort port = new InterruptPort(TahoeII.Pins.SW7, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeHigh);
            port.OnInterrupt += new NativeEventHandler(portSW7_OnInterrupt);
            sensorSW7.AddPort(port);

            //Default config
            SensorConfig config = new SensorConfig();
            config.Id = sensorSW7.Id;
            config.IsCoalesce = false;
            config.KeepHistory = true;
            config.KeepHistoryMaxRecords = 10;
            config.ScanFrequencyInMillis = 0;
            config.ServerUrl = GatewayServiceUrl;
            config.UseLocalStore = false;
            sensorSW7.Config = config;

            return sensorSW7;
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

        private SensorData GetButtonSW7SensorCurrentValue()
        {
            var inputPort = new InputPort(TahoeII.Pins.SW7, true, Port.ResistorMode.PullUp);
            return new SensorData() { Value = inputPort.Read().ToString().ToLower() };
        }

        void portSW7_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            this.SensorRaisedInterrupt(sensorSW7, new SensorData(){Value = "true"});
        }
    }
}
