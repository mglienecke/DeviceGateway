using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.IO;
using GlobalDataContracts;
using Newtonsoft.Json;
using System.Configuration;

namespace Tests.DeviceServer
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für UnitTest1
    /// </summary>
    [TestClass]
    public class DeviceServerREST_Test
    {
        private const string MimeTypeJson = "application/json";

        public DeviceServerREST_Test()
        {
            //
            // TODO: Konstruktorlogik hier hinzufügen
            //
        }

        private TestContext testContextInstance;
        private static string mDeviceAddress;

        /// <summary>
        ///Ruft den Textkontext mit Informationen über
        ///den aktuellen Testlauf sowie Funktionalität für diesen auf oder legt diese fest.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Zusätzliche Testattribute
        //
        // Sie können beim Schreiben der Tests folgende zusätzliche Attribute verwenden:
        //
        // Verwenden Sie ClassInitialize, um vor Ausführung des ersten Tests in der Klasse Code auszuführen.
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext) {
            mDeviceAddress = ConfigurationManager.AppSettings["DeviceAddress"];
        }
        
        // Verwenden Sie ClassCleanup, um nach Ausführung aller Tests in einer Klasse Code auszuführen.
        // [ClassCleanup()]
        public static void MyClassCleanup() { }
        
        // Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen. 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void GetSensors()
        {
            GetSensorsResult resultGetSensors = new GetSensorsResult();
            object result;
            Assert.IsTrue(SendGetRequest("/Sensors", MimeTypeJson, resultGetSensors, out result), result.ToString());
            Assert.IsTrue(resultGetSensors.Success);
            Assert.IsTrue(resultGetSensors.SensorList.Count > 0);
            foreach (Sensor sensor in resultGetSensors.SensorList)
            {
                Console.WriteLine("Device Id: {0}; Sensor Id: {1}; Description: {2}", sensor.DeviceId, sensor.Id, sensor.Description);
            }
        }

        [TestMethod]
        public void GetSensorCurrentValue()
        {
            List<Sensor> sensorsList = GetSensorList();
            object result;

            foreach (Sensor sensor in sensorsList)
            {
                SensorData currentValue = new SensorData();
                if (!sensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.Push) &&
                    !sensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.PushOnChange)){
                    Assert.IsTrue(SendGetRequest("/Sensors/" + sensor.Id + "/currentValue", MimeTypeJson, currentValue, out result), result.ToString());
                    Console.WriteLine("Timestamp: {0}; Value: {1}; CorrelationId: {2}", currentValue.GeneratedWhen, currentValue.Value, currentValue.CorrelationId);
                    }
            }
        }

        [TestMethod]
        public void GetSensorValues()
        {
            List<Sensor> sensorsList = GetSensorList();
            object result;
            foreach (Sensor sensor in sensorsList)
            {
                if (sensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.Pull) ||
                    sensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.Both))
                {
                    GetSensorDataResult resultGetSensorData = new GetSensorDataResult();
                    Assert.IsTrue(SendGetRequest("/Sensors/" + sensor.Id + "/values", MimeTypeJson, resultGetSensorData, out result), result.ToString());
                    Assert.IsTrue(resultGetSensorData.Success);
                    Assert.IsNotNull(resultGetSensorData.Data);
                    Assert.IsNotNull(resultGetSensorData.Data.Measures);
                    Assert.IsTrue(resultGetSensorData.Data.Measures.Count() > 0);

                    Console.WriteLine("Sensor id: {0}", resultGetSensorData.Data.SensorId);
                    foreach (SensorData value in resultGetSensorData.Data.Measures)
                    {
                        Console.WriteLine("Timestamp: {0}; Value: {1}; CorrelationId: {2}", value.GeneratedWhen, value.Value, value.CorrelationId);
                    }
                }
            }
        }

        [TestMethod]
        public void GetErrorLog()
        {
            List<ErrorLogEntry> entries = new List<ErrorLogEntry>();
            object result;
            Assert.IsTrue(SendGetRequest("/ErrorLog", MimeTypeJson, entries, out result), result.ToString());
            Assert.IsNotNull(entries);

            foreach (ErrorLogEntry entry in entries)
            {
                Console.WriteLine("[{0}]: {1}", entry.Timestamp, entry.Message);
            }
        }

        [TestMethod]
        public void GetSensorsValues()
        {
            GetSensorsResult resultGetSensors = new GetSensorsResult();
            object result;
            Assert.IsTrue(SendGetRequest("/Sensors", MimeTypeJson, resultGetSensors, out result), result.ToString());
            Assert.IsTrue(resultGetSensors.Success);
            Assert.IsTrue(resultGetSensors.SensorList.Count > 0);
            int countPullableSensors = 0;
            foreach (Sensor sensor in resultGetSensors.SensorList)
            {
                if (sensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.Both) ||
                    sensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.Pull))
                    countPullableSensors++;
            }

            GetMultipleSensorDataResult resultGetSensorData = new GetMultipleSensorDataResult();
            Assert.IsTrue(SendGetRequest("/Sensors/values", MimeTypeJson, resultGetSensorData, out result), result.ToString());
            Assert.IsTrue(resultGetSensorData.Success);
            Assert.IsNotNull(resultGetSensorData.SensorDataList);
            Assert.IsTrue(resultGetSensorData.SensorDataList.Count >= countPullableSensors);

            foreach (MultipleSensorData data in resultGetSensorData.SensorDataList)
            {
                Console.WriteLine("Sensor id: {0}", data.SensorId);
                foreach (SensorData value in data.Measures)
                {
                    Console.WriteLine("Timestamp: {0}; Value: {1}; CorrelationId: {2}", value.GeneratedWhen, value.Value, value.CorrelationId);
                }
            }
        }

        [TestMethod]
        public void PutSensorCurrentValue()
        {
            List<Sensor> sensorsList = GetSensorList();
            object result;

            foreach (Sensor sensor in sensorsList)
            {
                SensorData currentValue = new SensorData();
                currentValue.Value = "false";
                currentValue.GeneratedWhen = DateTime.Now;
                currentValue.CorrelationId = Guid.NewGuid().ToString();
                string content = String.Format(
@"{{
    ""GeneratedWhen"": ""{0}"",
    ""Value"": ""{1}"",
    ""CorrelationId"": ""{2}""
}}", currentValue.GeneratedWhen, currentValue.Value, currentValue.CorrelationId);
                OperationResult resultOp = new OperationResult();
                Assert.IsTrue(SendPutRequest("/Sensors/" + sensor.Id + "/currentValue", content, MimeTypeJson, resultOp, out result), result.ToString());

                Console.WriteLine("Sensor id: {0}; Success: {1}; Message: {2}", sensor.Id, resultOp.Success, resultOp.ErrorMessages);
            }
        }

        [TestMethod]
        public void PutSensorCurrentValue_WrongSensorId()
        {
                SensorData currentValue = new SensorData();
                object result;
                currentValue.Value = DateTime.Now.Ticks.ToString();
                currentValue.GeneratedWhen = DateTime.Now;
                currentValue.CorrelationId = Guid.NewGuid().ToString();
                string content = String.Format(
@"{{
    ""GeneratedWhen"": ""{0}"",
    ""Value"": ""{1}"",
    ""CorrelationId"": ""{2}""
}}", currentValue.GeneratedWhen, currentValue.Value, currentValue.CorrelationId);
                OperationResult resultOp = new OperationResult();
                Assert.IsTrue(SendPutRequest("/Sensors/" + "Fake.Sensor.Id" + "/currentValue", content, MimeTypeJson, resultOp, out result), result.ToString());

                Console.WriteLine("Sensor id: {0}; Success: {1}; Message: {2}", "Fake.Sensor.Id", resultOp.Success, resultOp.ErrorMessages);
        }

        //[TestMethod]
        public void PutDeviceConfig_TahoeII()
        {
            DeviceConfig config = new DeviceConfig();
            config.ServerUrl = "http://192.168.1.2/GatewayService";
            config.TimeoutInMillis = 10000;
            config.DefaultContentParserClassName = null;
            config.IsRefreshDeviceRegOnStartup = true;
            config.UseServerTime = true;

            //Temperature
            SensorConfig sconfig = new SensorConfig();
            sconfig.Id = "temperature1";
            sconfig.IsCoalesce = false;
            sconfig.KeepHistory = true;
            sconfig.KeepHistoryMaxRecords = 11;
            sconfig.ScanFrequencyInMillis = 10000;
            sconfig.ServerUrl = "http://192.168.1.2/GatewayService";
            sconfig.SensorDataRetrievalMode = SensorDataRetrievalMode.Push;
            sconfig.UseLocalStore = true;
            config.SensorConfigs.Add(sconfig);

            //AUX
            sconfig = new SensorConfig();
            sconfig.Id = "auxil";
            sconfig.IsCoalesce = false;
            sconfig.KeepHistory = true;
            sconfig.KeepHistoryMaxRecords = 12;
            sconfig.ScanFrequencyInMillis = 12000;
            sconfig.ServerUrl = "http://192.168.1.2/GatewayService";
            sconfig.SensorDataRetrievalMode = SensorDataRetrievalMode.Pull;
            sconfig.UseLocalStore = false;
            config.SensorConfigs.Add(sconfig);


            ContentWriterDelegate writer = (x) => {
                JsonSerializer serializer = new JsonSerializer(); 
                using (StreamWriter textWriter = new StreamWriter(x)){
                    serializer.Serialize(textWriter, config);
                }
            };
            OperationResult resultOp = new OperationResult();
            object result;
            
            Assert.IsTrue(SendPutRequest("/DeviceConfig", writer, MimeTypeJson, resultOp, out result));
            Assert.IsTrue(resultOp.Success);
        }

        [TestMethod]
        public void PutDeviceConfig_Empty()
        {
            ContentWriterDelegate writer = (x) =>
            {
                JsonSerializer serializer = new JsonSerializer();
                using (StreamWriter textWriter = new StreamWriter(x))
                {
                    serializer.Serialize(textWriter, "");
                }
            };
            OperationResult resultOp = new OperationResult();
            object result;

            Assert.IsTrue(SendPutRequest("/DeviceConfig", writer, MimeTypeJson, resultOp, out result));
            Assert.IsTrue(resultOp.Success);
        }

        [TestMethod]
        public void GetDeviceConfig()
        {
            DeviceConfig config = new DeviceConfig();

            object result;

            Assert.IsTrue(SendGetRequest("/DeviceConfig", MimeTypeJson, config, out result));
            Assert.IsNotNull(config.ServerUrl);
            Assert.AreNotEqual(String.Empty, config.ServerUrl.Trim());
            
        }

        #region Private methods...
        private List<Sensor> GetSensorList()
        {
            GetSensorsResult resultGetSensors = new GetSensorsResult();
            object result;
            Assert.IsTrue(SendGetRequest("/Sensors", MimeTypeJson, resultGetSensors, out result), result.ToString());
            Assert.IsTrue(resultGetSensors.Success);
            Assert.IsTrue(resultGetSensors.SensorList.Count > 0);

            return resultGetSensors.SensorList;
        }

        private bool SendGetRequest(string relativeUri, string contentType, object resultObjectTemplate, out object result, params object[] parameters)
        {
            try
            {
                String uriString = String.Format("http://" + mDeviceAddress + relativeUri, parameters);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uriString);
                request.Method = "GET";
                request.Accept = contentType;
                request.KeepAlive = false;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonSerializer serializer = new JsonSerializer();
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        serializer.Populate(reader, resultObjectTemplate);
                    }

                    result = resultObjectTemplate;
                    return true;
                }
                else
                {
                    result = String.Format( "HTTP REST GET request failed. Status: {0}; Description: {1}",
                        response.StatusCode, response.StatusDescription);
                    return false;
                }

            }
            catch (Exception exc)
            {
                result = String.Format("HTTP REST GET requuest failed. Error: {0}", exc.Message);
                return false;
            }
        }

        private bool SendPutRequest(string relativeUri, string content, string contentType, object resultObjectTemplate, out object result, params object[] parameters)
        {
            try
            {
                String uriString = String.Format("http://" + mDeviceAddress + relativeUri, parameters);

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uriString);
                request.Method = "PUT";
                request.Accept = contentType;
                request.KeepAlive = false;


                byte[] buffer = Encoding.UTF8.GetBytes(content);

                // request body
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Close();
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonSerializer serializer = new JsonSerializer();
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        serializer.Populate(reader, resultObjectTemplate);
                    }

                    result = resultObjectTemplate;
                    return true;
                }
                else
                {
                    result = String.Format("HTTP REST PUT request failed. Status: {0}; Description: {1}",
                        response.StatusCode, response.StatusDescription);
                    return false;
                }

            }
            catch (Exception exc)
            {
                result = String.Format("HTTP REST PUT requuest failed. Error: {0}", exc.Message);
                return false;
            }
        }

        private bool SendPutRequest(string relativeUri, ContentWriterDelegate contentWriter, string contentType, object resultObjectTemplate, out object result, params object[] parameters)
        {
            try
            {
                String uriString = String.Format("http://" + mDeviceAddress + relativeUri, parameters);

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uriString);
                request.Method = "PUT";
                request.Accept = contentType;
                request.KeepAlive = false;
                request.Timeout = 600000;


                // request body
                using (Stream stream = request.GetRequestStream())
                {
                    contentWriter(stream);
                    stream.Close();
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonSerializer serializer = new JsonSerializer();
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        serializer.Populate(reader, resultObjectTemplate);
                    }

                    result = resultObjectTemplate;
                    return true;
                }
                else
                {
                    result = String.Format("HTTP REST PUT request failed. Status: {0}; Description: {1}",
                        response.StatusCode, response.StatusDescription);
                    return false;
                }

            }
            catch (Exception exc)
            {
                result = String.Format("HTTP REST PUT requuest failed. Error: {0}", exc.Message);
                return false;
            }
        }
        #endregion

        private delegate void ContentWriterDelegate(Stream stream);
    }
}
