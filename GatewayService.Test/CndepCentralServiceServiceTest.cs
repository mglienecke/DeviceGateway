using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel.Web;
using GlobalDataContracts;
using System.ServiceModel;
using System.Net;
using System.IO;
using Common.Server;
using System.Web;
using System.Globalization;
using Common.Server.CNDEP;
using Newtonsoft.Json;

namespace GatewayServiceTest
{
    [TestClass]
    public class CndepCentralServiceServiceTest:TestBase
    {
        private const string MimeTypeJson = "application/json";
        private const int HttpRequestTimeoutInMillis = 60000;

        #region Properties...
        private static string CndepContentType
        {
            get;
            set;
        }

        private static string CndepServerAddress
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

        private static int CndepServerPort
        {
            get;
            set;
        }

        private static int CndepRequestTimeout
        {
            get;
            set;
        }

        private static int CndepRequestRetryCount
        {
            get;
            set;
        }

        private static CndepClient CndepClient
        {
            get;
            set;
        }

        private static CommunicationProtocol CndepCommunicationProtocol
        {
            get; set;
        }
        #endregion

        /// <summary>
        /// Initialize the assembly and the remote udpClient. If there is no remote connection then initialize Log4Net as well
        /// </summary>
        /// <param name="context">The context.</param>
        [ClassInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            RestClient = new WebClient();
            RestClient.Encoding = Encoding.UTF8;
            RestClient.Headers.Add(HttpRequestHeader.ContentType, MimeTypeJson);
            log4net.Config.XmlConfigurator.Configure();

            RestServerUrl = "http://localhost/GatewayService";

            CndepServerAddress = "192.168.1.10";
            CndepServerPort = 41120;
            CndepContentType = MimeTypeJson;
            CndepRequestRetryCount = 1;
            CndepRequestTimeout = 5000;
            CndepCommunicationProtocol = CommunicationProtocol.TCP;
            CndepClient = GetCndepClient(CndepServerPort, NetworkUtilities.GetIpV4AddressForDns(CndepServerAddress));
        }


        #region Registration of sensors, devices, triggers, sinks...
        #region RegisterDevice...
        [TestMethod]
        public void RegisterDevice()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();

