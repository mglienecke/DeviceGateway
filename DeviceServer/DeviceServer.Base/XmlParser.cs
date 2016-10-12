using System;
using Microsoft.SPOT;
using System.Collections;
using NetMf.CommonExtensions;

namespace DeviceServer.Base
{
    /// <summary>
    /// The class implements the XML version of the <see cref="IContentParser"/> interface.
    /// </summary>
    public class XmlParser:IContentParser
    {
        private const string DateTimeStringFormat = "yyyy-MM-dd HH:mm:ss.fff";

        private const string XmlDocTemplate =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
{0}";

        private const string ErrorLogEntriesResponseTemplate =
@"<GetErrorLogResponse>
   {0}
</GetErrorLogResponse>";

        private const string ErrorLogEntryTemplate =
@"<ErrorLogEntry>
   <Timestamp>{0}</Timestamp>
   <Message>{1}</Message>
</ErrorLogEntry>";

        private const string SensorDataTemplate =
@"<SensorData>
   <GeneratedWhen>{0}</GeneratedWhen>
   <Value>{1}</Value>
   <SensorId>{2}</SensorId>
   <CorrelationId>{3}</CorrelationId>
</SensorData>";

        private const string GetSensorDataResultTemplate =
@"{0}
<Data>{1}</Data>";

        private const string MultipleSensorDataTemplate =
@"<SensorId>{0}</SensorId>
<Measures>{1}</Measures>";

        private const string OperationResultResponseTemplate =
@"<OperationResult>
   {0}
</OperationResult>";

        private const string OperationResultElementsTemplate =
@"<Success>{0}</Success>
<ErrorMessages>{1}</ErrorMessages>";

        private const string GetMultipleSensorDataResultTemplate =
@"<GetMultipleSensorDataResult>
   {0}
   <SensorDataList>{1}</SensorDataList>
</GetMultipleSensorDataResult>";

        #region IContentParser members...
        /// <summary>
        /// The method parses a response content containing a serialized <see cref="OperationResult"/> object.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public OperationResult ParseOperationResult(string content)
        {
            OperationResult result = new OperationResult();
            result.Success = true;
            result.Timestamp = DateTime.Now.Ticks;
            result.ErrorMessages = "DUMMY OBJECT.";
            return result;
        }

        public SensorData ParseSensorData(string content)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The method creates response content for the sensors registration data request. 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="sensors"></param>
        /// <returns></returns>
        public string CreateGetSensorsResponseContent(string deviceId, ArrayList sensors)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The method creates response content for the error log request.
        /// </summary>
        /// <param name="errorLog"></param>
        /// <returns></returns>
        public string CreateGetErrorLogResponseContent(ICollection errorLog)
        {
            StringBuilder builder = new StringBuilder();
            IEnumerator enumerator = errorLog.GetEnumerator();
            foreach (ErrorLogEntry next in errorLog)
            {
                builder.Append(StringUtility.Format(ErrorLogEntryTemplate, next.Timestamp, next.Message));
                builder.AppendLine();
            }

            return StringUtility.Format(XmlDocTemplate, StringUtility.Format(ErrorLogEntriesResponseTemplate, builder.ToString()));
        }

        /// <summary>
        /// The method creates response content for teh cases when a request fails, and an error message has to be sent back. 
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public string CreateErrorResponseContent(string errorMessage)
        {
            return errorMessage;
        }

        /// <summary>
        /// The method creates response content for the current sensor data request.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public string CreateGetCurrentSensorDataResponseContent(SensorData data, SensorValueDataType dataType)
        {
            return StringUtility.Format(SensorDataTemplate, data.GeneratedWhen.ToString(DateTimeStringFormat), data.Value, data.SensorId, data.CorrelationId);
        }

