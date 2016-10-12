using System;
using System.Collections;
using System.Runtime.Serialization;

namespace GlobalDataContracts
{
    /// <summary>
    /// The class encapsulates configuration data of a single sensor
    /// </summary>
    [Serializable]
    public class SensorConfig
    {
        #region Property names...
        public const string PropertyNameId = "Id";
        public const string PropertyNameSensorDataRetrievalMode = "SensorDataRetrievalMode";
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
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// The property defines the sensor's data retrieval mode. It must be either <see cref="SensorDataRetrievalMode.Pull"/> or <see cref="SensorDataRetrievalMode.Push"/>
        /// </summary>
        [DataMember]
        public SensorDataRetrievalMode SensorDataRetrievalMode { get; set; }

        /// <summary>
        /// The property contains the URL of the server the sensor is to push its data to.
        /// </summary>
        [DataMember]
        public string ServerUrl { get; set; }

        /// <summary>
        /// The property contains a flag that defines if consequent equal sensor data values should be coalesced (only the first value gets stored)
        /// </summary>
        [DataMember]
        public bool IsCoalesce { get; set; }

        /// <summary>
        /// The property contains a flag that defines if the sensor should keep history of its data or store just the last read value.
        /// </summary>
        [DataMember]
        public bool KeepHistory { get; set; }

        /// <summary>
        /// The property contains a flag that defines if the sensor should keep its history in the run-time memory or store it in the flash memory.
        /// </summary>
        [DataMember]
        public bool UseLocalStore { get; set; }

        /// <summary>
        /// The property defines the maximum number of values to be kept in its history.
        /// </summary>
        [DataMember]
        public int KeepHistoryMaxRecords { get; set; }

        /// <summary>
        /// The property defines the time period (in millis) between consequent scans of the sensor's value.
        /// </summary>
        [DataMember]
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
        /// MIME type for JSON content.
        /// </summary>
        public const string ContentTypeJson = "application/json";
        /// <summary>
        /// MIME type for XML content.
        /// </summary>
        public const string ContentTypeXml = "text/xml";

        /// <summary>
        /// MIME type for text content.
        /// </summary>
        public const string ContentTypeTextPlain = "text/plain";

        /// <summary>
        /// MIME type for HTML content.
        /// </summary>
        public const string ContentTypeTextHtml = "text/html";

        #region Property names...
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyServerUrl = "ServerUrl";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertySensorDataExchangeCommTypes = "SensorDataExchangeCommTypes";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyServerCommType = "ServerCommType";
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


        public static readonly string DefaultDefaultContentParserClassName = "DeviceServer.Base.JsonParser, DeviceServer.Base";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DeviceConfig()
        {
            DefaultContentParserClassName = DefaultDefaultContentParserClassName;
            IsRefreshDeviceRegOnStartup = true;
            SensorConfigs = new ArrayList();
            TimeoutInMillis = DefaultHttpRequestTimeoutInMillis;
        }

        #region Properties...
        /// <summary>
        /// The property contains URL of the master server.
        /// </summary>
        [DataMember]
        public string ServerUrl{ get; set;}

        /// <summary>
        /// The property contains flag that specifies if the device registration has to be refreshed on device startup
        /// </summary>
        [DataMember]
        public bool IsRefreshDeviceRegOnStartup{ get; set;}

        /// <summary>
        /// The property contains the default message content parser class name.
        /// </summary>
        [DataMember]
        public string DefaultContentParserClassName{ get; set;}

        /// <summary>
        /// The property contains the HTTP request timeout value.
        /// </summary>
        [DataMember]
        public int TimeoutInMillis { get; set; }

        /// <summary>
        /// THe property contains configuration data for the device's sensors (<see cref="Sensor"/>).
        /// </summary>
        [DataMember]
        public ArrayList SensorConfigs { get; private set; }

        /// <summary>
        /// The property contains a flag defining if the device should use the server's time when reporting its sensors' values.
        /// </summary>
        [DataMember]
        public bool UseServerTime{ get; set; }

        /// <summary>
        /// The property contains a flag defining if the device display should be used for showing device server log messages.
        /// </summary>
        [DataMember]
        public bool UseDeviceDisplay { get; set; }

        /// <summary>
        /// The property contains config data for the CNDEP comm handler.
        /// </summary>
        public CndepConfig CndepConfig { get; set; }

        /// <summary>
        /// The property contains config data for the MSMQ comm handler.
        /// </summary>
        public MsmqConfig MsmqConfig { get; set; }

        /// <summary>
        /// Server communication type for handling requests.
        /// </summary>
        public string ServerCommType { get; set; }

        /// <summary>
        /// Sensor data exchange communication type for handling requests for single sensor data and, also, for sending sensor data to the server.
        /// </summary>
        public string SensorDataExchangeCommTypes { get; set; }
        #endregion
    }

    /// <summary>
    /// The class encapsulates configuration data for the CNDEP server communication handler.
    /// </summary>
    public class CndepConfig : ICloneable
    {

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

    /// <summary>
    /// The class encapsulates configuration data for the MSMQ server communication handler.
    /// </summary>
    public class MsmqConfig : ICloneable
    {

        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyInputQueueAddress = "InputQueueAddress";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyOutputQueueAddress = "OutputQueueAddress";
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
        public const int DefaultMsmqTimeoutInMillis = 10000;
        /// <summary>
        /// Default property value.
        /// </summary>
        public const int DefaultMsmqRequestRetryCount = 1;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MsmqConfig()
        {
        }

        #region Properties...
        /// <summary>
        /// Address of the input queue.
        /// </summary>
        public string InputQueueAddress { get; set; }
        /// <summary>
        /// Address of the output queue.
        /// </summary>
        public string OutputQueueAddress { get; set; }

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