            object responseObject;
            string content = ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDevice);
            OperationResult resultTemplate = new OperationResult();

            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterDevice, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult); 

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertSuccess(result);

            AssertDeviceRegistered(sampleDevice, true, true);
        }
        
        [TestMethod]
        public void RegisterDevice_NullContent()
        {
            object responseObject;
            string content = null;
            OperationResult resultTemplate = new OperationResult();

            Assert.IsFalse(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterDevice, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsFalse(responseObject is OperationResult); 
        }

        [TestMethod]
        public void RegisterDevice_EmptyContent()
        {
            object responseObject;
            string content = String.Empty;
            OperationResult resultTemplate = new OperationResult();

            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterDevice, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult); 

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
        }

        [TestMethod]
        public void RegisterDevice_ExistingId()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();

            object responseObject;
            string content = ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDevice);
            OperationResult resultTemplate = new OperationResult();

            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterDevice, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult); 

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertSuccess(result);

            Device sampleDevice1 = CreateSampleDevice();
            sampleDevice1.Id = sampleDevice.Id;
            content = ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDevice1);
            resultTemplate = new OperationResult();

            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterDevice, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult); 

            result = (OperationResult)responseObject;

            //Check
            AssertSuccess(result);

            AssertDeviceRegistered(sampleDevice1, true, true);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void RegisterDevice_InvalidId()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            sampleDevice.Id += "=>" + "1";

            object responseObject;
            string content = ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDevice);
            OperationResult resultTemplate = new OperationResult();

            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterDevice, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult); 

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);

            AssertDeviceRegistered(sampleDevice, false, false);
        }

        [TestMethod]
        public void RegisterDevice_InvalidData_DescriptionNull()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            sampleDevice.Description = null;

            object responseObject;
            string content = ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDevice);
            OperationResult resultTemplate = new OperationResult();

            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterDevice, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult); 

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
            AssertDeviceRegistered(sampleDevice, false, false);
        }

        [TestMethod]
        public void RegisterDevice_InvalidData_IdNull()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            sampleDevice.Id = null;

            object responseObject;
            string content = ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDevice);
            OperationResult resultTemplate = new OperationResult();

            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterDevice, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult); 

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
        }
        #endregion
        
        #region UpdateDevice...
        //In the context of the CNDEP  GatewayService updating a device is the same as registering it.
        #endregion

        #region RegisterSensors...

       [TestMethod]
        public void RegisterSensors()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Device sampleDevice2 = CreateAndRegisterSampleDevice();
            Device sampleDevice3 = CreateAndRegisterSampleDevice();

            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            Sensor sensor12 = CreateSampleSensor(sampleDevice1);
            Sensor sensor21 = CreateSampleSensor(sampleDevice2);

            //Device 1
            object responseObject;
           var device1Sensors = new Sensor[] {sensor11, sensor12};
            string content = ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors);
            OperationResult resultTemplate = new OperationResult();

            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterSensors, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertSuccess(result);
            AssertDeviceSensorsRegistered(device1Sensors.ToArray(), sampleDevice1);

            //Device 2
            Sensor[] device2Sensors = new Sensor[] { sensor21 };
            content = ContentParserFactory.GetParser(MimeTypeJson).Encode(device2Sensors);
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterSensors, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);
            AssertDeviceSensorsRegistered(device2Sensors, sampleDevice2);

            //Device 3
            AssertDeviceSensorsRegistered(new Sensor[] { }, sampleDevice3);
        }

       [TestMethod]
        public void RegisterSensors_ExistingSensors()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Device sampleDevice2 = CreateAndRegisterSampleDevice();
            Device sampleDevice3 = CreateAndRegisterSampleDevice();

            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            Sensor sensor12 = CreateSampleSensor(sampleDevice1);
            Sensor sensor21 = CreateSampleSensor(sampleDevice2);

            //Device 1
            object responseObject;
            var device1Sensors = new Sensor[] { sensor11, sensor12 };
            string content = ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors);
            OperationResult resultTemplate = new OperationResult();

            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterSensors, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertSuccess(result);
            AssertDeviceSensorsRegistered(device1Sensors.ToArray(), sampleDevice1);

            //Device 2
            Sensor[] device2Sensors = new Sensor[] { sensor21 };
            content = ContentParserFactory.GetParser(MimeTypeJson).Encode(device2Sensors);
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterSensors, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);
            AssertDeviceSensorsRegistered(device2Sensors, sampleDevice2);

            //Device 3
            AssertDeviceSensorsRegistered(new Sensor[] { }, sampleDevice3);

            //Create new sensors
            Sensor sensor11_ = CreateSampleSensor(sampleDevice1);
            Sensor sensor21_ = CreateSampleSensor(sampleDevice2);
            Sensor sensor22_ = CreateSampleSensor(sampleDevice2);
            Sensor sensor31_ = CreateSampleSensor(sampleDevice2);

            //Device 1
            content = ContentParserFactory.GetParser(MimeTypeJson).Encode(new Sensor[]{});
            resultTemplate = new OperationResult();

            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterSensors, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            result = (OperationResult)responseObject;

            //Check
            AssertSuccess(result);
            AssertDeviceSensorsRegistered(device1Sensors.ToArray(), sampleDevice1);

            //With update
            sensor11_.Id = sensor11.Id;
            device1Sensors[0] = sensor11_;

            content = ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors);
            resultTemplate = new OperationResult();

            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterSensors, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            result = (OperationResult)responseObject;

            //Check
            AssertSuccess(result);
            AssertDeviceSensorsRegistered(device1Sensors.ToArray(), sampleDevice1);

            //Device 2
            sensor21_.Id = sensor21.Id; //Update
            device2Sensors = new Sensor[] { sensor21_, sensor22_ };

            content = ContentParserFactory.GetParser(MimeTypeJson).Encode(device2Sensors);
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterSensors, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);
            AssertDeviceSensorsRegistered(device2Sensors, sampleDevice2);

            //Device 3
            Sensor[] device3Sensors = new Sensor[] {  };
            content = ContentParserFactory.GetParser(MimeTypeJson).Encode(device3Sensors);
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterSensors, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);
            AssertDeviceSensorsRegistered(device2Sensors, sampleDevice2);

            AssertDeviceSensorsRegistered(device3Sensors, sampleDevice3);
        }

        
       [TestMethod]
        public void RegisterSensors_NullContent()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();

            //Device 1
            object responseObject;
            OperationResult resultTemplate = new OperationResult();
            Assert.IsFalse(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterSensors, 0, null, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
        }

       [TestMethod]
        public void RegisterSensors_EmptyContent()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();

            //Device 1
            object responseObject;
            OperationResult resultTemplate = new OperationResult();
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterSensors, 0, String.Empty, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());

            Assert.IsNotNull(responseObject);
            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

       [TestMethod]
        public void RegisterSensors_NullElement()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();

            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            //Device 1
            object responseObject;
            Sensor[] device1Sensors = new Sensor[] { sensor11, null };
            string content = ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors);
            OperationResult resultTemplate = new OperationResult();
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterSensors, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            

            Assert.IsNotNull(responseObject);
            OperationResult resultRegSensors = (OperationResult)responseObject;
            AssertFailure(resultRegSensors);
        }

       [TestMethod]
        public void RegisterSensors_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();

            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            Sensor sensor12 = CreateSampleSensor(sampleDevice1);

            //Device 1
            object responseObject;
            Sensor[] device1Sensors = new Sensor[] { sensor11, sensor12 };
            string content = ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors);
            OperationResult resultTemplate = new OperationResult();
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterSensors, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());

            Assert.IsNotNull(responseObject);
            OperationResult resultRegSensors = (OperationResult)responseObject;
            AssertFailure(resultRegSensors);
        }

       [TestMethod]
        public void RegisterSensors_InvalidData_NullDescription()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            sensor11.Description = null;

            //Device 1
            object responseObject;
            Sensor[] device1Sensors = new Sensor[] { sensor11 };
            string content = ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors);
            OperationResult resultTemplate = new OperationResult();
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterSensors, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());

            Assert.IsNotNull(responseObject);
            OperationResult resultRegSensors = (OperationResult)responseObject;
            AssertFailure(resultRegSensors);
        }

       [TestMethod]
        public void RegisterSensors_InvalidData_NullId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            sensor11.Id = null;

            //Device 1
            object responseObject;
            Sensor[] device1Sensors = new Sensor[] { sensor11 };
            string content = ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors);
            OperationResult resultTemplate = new OperationResult();
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterSensors, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());

            Assert.IsNotNull(responseObject);
            OperationResult resultRegSensors = (OperationResult)responseObject;
            AssertFailure(resultRegSensors);
        }

       [TestMethod]
        public void RegisterSensors_InvalidData_NullUnitSymbol()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            sensor11.UnitSymbol = null;

            //Device 1
            object responseObject;
            Sensor[] device1Sensors = new Sensor[] { sensor11 };
            string content = ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors);
            OperationResult resultTemplate = new OperationResult();
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdRegisterSensors, 0, content, CndepContentType, resultTemplate, out responseObject), responseObject.ToString());

            Assert.IsNotNull(responseObject);
            OperationResult resultRegSensors = (OperationResult)responseObject;
            AssertFailure(resultRegSensors);
        }
        #endregion
        
        #region UpdateSensor...
       //In the context of the CNDEP  GatewayService updating sensors of a device is the same as registering them.
        #endregion

        #endregion
        
        #region Sensors, devices, sensor data...

        #region StoreMultipleSensorData...
        /*
       [TestMethod]
        public void StoreMultipleSensorData()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

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
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

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
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);
            Sensor sensor12 = CreateAndRegisterSampleSensor(sampleDevice1);

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
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

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
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

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
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

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
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

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
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

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
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

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

        #region StoreSingleSensorData...

        //[TestMethod]
        //public void A_TEST_StoreSingleSensorData()
        //{
        //    SensorData[] sampleData = new SensorData[5];
        //    for (int i = 0; i < sampleData.Length; i++)
        //    {
        //        sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString()};
        //    }

        //    //Store
        //    object responseObject;
        //    OperationResult resultTemplate = new OperationResult();

        //    StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = "sensor_A1", Measures = sampleData } }, DeviceId = "A192.168.1.1" };
        //    Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
        //        CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
        //    Assert.IsNotNull(responseObject);
        //    Assert.IsTrue(responseObject is OperationResult);

        //    OperationResult result = (OperationResult)responseObject;

        //    //Check
        //    AssertSuccess(result);
        //}

       [TestMethod]
        public void StoreSingleSensorData()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            SensorData[] sampleData = new SensorData[5];
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id, Measures = sampleData } }, DeviceId = sampleDevice1.Id };
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request), 
                CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertSuccess(result);
        }

       [TestMethod]
       public void StoreSingleSensorData_LargeData()
       {
           Device sampleDevice1 = CreateAndRegisterSampleDevice();
           Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

           SensorData[] sampleData = new SensorData[700];
           for (int i = 0; i < sampleData.Length; i++)
           {
               sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
           }

           //Store
           object responseObject;
           OperationResult resultTemplate = new OperationResult();

           StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id, Measures = sampleData } }, DeviceId = sampleDevice1.Id };
           Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
               CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
           Assert.IsNotNull(responseObject);
           Assert.IsTrue(responseObject is OperationResult);

           OperationResult result = (OperationResult)responseObject;

           //Check
           AssertSuccess(result);
       }

       [TestMethod]
        public void StoreSingleSensorData_EmptyDeviceId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            SensorData[] sampleData = new SensorData[5];
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id, Measures = sampleData } }, DeviceId = String.Empty };
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
                CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_EmptySensorID()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            SensorData[] sampleData = new SensorData[5];
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = String.Empty, Measures = sampleData } }, DeviceId = sampleDevice1.Id };
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
                CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            SensorData[] sampleData = new SensorData[5];
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id, Measures = sampleData } }, DeviceId = "Fake!" };
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
                CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_DeviceIdSensorIdMismatch()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Device sampleDevice2 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            SensorData[] sampleData = new SensorData[5];
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id, Measures = sampleData } }, DeviceId = sampleDevice2.Id };
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
                CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_NonExistingSensorId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            SensorData[] sampleData = new SensorData[5];
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = "Fake!", Measures = sampleData } }, DeviceId = sampleDevice1.Id };
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
                CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_NullSensorData()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id } }, DeviceId = sampleDevice1.Id };
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
                CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
        }

       [TestMethod]
       public void StoreSingleSensorData_NullMultipleSensorData()
       {
           Device sampleDevice1 = CreateAndRegisterSampleDevice();
           Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

           //Store
           object responseObject;
           OperationResult resultTemplate = new OperationResult();

           StoreSensorDataRequest request = new StoreSensorDataRequest() { DeviceId = sampleDevice1.Id };
           Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
               CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
           Assert.IsNotNull(responseObject);
           Assert.IsTrue(responseObject is OperationResult);

           OperationResult result = (OperationResult)responseObject;

           //Check
           AssertFailure(result);
       }

       [TestMethod]
        public void StoreSingleSensorData_ZeroElementSensorData()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            SensorData[] sampleData = new SensorData[0];


            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id, Measures = sampleData } }, DeviceId = sampleDevice1.Id };
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
                CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_NullSensorDataArrayElement()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            SensorData[] sampleData = new SensorData[1];

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = sensor11.Id, Measures = sampleData } }, DeviceId = sampleDevice1.Id };
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
                CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_InvalidSensorDataGeneratedWhenField()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

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
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
                CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);
        }

       [TestMethod]
        public void StoreSingleSensorData_NullSensorDataValueField()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

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
            Assert.IsTrue(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
                CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Assert.IsNotNull(responseObject);
            Assert.IsTrue(responseObject is OperationResult);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);
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

        private Device CreateAndRegisterSampleDevice()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();

            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(RestServerUrl + String.Format(ServerHttpRestUrls.SingleDeviceWithId, sampleDevice.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDevice),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertSuccess(result);
            AssertDeviceRegistered(sampleDevice, true, true);

            return sampleDevice;
        }

        private Sensor CreateAndRegisterSampleSensor(Device device)
        {
            //Create
            Sensor sampleSensor = CreateSampleSensor(device);

            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(RestServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdMultipleSensors, device.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(new Sensor[]{sampleSensor}),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertSuccess(result);

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
            Assert.AreEqual(sensors.Count(), resultSensorsForDevice.Sensors.Count());

            for (int i = 0; i < sensors.Length; i++)
            {
                Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensors[i].Id) != null);
                AssertAreEqual(sensors[i], resultSensorsForDevice.Sensors.First(x => x.Id == sensors[i].Id));
            }
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

        private static CndepClient GetCndepClient(int serverPort, IPAddress serverAddress)
        {
            CndepClient client;
            switch (CndepCommunicationProtocol)
            {
                case CommunicationProtocol.UDP:
                    client = new CndepUdpClient(serverPort, serverAddress);
                    break;
                case CommunicationProtocol.TCP:
                    client = new CndepTcpClient(serverPort, serverAddress);
                    break;
                default:
                    throw new Exception(String.Format("Unhandled CommunicationProtocol value: {0}.", CndepCommunicationProtocol));
            }

            return client;
        }

        private bool SendCndepRequest(string serverAddress, int serverPort, byte command, byte function, string requestData, string contentType, object resultObjectTemplate, out object result, params object[] parameters)
        {
            try
            {
                using (CndepClient udpClient = GetCndepClient(serverPort, Dns.GetHostAddresses(serverAddress)[0]))
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
                        response = udpClient.Send(request, CndepRequestTimeout);

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
                                        result = String.Format("Server replied with an empty string.");
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
                    while (response == null && retryCount++ < CndepRequestRetryCount);

                    result = "Response timeout";
                    return false;
                }

            }
            catch (Exception exc)
            {
                result = String.Format("CNDEP request failed. Error: {0}", exc.Message);
                return false;
            }
        }

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
