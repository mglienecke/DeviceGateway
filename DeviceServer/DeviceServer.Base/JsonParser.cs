using System;
using Microsoft.SPOT;
using NetMf.CommonExtensions;
using System.Collections;
using netduino.helpers.Helpers;
using DeviceServer.Base.Properties;
using Microsoft.SPOT.Hardware;

namespace DeviceServer.Base
{
    /// <summary>
    /// The class provides utility methods for creating JSON responses and requests.
    /// </summary>
    public sealed class JsonParser: IContentParser
    {
        private static readonly string TrueString = Boolean.TrueString.ToLower();

        private const string DateTimeStringFormat = "yyyy-MM-dd HH:mm:ss.fff";

        #region JSON responses...
        private const string GetSensorsResultTemplate =
@"{{
    ""Success"": {0},
    ""ErrorMessages"": ""{1}"",
    ""SensorList"": [{2}]
}}";

        private const string GetSensorDataResultTemplate =
@"{{
    ""Success"": {0},
    ""ErrorMessages"": ""{1}"",
    ""Data"": {2}
}}";

        private const string MultipleSensorDataTemplate =
@"{{
    ""SensorId"": ""{0}"",
    ""Measures"": {1}
}}";

        private const string StoreSensorDataRequestTemplate =
@"{{
    ""DeviceId"": ""{0}"",
    ""Data"": {1}
}}";

        private const string OperationResultTemplate =
@"{{
    ""Success"": {0},
    ""ErrorMessages"": ""{1}""
}}";

        private const string GetMultipleSensorDataResultTemplate =
@"{{
    ""Success"":{0},
    ""ErrorMessages"": ""{1}"",
    ""SensorDataList"":[{2}]
}}";

        private const string SingleSensorValuesTemplate =
@"{{
    ""SensorData"": 
        {0}
}}";


        private const string SensorValueTemplate =
@"{{
    ""GeneratedWhen"": ""{0}"",
    ""Value"": {1},
    ""CorrelationId"": ""{2}""
}}";

        private const string ArrayTemplate =
@"[{0}]";

        private const string DeviceTemplate =
@"{{
    ""Id"": ""{0}"",
    ""Description"":""{1}"",
    ""DeviceIpEndPoint"": ""{2}"",
    ""Location"": {3}
}}";

        private const string LocationTemplate =
@"{{
    ""Name"": ""{0}"",
    ""Latitude"": {1},
    ""Longitude"": {2},
    ""Elevation"": {3}
}}";

        private const string SensorTemplate =
@"""DeviceId"": ""{0}"",
""Id"":""{1}"",
""Description"":""{2}"",
""IsVirtualSensor"": false,
""UnitSymbol"":""{3}"",
""SensorDataRetrievalMode"": {4},
""SensorValueDataType"": {5},
""PullModeCommunicationType"": {6},
""PushModeCommunicationType"": {13},
""Category"": ""{7}"",
""PullFrequencyInSeconds"": {8},
""ShallSensorDataBePersisted"": {9},
""PersistDirectlyAfterChange"": {10},
""PullModeDotNetObjectType"": ""{11}"",
""IsSynchronousPushToActuator"": {14},
""IsActuator"": {15},
""Config"": {12}";

        private const string SensorConfigTemplate =
@"""Id"": ""{0}"",
""PushUrl"": ""{1}"",
""IsCoalesce"": {2},
""KeepHistory"": {3},
""KeepHistoryMaxRecords"": {4},
""UseLocalStore"": {5},
""ScanFrequencyInMillis"": {6}";

        private const string ErrorLogEntryTemplate =
@"{{
    ""Timestamp"":""{0}"",
    ""Message"":""{1}""
}}";

        private const string GetDeviceConfigTemplate =
