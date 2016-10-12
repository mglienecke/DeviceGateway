using System;
using Microsoft.SPOT;

using AUG.SPOT.Hardware.AMI;
using DeviceServer.Base;
using Microsoft.SPOT.Net.NetworkInformation;
using DeviceServer.AUG;
using Microsoft.SPOT.Hardware;
using AUG.SPOT.Hardware;
using Microsoft.SPOT.IO;
using System.IO;

namespace DeviceServer.AUG
{
    public class Program : DeviceServerBase
    {
        private const string SensorIdTemperature = "temperature";
        private const string SensorIdTouchscreenButton = "touchscreenButton";
        private const string SensorIdLedStatus = "ledStatus";

        private Sensor sensorTemperature;
        private Sensor sensorTouchscreenButton;
        private Sensor sensorLedStatus;
        private TMP100Sensor temperature = new TMP100Sensor();
        private StatusLed ledStatus = new StatusLed();

        public static void Main()
        {
            try
            {
                DeviceServerBase server = new Program();
                DeviceServerBase.Run(server);

                if (server.Config.UseDeviceDisplay)
                {
                    OLED_SPI display = new OLED_SPI(true);
                    DeviceServerBase.StartTextDisplayApplication("AUGE lectronics AMI device server is running.");
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
            //Impossib;e to get anything.
            // CLR_E_INVALID_DRIVER on every .Exists call.
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
            config.CndepConfig = new CndepConfig();
            config.CndepConfig.ServerAddress = "192.168.1.2";
            config.CndepConfig.ServerContentType = DeviceConfig.DefaultDefaultContentType;
            config.CndepConfig.RemoteServerPort = 41120;
            config.CndepConfig.LocalServerPort = 41120;
            config.CndepConfig.LocalClientPort = 41121;
            config.CndepConfig.TimeoutInMillis = 5000;
            config.CndepConfig.RequestRetryCount = 1;
            config.TimeoutInMillis = 1000000;
            config.ServerUrl = "http://192.168.1.2/GatewayService";
            config.DefaultContentType = DeviceConfig.DefaultDefaultContentType;
            config.IsRefreshDeviceRegOnStartup = true;
            config.UseServerTime = true;

            config.ServerCommType = CommunicationConstants.ServerCommTypeHttpRest;
            config.SensorDataExchangeCommTypes = CommunicationConstants.ServerCommTypeHttpRest + DeviceConfig.ListSeparatorChar + CommunicationConstants.ServerCommTypeUdpCndep;

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
            Sensors.Add(CreateSensorTemperature(101));

            Sensors.Add(CreateSensorLedStatus());
        }

        private Sensor CreateSensorTemperature(uint id)
        {
            sensorTemperature = new Sensor();
            sensorTemperature.Id = SensorIdTemperature + id.ToString();
            sensorTemperature.InternalId = id;
            sensorTemperature.DeviceId = Device.Id;
            sensorTemperature.Category = Resources.GetString(Resources.StringResources.DefaultSensorCategoryName);
            sensorTemperature.Description = Resources.GetString(Resources.StringResources.DescriptionSensorTemperature);

            sensorTemperature.PullModeCommunicationType = PullModeCommunicationType.CNDEP;
            sensorTemperature.PushModeCommunicationType = PullModeCommunicationType.CNDEP;
            sensorTemperature.SensorDataRetrievalMode = SensorDataRetrievalMode.Pull;
            sensorTemperature.UnitSymbol = Resources.GetString(Resources.StringResources.SensorUnitSymbolTemperature); ;
            sensorTemperature.SensorValueDataType = SensorValueDataType.Decimal;
            sensorTemperature.ValueGetter = GetTemperatureSensorCurrentValue;
            sensorTemperature.ShallSensorDataBePersisted = true;

            //Default config
            SensorConfig config = new SensorConfig();
            config.Id = sensorTemperature.Id;
            config.IsCoalesce = true;
            config.KeepHistory = true;
            config.KeepHistoryMaxRecords = 10;
            config.ScanFrequencyInMillis = 1000;
            config.ServerUrl = GatewayServiceUrl;
            config.UseLocalStore = false;
            sensorTemperature.Config = config;

            return sensorTemperature;
        }

        private SensorData GetTemperatureSensorCurrentValue()
        {
            return new SensorData() { Value = temperature.Temperature().ToString().ToLower() };
        }

        private Sensor CreateSensorLedStatus()
        {
            sensorLedStatus = new Sensor();
            sensorLedStatus.Id = SensorIdLedStatus;
            sensorLedStatus.InternalId = 2;
            sensorLedStatus.DeviceId = Device.Id;
            sensorLedStatus.Category = Resources.GetString(Resources.StringResources.DefaultSensorCategoryName);
            sensorLedStatus.Description = Resources.GetString(Resources.StringResources.DescriptionSensorLedStatus);
            sensorLedStatus.PullModeCommunicationType = PullModeCommunicationType.REST;
            sensorLedStatus.PullModeCommunicationType = PullModeCommunicationType.CNDEP;
            sensorLedStatus.PushModeCommunicationType = PullModeCommunicationType.CNDEP;
            sensorLedStatus.SensorDataRetrievalMode = SensorDataRetrievalMode.Pull;
            sensorLedStatus.UnitSymbol = Resources.GetString(Resources.StringResources.SensorUnitSymbolLedStatus); ;
            sensorLedStatus.SensorValueDataType = SensorValueDataType.Bit;
            sensorLedStatus.ValueGetter = GetSensorLedStatusCurrentValue;
            sensorLedStatus.ValueSetter = SetSensorLedStatusCurrentValue;
            sensorLedStatus.ShallSensorDataBePersisted = true;

            //Default config
            SensorConfig config = new SensorConfig();
            config.Id = sensorLedStatus.Id;
            config.IsCoalesce = true;
            config.KeepHistory = true;
            config.KeepHistoryMaxRecords = 10;
            config.ScanFrequencyInMillis = 1000;
            config.ServerUrl = GatewayServiceUrl;
            config.UseLocalStore = true;
            sensorLedStatus.Config = config;

            return sensorLedStatus;
        }

        private SensorData GetSensorLedStatusCurrentValue()
        {
            return new SensorData() { Value = ledStatus.State.ToString() };
        }

        private void SetSensorLedStatusCurrentValue(object value)
        {
            if (value != null)
            {
                switch (value.ToString().ToLower())
                {
                    case "green":
                    case "2":
                        ledStatus.State = StatusLed.ColorState.Green;
                        break;
                    case "red":
                    case "1":
                        ledStatus.State = StatusLed.ColorState.Red;
                        break;
                    case "yellow":
                    case "3":
                        ledStatus.State = StatusLed.ColorState.Yellow;
                        break;
                    case "off":
                    case "0":
                        ledStatus.State = StatusLed.ColorState.Off;
                        break;
                    default:
                        Debug.Print("Unrecognized Status LED value: " + value);
                        break;
                }
            }
        }
    }
}

