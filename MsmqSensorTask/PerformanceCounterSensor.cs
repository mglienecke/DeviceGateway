using System.Net.NetworkInformation;
using System.Threading;
using GlobalDataContracts;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using fastJSON;
using System.Messaging;

namespace MsmqSensorTask
{
    /// <summary>
    /// The class implements functionality for PUSH-ing sensor values into a DeviceGateway instance using the HTTP REST interface.
    /// </summary>
    public class PerformanceCounterSensor
    {
        #region Constants...

        /// <summary>
        /// Config parameter. The value is supposed to be in the form <see cref="PerformanceCounterConfigPropertyValueFormat"/>.
        /// </summary>
        public const string CfgPrefixPerformanceCounter = "PerfCounter.";

        /// <summary>
        /// Config parameter
        /// </summary>
        public const string CfgServerUri = "ServerUri";

        /// <summary>
        /// Config parameter - in the TimeSpan format.
        /// </summary>
        public const string CfgSensorScanningPeriod = "SensorScanningPeriod";

        /// <summary>
        /// Config parameter.
        /// </summary>
        public const string CfgDeviceId = "DeviceId";

        /// <summary>
        /// Config parameter - value: MSMQ queue address, string.
        /// </summary>
        public const string CfgMsmqInputQueueAddress = "DeviceGatewayInputQueueAddress";

        /// <summary>
        /// Config parameter - value: MSMQ queue address, string.
        /// </summary>
        public const string CfgMsmqOutputQueueAddress = "DeviceGatewayOuputQueueAddress";

        /// <summary>
        /// Config parameter - value: MSMQ queue address, string.
        /// </summary>
        public const string CfgMsmqCommHandlerInputQueueAddress = "DeviceGatewayCommHandlerInputQueueAddress";

        /// <summary>
        /// Config parameter - value: MSMQ queue address, string.
        /// </summary>
        public const string CfgMsmqCommHandlerOutputQueueAddress = "DeviceGatewayCommHandlerOutputQueueAddress";

        //private const string PerformanceCounterConfigPropertyValueFormat =
        //    "<CategoryName>|<CounterName>|<InstanceName>|<ValueType: {Raw|Calculated|Sample}>";
        private const string PerformanceCounterConfigPropertyValueFormat =
            "<CategoryName>|<CounterName>|<InstanceName>|<ValueType: {Raw|Calculated}>";

        /// <summary>
        /// Performance counter value type name.
        /// </summary>
        public const string PerfomanceCounterValueRaw = "Raw";

        /// <summary>
        /// Performance counter value type name.
        /// </summary>
        public const string PerfomanceCounterValueCalculated = "Calculated";

        /// <summary>
        /// Performance counter value type name.
        /// </summary>
        public const string PerfomanceCounterValueSample = "Sample";

        /// <summary>
        /// Timeout for a Web call in millis.
        /// </summary>
        public const int WebCallTimeout = 30000;

        public static readonly TimeSpan MsmqWaitTimeout = new TimeSpan(0, 0, 30);

        /// <summary>
        /// MIME type for HTTP REST calls.
        /// </summary>
        public const string ContentMimeType = "application/json";

        private const string UriTemplateRegisterDevice = "{0}/SingleDevice/{1}/";
        private const string UriTemplateRegisterSensors = "{0}/SingleDevice/{1}/MultipleSensors/";
        private const string UriTemplatePushSensorData = "{0}/SingleDevice/{1}/MultipleSensors/SensorData";

        #endregion

        /// <summary>
        /// Handler for outputting info log messages.
        /// </summary>
        /// <param name="message"></param>
        public delegate void WriteInfoLogHandler(string message);

        /// <summary>
        /// Handler for outputting error log messages.
        /// </summary>
        /// <param name="message"></param>
        public delegate void WriteErrorLogHandler(string message);

        /// <summary>
        /// Handler for passing retrieved sensor data to the UI.
        /// </summary>
        /// <param name="sensorData"></param>
        public delegate void SensorDataRetrievedHandler(RetrievedSensorValue sensorData);

