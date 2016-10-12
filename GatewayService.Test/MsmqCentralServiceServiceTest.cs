using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel.Web;
using GlobalDataContracts;
using System.ServiceModel;
using System.Net;
using System.IO;
using Common.Server;
using System.Web;
using System.Globalization;
using Common.Server.Msmq;
using Newtonsoft.Json;
using System.Messaging;
using System.Configuration;

namespace GatewayServiceTest
{
    [TestClass]
    public class MsmqCentralServiceServiceTest:TestBase
    {
        private const string MimeTypeJson = "application/json";
        private const int HttpRequestTimeoutInMillis = 60000;

        #region Properties...
        private static string MsmqInputQueueAddress
        {
            get;
            set;
        }

        private static string MsmqOutputQueueAddress
        {
            get;
            set;
        }

        private static string MsmqCommHandlerInputQueueAddress
        {
            get;
            set;
        }

        private static string MsmqCommHandlerOutputQueueAddress
        {
            get;
            set;
        }

        private static string RestServerUrl
        {
            get;
            set;
        }

        private static WebClient RestClient
        {
            get;
            set;
        }
        #endregion

        /// <summary>
        /// Initialize the assembly and the remote client. If there is no remote connection then initialize Log4Net as well
        /// </summary>
        /// <param name="context">The context.</param>
        [ClassInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            RestClient = new WebClient();
            RestClient.Encoding = Encoding.UTF8;
            RestClient.Headers.Add(HttpRequestHeader.ContentType, MimeTypeJson);
            log4net.Config.XmlConfigurator.Configure();

            RestServerUrl = ConfigurationManager.AppSettings["RestServerUrl"];

            MsmqInputQueueAddress = ConfigurationManager.AppSettings["MsmqInputQueueAddress"];
            MsmqOutputQueueAddress = ConfigurationManager.AppSettings["MsmqOutputQueueAddress"];
            MsmqCommHandlerInputQueueAddress = ConfigurationManager.AppSettings["MsmqCommHandlerInputQueueAddress"];
            MsmqCommHandlerOutputQueueAddress = ConfigurationManager.AppSettings["MsmqCommHandlerOutputQueueAddress"];
        }


       
        #region Sensors, devices, sensor data...

        #region StoreMultipleSensorData...
        /*
       [TestMethod]
        public void StoreMultipleSensorData()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString() };
            }

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData};

            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(RestServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);
        }

       [TestMethod]
        public void StoreMultipleSensorData_DeviceIdSensorIdMismatch()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Device sampleDevice2 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString() };
            }
            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData};

            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(RestServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice2.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;

            //Success - but with errors
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.ErrorMessages);
        }

       [TestMethod]
        public void StoreMultipleSensorData_MultipleSensors()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);
            Sensor sensor12 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            MultipleSensorData sampleData1 = new MultipleSensorData();
            sampleData1.SensorId = sensor11.Id;
            sampleData1.Measures = new SensorData[5];
            for (int i = 0; i < sampleData1.Measures.Length; i++)
            {
                sampleData1.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString() };
            }
            MultipleSensorData sampleData2 = new MultipleSensorData();
            sampleData2.SensorId = sensor12.Id;
            sampleData2.Measures = new SensorData[5];
            for (int i = 0; i < sampleData2.Measures.Length; i++)
            {
                sampleData2.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString() };
            }

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData1, sampleData2};

            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(RestServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);
        }

       [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void StoreMultipleSensorData_EmptyDeviceId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString() };
            }

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData };

            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(RestServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, String.Empty), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

       [TestMethod]
        public void StoreMultipleSensorData_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString() };
            }

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData };

            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(RestServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, "-NotExistingId-"), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

       [TestMethod]
        public void StoreMultipleSensorData_NullSensorData()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = null;

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData };
            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(RestServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

       // [TestMethod]
        public void StoreMultipleSensorData_NullSensorDataArrayElement()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString() };
            }
            sampleData.Measures[1] = null;

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData };
            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(RestServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

       // [TestMethod]
        public void StoreMultipleSensorData_InvalidSensorDataGeneratedWhenField()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString() };
            }
            sampleData.Measures[0].GeneratedWhen = DateTime.MinValue;

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData };
            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(RestServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);
        }

       [TestMethod]
        public void StoreMultipleSensorData_NullSensorDataValueField()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString() };
            }
            sampleData.Measures[0].Value = null;

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData };
            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(RestServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);
        }
         */
        #endregion