@"{{
    ""ServerUrl"": ""{0}"",
    ""ServerContentType"": ""{1}"", 
    ""ServerCommType"":""{15}"",
    ""SensorDataExchangeCommTypes"":""{16}"",
    ""IsRefreshDeviceRegOnStartup"": {2},
    ""TimeoutInMillis"": {5},
    ""DefaultContentType"": ""{4}"",
    ""SensorConfigs"":
    [
        {6}
    ],
    ""UseServerTime"": {3},
    ""UseDeviceDisplay"": {7},
    ""CndepConfig"": 
    {{
        ""ServerAddress"": ""{8}"",
        ""RemoteServerPort"": {9},
        ""LocalServerPort"": {10},
        ""LocalClientPort"": {11},
        ""ServerContentType"": ""{12}"",
        ""TimeoutInMillis"": {13},
        ""RequestRetryCount"": {14}
    }}
}}";
        #endregion

        #region IContentParser...
        public OperationResult ParseOperationResult(string content)
        {
            try
            {
                JSONParser parser = new JSONParser();
                Hashtable parsed = parser.Parse(content);

                OperationResult result = new OperationResult();
                //Success - must be there!
                string successField = ((String)parsed[OperationResult.PropertyNameSuccess]);
                result.Success = TrueString == successField;
                //Small hack for unexpected "TrUe" strings
                if (!result.Success)
                    result.Success = TrueString == successField.ToLower();

                //ErrorMessages
                object temp = parsed[OperationResult.PropertyNameErrorMessages];
                if (temp != null) result.ErrorMessages = (string)temp;

                //Timestamp
                temp = parsed[OperationResult.PropertyNameTimestamp];
                if (temp != null) result.Timestamp = Int64.Parse((string)temp);

                return result;
            }
            catch(Exception exc)
            {
                DeviceServerBase.RunningInstance.LogError(exc, Properties.Resources.GetString(Properties.Resources.StringResources.ExceptionFailedParsingJsonOperationResult), content);
                return null;
            }
        }

        public SensorData ParseSensorData(string content)
        {
            try
            {
                JSONParser parser = new JSONParser();
                Hashtable parsed = parser.Parse(content);
                string value;
                SensorData result = new SensorData();
                if (parser.Find(SensorData.PropertyValue, parsed, out value))
                {
                    result.Value = value;
                    return result;
                }
                else
                {
                    return null;
                }

                //There is no parsing for the GeneratedWhen property, as it is not needed in this context
            }
            catch (Exception exc)
            {
                DeviceServerBase.RunningInstance.LogError(exc, Properties.Resources.GetString(Properties.Resources.StringResources.ExceptionFailedParsingJsonOperationResult), content);
                return null;
            }
        }

        public string CreateGetSensorsResponseContent(string deviceId, ArrayList sensors)
        {
            return StringUtility.Format(GetSensorsResultTemplate, "true", String.Empty, JsonParser.EncodeSensors(sensors)); 
        }

        public string CreateGetErrorLogResponseContent(ICollection errorLog)
        {
            NetMf.CommonExtensions.StringBuilder builder = new NetMf.CommonExtensions.StringBuilder();
            IEnumerator enumerator = errorLog.GetEnumerator();
            foreach (ErrorLogEntry next in errorLog)
            {
                builder.Append(StringUtility.Format(ErrorLogEntryTemplate, next.Timestamp, next.Message));
                builder.AppendLine(", ");
            }

            if (builder.Length > 0)
            {
                //Cut the trailing comma
                builder.Remove(builder.Length - 2, 2);
            }

            return StringUtility.Format(ArrayTemplate, builder.ToString());
        }

        public string CreateErrorResponseContent(string errorMessage)
        {
            return errorMessage;
        }

        public string CreateGetCurrentSensorDataResponseContent(SensorData data, SensorValueDataType dataType)
        {
            return JsonParser.EncodeSensorData(data, dataType);
        }

        public string CreateGetLastSensorValuesResponse(Sensor sensor)
        {
            NetMf.CommonExtensions.StringBuilder data = new NetMf.CommonExtensions.StringBuilder();
            JsonParser.FillInEncodedSensorValueList(data, sensor);

            return StringUtility.Format(GetSensorDataResultTemplate, "true", String.Empty, 
                StringUtility.Format(MultipleSensorDataTemplate, sensor.Id, StringUtility.Format(ArrayTemplate, data.ToString())));
        }

        public string CreateGetLastSensorsValuesResponse(ArrayList sensors){
            return StringUtility.Format(GetMultipleSensorDataResultTemplate, "true", String.Empty, JsonParser.EncodeAllSensorsValuesList(sensors));
        }

        public string CreatePutSensorsRequestContent(ArrayList sensors)
        {
            return ArrayTemplate.Replace("{0}", EncodeSensors(sensors));
        }

        public string CreatePutDeviceRequestContent(Device device)
        {
            string locationString = device.Location == null ? String.Empty :
                StringUtility.Format(LocationTemplate, device.Location.Name,
                device.Location.Latitude, device.Location.Longitude, device.Location.Elevation);

            return StringUtility.Format(DeviceTemplate, device.Id, device.Description, device.DeviceIpEndPoint, locationString);
        }

        /// <summary>
        /// The method creates request content for putting sensor data to the server.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public string PutSensorDataRequest(SensorData[] data, SensorValueDataType dataType)
        {
            return JsonParser.EncodeSensorData(data, dataType);
        }

        /// <summary>
        /// The method creates response content for getting the device's config data.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public string CreateGetDeviceConfigResponseContent(DeviceConfig config)
        {

            NetMf.CommonExtensions.StringBuilder sensorConfigBuilder = new NetMf.CommonExtensions.StringBuilder();
            if (config.SensorConfigs.Count > 0)
            {
                for (int i = 0; i < config.SensorConfigs.Count; i++)
                {
                    sensorConfigBuilder.Append(EncodeSensorConfig((SensorConfig)config.SensorConfigs[i]));
                    sensorConfigBuilder.Append(",\n");
                }
                sensorConfigBuilder.Remove(sensorConfigBuilder.Length - 2, 2);
            }

            return StringUtility.Format(GetDeviceConfigTemplate,
                //Server URL
                config.ServerUrl,
                //Server MIME content type
                config.ServerContentType,
                //PropertyIsRefreshDeviceRegOnStartup
                config.IsRefreshDeviceRegOnStartup.ToString().ToLower(),
                //PropertyUseServerTime
                config.UseServerTime.ToString().ToLower(),
                //PropertyDefaultContentType
                config.DefaultContentType,
                //TimeoutInMillis
                config.TimeoutInMillis,
                //Sensor configs
                sensorConfigBuilder.ToString(),
                //UseDeviceDisplay
                config.UseDeviceDisplay.ToString().ToLower(),
                //CNDEP
                //Server address
                config.CndepConfig.ServerAddress,
                //Remote server port,
                config.CndepConfig.RemoteServerPort,
                //Local server port
                config.CndepConfig.LocalServerPort,
                //Local client port,
                config.CndepConfig.LocalClientPort,
                //Server content type.
                config.CndepConfig.ServerContentType,
                //Timeout in millis,
                config.CndepConfig.TimeoutInMillis,
                //Retry count
                config.CndepConfig.RequestRetryCount,
                //Server comm type
                config.ServerCommType,
                //Sensor data exchange comm types
                config.SensorDataExchangeCommTypes
                    );
        }

        /// <summary>
        /// The method parses the request content for the encoded <see cref="DeviceConfig"/> object.
        /// </summary>
        /// <param name="requestContent"></param>
        /// <returns></returns>
        public DeviceConfig ParseDeviceConfig(string requestContent)
        {
            if (requestContent == "\"\"")
                return null;

            JSONParser parser = new JSONParser();
            parser.AutoCasting = false;
            Hashtable parsed = parser.Parse(requestContent);

            //Special case to reset the device configuration
            if (parsed.Count == 0)
            {
                return null;
            }

            DeviceConfig deviceConfig = CreateDeviceConfig(parsed);

            ArrayList sensorConfigListRaw;
            ArrayList sensorConfigList = new ArrayList();
            if (parser.Find(DeviceConfig.PropertySensorConfigs, parsed, out sensorConfigListRaw))
            {
                foreach (Hashtable nextConfigRaw in sensorConfigListRaw)
                {
                    deviceConfig.SensorConfigs.Add(CreateSensorConfig(nextConfigRaw));
                }
            }

            return deviceConfig;
        }

        public string CreateOperationResultResponse(bool success, string message)
        {
            return StringUtility.Format(OperationResultTemplate, success.ToString().ToLower(), message);
        }

        /// <summary>
        /// The property getter returns the MIME content type supported by specific parser implementation.
        /// </summary>
        public string MimeContentType { get { return CommunicationConstants.ContentTypeApplicationJson; } }

        /// <summary>
        /// The methid create request content for putting multiple sensor data to the server.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string StoreSensorDataRequest(StoreSensorDataRequest request)
        {
            return StringUtility.Format(StoreSensorDataRequestTemplate, request.DeviceId, 
                EncodeMultipleSensorData(request.Data));
        }
        #endregion

        internal static SensorConfig CreateSensorConfig(Hashtable data)
        {
            SensorConfig config = new SensorConfig();

            //ID
            string valueStr;
            if (JSONParser.Find(SensorConfig.PropertyNameId, data, false, out valueStr))
            {
                config.Id = valueStr;
            }
            else
            {
                throw new Exception(StringUtility.Format(Resources.GetString(Resources.StringResources.ExceptionMissingJSONSensorConfigProperty),
                    SensorConfig.PropertyNameId));
            }

            //Server
            if (JSONParser.Find(SensorConfig.PropertyNameServerUrl, data, false, out valueStr))
            {
                config.ServerUrl = valueStr;
            }
            else
            {
                /*
                //If the sensor should be able to PUSH, we need the value!
                if (config.SensorDataRetrievalMode != Base.SensorDataRetrievalMode.Pull)
                {
                    throw new Exception(StringUtility.Format(Resources.GetString(Resources.StringResources.ExceptionMissingJSONSensorConfigProperty),
                        SensorConfig.PropertyNameServerUrl));
                }
                else
                {
                    config.ServerUrl = null;
                }
                 */
            }

            //IsCoalesce
            bool valueBool;
            if (JSONParser.Find(SensorConfig.PropertyNameIsCoalesce, data, false, out valueBool))
            {
                config.IsCoalesce = valueBool;
            }
            else
            {
                config.IsCoalesce = false;
            }

            //UseLocalStore
            if (JSONParser.Find(SensorConfig.PropertyNameUseLocalStore, data, false, out valueBool))
            {
                config.UseLocalStore = valueBool;
            }
            else
            {
                config.UseLocalStore = false;
            }

            //KeepHistory
            if (JSONParser.Find(SensorConfig.PropertyNameKeepHistory, data, false, out valueBool))
            {
                config.KeepHistory = valueBool;
            }
            else
            {
                config.KeepHistory = false;
            }

            //KeepHistoryeMaxRecords
            Int32 valueInt;
            if (JSONParser.Find(SensorConfig.PropertyNameKeepHistoryMaxRecords, data, false, out valueInt))
            {
                config.KeepHistoryMaxRecords = valueInt;
            }
            else
            {
                if (config.KeepHistory == true)
                {
                    throw new Exception(StringUtility.Format(Resources.GetString(Resources.StringResources.ExceptionMissingJSONSensorConfigProperty),
                        SensorConfig.PropertyNameKeepHistoryMaxRecords));
                }
                else
                {
                    config.KeepHistoryMaxRecords = -1;
                }
            }

            //ScanFrequencyInMillis
            if (JSONParser.Find(SensorConfig.PropertyNameScanFrequencyInMillis, data, false, out valueInt))
            {
                config.ScanFrequencyInMillis = valueInt;
            }
            else
            {
                /*
                if (config.SensorDataRetrievalMode == SensorDataRetrievalMode.Pull && config.KeepHistory == false)
                {
                    //When PULL and no history collection - just set to 0
                    config.KeepHistoryMaxRecords = 0;
                }
                else
                {
                    throw new Exception(StringUtility.Format(Resources.GetString(Resources.StringResources.ExceptionMissingJSONSensorConfigProperty),
                        SensorConfig.PropertyNameScanFrequencyInMillis));
                }
                 */
            }

            return config;
        }

        internal static DeviceConfig CreateDeviceConfig(Hashtable data)
        {
            DeviceConfig config = new DeviceConfig();

            //Server URL
            string valueStr;
            if (JSONParser.Find(DeviceConfig.PropertyServerUrl, data, false, out valueStr))
            {
                config.ServerUrl = valueStr;
            }
            else
            {
                throw new Exception(StringUtility.Format(Resources.GetString(Resources.StringResources.ExceptionMissingJSONDeviceConfigProperty),
                    DeviceConfig.PropertyServerUrl));
            }

            //Server MIME content type
            if (JSONParser.Find(DeviceConfig.PropertyServerContentType, data, false, out valueStr))
            {
                config.ServerContentType = valueStr;
            }
            else
            {
                config.ServerContentType = CommunicationConstants.ContentTypeApplicationJson;
            }

            //Server comm type
            if (JSONParser.Find(DeviceConfig.PropertyServerCommType, data, false, out valueStr))
            {
                config.ServerCommType = valueStr;
            }
            else
            {
                config.ServerCommType = CommunicationConstants.ServerCommTypeHttpRest;
            }

            //Sensor data exchange comm type
            if (JSONParser.Find(DeviceConfig.PropertySensorDataExchangeCommTypes, data, false, out valueStr))
            {
                config.SensorDataExchangeCommTypes = valueStr;
            }
            else
            {
                config.SensorDataExchangeCommTypes = CommunicationConstants.ServerCommTypeHttpRest;
            }
            
            //PropertyIsRefreshDeviceRegOnStartup
            bool valueBool;
            if (JSONParser.Find(DeviceConfig.PropertyIsRefreshDeviceRegOnStartup, data, false, out valueBool))
            {
                config.IsRefreshDeviceRegOnStartup = valueBool;
            }
            else
            {
                config.IsRefreshDeviceRegOnStartup = true;
            }

            //PropertyUseServerTime
            if (JSONParser.Find(DeviceConfig.PropertyUseServerTime, data, false, out valueBool))
            {
                config.UseServerTime = valueBool;
            }
            else
            {
                config.UseServerTime = true;
            }

            //PropertyDefaultContentType
            if (JSONParser.Find(DeviceConfig.PropertyDefaultContentType, data, false, out valueStr))
            {
                config.DefaultContentType = valueStr;
            }
            else
            {
                //Set the default
                config.DefaultContentType = DeviceConfig.DefaultDefaultContentType;
            }

            //TimeoutInMillis
            int valueInt;
            if (JSONParser.Find(DeviceConfig.PropertyTimeoutInMillis, data, false, out valueInt))
            {
                config.TimeoutInMillis = valueInt;
            }
            else
            {
                //Set the default
                config.TimeoutInMillis = DeviceConfig.DefaultHttpRequestTimeoutInMillis;
            }

            //PropertyUseDeviceDisplay
            if (JSONParser.Find(DeviceConfig.PropertyUseDeviceDisplay, data, false, out valueBool))
            {
                config.UseDeviceDisplay = valueBool;
            }
            else
            {
                config.UseDeviceDisplay = false; ;
            }

            //CndepConfig
            Hashtable valueHash;
            if (JSONParser.Find(DeviceConfig.PropertyCndepConfig, data, false, out valueHash))
            {
                config.CndepConfig = CreateCndepConfig(valueHash);
            }
            else
            {
                config.CndepConfig = null;
            }

            return config;
        }

        internal static CndepConfig CreateCndepConfig(Hashtable data)
        {
            CndepConfig config = new CndepConfig();

            //Server address
            string valueStr;
            if (JSONParser.Find(CndepConfig.PropertyServerAddress, data, false, out valueStr))
            {
                config.ServerAddress = valueStr;
            }
            else
            {
                throw new Exception(StringUtility.Format(Resources.GetString(Resources.StringResources.ExceptionMissingJSONDeviceConfigProperty),
                    CndepConfig.PropertyServerAddress));
            }

            //Server MIME content type
            if (JSONParser.Find(CndepConfig.PropertyServerContentType, data, false, out valueStr))
            {
                config.ServerContentType = valueStr;
            }
            else
            {
                config.ServerContentType = CommunicationConstants.ContentTypeApplicationJson;
            }

            //TimeoutInMillis
            int valueInt;
            if (JSONParser.Find(CndepConfig.PropertyTimeoutInMillis, data, false, out valueInt))
            {
                config.TimeoutInMillis = valueInt;
            }
            else
            {
                //Set the default
                config.TimeoutInMillis = CndepConfig.DefaultCndepTimeoutInMillis;
            }

            //RemoteServerPort
            if (JSONParser.Find(CndepConfig.PropertyRemoteServerPort, data, false, out valueInt))
            {
                config.RemoteServerPort = valueInt;
            }
            else
            {
                //Set the default
                config.RemoteServerPort = CndepConfig.DefaultCndepRemoteServerPort;
            }

            //LocalServerPort
            if (JSONParser.Find(CndepConfig.PropertyLocalServerPort, data, false, out valueInt))
            {
                config.LocalServerPort = valueInt;
            }
            else
            {
                //Set the default
                config.LocalServerPort = CndepConfig.DefaultCndepLocalServerPort;
            }

            //LocalClientPort
            if (JSONParser.Find(CndepConfig.PropertyLocalClientPort, data, false, out valueInt))
            {
                config.LocalClientPort = valueInt;
            }
            else
            {
                //Set the default
                config.LocalClientPort = CndepConfig.DefaultCndepLocalClientPort;
            }

            //RequestRetryCount
            if (JSONParser.Find(CndepConfig.PropertyRequestRetryCount, data, false, out valueInt))
            {
                config.RequestRetryCount = valueInt;
            }
            else
            {
                //Set the default
                config.RequestRetryCount = CndepConfig.DefaultCndepRequestRetryCount;
            }

            return config;
        }


        internal static string EncodeSensor(Sensor sensor)
        {
            return StringUtility.Format(SensorTemplate,
                sensor.DeviceId,
                sensor.Id,
                sensor.Description,
                sensor.UnitSymbol,
                sensor.SensorDataRetrievalMode,
                sensor.SensorValueDataType,
                sensor.PullModeCommunicationType,
                sensor.Category,
                sensor.Config == null ? 0 : sensor.Config.ScanFrequencyInMillis / 1000,
                sensor.ShallSensorDataBePersisted.ToString().ToLower(),
                sensor.PersistDirectlyAfterChange.ToString().ToLower(),
                sensor.PullModeDotNetObjectType,
                EncodeSensorConfig(sensor.Config),
                sensor.PushModeCommunicationType,
                sensor.IsSynchronousPushToActuator,
                sensor.IsActuator);
        }

        internal static string EncodeSensorConfig(SensorConfig sensorConfig)
        {
            if (sensorConfig == null)
            {
                return "null";
            }
            else
            {
                NetMf.CommonExtensions.StringBuilder builder = new NetMf.CommonExtensions.StringBuilder();
                builder.AppendLine("{");

                builder.AppendFormat(SensorConfigTemplate,
                    sensorConfig.Id,
                    sensorConfig.ServerUrl,
                    sensorConfig.IsCoalesce.ToString().ToLower(),
                    sensorConfig.KeepHistory.ToString().ToLower(),
                    sensorConfig.KeepHistoryMaxRecords,
                    sensorConfig.UseLocalStore.ToString().ToLower(),
                    sensorConfig.ScanFrequencyInMillis);

                builder.AppendLine("}");

                return builder.ToString();
            }
        }

        internal static string EncodeSensorData(SensorData data, SensorValueDataType expectedDataType)
        {
            return StringUtility.Format(SensorValueTemplate, data.GeneratedWhen.ToString(DateTimeStringFormat), JsonParser.EncodeValue(data.Value, expectedDataType),
                data.CorrelationId);
        }

        internal static string EncodeSensorData(SensorData[] data, SensorValueDataType expectedDataType)
        {
            StringBuilder sensorDataContent = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sensorDataContent.Append(StringUtility.Format(SensorValueTemplate, data[i].GeneratedWhen.ToString(DateTimeStringFormat), JsonParser.EncodeValue(data[i].Value, expectedDataType),
                    data[i].CorrelationId));
                sensorDataContent.Append(",\n");
            }
            sensorDataContent.Remove(sensorDataContent.Length - 2, 2);

            return StringUtility.Format(ArrayTemplate, sensorDataContent);
        }

        internal static string EncodeMultipleSensorData(MultipleSensorData[] data)
        {
            StringBuilder sensorDataContent = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sensorDataContent.Append(StringUtility.Format(MultipleSensorDataTemplate, data[i].SensorId, EncodeSensorData(data[i].Measures, data[i].DataType)));
                sensorDataContent.Append(",\n");
            }
            sensorDataContent.Remove(sensorDataContent.Length - 2, 2);

            return StringUtility.Format(ArrayTemplate, sensorDataContent);
        }

        internal static string EncodeValue(string value, SensorValueDataType expectedDataType)
        {
            if (value == null)
            {
                return "null";
            }
            else
            {
                if (expectedDataType == SensorValueDataType.String)
                {
                    return @"""0""".Replace("0", value.ToString());
                }
                else
                {
                    return value;
                }
            }
        }

        internal static string EncodeSingleSensorValues(Sensor sensor)
        {
            NetMf.CommonExtensions.StringBuilder result = new NetMf.CommonExtensions.StringBuilder();
            FillInEncodedSensorValueList(result, sensor);
            return StringUtility.Format(MultipleSensorDataTemplate, sensor.Id, StringUtility.Format(ArrayTemplate, result.ToString()));
        }

        internal static string EncodeAllSensorsValuesList(ArrayList sensors)
        {
            NetMf.CommonExtensions.StringBuilder builderList = new NetMf.CommonExtensions.StringBuilder(",");

            //Add the first
            if (sensors.Count > 0)
            {
                if (((Sensor)sensors[0]).SensorDataRetrievalMode == SensorDataRetrievalMode.Both ||
                    ((Sensor)sensors[0]).SensorDataRetrievalMode == SensorDataRetrievalMode.Pull)
                    builderList.AppendLine(JsonParser.EncodeSingleSensorValues((Sensor)sensors[0]));
                else
                    builderList.Remove(0, 1);
            }

            //Add the rest
            if (sensors.Count > 1)
            {
                for (int i = 1; i < sensors.Count; i++)
                {
                    if (((Sensor)sensors[i]).SensorDataRetrievalMode == SensorDataRetrievalMode.Both ||
                    ((Sensor)sensors[i]).SensorDataRetrievalMode == SensorDataRetrievalMode.Pull)
                    {
                        builderList.AppendLine(",");
                        builderList.AppendLine(JsonParser.EncodeSingleSensorValues((Sensor)sensors[i]));
                    }
                }
            }

            builderList.Remove(0, 1);
            return builderList.ToString();
        }

        internal static void FillInEncodedSensorValueList(NetMf.CommonExtensions.StringBuilder result, Sensor sensor)
        {
            SensorData data = sensor.ReadSensorValue();
            result.AppendLine(StringUtility.Format(SensorValueTemplate, data.GeneratedWhen.ToString(DateTimeStringFormat), JsonParser.EncodeValue(data.Value, sensor.SensorValueDataType),
                data.CorrelationId));

            if (sensor.StoredValues != null)
            {
                object[] storedValues = sensor.StoredValues.ToArray();
                foreach (SensorData nextData in storedValues)
                {
                    result.AppendLine(",");
                    result.AppendLine(StringUtility.Format(SensorValueTemplate, data.GeneratedWhen.ToString(DateTimeStringFormat), JsonParser.EncodeValue(nextData.Value, sensor.SensorValueDataType),
                        nextData.CorrelationId));
                }
            }
        }

        internal static string EncodeSensors(ArrayList sensors)
        {
            NetMf.CommonExtensions.StringBuilder builder = new NetMf.CommonExtensions.StringBuilder();

            //Put the first if any
            if (sensors.Count > 0)
            {
                builder.AppendLine("{");
                builder.Append(EncodeSensor((Sensor)sensors[0]));
                builder.AppendLine("}");
            }

            //Put the rest if exist
            if (sensors.Count > 1)
            {
                for (int i = 1; i < sensors.Count; i++)
                {
                    builder.AppendLine(",");
                    builder.AppendLine("{");
                    builder.Append(EncodeSensor((Sensor)sensors[i]));
                    builder.AppendLine("}");
                }
            }

            return builder.ToString();
        }
    }
}
