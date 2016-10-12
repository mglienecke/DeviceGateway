using System;
using Microsoft.SPOT;

using GHIElectronics.NETMF.Hardware;
using DeviceServer.Base;
using Microsoft.SPOT.Net.NetworkInformation;
using DeviceServer.ChipworkX.Properties;
using Microsoft.SPOT.Hardware;
using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT.IO;
using System.IO;
using GHIElectronics.NETMF.IO;

namespace DeviceServer.ChipworkX
{
    public class Program : DeviceServerBase
    {
        private const string SensorIdPA23Up = "PA23Up";
        private const string SensorIdLedPC5 = "LedPC5";

        private Sensor sensorPA23Up;
        private Sensor sensorLedPC5;

        public static void Main()
        {
            try
            {
                DeviceServerBase server = new Program();
                DeviceServerBase.Run(server);

                if (server.Config.UseDeviceDisplay)
                {
                    DeviceServerBase.StartTextDisplayApplication("GHIElectronics ChipworkX device server is running.");
                }
            }
            catch (Exception exc)
            {
                Debug.Print(exc.ToString());
            }
        }

        /*
        /// <summary>
        /// The method loads device configuration stored on a removable media (SD card).
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected override string GetConfigContentFromRemovableMedia(string fileName)
        {
            PersistentStorage storage = new PersistentStorage("SD");
            storage.MountFileSystem();


            //Find the SD card volume
            var volumes = VolumeInfo.GetVolumes();
            VolumeInfo sdCardVolume = null;
            foreach (VolumeInfo volume in volumes)
            {
                if (volume.Name == "SD")
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

            return null;
        }
         */

        /// <summary>
        /// The method returns default device config.
        /// </summary>
        /// <returns></returns>
        protected override DeviceConfig GetDefaultDeviceConfig()
        {
            DeviceConfig config = new DeviceConfig();
            config.ServerUrl = GatewayServiceUrl;
            config.TimeoutInMillis = 5000;
            config.DefaultContentType = typeof(JsonParser).AssemblyQualifiedName;
            config.IsRefreshDeviceRegOnStartup = true;
            config.UseServerTime = true;
            config.ServerCommType = CommunicationConstants.ServerCommTypeHttpRest;
            config.UseDeviceDisplay = true;

            return config;
        }

        /// <summary>
        /// The method initializes device for a specific implementation.
        /// </summary>
        protected override void LoadDevice()
        {
            Device device = new Device();
            device.Description = Resources.GetString(Resources.StringResources.DescriptionBoard);
            device.DeviceIpEndPoint = NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress;
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
            Sensors.Add(CreateSensorButtonPA23Up());
            Sensors.Add(CreateSensorLedPC5());
        }

        private Sensor CreateSensorButtonPA23Up()
        {
            sensorPA23Up = new Sensor();
            sensorPA23Up.Id = SensorIdPA23Up;
            sensorPA23Up.InternalId = 4;
            sensorPA23Up.DeviceId = Device.Id;
            sensorPA23Up.Category = Resources.GetString(Resources.StringResources.DefaultSensorCategoryName);
            sensorPA23Up.Description = Resources.GetString(Resources.StringResources.DescriptionSensorButtonPA23Up);
            sensorPA23Up.PullModeCommunicationType = PullModeCommunicationType.REST;
            sensorPA23Up.SensorDataRetrievalMode = SensorDataRetrievalMode.PushOnChange;
            sensorPA23Up.UnitSymbol = Resources.GetString(Resources.StringResources.SensorUnitSymbolButtonPA23Up); ;
            sensorPA23Up.SensorValueDataType = SensorValueDataType.Bit;
            sensorPA23Up.ValueGetter = GetButtonPA23UpSensorCurrentValue;
            sensorPA23Up.ShallSensorDataBePersisted = true;

            InterruptPort port = new InterruptPort(GHIElectronics.NETMF.Hardware.ChipworkX.Pin.PA23, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeHigh);
            port.OnInterrupt += new NativeEventHandler(portPA23Up_OnInterrupt);

            //Default config
            SensorConfig config = new SensorConfig();
            config.Id = sensorPA23Up.Id;
            config.IsCoalesce = false;
            config.KeepHistory = true;
            config.KeepHistoryMaxRecords = 10;
            config.ScanFrequencyInMillis = 0;
            config.ServerUrl = GatewayServiceUrl;
            config.UseLocalStore = false;
            sensorPA23Up.Config = config;

            return sensorPA23Up;
        }

