using System;
using System.Collections;

namespace DeviceServer.Simulator
{
    /// <summary>
    /// The class encapsulates configuration data of a single sensor
    /// </summary>
    [Serializable]
    public class SensorConfig
    {
        #region Property names...
        public const string PropertyNameId = "Id";
        public const string PropertyNameServerUrl = "ServerUrl";
        public const string PropertyNameIsCoalesce = "IsCoalesce";
        public const string PropertyNameUseLocalStore = "UseLocalStore";
        public const string PropertyNameKeepHistory = "KeepHistory";
        public const string PropertyNameKeepHistoryMaxRecords = "KeepHistoryMaxRecords";
        public const string PropertyNameScanFrequencyInMillis = "ScanFrequencyInMillis";
        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SensorConfig()
        {
        }

        #region Properties...
        /// <summary>
        /// The property contains id of the sensor the config belongs to.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The property contains the URL of the server the sensor is to push its data to.
        /// </summary>
        public string ServerUrl { get; set; }

        /// <summary>
        /// The property contains a flag that defines if consequent equal sensor data values should be coalesced (only the first value gets stored)
        /// </summary>
        public bool IsCoalesce { get; set; }

        /// <summary>
        /// The property contains a flag that defines if the sensor should keep history of its data or store just the last read value.
        /// </summary>
        public bool KeepHistory { get; set; }

        /// <summary>
        /// The property contains a flag that defines if the sensor should keep its history in the run-time memory or store it in the flash memory.
        /// </summary>
        public bool UseLocalStore { get; set; }

        /// <summary>
        /// The property defines the maximum number of values to be kept in its history.
        /// </summary>
        public int KeepHistoryMaxRecords { get; set; }

        /// <summary>
        /// The property defines the time period (in millis) between consequent scans of the sensor's value.
        /// </summary>
        public int ScanFrequencyInMillis { get; set; }
        #endregion
    }

    /// <summary>
    /// The class encapsulates config data for a single device;
    /// </summary>
    [Serializable]
    public class DeviceConfig
    {
        #region Property name constants...
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyServerUrl = "ServerUrl";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyServerContentType = "ContentType";
        /// <summary>
        /// Property name
        /// </summary>
        public const string PropertyServerCommType    = "ServerCommType";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyIsRefreshDeviceRegOnStartup = "IsRefreshDeviceRegOnStartup";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyDefaultContentType = "DefaultContentType";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyTimeoutInMillis = "TimeoutInMillis";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertySensorConfigs = "SensorConfigs";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyUseServerTime = "UseServerTime";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyUseDeviceDisplay = "UseDeviceDisplay";
        #endregion

        /// <summary>
        /// Default HTTP request timeout in millis.
        /// </summary>
        public const int DefaultHttpRequestTimeoutInMillis = 5000;

        /// <summary>
        /// Default id
        /// </summary>
        public const uint DefaultId = 1;

        /// <summary>
        /// Default message content type;
        /// </summary>
        public const string DefaultDefaultContentType = CommunicationConstants.ContentTypeApplicationJson;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DeviceConfig()
        {
            DefaultContentType = DefaultDefaultContentType;
            IsRefreshDeviceRegOnStartup = true;
            SensorConfigs = new ArrayList();
            TimeoutInMillis = DefaultHttpRequestTimeoutInMillis;
            ServerContentType = CommunicationConstants.ContentTypeApplicationJson;
            UseDeviceDisplay = false;
        }

        #region Properties...
        /// <summary>
        /// The property contains URL of the master server.
        /// </summary>
        public string ServerUrl{ get; set;}

        /// <summary>
        /// The property contains MIME content type for server communication.
        /// </summary>
        public string ServerContentType { get; set; }

        /// <summary>
        /// The property contains name of the server communication type (<see cref="Http"/>)
        /// </summary>
        public string ServerCommType {get; set;}

        /// <summary>
        /// The property contains flag that specifies if the device registration has to be refreshed on device startup
        /// </summary>
        public bool IsRefreshDeviceRegOnStartup{ get; set;}

        /// <summary>
        /// The property contains the default message content MIME type.
        /// </summary>
        public string DefaultContentType{ get; set;}

        /// <summary>
        /// The property contains the HTTP request timeout value.
        /// </summary>
        public int TimeoutInMillis { get; set; }

        /// <summary>
        /// THe property contains configuration data for the device's sensors (<see cref="Sensor"/>).
        /// </summary>
        public ArrayList SensorConfigs { get; set; }

        /// <summary>
        /// The property contains a flag defining if the device should use the server's time when reporting its sensors' values.
        /// </summary>
        public bool UseServerTime{ get; set; }

        /// <summary>
        /// The property contains a flag that defines if the device's display should be used.
        /// </summary>
        public bool UseDeviceDisplay { get; set; }
        #endregion

        internal void Reload(DeviceConfig newConfig)
        {
            SensorConfigs.Clear();
            SensorConfigs.AddRange(newConfig.SensorConfigs.ToArray());

            ServerUrl = newConfig.ServerUrl;
            DefaultContentType = newConfig.DefaultContentType;
            IsRefreshDeviceRegOnStartup = newConfig.IsRefreshDeviceRegOnStartup;
            UseServerTime = newConfig.UseServerTime;
            TimeoutInMillis = newConfig.TimeoutInMillis;
            UseDeviceDisplay = newConfig.UseDeviceDisplay;
        }
    }
}
