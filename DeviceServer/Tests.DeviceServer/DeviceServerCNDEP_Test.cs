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
using Common.Server.CNDEP;

namespace Tests.DeviceServer
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für UnitTest1
    /// </summary>
    [TestClass]
    public class DeviceServerCNDEP_Test
    {
        private const string MimeTypeJson = "application/json";

        public DeviceServerCNDEP_Test()
        {
            //
            // TODO: Konstruktorlogik hier hinzufügen
            //
        }

        private TestContext testContextInstance;
        private static string mDeviceAddress;
        private static int mDevicePort = 41120;
        private static int mRequestRetryCount = 1;
        private static int mRequestTimeout = 5000;

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
            mDeviceAddress = ConfigurationManager.AppSettings["CNDEP.DeviceAddress"];
            
            if (ConfigurationManager.AppSettings["CNDEP.DevicePort"] != null)
                mDevicePort = Int32.Parse(ConfigurationManager.AppSettings["CNDEP.DevicePort"]);

            if (ConfigurationManager.AppSettings["CNDEP.RequestTimeout"] != null)
                mRequestTimeout = Int32.Parse(ConfigurationManager.AppSettings["CNDEP.RequestTimeout"]);

            if (ConfigurationManager.AppSettings["CNDEP.RequestRetryCount"] != null)
                mRequestRetryCount = Int32.Parse(ConfigurationManager.AppSettings["CNDEP.RequestRetryCount"]);
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


        #region GetSensorCurrentValue...
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
                        Assert.IsTrue(SendCndepRequest(mDeviceAddress, mDevicePort, CndepConstants.CmdGetSensorData, CndepConstants.FncGetSensorDataCurrentValue, sensor.Id, MimeTypeJson, currentValue, out result), result.ToString());
                        Console.WriteLine("Timestamp: {0}; Value: {1}; CorrelationId: {2}", currentValue.GeneratedWhen, currentValue.Value, currentValue.CorrelationId);
                    }
            }
        }

        [TestMethod]
        public void GetSensorCurrentValue_NoSensorId()
        {
            object result;

            SensorData currentValue = new SensorData();
            Assert.IsTrue(SendCndepRequest(mDeviceAddress, mDevicePort, CndepConstants.CmdGetSensorData, CndepConstants.FncGetSensorDataCurrentValue, String.Empty, MimeTypeJson, currentValue, out result), result.ToString());
            Assert.AreEqual(String.Empty, currentValue.Value);
            Console.WriteLine("Timestamp: {0}; Value: {1}; CorrelationId: {2}", currentValue.GeneratedWhen, currentValue.Value, currentValue.CorrelationId);
        }

        [TestMethod]
        public void GetSensorCurrentValue_InvalidSensorId()
        {
            object result;

            SensorData currentValue = new SensorData();
            Assert.IsTrue(SendCndepRequest(mDeviceAddress, mDevicePort, CndepConstants.CmdGetSensorData, CndepConstants.FncGetSensorDataCurrentValue, "Fake!", MimeTypeJson, currentValue, out result), result.ToString());
            Assert.AreEqual(String.Empty, currentValue.Value);
            Console.WriteLine("Timestamp: {0}; Value: {1}; CorrelationId: {2}", currentValue.GeneratedWhen, currentValue.Value, currentValue.CorrelationId);
        }
        #endregion

        #region GetSensorValues...
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
                    Assert.IsTrue(SendCndepRequest(mDeviceAddress, mDevicePort, CndepConstants.CmdGetSensorData, CndepConstants.FncGetSensorDataAllValues, sensor.Id, MimeTypeJson, resultGetSensorData, out result), result.ToString());
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
        public void GetSensorValues_NoSensorId()
        {
            object result;

            GetSensorDataResult resultGetSensorData = new GetSensorDataResult();
            Assert.IsTrue(SendCndepRequest(mDeviceAddress, mDevicePort, CndepConstants.CmdGetSensorData, CndepConstants.FncGetSensorDataAllValues, String.Empty, MimeTypeJson, resultGetSensorData, out result), result.ToString());
            Assert.IsFalse(resultGetSensorData.Success);
            Assert.IsNull(resultGetSensorData.Data);
        }

        [TestMethod]
        public void GetSensorValues_InvalidSensorId()
        {
            object result;

            GetSensorDataResult resultGetSensorData = new GetSensorDataResult();
            Assert.IsTrue(SendCndepRequest(mDeviceAddress, mDevicePort, CndepConstants.CmdGetSensorData, CndepConstants.FncGetSensorDataAllValues, "Fake!", MimeTypeJson, resultGetSensorData, out result), result.ToString());
            Assert.IsFalse(resultGetSensorData.Success);
            Assert.IsNull(resultGetSensorData.Data);
        }
        #endregion

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

        private bool SendCndepRequest(string deviceAddress, int devicePort, byte command, byte function, string requestData, string contentType, object resultObjectTemplate, out object result, params object[] parameters)
        {
            try
            {
                using (CndepUdpClient udpClient = new CndepUdpClient(devicePort, Dns.GetHostAddresses(deviceAddress)[0]))
                {
                    byte[] data = UTF8Encoding.UTF8.GetBytes(requestData);

                    //Create message
                    CndepMessageRequest request = new CndepMessageRequest(GetSessionId(), command, function, data);

                    //Connect and send in the sync mode
                    udpClient.Open();

                    CndepMessageResponse response = null;
                    int retryCount = 0;

                    do
                    {
                        response = udpClient.Send(request, mRequestTimeout);

                        string responseDataStr;
                        if (response != null)
                        {
                            switch (response.ResponseId)
                            {
                                case CndepCommands.RspOK:
                                case CndepCommands.RspError:
                                    //Get data
                                    responseDataStr = UTF8Encoding.UTF8.GetString(response.Data, 0, response.Data.Length);

                                    //Check data
                                    if (responseDataStr == null || responseDataStr.Length == 0)
                                    {
                                        result = String.Format("Device replied with an empty string.");
                                        return false;
                                    }

                                    JsonSerializer serializer = new JsonSerializer();
                                    using (StringReader reader = new StringReader(responseDataStr))
                                    {
                                        serializer.Populate(reader, resultObjectTemplate);
                                    }

                                    result = resultObjectTemplate;
                                    return true;
                                default:
                                    result = String.Format("Unknown CNDEP response code received. Response code: {0}",
                                        response.ResponseId);
                                    return false;
                            }
                        }
                    }
                    while (response == null && retryCount++ < mRequestRetryCount);

                    result = "Response timeout";
                    return false;
                }

            }
            catch (Exception exc)
            {
                result = String.Format("UDP CNDEP request failed. Error: {0}", exc.Message);
                return false;
            }
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
                    result = String.Format("HTTP REST GET request failed. Status: {0}; Description: {1}",
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
        #endregion

        private delegate void ContentWriterDelegate(Stream stream);

        private byte mSessionId;
        private byte GetSessionId()
        {
            lock (this)
            {
                return mSessionId++;
            }
        }
    }
}
