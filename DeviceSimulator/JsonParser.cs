using System;
using System.Collections;
using System.Globalization;
using System.Text;
using fastJSON;

namespace DeviceServer.Simulator
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
    ""Measures"": [{1}]
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
""Category"": ""{7}"",
""PullFrequencyInSeconds"": {8},
""ShallSensorDataBePersisted"": {9},
""PersistDirectlyAfterChange"": {10},
""PullModeDotNetObjectType"": ""{11}"",
""PushModeCommunicationType"": {12},
""IsSynchronousPushToActuator"": {14},
""IsActuator"": {15},
""Config"": {13}";

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
    ""IsRefreshDeviceRegOnStartup"": {2},
    ""TimeoutInMillis"": {5},
    ""DefaultContentType"": ""{4}"",
    ""SensorConfigs"":
    [
        {6}
    ],
    ""UseServerTime"": {3},
    ""UseDeviceDisplay"": {7}
}}";
        #endregion

        #region IContentParser...
        public OperationResult ParseOperationResult(string content)
        {
            try
            {
                return (OperationResult)JSON.Instance.ToObject(content, typeof(OperationResult));
            }
            catch(Exception exc)
            {
                DeviceServerSimulator.RunningInstance.LogError(exc, Properties.Resources.ExceptionFailedParsingJsonOperationResult, content);
                return null;
            }
        }

        public SensorData ParseSensorData(string content)
        {
            try
            {
                return JSON.Instance.ToObject(content, typeof(SensorData)) as SensorData;
            }
            catch (Exception exc)
            {
                DeviceServerSimulator.RunningInstance.LogError(exc, Properties.Resources.ExceptionFailedParsingJsonOperationResult, content);
                return null;
            }
        }

        public PutActuatorDataRequest ParsePutActuatorDataRequest(string content)
        {
            try
            {
                return JSON.Instance.ToObject(content, typeof(PutActuatorDataRequest)) as PutActuatorDataRequest;
            }
            catch (Exception exc)
            {
                DeviceServerSimulator.RunningInstance.LogError(exc, Properties.Resources.ExceptionFailedParsingJsonOperationResult, content);
                return null;
            }
        }

        public string CreateGetSensorsResponseContent(string deviceId, ArrayList sensors)
        {
            return String.Format(GetSensorsResultTemplate, "true", String.Empty, JsonParser.EncodeSensors(sensors)); 
        }

        public string CreateGetErrorLogResponseContent(ICollection errorLog)
        {
            StringBuilder builder = new StringBuilder();
            IEnumerator enumerator = errorLog.GetEnumerator();
            foreach (ErrorLogEntry next in errorLog)
            {
                builder.Append(String.Format(ErrorLogEntryTemplate, next.Timestamp, next.Message));
                builder.AppendLine(", ");
            }

            if (builder.Length > 0)
            {
                //Cut the trailing comma
                builder.Remove(builder.Length - 2, 2);
            }

            return String.Format(ArrayTemplate, builder.ToString());
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
            StringBuilder data = new StringBuilder();
            JsonParser.FillInEncodedSensorValueList(data, sensor);

            return String.Format(GetSensorDataResultTemplate, "true", String.Empty, 
                String.Format(MultipleSensorDataTemplate, sensor.Id, data.ToString()));
        }

        public string CreateGetLastSensorsValuesResponse(ArrayList sensors){
            return String.Format(GetMultipleSensorDataResultTemplate, "true", String.Empty, JsonParser.EncodeAllSensorsValuesList(sensors));
        }

        public string CreatePutSensorsRequestContent(ArrayList sensors)
        {
            return ArrayTemplate.Replace("{0}", EncodeSensors(sensors));
        }

        public string CreatePutDeviceRequestContent(Device device)
        {
            string locationString = device.Location == null ? String.Empty :
                String.Format(LocationTemplate, device.Location.Name,
                device.Location.Latitude.ToString(CultureInfo.InvariantCulture), 
                device.Location.Longitude.ToString(CultureInfo.InvariantCulture), 
                device.Location.Elevation.ToString(CultureInfo.InvariantCulture));

            return String.Format(DeviceTemplate, device.Id, device.Description, device.DeviceIpEndPoint, locationString);
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
        /// The method creates request content for putting multiple sensor data to the server.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string PutSensorDataRequest(MultipleSensorData[] data)
        {
            return JsonParser.EncodeSensorData(data);
        }

        /// <summary>
        /// The method creates response content for getting the device's config data.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public string CreateGetDeviceConfigResponseContent(DeviceConfig config)
        {

            StringBuilder sensorConfigBuilder = new StringBuilder();
            if (config.SensorConfigs.Count > 0)
            {
                for (int i = 0; i < config.SensorConfigs.Count; i++)
                {
                    sensorConfigBuilder.Append(EncodeSensorConfig((SensorConfig)config.SensorConfigs[i]));
                    sensorConfigBuilder.Append(",\n");
                }
                sensorConfigBuilder.Remove(sensorConfigBuilder.Length - 2, 2);
            }

            return String.Format(GetDeviceConfigTemplate,
                //Server URL
                config.ServerUrl,
                //Server MIME content type
                config.ServerContentType,
                //PropertyIsRefreshDeviceRegOnStartup
                config.IsRefreshDeviceRegOnStartup,
                //PropertyUseServerTime
                config.UseServerTime,
                //PropertyDefaultContentType
                config.DefaultContentType,
                //TimeoutInMillis
                config.TimeoutInMillis,
                //Sensor configs
                sensorConfigBuilder.ToString(),
                //UseDeviceDisplay
                config.UseDeviceDisplay
                    );
        }

        /// <summary>
        /// The method parses the request content for the encoded <see cref="DeviceConfig"/> object.
        /// </summary>
        /// <param name="requestContent"></param>
        /// <returns></returns>
        public DeviceConfig ParseDeviceConfig(string requestContent)
        {
            return (DeviceConfig)JSON.Instance.ToObject(requestContent, typeof(DeviceConfig));
        }

        public string CreateOperationResultResponse(bool success, string message)
        {
            return String.Format(OperationResultTemplate, success.ToString().ToLower(), message);
        }

        /// <summary>
        /// The property getter returns the MIME content type supported by specific parser implementation.
        /// </summary>
        public string MimeContentType { get { return CommunicationConstants.ContentTypeApplicationJson; } }
        #endregion

        internal static string EncodeSensor(Sensor sensor)
        {
            return String.Format(SensorTemplate,
                sensor.DeviceId,
                sensor.Id,
                sensor.Description,
                sensor.UnitSymbol,
                (Int64)sensor.SensorDataRetrievalMode,
                (Int64)sensor.SensorValueDataType,
                (Int64)sensor.PullModeCommunicationType,
                sensor.Category,
                sensor.Config == null ? 0 : sensor.Config.ScanFrequencyInMillis / 1000,
                sensor.ShallSensorDataBePersisted.ToString().ToLower(),
                sensor.PersistDirectlyAfterChange.ToString().ToLower(),
                sensor.PullModeDotNetObjectType,
                (Int64)sensor.PushModeCommunicationType,
                EncodeSensorConfig(sensor.Config),
                sensor.IsSynchronousPushToActuator.ToString().ToLower(),
                sensor.IsActuator.ToString().ToLower());
        }

        internal static string EncodeSensorConfig(SensorConfig sensorConfig)
        {
            if (sensorConfig == null)
            {
                return "null";
            }
            else
            {
                StringBuilder builder = new StringBuilder();
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
            return String.Format(SensorValueTemplate, data.GeneratedWhen.ToString(DateTimeStringFormat), JsonParser.EncodeValue(data.Value, expectedDataType),
                data.CorrelationId);
        }


        internal static string EncodeSensorData(SensorData[] data, SensorValueDataType expectedDataType)
        {
            StringBuilder sensorDataContent = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sensorDataContent.Append(String.Format(SensorValueTemplate, data[i].GeneratedWhen.ToString(DateTimeStringFormat),
                                                       JsonParser.EncodeValue(data[i].Value, expectedDataType),
                                                       data[i].CorrelationId));
                sensorDataContent.Append(",\n");
            }
            sensorDataContent.Remove(sensorDataContent.Length - 2, 2);

            return String.Format(ArrayTemplate, sensorDataContent);
        }


        internal static string EncodeSensorData(MultipleSensorData[] data)
        {
            StringBuilder multipleSensorDataContent = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                StringBuilder sensorDataContent = new StringBuilder();
                if (data[i].Measures != null)
                {
                    for (int j = 0; i < data[i].Measures.Length; j++)
                    {
                        sensorDataContent.Append(String.Format(SensorValueTemplate,
                                                               data[i].Measures[j].GeneratedWhen.ToString(DateTimeStringFormat),
                                                               JsonParser.EncodeValue(data[i].Measures[j].Value, SensorValueDataType.AsIs),
                                                               data[i].Measures[j].CorrelationId));
                        sensorDataContent.Append(",\n");
                    }
                    sensorDataContent.Remove(sensorDataContent.Length - 2, 2);
                }
                multipleSensorDataContent.Append(String.Format(MultipleSensorDataTemplate, data[i].SensorId,
                                                               String.Format(ArrayTemplate, sensorDataContent)));
                multipleSensorDataContent.Append(",\n");
            }

            multipleSensorDataContent.Remove(multipleSensorDataContent.Length - 2, 2);

            return String.Format(ArrayTemplate, multipleSensorDataContent);
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
            StringBuilder result = new StringBuilder();
            FillInEncodedSensorValueList(result, sensor);
            return String.Format(MultipleSensorDataTemplate, sensor.Id, result.ToString());
        }

        internal static string EncodeAllSensorsValuesList(ArrayList sensors)
        {
            StringBuilder builderList = new StringBuilder();

            //Add the first
            if (sensors.Count > 0)
            {
                builderList.AppendLine(JsonParser.EncodeSingleSensorValues((Sensor)sensors[0]));
            }

            //Add the rest
            if (sensors.Count > 1)
            {
                for (int i = 1; i < sensors.Count; i++)
                {
                    builderList.AppendLine(",");
                    builderList.AppendLine(JsonParser.EncodeSingleSensorValues((Sensor)sensors[i]));
                }
            }

            return builderList.ToString();
        }

        internal static void FillInEncodedSensorValueList(StringBuilder result, Sensor sensor)
        {
            SensorData data = sensor.ReadSensorValue();
            result.AppendLine(String.Format(SensorValueTemplate, data.GeneratedWhen.ToString(DateTimeStringFormat), JsonParser.EncodeValue(data.Value, sensor.SensorValueDataType),
                data.CorrelationId));

            if (sensor.StoredValues != null)
            {
                object[] storedValues = sensor.StoredValues.ToArray();
                foreach (SensorData nextData in storedValues)
                {
                    result.AppendLine(",");
                    result.AppendLine(String.Format(SensorValueTemplate, data.GeneratedWhen.ToString(DateTimeStringFormat), JsonParser.EncodeValue(nextData.Value, sensor.SensorValueDataType),
                        data.CorrelationId));
                }
            }
        }

        internal static string EncodeSensors(ArrayList sensors)
        {
            StringBuilder builder = new StringBuilder();

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