        #region GetCurrentSensorData...

        [TestMethod]
        public void GetCurrentSensorData()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice("MSMQ Test device");
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Pull);

            SensorData[] sampleData = new SensorData[5];
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            //Get request
            string correlationId;
            object result;
            Thread.Sleep(8000);
            Assert.IsTrue(
                ReceiveMsmqRequest(MsmqCommHandlerOutputQueueAddress, MsmqDeviceCommunicationHandler.GetMessageLabel(MsmqMessageRequest.RequestTypeGetSensorCurrentDataPrefix,
                                                                                  sampleDevice1, sensor11).ToString(), null, out result, out correlationId));

            //Send response
            string resultMessage;
            Assert.IsTrue(SendMsmqResponse(MsmqCommHandlerInputQueueAddress, MsmqDeviceCommunicationHandler.GetMessageLabel(MsmqMessageResponse.ResponseTypeGetSensorData, sampleDevice1, sensor11).ToString(), 
                ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleData[0]), correlationId, out resultMessage), resultMessage);
        }
        #endregion

        #region StoreSingleSensorData...

        [TestMethod]
        public void StoreSingleSensorData()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice("MSMQ Test device");
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            SensorData[] sampleData = new SensorData[5];CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id, Measures = sampleData } }, DeviceId = sampleDevice1.Id };
            Assert.IsTrue(SendMsmqRequestWithResponse(MsmqMessageRequest.RequestTypeStoreSensorData, ContentParserFactory.GetParser(MimeTypeJson).Encode(request), out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertSuccess(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_EmptyDeviceId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice("MSMQ Test device");
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            SensorData[] sampleData = new SensorData[5];
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id, Measures = sampleData } }, DeviceId = String.Empty };
            Assert.IsTrue(SendMsmqRequestWithResponse(MsmqMessageRequest.RequestTypeStoreSensorData, ContentParserFactory.GetParser(MimeTypeJson).Encode(request), out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_EmptySensorID()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice("MSMQ Test device");
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            SensorData[] sampleData = new SensorData[5];
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = String.Empty, Measures = sampleData } }, DeviceId = sampleDevice1.Id };
            Assert.IsTrue(SendMsmqRequestWithResponse(MsmqMessageRequest.RequestTypeStoreSensorData, ContentParserFactory.GetParser(MimeTypeJson).Encode(request), out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice("MSMQ Test device");
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            SensorData[] sampleData = new SensorData[5];
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id, Measures = sampleData } }, DeviceId = "Fake!" };
            Assert.IsTrue(SendMsmqRequestWithResponse(MsmqMessageRequest.RequestTypeStoreSensorData, ContentParserFactory.GetParser(MimeTypeJson).Encode(request), out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_DeviceIdSensorIdMismatch()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice("MSMQ Test device");
            Device sampleDevice2 = CreateAndRegisterSampleDevice("MSMQ Test device 11");
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            SensorData[] sampleData = new SensorData[5];
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id, Measures = sampleData } }, DeviceId = sampleDevice2.Id };
            Assert.IsTrue(SendMsmqRequestWithResponse(MsmqMessageRequest.RequestTypeStoreSensorData, ContentParserFactory.GetParser(MimeTypeJson).Encode(request), out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_NonExistingSensorId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice("MSMQ Test device");
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            SensorData[] sampleData = new SensorData[5];
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = "Fake!", Measures = sampleData } }, DeviceId = sampleDevice1.Id };
            Assert.IsTrue(SendMsmqRequestWithResponse(MsmqMessageRequest.RequestTypeStoreSensorData, ContentParserFactory.GetParser(MimeTypeJson).Encode(request), out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_NullSensorData()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice("MSMQ Test device");
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id } }, DeviceId = sampleDevice1.Id };
            Assert.IsTrue(SendMsmqRequestWithResponse(MsmqMessageRequest.RequestTypeStoreSensorData, ContentParserFactory.GetParser(MimeTypeJson).Encode(request), out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
        }

       [TestMethod]
       public void StoreSingleSensorData_NullMultipleSensorData()
       {
           Device sampleDevice1 = CreateAndRegisterSampleDevice("MSMQ Test device");
           Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

           //Store
           object responseObject;
           OperationResult resultTemplate = new OperationResult();

           StoreSensorDataRequest request = new StoreSensorDataRequest() { DeviceId = sampleDevice1.Id };
           Assert.IsTrue(SendMsmqRequestWithResponse(MsmqMessageRequest.RequestTypeStoreSensorData, ContentParserFactory.GetParser(MimeTypeJson).Encode(request), out responseObject), responseObject.ToString());
           Assert.IsNotNull(responseObject);
           Assert.IsTrue(responseObject is OperationResult);

           OperationResult result = (OperationResult)responseObject;

           //Check
           AssertFailure(result);
       }

       [TestMethod]
        public void StoreSingleSensorData_ZeroElementSensorData()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice("MSMQ Test device");
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            SensorData[] sampleData = new SensorData[0];


            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id, Measures = sampleData } }, DeviceId = sampleDevice1.Id };
            Assert.IsTrue(SendMsmqRequestWithResponse(MsmqMessageRequest.RequestTypeStoreSensorData, ContentParserFactory.GetParser(MimeTypeJson).Encode(request), out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_NullSensorDataArrayElement()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice("MSMQ Test device");
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            SensorData[] sampleData = new SensorData[1];

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id, Measures = sampleData } }, DeviceId = sampleDevice1.Id };
            Assert.IsTrue(SendMsmqRequestWithResponse(MsmqMessageRequest.RequestTypeStoreSensorData, ContentParserFactory.GetParser(MimeTypeJson).Encode(request), out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_InvalidSensorDataGeneratedWhenField()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice("MSMQ Test device");
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            SensorData[] sampleData = new SensorData[5];
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }
            sampleData[0].GeneratedWhen = DateTime.MinValue;

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id, Measures = sampleData } }, DeviceId = sampleDevice1.Id };
            Assert.IsTrue(SendMsmqRequestWithResponse(MsmqMessageRequest.RequestTypeStoreSensorData, ContentParserFactory.GetParser(MimeTypeJson).Encode(request), out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_NullSensorDataValueField()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice("MSMQ Test device");
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1, SensorDataRetrievalMode.Push);

            SensorData[] sampleData = new SensorData[5];
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }
            sampleData[0].Value = null;

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id, Measures = sampleData } }, DeviceId = sampleDevice1.Id };
            Assert.IsTrue(SendMsmqRequestWithResponse(MsmqMessageRequest.RequestTypeStoreSensorData, ContentParserFactory.GetParser(MimeTypeJson).Encode(request), out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);
        }

       [TestMethod]
       public void StoreSingleSensorData_Actuator()
       {
           Device sampleDevice1 = CreateAndRegisterSampleDevice("MSMQ Test device");
           Sensor sensor11 = CreateAndRegisterSampleActuator(sampleDevice1);

           SensorData[] sampleData = new SensorData[5];
           for (int i = 0; i < sampleData.Length; i++)
           {
               sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
           }

           //Store
           object responseObject;
           OperationResult resultTemplate = new OperationResult();

           StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id, Measures = sampleData } }, DeviceId = sampleDevice1.Id };
           Assert.IsTrue(SendMsmqRequestWithResponse(MsmqMessageRequest.RequestTypeStoreSensorData, ContentParserFactory.GetParser(MimeTypeJson).Encode(request), out responseObject), responseObject.ToString());
           Assert.IsNotNull(responseObject);
           Assert.IsTrue(responseObject is OperationResult);

           OperationResult result = (OperationResult)responseObject;

           //Check
           AssertSuccess(result);

           for (int i = 0; i < sampleData.Length; i++)
           {
               object requestBody;
               string correlationId;
               ReceiveMsmqRequest(MsmqCommHandlerOutputQueueAddress, "SET_DATA:" + sampleDevice1.Id + "=>" + sensor11.Id, typeof(SensorData), out requestBody, out correlationId);
               Assert.IsNotNull(result);
               Assert.IsTrue(requestBody is SensorData);
           }
       }
        #endregion
        
        #endregion


        private void AssertDeviceRegistered(Device device, bool expectedResult, bool checkDataForEquality)
        {
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(RestServerUrl + ServerHttpRestUrls.DeviceIsUsed, device.Id ), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(IsDeviceIdUsedResult), out responseObject));
            Assert.IsNotNull(responseObject);
            IsDeviceIdUsedResult resultDeviceReg = (IsDeviceIdUsedResult)responseObject;
            AssertSuccess(resultDeviceReg);
            Assert.AreEqual(expectedResult, resultDeviceReg.IsUsed);

            if (expectedResult == true)
            {
                Assert.IsTrue(HttpClient.SendGetRequest(RestServerUrl + ServerHttpRestUrls.MultipleDevices + GetIdQueryString(new string[] { device.Id }), MimeTypeJson,
                    HttpRequestTimeoutInMillis, typeof(GetDevicesResult), out responseObject));
                Assert.IsNotNull(responseObject);

                GetDevicesResult resultDevices = (GetDevicesResult)responseObject;
                AssertSuccess(resultDevices);

                Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == device.Id) != null);
                if (checkDataForEquality)
                    AssertAreEqual(device, resultDevices.Devices.First(x => x.Id == device.Id));
            }
        }

        private bool CheckDeviceRegistered(Device device, bool checkDataForEquality)
        {
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(RestServerUrl + ServerHttpRestUrls.DeviceIsUsed, device.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(IsDeviceIdUsedResult), out responseObject));
            Assert.IsNotNull(responseObject);
            IsDeviceIdUsedResult resultDeviceReg = (IsDeviceIdUsedResult)responseObject;
            AssertSuccess(resultDeviceReg);


            if (resultDeviceReg.IsUsed == true)
            {
                Assert.IsTrue(HttpClient.SendGetRequest(RestServerUrl + ServerHttpRestUrls.MultipleDevices + GetIdQueryString(new string[] { device.Id }), MimeTypeJson,
                    HttpRequestTimeoutInMillis, typeof(GetDevicesResult), out responseObject));
                Assert.IsNotNull(responseObject);

                GetDevicesResult resultDevices = (GetDevicesResult)responseObject;
                AssertSuccess(resultDevices);

                Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == device.Id) != null);
                if (checkDataForEquality)
                    AssertAreEqual(device, resultDevices.Devices.First(x => x.Id == device.Id));
            }

            return resultDeviceReg.IsUsed;
        }

        private Device CreateAndRegisterSampleDevice(string deviceId)
        {
            //Create
            Device sampleDevice = CreateSampleMsmqDevice(deviceId);

            if (CheckDeviceRegistered(sampleDevice, true) == false)
            {
                object responseObject;
                Assert.IsTrue(
                    HttpClient.SendPutRequest(RestServerUrl + String.Format(ServerHttpRestUrls.SingleDeviceWithId, sampleDevice.Id),
                                              ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDevice),
                                              MimeTypeJson, HttpRequestTimeoutInMillis, typeof (OperationResult), out responseObject));
                Assert.IsNotNull(responseObject);

                OperationResult result = (OperationResult) responseObject;

                //Check
                AssertSuccess(result);


                AssertDeviceRegistered(sampleDevice, true, true);
            }

            return sampleDevice;
        }

        private Sensor CreateAndRegisterSampleSensor(Device device, SensorDataRetrievalMode retrievalMode)
        {
            //Create
            Sensor sampleSensor;
            if (retrievalMode == SensorDataRetrievalMode.Push)
                sampleSensor = CreateSampleMsmqPushSensor(device);
            else
                if (retrievalMode == SensorDataRetrievalMode.Pull)
                    sampleSensor = CreateSampleMsmqPullSensor(device);
                else
                    throw new ArgumentOutOfRangeException("retrievalMode");

            //if (CheckDeviceSensorsRegistered(new Sensor[] { sampleSensor }, device, true) == false)
            //{
                object responseObject;
                Assert.IsTrue(
                    HttpClient.SendPutRequest(RestServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdMultipleSensors, device.Id),
                                              ContentParserFactory.GetParser(MimeTypeJson).Encode(new Sensor[] { sampleSensor }),
                                              MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
                Assert.IsNotNull(responseObject);

                OperationResult result = (OperationResult)responseObject;

                //Check
                AssertSuccess(result);
            //}

            return sampleSensor;
        }

        private Sensor CreateAndRegisterSampleActuator(Device device)
        {
            //Create
            Sensor sampleSensor = CreateSampleMsmqActuator(device);

            if (CheckDeviceSensorsRegistered(new Sensor[] { sampleSensor }, device, true) == false)
            {
                object responseObject;
                Assert.IsTrue(
                    HttpClient.SendPutRequest(RestServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdMultipleSensors, device.Id),
                                              ContentParserFactory.GetParser(MimeTypeJson).Encode(new Sensor[] { sampleSensor }),
                                              MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
                Assert.IsNotNull(responseObject);

                OperationResult result = (OperationResult)responseObject;

                //Check
                AssertSuccess(result);
            }

            return sampleSensor;
        }

        private void AssertDeviceSensorsRegistered(Sensor[] sensors, Device device)
        {
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(RestServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, device.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorsForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetSensorsForDeviceResult resultSensorsForDevice = (GetSensorsForDeviceResult)responseObject;

            AssertSuccess(resultSensorsForDevice);
            Assert.IsTrue(sensors.Count() <= resultSensorsForDevice.Sensors.Count());

            for (int i = 0; i < sensors.Length; i++)
            {
                Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensors[i].Id) != null);
                AssertAreEqual(sensors[i], resultSensorsForDevice.Sensors.First(x => x.Id == sensors[i].Id));
            }
        }

        private bool CheckDeviceSensorsRegistered(Sensor[] sensors, Device device, bool checkForEquality)
        {
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(RestServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, device.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorsForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetSensorsForDeviceResult resultSensorsForDevice = (GetSensorsForDeviceResult)responseObject;

            AssertSuccess(resultSensorsForDevice);
            if (sensors.Count() > resultSensorsForDevice.Sensors.Count())
            {
                return false;
            }

            foreach (var sensor in sensors)
            {
                if (resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor.Id) == null)
                    return false;
                
                if (checkForEquality)
                    AssertAreEqual(sensor, resultSensorsForDevice.Sensors.First(x => x.Id == sensor.Id));
            }

            return true;
        }

        private string GetIdQueryString(string[] ids)
        {
            StringBuilder builder = new StringBuilder("?");
            for (int i = 0; i < ids.Length; i++)
            {
                builder.AppendFormat("id{0}={1}&", i, ids[i]);
            }
            builder.Remove(builder.Length - 1, 1);

            return builder.ToString();
        }

        private bool SendMsmqRequestWithResponse(string requestType, string requestBody, out object result)
        {
            try
            {
                string correlationId;
                using (MessageQueue queue = new MessageQueue(MsmqInputQueueAddress))
                {
                    Message message = new Message();
                    message.Body = requestBody;
                    message.Label = requestType;
                    message.Formatter = new XmlMessageFormatter(new[] { "System.String,mscorlib" });

                    queue.Send(message);
                    correlationId = message.Id;
                }

                using (MessageQueue queue = new MessageQueue(MsmqOutputQueueAddress))
                {
                    Message response = queue.ReceiveByCorrelationId(correlationId, new TimeSpan(0, 10, 0));
                    
                    //Message response = queue.Receive(new TimeSpan(0, 10, 0));
                    response.Formatter = new XmlMessageFormatter(new[] { "System.String,mscorlib" });
                    if (response.Label == MsmqMessageResponse.ResponseTypeStoreSensorData)
                    {
                        result = (StoreSensorDataResult)ContentParserFactory.GetParser(MimeTypeJson).Decode(response.Body.ToString(), typeof(StoreSensorDataResult));
                        return true;
                    }
                    else
                        if (response.Label == MsmqMessageResponse.ResponseTypeError)
                        {
                            result = (OperationResult)ContentParserFactory.GetParser(MimeTypeJson).Decode(response.Body.ToString(), typeof(OperationResult));
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

        private bool SendMsmqResponse(string queueAddress, string requestType, string requestBody, string correlationId, out string result)
        {
            try
            {
                using (MessageQueue queue = new MessageQueue(queueAddress))
                {
                    Message message = new Message();
                    message.Body = requestBody;
                    message.Label = requestType;
                    message.CorrelationId = correlationId;
                    message.Formatter = new XmlMessageFormatter(new[] { "System.String,mscorlib" });

                    queue.Send(message);
                    result = null;
                    return true;
                }
            }
            catch (Exception exc)
            {
                result = String.Format("MSMQ response failed. Error: {0}", exc.Message);
                return false;
            }
        }

        private bool ReceiveMsmqRequest(string queueAddress, string expectedRequestType, Type resultType, out object result, out string correlationId)
        {
            try
            {
                using (MessageQueue queue = new MessageQueue(queueAddress))
                {
                    Message request = null;
                    var messageEnumerator = queue.GetMessageEnumerator2();
                    while (messageEnumerator.MoveNext(new TimeSpan(0, 1, 0)))
                    {
                        if (messageEnumerator.Current.Label == expectedRequestType)
                        {
                            request = messageEnumerator.RemoveCurrent();
                            break;
                        }
                    }

                    if (request == null)
                    {
                        correlationId = null;
                        result = String.Format("No message with the required label has been found. Label: {0}", expectedRequestType);
                        return false;
                    }

                    request.Formatter = new XmlMessageFormatter(new[] { "System.String,mscorlib" });
                    if (request.Body == null || request.Body.ToString() == "OK")
                    {
                        result = null;
                    }
                    else
                    {
                        result = ContentParserFactory.GetParser(MimeTypeJson).Decode(request.Body.ToString(), resultType);
                    }
                    correlationId = request.Id;
                    return true;
                }
            }
            catch (Exception exc)
            {
                result = String.Format("MSMQ request failed. Error: {0}", exc.Message);
                correlationId = null;
                return false;
            }
        }

        internal Sensor CreateSampleMsmqPullSensor(Device device)
        {
            Sensor result = new Sensor();
            result.Category = "C";
            result.Description = "MSMQ Test sensor";
            result.DeviceId = device == null ? null : device.Id;
            result.Id = "MsmqTestSensorPull";
            result.InternalSensorId = 0;
            result.IsVirtualSensor = false;
            result.PullFrequencyInSeconds = 10;
            result.PullModeCommunicationType = PullModeCommunicationType.MSMQ;
            result.PushModeCommunicationType = PullModeCommunicationType.MSMQ;
            result.SensorDataRetrievalMode = SensorDataRetrievalMode.Pull;
            result.SensorValueDataType = SensorValueDataType.String;
            result.ShallSensorDataBePersisted = true;
            result.PersistDirectlyAfterChange = true;
            result.UnitSymbol = "U";
            result.IsSynchronousPushToActuator = false;
            result.IsActuator = false;

            return result;
        }

        internal Sensor CreateSampleMsmqPushSensor(Device device)
        {
            Sensor result = new Sensor();
            result.Category = "C";
            result.Description = "MSMQ Test sensor";
            result.DeviceId = device == null ? null : device.Id;
            result.Id = "MsmqTestSensorPush";
            result.InternalSensorId = 0;
            result.IsVirtualSensor = false;
            result.PullFrequencyInSeconds = 10;
            result.PullModeCommunicationType = PullModeCommunicationType.MSMQ;
            result.PushModeCommunicationType = PullModeCommunicationType.MSMQ;
            result.SensorDataRetrievalMode = SensorDataRetrievalMode.Push;
            result.SensorValueDataType = SensorValueDataType.String;
            result.ShallSensorDataBePersisted = true;
            result.PersistDirectlyAfterChange = true;
            result.UnitSymbol = "U";
            result.IsSynchronousPushToActuator = false;
            result.IsActuator = false;

            return result;
        }

        internal Sensor CreateSampleMsmqActuator(Device device)
        {
            Sensor result = new Sensor();
            result.Category = "C";
            result.Description = "MSMQ Test sensor actuator";
            result.DeviceId = device == null ? null : device.Id;
            result.Id = "MsmqTestSensorActuator";
            result.InternalSensorId = 0;
            result.IsVirtualSensor = false;
            result.PullFrequencyInSeconds = 0;
            result.PullModeCommunicationType = PullModeCommunicationType.MSMQ;
            result.PushModeCommunicationType = PullModeCommunicationType.MSMQ;
            result.SensorDataRetrievalMode = SensorDataRetrievalMode.Push;
            result.SensorValueDataType = SensorValueDataType.String;
            result.ShallSensorDataBePersisted = true;
            result.PersistDirectlyAfterChange = true;
            result.UnitSymbol = "U";
            result.IsSynchronousPushToActuator = true;
            result.IsActuator = true;

            return result;
        }

        internal Device CreateSampleMsmqDevice(string deviceId)
        {
            Device result = new Device();
            result.Description = deviceId;
            result.DeviceIpEndPoint = "IP:1.1.1.1";
            result.Id = deviceId;
            result.Location = new Location();
            result.Location.Longitude = 1;
            result.Location.Latitude = 1;
            result.Location.Elevation = 1;
            result.Location.Name = "Loc";

            return result;
        }
    }
}