        private string _serverUri;
        private string _deviceId;
        private Device _device;
        private Sensor[] _sensors;
        private Timer _sensorScanningTimer;
        private TimeSpan _sensorScanningPeriod;
        private Thread _actuatorReceivingThread;

        private bool _doScanSensors = false;
        private int CorrelationId { get; set; }

        /// <summary>
        /// Sensor name -> (Performance counter; Value type)
        /// </summary>
        private Dictionary<string, Tuple<PerformanceCounter, string>> _performanceCounters =
            new Dictionary<string, Tuple<PerformanceCounter, string>>();

        #region Properties...
        private string MsmqInputQueueAddress
        {
            get;
            set;
        }

        private string MsmqOutputQueueAddress
        {
            get;
            set;
        }

        private string MsmqCommHandlerInputQueueAddress
        {
            get;
            set;
        }

        private string MsmqCommHandlerOutputQueueAddress
        {
            get;
            set;
        }
        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PerformanceCounterSensor()
        {
            fastJSON.JSON.Instance.SerializeNullValues = true;
            fastJSON.JSON.Instance.ShowReadOnlyProperties = true;
            fastJSON.JSON.Instance.UseUTCDateTime = false;
            fastJSON.JSON.Instance.IndentOutput = false;
            fastJSON.JSON.Instance.UsingGlobalTypes = false;
            fastJSON.JSON.Instance.UseSerializerExtension = false;
        }


        /// <summary>
        /// The method initializes the sensors using the provided config data.
        /// </summary>
        /// <param name="settings"></param>
        public void InitSensors(NameValueCollection settings)
        {
            InitPerformanceCounters(settings);

            //Sensor URI
            _serverUri = settings[CfgServerUri];
            if (String.IsNullOrEmpty(_serverUri))
                throw new ConfigurationErrorsException(String.Format("Missing or empty {0} configuration parameter.", CfgServerUri));

            try
            {
                new Uri(_serverUri);
            }
            catch (Exception exc)
            {
                throw new ConfigurationErrorsException(String.Format("Invalid {0} configuration parameter value: {1}.", CfgServerUri,
                                                                     _serverUri));
            }

            var sensorScanningPeriodStr = settings[CfgSensorScanningPeriod];
            if (String.IsNullOrEmpty(sensorScanningPeriodStr))
                throw new ConfigurationErrorsException(String.Format("Missing or empty {0} configuration parameter.", CfgSensorScanningPeriod));

            try
            {
                _sensorScanningPeriod = TimeSpan.Parse(sensorScanningPeriodStr, CultureInfo.InvariantCulture);
            }
            catch (Exception exc)
            {
                throw new ConfigurationErrorsException(String.Format("Invalid configuration parameter {0} value {1}. Error: {2}", CfgSensorScanningPeriod,
                    sensorScanningPeriodStr, exc.Message));
            }

            //Device id 
            _deviceId = settings[CfgDeviceId];
            if (String.IsNullOrEmpty(_deviceId))
                throw new ConfigurationErrorsException(String.Format("Missing or empty {0} configuration parameter.", CfgDeviceId));

            //MSMQ stuff
            MsmqInputQueueAddress = settings[CfgMsmqInputQueueAddress];
            if (String.IsNullOrEmpty(MsmqInputQueueAddress))
                throw new ConfigurationErrorsException(String.Format("Missing or empty {0} configuration parameter.", CfgMsmqInputQueueAddress));

            MsmqOutputQueueAddress = settings[CfgMsmqOutputQueueAddress];
            if (String.IsNullOrEmpty(MsmqOutputQueueAddress))
                throw new ConfigurationErrorsException(String.Format("Missing or empty {0} configuration parameter.", CfgMsmqOutputQueueAddress));

            MsmqCommHandlerInputQueueAddress = settings[CfgMsmqCommHandlerInputQueueAddress];
            if (String.IsNullOrEmpty(MsmqInputQueueAddress))
                throw new ConfigurationErrorsException(String.Format("Missing or empty {0} configuration parameter.", CfgMsmqCommHandlerInputQueueAddress));

            MsmqCommHandlerOutputQueueAddress = settings[CfgMsmqCommHandlerOutputQueueAddress];
            if (String.IsNullOrEmpty(MsmqOutputQueueAddress))
                throw new ConfigurationErrorsException(String.Format("Missing or empty {0} configuration parameter.", CfgMsmqCommHandlerOutputQueueAddress));

            if (RegisterDevice())
            {
                WriteInfoLog("!!! Device {0} successfully registered. !!!", _deviceId);
            }
            else
            {
                return;
            }

            if (RegisterSensors())
            {
                WriteInfoLog("!!! Sensors for the device {0} successfully registered. !!!", _deviceId);
            }
            else
            {
                return;
            }
        }


