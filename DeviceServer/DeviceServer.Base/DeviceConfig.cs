using System;
using System.Collections;
using Microsoft.SPOT;
using netduino.helpers.Helpers;
using NetMf.CommonExtensions;
using DeviceServer.Base.Properties;
using Microsoft.SPOT.Hardware;

namespace DeviceServer.Base
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
        /// <summary>
        /// Separator char for strings that contains string lists
        /// </summary>
        public const char ListSeparatorChar = ';';

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
        /// Property name
        /// </summary>
        public const string PropertySensorDataExchangeCommTypes = "SensorDataExchangeCommTypes";
        /// <summary>
        /// Property name
        /// </summary>
        public const string PropertySensorDataExchangeCommType = "SensorDataExchangeCommType";
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
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyCndepConfig = "CndepConfig";
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
        /// The property contains name of the sensor data exchange communication type list (<see cref="ListSeparatorChar"/>-separated) (<see cref="Http"/>)
        /// </summary>
        public string SensorDataExchangeCommTypes { get; set; }

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
        public ArrayList SensorConfigs { get; private set; }

        /// <summary>
        /// The property contains a flag defining if the device should use the server's time when reporting its sensors' values.
        /// </summary>
        public bool UseServerTime{ get; set; }

        /// <summary>
        /// The property contains a flag that defines if the device's display should be used.
        /// </summary>
        public bool UseDeviceDisplay { get; set; }

        /// <summary>
        /// The property contains config data for the CNDEP comm handler.
        /// </summary>
        public CndepConfig CndepConfig { get; set; }
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
            ServerCommType = newConfig.ServerCommType;
            SensorDataExchangeCommTypes = newConfig.SensorDataExchangeCommTypes;
            ServerContentType = newConfig.ServerContentType;
            CndepConfig = (CndepConfig)newConfig.CndepConfig.Clone();
        }
    }

    /// <summary>
    /// The class encapsulates configuration data for the CNDEP server communication handler.
    /// </summary>
    public class CndepConfig : ICloneable
    {
        #region Property names...
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyServerAddress = "ServerAddress";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyRemoteServerPort = "RemoteServerPort";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyLocalServerPort = "LocalServerPort";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyLocalClientPort = "LocalClientPort";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyServerContentType = "ServerContentType";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyTimeoutInMillis = "TimeoutInMillis";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyRequestRetryCount = "RequestRetryCount";

        /// <summary>
        /// Default property value.
        /// </summary>
        public const int DefaultCndepTimeoutInMillis = 10000;
        /// <summary>
        /// Default property value.
        /// </summary>
        public const int DefaultCndepLocalServerPort = 41120;
        /// <summary>
        /// Default property value.
        /// </summary>
        public const int DefaultCndepLocalClientPort = 41121;
        /// <summary>
        /// Default property value.
        /// </summary>
        public const int DefaultCndepRemoteServerPort = 41120;
        /// <summary>
        /// Default property value.
        /// </summary>
        public const int DefaultCndepRequestRetryCount = 1;
        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CndepConfig()
        {
        }

        #region Properties...
        /// <summary>
        /// Hostname of the target CNDEP server.
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        /// Remote UDP port number of the target CNDEP server.
        /// </summary>
        public int RemoteServerPort { get; set; }

        /// <summary>
        /// Local UDP port number for receiving CNDEP requests.
        /// </summary>
        public int LocalServerPort { get; set; }

        /// <summary>
        /// Local UDP port number for receiving CNDEP responses.
        /// </summary>
        public int LocalClientPort { get; set; }

        /// <summary>
        /// Server content type (MIME type like "application/json")
        /// </summary>
        public string ServerContentType { get; set; }

        /// <summary>
        /// Server call timeout in millis.
        /// </summary>
        public int TimeoutInMillis { get; set; }

        /// <summary>
        /// Number of retries for a timeouted request.
        /// </summary>
        public int RequestRetryCount { get; set; }
        #endregion

        /// <summary>
        /// The method makes a deep copy of the object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