        /// <summary>
        /// The method creates response content for the last sensor data list request.
        /// </summary>
        /// <param name="sensor"></param>
        /// <returns></returns>
        public string CreateGetLastSensorValuesResponse(Sensor sensor)
        {
            StringBuilder data = new StringBuilder();
            XmlParser.FillInEncodedSensorValueList(data, sensor);

            return StringUtility.Format(GetSensorDataResultTemplate, 
                StringUtility.Format(OperationResultElementsTemplate, "true", String.Empty),
                StringUtility.Format(MultipleSensorDataTemplate, sensor.Id, data.ToString()));
        }

        /// <summary>
        /// The method creates response content for the all device's sensors last data list request
        /// </summary>
        /// <param name="sensors"></param>
        /// <returns></returns>
        public string CreateGetLastSensorsValuesResponse(ArrayList sensors)
        {
            return StringUtility.Format(GetMultipleSensorDataResultTemplate, 
                StringUtility.Format("true", String.Empty), 
                XmlParser.EncodeAllSensorsValuesList(sensors));
        }

        /// <summary>
        /// The method creates request content for the sensor registration renewal request.
        /// </summary>
        /// <param name="sensors"></param>
        /// <returns></returns>
        public string CreatePutSensorsRequestContent(ArrayList sensors) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The method creates request content for the device registration renewal request.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public string CreatePutDeviceRequestContent(Device device)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The method creates request content for putting sensor data to the server.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public string PutSensorDataRequest(SensorData[] data, SensorValueDataType dataType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The method parses the request content for the encoded <see cref="DeviceConfig"/> object.
        /// </summary>
        /// <param name="requestContent"></param>
        /// <returns></returns>
        public DeviceConfig ParseDeviceConfig(string requestContent)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The method creates response content for getting the device's config data.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public string CreateGetDeviceConfigResponseContent(DeviceConfig config)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The method creates response content with encoded <see cref="OperationResult"/> object.
        /// </summary>
        /// <param name="success"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public string CreateOperationResultResponse(bool success, string message)
        {
            return StringUtility.Format(OperationResultResponseTemplate, success.ToString().ToLower(), message);
        }

        /// <summary>
        /// The property getter returns the MIME content type supported by specific parser implementation.
        /// </summary>
        public string MimeContentType {
            get
            {
                return "text/xml";
            }
        }

        /// <summary>
        /// The methid create request content for putting multiple sensor data to the server.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string StoreSensorDataRequest(StoreSensorDataRequest request)
        {
            throw new NotImplementedException();
        }
        #endregion

        internal static void FillInEncodedSensorValueList(StringBuilder result, Sensor sensor)
        {
            SensorData data = sensor.ReadSensorValue();
            result.AppendLine(StringUtility.Format(SensorDataTemplate, data.GeneratedWhen.ToString(DateTimeStringFormat), data.Value,
                                                   data.SensorId, data.CorrelationId));

            if (sensor.StoredValues != null)
            {
                object[] storedValues = sensor.StoredValues.ToArray();
                foreach (SensorData nextData in storedValues)
                {
                    result.AppendLine(StringUtility.Format(SensorDataTemplate, data.GeneratedWhen.ToString(DateTimeStringFormat),
                                                           nextData.Value, nextData.SensorId, nextData.CorrelationId));
                }
            }
        }

        internal static string EncodeAllSensorsValuesList(ArrayList sensors)
        {
            StringBuilder builderList = new StringBuilder();

            //Add the first
            if (sensors.Count > 0)
            {
                builderList.AppendLine(XmlParser.EncodeSingleSensorValues((Sensor)sensors[0]));
            }

            //Add the rest
            if (sensors.Count > 1)
            {
                for (int i = 1; i < sensors.Count; i++)
                {
                    builderList.AppendLine(XmlParser.EncodeSingleSensorValues((Sensor)sensors[i]));
                }
            }

            return builderList.ToString();
        }

        internal static string EncodeSingleSensorValues(Sensor sensor)
        {
            StringBuilder result = new StringBuilder();
            XmlParser.FillInEncodedSensorValueList(result, sensor);
            return StringUtility.Format(MultipleSensorDataTemplate, sensor.Id, result.ToString());
        }
    }
}