        /// <summary>
        /// Start scanning the configured sensors.
        /// </summary>
        public void StartSendingSensors()
        {
            if (_sensors != null && _sensors.Length > 0)
            {
                _doScanSensors = true;
                _sensorScanningTimer = new Timer(ScanSensors, _sensors, TimeSpan.Zero, _sensorScanningPeriod);
            }
        }

        /// <summary>
        /// Start scanning the configured sensors.
        /// </summary>
        public void StartReceivingSensors()
        {
            if (_sensors != null && _sensors.Length > 0)
            {
                _actuatorReceivingThread = new Thread(ReadGatewaySensors);
                _actuatorReceivingThread.IsBackground = true;
                _actuatorReceivingThread.Start();
            }
        }


        /// <summary>
        /// Stop scanning and sending the configured sensors.
        /// </summary>
        public void StopSendingSensors()
        {
            _doScanSensors = false;
            if (_sensorScanningTimer != null)
            {
                _sensorScanningTimer.Dispose();
                _sensorScanningTimer = null;
            }
        }

        /// <summary>
        /// Stop scanning the configured sensors.
        /// </summary>
        public void StopReceivingSensors()
        {
            if (_actuatorReceivingThread != null)
            {
                _actuatorReceivingThread.Abort();
                _actuatorReceivingThread = null;
            }
        }

        private void ScanSensors(object state)
        {
            if (_doScanSensors == false)
                return;

            var sensors = (Sensor[])state;
            var data = new MultipleSensorData[sensors.Length];
            int index = 0;
            foreach (var sensor in sensors)
            {
                data[index++] = new MultipleSensorData()
                    {
                        SensorId = sensor.Id,
                        Measures =
                            new[]
                                {
                                    new SensorData(DateTime.Now,
                                                   Convert.ToString(GetPerformanceCounterValue(_performanceCounters[sensor.Id])),
                                                   (CorrelationId++).ToString())
                                }
                    };
            }

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = data, DeviceId = _deviceId };

            string baseErrorMessage = "Failed pushing sensor data to the server.";
            try
            {
                // Create the tracking point
                //foreach (var sensorData in data)
                //{
                //    TrackingPoint.TrackingPoint.CreateTrackingPoint("MSMQSensorTask: PushValuesToServer_MSMQ", String.Format("Device id: {0}; Sensor data: {1}", _deviceId, sensorData.SensorId), 0, sensorData.Measures[0].CorrelationId);
                //}

                object responseObject;

                Stopwatch watch = new Stopwatch();
                TrackingPoint.TrackingPoint.CreateTrackingPoint
                    (
                     "MSMQSensorTask: Before PushValuesToServer_MSMQ",
                        String.Format("Device id: {0} - Sensorcount: {1}", _deviceId, data.Length),
                        0,
                        string.Format("{0} - {1}", data[0].Measures[0].CorrelationId, data[data.Length - 1].Measures[0].CorrelationId));
                watch.Start();
                SendMsmqRequestWithResponse("STORE_DATA", JSON.Instance.ToJSON(request), out responseObject);
                watch.Stop();
                TrackingPoint.TrackingPoint.CreateTrackingPoint
                    (
                     "MSMQSensorTask: After PushValuesToServer_MSMQ",
                        String.Format("Device id: {0} - Sensorcount: {1}", _deviceId, data.Length),
                        watch.ElapsedMilliseconds,
                        string.Format("{0} - {1}", data[0].Measures[0].CorrelationId, data[data.Length - 1].Measures[0].CorrelationId));

                var opResult = responseObject as OperationResult;
                if (opResult != null)
                {
                    //Success or not?
                    if (opResult.Success == false)
                    {
                        //Error
                        WriteErrorLog(
                            baseErrorMessage + "\n" + "Device Gateway reported an error. Error: {0}",
                            opResult.ErrorMessages);
                    }
                }
                else
                {
                    WriteErrorLog(baseErrorMessage + "\n" + "Request unsuccessful. Error: {0}", responseObject.ToString());
                }
            }
            catch (Exception exc)
            {
                WriteErrorLog(baseErrorMessage + " Error message: {0}", exc.Message);
            }
        }

