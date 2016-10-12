using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviceServer.Simulator
{
    /// <summary>
    /// The class declares constants for the device-server communications.
    /// </summary>
    public sealed class CommunicationConstants
    {
        #region Server request URLs...
        public const string ServerRequestUrlRegisterDeviceSensors = "{0}/SingleDevice/{1}/MultipleSensors/";
        public const string ServerRequestUrlRegisterDevice = "{0}/SingleDevice/{1}/";
        public const string ServerRequestUrlStoreSensorData = "{0}/SingleDevice/{1}/SingleSensor/{2}/data";
        public const string ServerRequestUrlStoreMultipleSensorData = "{0}/SingleDevice/{1}/MultipleSensors/SensorData";
        #endregion

        #region Device server request URLs...
        public const string RequestGetSensorCurrentValue = "GET /Sensors/*/currentValue";
        public const string RequestGetSensorValues = "GET /Sensors/*/values";
        public const string RequestGetSensors = "GET /Sensors";
        public const string RequestPutDeviceConfig = "PUT /DeviceConfig";
        public const string RequestGetDeviceConfig = "GET /DeviceConfig";
        public const string RequestGetSensorsValues = "GET /Sensors/values";
        public const string RequestPutSensorCurrentValue = "PUT /Sensors/*/currentValue";
        public const string RequestGetErrorLog = "GET /ErrorLog";
        #endregion

        #region Content type constants...
        public const string ContentTypeApplicationJson = "application/json";
        public const string ContentTypeTextXml = "text/xml";
        public const string ContentTypeTextHtml = "text/html";
        public const string ContentTypeTextPlain = "text/plain";
        #endregion

        #region Server communication handler types...
        public const string ServerCommTypeHttpRest = "HttpRest";
        public const string ServerCommTypeWcfSoap = "WcfSoap";
        public const string ServerCommTypeCndep = "Cndep";
        #endregion


        public const char PathSplitChar = '/';
        public const int SensorIdPos = 2;
    }
}