        private SensorData GetButtonPA23UpSensorCurrentValue()
        {
            var inputPort = new InputPort(GHIElectronics.NETMF.Hardware.ChipworkX.Pin.PA23, true, Port.ResistorMode.PullUp);
            return new SensorData() { Value = inputPort.Read().ToString().ToLower() };
        }

        void portPA23Up_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            this.SensorRaisedInterrupt(sensorPA23Up, new SensorData() { Value = "true" });
        }

        private Sensor CreateSensorLedPC5()
        {
            sensorLedPC5 = new Sensor();
            sensorLedPC5.Id = SensorIdLedPC5;
            sensorLedPC5.InternalId = 5;
            sensorLedPC5.DeviceId = Device.Id;
            sensorLedPC5.Category = Resources.GetString(Resources.StringResources.DefaultSensorCategoryName);
            sensorLedPC5.Description = Resources.GetString(Resources.StringResources.DescriptionSensorLedPC5);
            sensorLedPC5.PullModeCommunicationType = PullModeCommunicationType.REST;
            sensorLedPC5.SensorDataRetrievalMode = SensorDataRetrievalMode.Pull;
            sensorLedPC5.UnitSymbol = Resources.GetString(Resources.StringResources.SensorUnitSymbolLedPC5); ;
            sensorLedPC5.SensorValueDataType = SensorValueDataType.Bit;
            sensorLedPC5.ValueGetter = GetSensorLedPC5CurrentValue;
            sensorLedPC5.ValueSetter = SetSensorLedPC5CurrentValue;
            sensorLedPC5.ShallSensorDataBePersisted = true;
            
            //InterruptPort port = new InterruptPort(GHIElectronics.NETMF.Hardware.ChipworkX.Pin.PC5, true, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeHigh);
            //port.OnInterrupt +=new NativeEventHandler(portLedPC5_OnInterrupt);

            //Default config
            SensorConfig config = new SensorConfig();
            config.Id = sensorLedPC5.Id;
            config.IsCoalesce = true;
            config.KeepHistory = true;
            config.KeepHistoryMaxRecords = 10;
            config.ScanFrequencyInMillis = 5000;
            config.ServerUrl = GatewayServiceUrl;
            config.UseLocalStore = true;
            sensorLedPC5.Config = config;

            return sensorLedPC5;
        }

        private SensorData GetSensorLedPC5CurrentValue()
        {
            using (var inputPort = new InputPort(GHIElectronics.NETMF.Hardware.ChipworkX.Pin.PC5, true, Port.ResistorMode.PullUp))
                return new SensorData() { Value = inputPort.Read().ToString().ToLower() };
        }

        private void SetSensorLedPC5CurrentValue(object value)
        {
            if (value == null || value.Equals(0) || value.Equals("false") ){
                using (var outputPort = new OutputPort(GHIElectronics.NETMF.Hardware.ChipworkX.Pin.PC5, false))
                    outputPort.Write(false);
            }
            else
                if ( value.Equals(1) || value.Equals("true") ){
                    using (var outputPort = new OutputPort(GHIElectronics.NETMF.Hardware.ChipworkX.Pin.PC5, false))
                        outputPort.Write(true);
                }
        }

        void portLedPC5_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            //this.SensorRaisedInterrupt(sensorLedPC5, sensorLedPC5.ValueGetter());
        }
    }
}