        private void ReadGatewaySensors()
        {
            try
            {
                while (true)
                {
                    //Collect sensor ids
                    var sensorIds = new List<string>();
                    foreach (var sensor in _sensors.Where(x => x.IsActuator))
                    {
                        sensorIds.Add(sensor.Id);
                    }

                    try
                    {
                        string sensorId;
                        SensorData result;
                        if (ReceiveMsmqSetDataRequest(MsmqCommHandlerOutputQueueAddress, sensorIds.ToArray(),
                                                      out result, out sensorId))
                        {
                            var receivedData = new RetrievedSensorValue()
                                {
                                    SensorId = sensorId,
                                    SensorValue = result.Value,
                                    Timestamp = result.GeneratedWhen.ToLongTimeString(),
                                    SensorDescription = _sensors.First(x => x.Id == sensorId).Description
                                };

                            // track
                            TrackingPoint.TrackingPoint.CreateTrackingPoint("MSMQSensorTask: GetValuesFromServer_MSMQ", String.Format("Device id: {0}; Sensor id: {1}", _deviceId, sensorId), 0, result.CorrelationId);

                            SensorDataRetrieved(receivedData);
                        }
                        else
                        {
                            if (result != null)
                            {
                                WriteErrorLog("Failed reading MSMQ request. Error: {0}", result);
                            }
                        }
                    }
                    catch (ThreadAbortException)
                    {
                    }
                    catch (Exception exc)
                    {
                        WriteErrorLog("Failed reading MSMQ request. Error: {0}", exc.Message);
                    }
                }
            }
            catch (ThreadAbortException exc)
            {
            }
        }

        private void InitPerformanceCounters(NameValueCollection settings)
        {
            foreach (var entryName in settings.AllKeys)
            {
                if (entryName.StartsWith(CfgPrefixPerformanceCounter))
                {
                    var entryValue = settings[entryName];
                    var counterName = entryName.Substring(CfgPrefixPerformanceCounter.Length);
                    if (String.IsNullOrEmpty(counterName))
                        throw new ConfigurationErrorsException(
                            String.Format("Performance counter sensor name may not be empty. Config entry: '{0}'", entryName));


                    var splitValue = entryValue.Split(new char[] { '|' }, StringSplitOptions.None);
                    if (splitValue.Length != 4)
                        throw new ConfigurationErrorsException(
                            String.Format("Invalid configuration property {0} value format.\nCorrect format: {1}",
                                          entryName, PerformanceCounterConfigPropertyValueFormat));

                    if (splitValue[3] != PerfomanceCounterValueCalculated &&
                        //splitValue[3] != PerfomanceCounterValueSample &&
                        splitValue[3] != PerfomanceCounterValueRaw)
                        throw new ConfigurationErrorsException(
                            String.Format("Performance counter value type must be one of the following:\n{0}\n{1}",
                                          PerfomanceCounterValueRaw, PerfomanceCounterValueCalculated)); /* + ",\n" +
                                                               PerfomanceCounterValueSample);*/

                    _performanceCounters[entryName] = new Tuple<PerformanceCounter, string>(
                        new PerformanceCounter()
                            {
                                CategoryName = splitValue[0],
                                CounterName = splitValue[1],
                                InstanceName = splitValue[2]
                            },
                        splitValue[3]);

                }
            }
        }

        private object GetPerformanceCounterValue(Tuple<PerformanceCounter, string> counter)
        {
            switch (counter.Item2)
            {
                case PerfomanceCounterValueRaw:
                    return counter.Item1.RawValue;
                case PerfomanceCounterValueCalculated:
                    return counter.Item1.NextValue();
                //case PerfomanceCounterValueSample:
                //    return counter.Item1.NextSample();
                default:
                    throw new ArgumentException(String.Format("Unsupported performance counter value type: {0}.", counter.Item2));
            }
        }

        private bool RegisterDevice()
        {
            //Register the device
            _device = new Device();
            _device.Description = "MsmqSensorTask";
            var nis = NetworkInterface.GetAllNetworkInterfaces()[0];
            _device.DeviceIpEndPoint = nis.GetIPProperties().UnicastAddresses[0].Address.ToString();
            _device.Location = new Location() { Elevation = 1, Latitude = 2, Longitude = 3, Name = "Dummy location" };
            _device.Id = _deviceId;

            try
            {
                HttpStatusCode statusCode;
                string content;
                string responseContentType;
                //Refresh the device
                //PUT /Devices/X
                SendPutRequest(String.Format(UriTemplateRegisterDevice, _serverUri, _deviceId),
                                             JSON.Instance.ToJSON(_device),
                                             ContentMimeType,
                                             WebCallTimeout,
                                             out statusCode, out responseContentType, out content);

                //Check response, log errors
                return HandleOperationResultResponse(statusCode, content, "Failed refsreshing device registration.");
            }
            catch (WebException exc)
            {
                WriteErrorLog("Failed refreshing device registration. Status: {0}; Error message: {1}", exc.Status, exc.Message);
                return false;
            }
        }

        private bool RegisterSensors()
        {
            //Register the sensors
            _sensors = new Sensor[_performanceCounters.Count];
            int index = 0;
            foreach (string name in _performanceCounters.Keys)
            {
                var performanceCounter = _performanceCounters[name];
                _sensors[index++] = new Sensor()
                {
                    Id = name,
                    DeviceId = _device.Id,
                    Description = "Performance counter sensor " + performanceCounter.Item1.CounterName,
                    Category = "Performance counter",
                    SensorValueDataType =
                    performanceCounter.Item2 == PerfomanceCounterValueRaw
                    ? SensorValueDataType.Long
                    : performanceCounter.Item2 == PerfomanceCounterValueCalculated
                    ? SensorValueDataType.Decimal
                    : SensorValueDataType.String,
                    PushModeCommunicationType = PullModeCommunicationType.MSMQ,
                    SensorDataRetrievalMode = SensorDataRetrievalMode.Push,
                    PullFrequencyInSeconds = 0,
                    UnitSymbol = "",
                    ShallSensorDataBePersisted = true,
                    IsActuator = true
                };
            }

            try
            {
                HttpStatusCode statusCode;
                string content;
                string responseContentType;

                //Refresh the sensors
                //PUT /Devices/X/Sensors/Y
                bool result = SendPutRequest(String.Format(UriTemplateRegisterSensors, _serverUri, _deviceId),
                                             JSON.Instance.ToJSON(_sensors), ContentMimeType,
                                             WebCallTimeout,
                                             out statusCode, out responseContentType, out content);

                //Check response, log errors
                return HandleOperationResultResponse(statusCode, content, "Failed refsreshing device sensors registration");
            }
            catch (WebException exc)
            {
                WriteErrorLog("Failed refreshing device sensors registration. Status: {0}; Error message: {1}", exc.Status, exc.Message);
                return false;
            }
        }

        #region HTTP REST communication...
        private bool SendPutRequest(string targetUrl, string content, string contentType, int timeout, out HttpStatusCode responseCode,
                           out string responseContentType, out string responseContent)
        {
            var request = CreatePutRequest(targetUrl, content, contentType);

            request.Timeout = timeout;

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                return HandleResponse(true, response, out responseCode, out responseContentType, out responseContent);
            }
        }

        private HttpWebRequest CreatePutRequest(string targetUrl, string content, string contentType)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);

            var request = (HttpWebRequest)WebRequest.Create(targetUrl);

            // request line
            request.Method = "PUT";

            // request headers
            request.ContentLength = buffer.Length;
            request.ContentType = contentType;
            request.KeepAlive = false;

            // request body
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(buffer, 0, buffer.Length);
                stream.Close();
            }

            return request;
        }

        public bool HandleResponse(bool writeInfoLog, HttpWebResponse response, out HttpStatusCode responseCode, out string responseContentType, out string responseContent)
        {
            //Get the response
            responseCode = response.StatusCode;
            responseContentType = response.ContentType;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                responseContent = reader.ReadToEnd();
            }

            if (writeInfoLog)
            {
                //Debug output
                WriteInfoLog("        " + response.Method + ": " + response.ResponseUri.OriginalString);
                WriteInfoLog("        " + "Status code: " + response.StatusCode);
                WriteInfoLog("        " + "Status description: " + response.StatusDescription);
                WriteInfoLog("        " + "Response content: " + responseContent);
            }

            return responseCode == HttpStatusCode.OK;
        }

        private bool HandleOperationResultResponse(HttpStatusCode responseStatusCode, string responseContent, string baseErrorMessage)
        {
            OperationResult opResult;
            try
            {
                opResult = (OperationResult)JSON.Instance.ToObject(responseContent, typeof(OperationResult));
            }
            catch (Exception exc)
            {
                WriteErrorLog(baseErrorMessage + "\n" + "Failed parsing JSON operation result.\nContent: {0}\nError: {1}", responseContent, exc.Message);
                return false;
            }

            if (opResult != null)
            {
                //Success or not?
                if (opResult.Success)
                {
                    //OK
                    return true;
                }

                //Error
                WriteErrorLog(baseErrorMessage + "\n" + "Device Gateway reported an error. Response status: {0}; Error: {1}", responseStatusCode, opResult.ErrorMessages);
            }
            else
            {
                WriteErrorLog(baseErrorMessage + "\n" + "Request unsuccessful. Response status: {0}", responseStatusCode);
            }

            return false;
        }

        private GetMultipleSensorDataResult HandleGetMultipleSensorDataResultResponse(HttpStatusCode responseStatusCode, string responseContent, string baseErrorMessage)
        {
            GetMultipleSensorDataResult opResult;
            try
            {
                opResult = (GetMultipleSensorDataResult)JSON.Instance.ToObject(responseContent, typeof(GetMultipleSensorDataResult));
            }
            catch (Exception exc)
            {
                WriteErrorLog(baseErrorMessage + "\n" + "Failed parsing JSON operation result.\nContent: {0}\nError: {1}", responseContent, exc.Message);
                return null;
            }

            if (opResult != null)
            {
                //Success or not?
                if (opResult.Success)
                {
                    //OK
                    return opResult;
                }

                //Error
                WriteErrorLog(baseErrorMessage + "\n" + "Device Gateway reported an error. Response status: {0}; Error: {1}", responseStatusCode, opResult.ErrorMessages);
            }
            else
            {
                WriteErrorLog(baseErrorMessage + "\n" + "Request unsuccessful. Response status: {0}", responseStatusCode);
            }

            return null;
        }

        public bool SendGetRequest(string targetUrl, string contentType, int timeout, Type responseObjectType, out object responseObject)
        {
            string responseContentType, responseContent;
            HttpStatusCode responseCode;
            if (SendGetRequest(targetUrl, contentType, timeout, out responseCode, out responseContentType, out responseContent))
            {
                responseObject = HandleGetMultipleSensorDataResultResponse(responseCode, responseContent, "Failed retrieving the latest sensor data.");
                return responseObject == null ? false : true;
            }

            responseObject = null;
            return false;
        }

        private bool SendGetRequest(string targetUrl, string contentType, int timeout, out HttpStatusCode responseCode, out string responseContentType, out string responseContent)
        {
            var request = (HttpWebRequest)WebRequest.Create(targetUrl);

            // request line
            request.Method = WebRequestMethods.Http.Get;

            request.ContentType = contentType;
            request.KeepAlive = false;
            request.Timeout = timeout;

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                return HandleResponse(false, response, out responseCode, out responseContentType, out responseContent);
            }
        }
        #endregion

        #region MSMQ communication...
        private bool SendMsmqRequestWithResponse(string requestType, string requestBody, out object result)
        {
            try
            {
                string messageIdToCorrelate;
                using (MessageQueue queue = new MessageQueue(MsmqInputQueueAddress))
                {
                    Message message = new Message();
                    message.Body = requestBody;
                    message.Label = requestType;
                    message.Formatter = new XmlMessageFormatter(new[] { "System.String,mscorlib" });

                    queue.Send(message);
                    messageIdToCorrelate = message.Id;
                }

                //Log
                WriteInfoLog("Request: {0}", requestBody);

                using (MessageQueue queue = new MessageQueue(MsmqOutputQueueAddress))
                {
                    Message response = queue.ReceiveByCorrelationId(messageIdToCorrelate, new TimeSpan(0, 10, 0));

                    //Message response = queue.Receive(new TimeSpan(0, 10, 0));
                    response.Formatter = new XmlMessageFormatter(new[] { "System.String,mscorlib" });

                    //Log
                    WriteInfoLog("    Response: {0}", response.Body.ToString());

                    if (response.Label == "RESP_STORE_DATA")
                    {
                        result = JSON.Instance.ToObject(response.Body.ToString(), typeof(StoreSensorDataResult));
                        return true;
                    }
                    else
                        if (response.Label == "ERR")
                        {
                            result = JSON.Instance.ToObject(response.Body.ToString(), typeof(OperationResult));
                            return true;
                        }
                    {
                        throw new Exception("Unknown response type: " + response.Label);
                    }
                }
            }
            catch (Exception exc)
            {
                result = String.Format("MSMQ request failed. Error: {0}", exc.Message);
                return false;
            }
        }

        private bool ReceiveMsmqSetDataRequest(string queueAddress, string[] sensorIds, out SensorData result, out string resultSensorId)
        {
            try
            {
                using (MessageQueue queue = new MessageQueue(queueAddress))
                {
                    resultSensorId = null;
                    Message request = null;
                    var messageEnumerator = queue.GetMessageEnumerator2();
                    while (messageEnumerator.MoveNext(MsmqWaitTimeout))
                    {
                        foreach (var sensorId in sensorIds)
                        {
                            if (messageEnumerator.Current.Label == String.Format("SET_DATA:{0}=>{1}", _deviceId, sensorId))
                            {
                                request = messageEnumerator.RemoveCurrent();
                                resultSensorId = sensorId;
                                break;
                            }
                        }

                        if (request != null)
                            break;
                    }

                    if (request == null)
                    {
                        resultSensorId = null;
                        result = null;
                        return false;
                    }

                    request.Formatter = new XmlMessageFormatter(new[] { "System.String,mscorlib" });

                    //Log
                    WriteInfoLog("Incoming request SET_DATA: {0}", request.Body);

                    if (request.Body == null || request.Body.ToString() == "OK")
                    {
                        result = null;
                        resultSensorId = null;
                    }
                    else
                    {
                        result = (SensorData)JSON.Instance.ToObject(request.Body.ToString(), typeof(SensorData));
                    }
                    return true;
                }
            }
            catch (Exception exc)
            {
                result = String.Format("Failed reading MSMQ request. Error: {0}", exc.Message);
                resultSensorId = null;
                return false;
            }
        }
        #endregion

        private void WriteErrorLog(string message, params object[] parameters)
        {
            if (LogError != null) LogError(String.Format(message, parameters));
        }

        private void WriteInfoLog(string message, params object[] parameters)
        {
            if (LogInfo != null) LogInfo(String.Format(message, parameters));
        }

        private void WriteErrorLog(string message)
        {
            if (LogError != null) LogError(message);
        }

        private void WriteInfoLog(string message)
        {
            if (LogInfo != null) LogInfo(message);
        }

        /// <summary>
        /// Handler for writing error log messages.
        /// </summary>
        public WriteErrorLogHandler LogError { get; set; }

        /// <summary>
        /// Handler for writing info log messages.
        /// </summary>
        public WriteInfoLogHandler LogInfo { get; set; }

        /// <summary>
        /// Handler for passing retrieved sensor data to the UI.
        /// </summary>
        public SensorDataRetrievedHandler SensorDataRetrieved { get; set; }
    }
}
