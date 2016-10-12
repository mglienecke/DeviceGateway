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

namespace GatewayServiceTest
{
    [TestClass]
    public class RESTfulHttpGatewaysServiceTest:TestBase
    {
        private const string MimeTypeJson = "application/json";
        private const int HttpRequestTimeoutInMillis = 60000;

        private static string ServerUrl
        {
            get;
            set;
        }

        private static WebClient Client
        {
            get;
            set;
        }

        /// <summary>
        /// Initialize the assembly and the remote client. If there is no remote connection then initialize Log4Net as well
        /// </summary>
        /// <param name="context">The context.</param>
        [ClassInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            Client = new WebClient();
            Client.Encoding = Encoding.UTF8;
            Client.Headers.Add(HttpRequestHeader.ContentType, MimeTypeJson);
            log4net.Config.XmlConfigurator.Configure();

            ServerUrl = "http://localhost/GatewayService";
        }


        #region Registration of sensors, devices, triggers, sinks...
/*
        #region SPECIAL...
        [TestMethod]
        public void RegisterDevice_Special()
        {
            //Create
            Device sampleDevice = new Device();
            sampleDevice.Description = "Tahoe II device";
            sampleDevice.DeviceIpEndPoint = "192.168.1.12";
            sampleDevice.Id = "192.168.1.12";
            sampleDevice.Location = new Location();
            sampleDevice.Location.Longitude = 0;
            sampleDevice.Location.Latitude = 0;
            sampleDevice.Location.Elevation = 0;
            sampleDevice.Location.Name = "Zero";

            OperationResult result = Channel.RegisterDevice(sampleDevice);

            log4net.LogManager.GetLogger(this.GetType()).Info("SEE ME???");

            //Check
            AssertSuccess(result);

            GetDevicesResult resultDevices = Channel.GetDevicesByIds(new string[] { sampleDevice.Id });
            AssertSuccess(resultDevices);
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice.Id) != null);
            AssertAreEqual(sampleDevice, resultDevices.Devices.First(x => x.Id == sampleDevice.Id));
        }

        //[TestMethod]
        public void RegisterSensors_Special()
        {
            string sampleDeviceId = "192.168.1.12";

            Sensor sensor1 = new Sensor();
            sensor1.Category = "C";
            sensor1.Description = "Tahoe II temperature sensor";
            sensor1.DeviceId = sampleDeviceId;
            sensor1.Id =  "temperature";
            sensor1.InternalSensorId = 0;
            sensor1.IsVirtualSensor = false;
            sensor1.PullFrequencyInSeconds = 10;
            sensor1.PullModeCommunicationType = PullModeCommunicationType.REST;
            sensor1.PullModeDotNetObjectType = null;
            sensor1.SensorDataRetrievalMode = SensorDataRetrievalMode.Push;
            sensor1.SensorValueDataType = SensorValueDataType.Decimal;
            sensor1.ShallSensorDataBePersisted = true;
            sensor1.PersistDirectlyAfterChange = true;
            sensor1.UnitSymbol = "C";
            sensor1.VirtualSensorDefinition = new VirtualSensorDefinition();
            sensor1.VirtualSensorDefinition.Definition = null;
            sensor1.VirtualSensorDefinition.VirtualSensorCalculationType = VirtualSensorCalculationType.Undefined;
            sensor1.VirtualSensorDefinition.VirtualSensorDefinitionType = VirtualSensorDefinitionType.Undefined;

            Sensor sensor2 = new Sensor();
            sensor1.Category = "C";
            sensor1.Description = "Tahoe II AUX sensor";
            sensor1.DeviceId = sampleDeviceId;
            sensor1.Id =  "aux";
            sensor1.InternalSensorId = 0;
            sensor1.IsVirtualSensor = false;
            sensor1.PullFrequencyInSeconds = 10;
            sensor1.PullModeCommunicationType = PullModeCommunicationType.REST;
            sensor1.PullModeDotNetObjectType = null;
            sensor1.SensorDataRetrievalMode = SensorDataRetrievalMode.Pull;
            sensor1.SensorValueDataType = SensorValueDataType.Decimal;
            sensor1.ShallSensorDataBePersisted = true;
            sensor1.PersistDirectlyAfterChange = true;
            sensor1.UnitSymbol = "X";
            sensor1.VirtualSensorDefinition = null;

            Sensor sensor3 = new Sensor();
            sensor1.Category = "C";
            sensor1.Description = "Tahoe II BAT sensor";
            sensor1.DeviceId = sampleDeviceId;
            sensor1.Id = "bat";
            sensor1.InternalSensorId = 0;
            sensor1.IsVirtualSensor = false;
            sensor1.PullFrequencyInSeconds = 0;
            sensor1.PullModeCommunicationType = PullModeCommunicationType.REST;
            sensor1.PullModeDotNetObjectType = null;
            sensor1.SensorDataRetrievalMode = SensorDataRetrievalMode.Push;
            sensor1.SensorValueDataType = SensorValueDataType.Decimal;
            sensor1.ShallSensorDataBePersisted = true;
            sensor1.PersistDirectlyAfterChange = true;
            sensor1.UnitSymbol = "X";
            sensor1.VirtualSensorDefinition = new VirtualSensorDefinition();
            sensor1.VirtualSensorDefinition.Definition = null;
            sensor1.VirtualSensorDefinition.VirtualSensorCalculationType = VirtualSensorCalculationType.Undefined;
            sensor1.VirtualSensorDefinition.VirtualSensorDefinitionType = VirtualSensorDefinitionType.Undefined;

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor1, sensor2, sensor3 });
            AssertSuccess(resultRegSensors);

            GetSensorsForDeviceResult resultSensorsForDevice = Channel.GetSensorsForDevice(sampleDeviceId);
            AssertSuccess(resultSensorsForDevice);

            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 2);
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor1.Id) != null);
            AssertAreEqual(sensor1, resultSensorsForDevice.Sensors.First(x => x.Id == sensor1.Id));
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor2.Id) != null);
            AssertAreEqual(sensor2, resultSensorsForDevice.Sensors.First(x => x.Id == sensor2.Id));
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor3.Id) != null);
            AssertAreEqual(sensor3, resultSensorsForDevice.Sensors.First(x => x.Id == sensor3.Id));
        }
        #endregion
        */
        #region RegisterDevice...
        [TestMethod]
        public void RegisterDevice()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();

            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.SingleDeviceWithId, sampleDevice.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDevice), 
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertSuccess(result);

            AssertDeviceRegistered(sampleDevice, true, true);
        }
        
        [TestMethod]
        public void RegisterDevice_NullContent()
        {
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.SingleDeviceWithId, Guid.NewGuid().ToString()), null, 
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
        }

        [TestMethod]
        public void RegisterDevice_EmptyContent()
        {
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.SingleDeviceWithId, Guid.NewGuid().ToString()), String.Empty,
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

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
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.SingleDeviceWithId, sampleDevice.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDevice),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertSuccess(result);

            Device sampleDevice1 = CreateSampleDevice();
            sampleDevice1.Id = sampleDevice.Id;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.SingleDeviceWithId, sampleDevice.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDevice1),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

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
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.SingleDeviceWithId, sampleDevice.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDevice),
                 MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

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
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.SingleDeviceWithId, sampleDevice.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDevice),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;

            //Check
            AssertFailure(result);
            AssertDeviceRegistered(sampleDevice, false, false);
        }

        [TestMethod]
        [ExpectedException(typeof (WebException))]
        public void RegisterDevice_InvalidData_IdNull()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            sampleDevice.Id = null;

            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.SingleDeviceWithId, sampleDevice.Id),
                                                    ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDevice),
                                                    MimeTypeJson, HttpRequestTimeoutInMillis, typeof (OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult) responseObject;

            //Check
            AssertFailure(result);
        }

        #endregion
        
        #region UpdateDevice...
        //In the context of the RESTfull HTTP GatewayService updating a device is the same as registering it (the same PUT method).
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
            Sensor[] device1Sensors = new Sensor[]{sensor11, sensor12};
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

            Assert.IsNotNull(responseObject);
            OperationResult resultRegSensors = (OperationResult)responseObject;
            AssertSuccess(resultRegSensors);
            AssertDeviceSensorsRegistered(device1Sensors.ToArray(), sampleDevice1);

            //Device 2
            Sensor[] device2Sensors = new Sensor[] { sensor21 };
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice2.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(device2Sensors),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            
            Assert.IsNotNull(responseObject);
            resultRegSensors = (OperationResult)responseObject;
            AssertSuccess(resultRegSensors);
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
            Sensor[] device1Sensors = new Sensor[] { sensor11, sensor12 };
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

            Assert.IsNotNull(responseObject);
            OperationResult resultRegSensors = (OperationResult)responseObject;
            AssertSuccess(resultRegSensors);
            AssertDeviceSensorsRegistered(device1Sensors.ToArray(), sampleDevice1);

            //Device 2
            Sensor[] device2Sensors = new Sensor[] { sensor21 };
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice2.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(device2Sensors),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

            Assert.IsNotNull(responseObject);
            resultRegSensors = (OperationResult)responseObject;
            AssertSuccess(resultRegSensors);
            AssertDeviceSensorsRegistered(device2Sensors, sampleDevice2);

            //Device 3
            AssertDeviceSensorsRegistered(new Sensor[] { }, sampleDevice3);

            //Create new sensors
            Sensor sensor11_ = CreateSampleSensor(sampleDevice1);
            Sensor sensor21_ = CreateSampleSensor(sampleDevice2);
            Sensor sensor22_ = CreateSampleSensor(sampleDevice2);
            Sensor sensor31_ = CreateSampleSensor(sampleDevice2);

            //Device 1
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(new Sensor[] { }),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

            Assert.IsNotNull(responseObject);
            resultRegSensors = (OperationResult)responseObject;
            AssertSuccess(resultRegSensors);
            AssertDeviceSensorsRegistered(device1Sensors.ToArray(), sampleDevice1);

            //With update
            sensor11_.Id = sensor11.Id;
            device1Sensors[0] = sensor11_;

            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

            Assert.IsNotNull(responseObject);
            resultRegSensors = (OperationResult)responseObject;
            AssertSuccess(resultRegSensors);
            AssertDeviceSensorsRegistered(device1Sensors.ToArray(), sampleDevice1);

            //Device 2
            sensor21_.Id = sensor21.Id; //Update
            device2Sensors = new Sensor[] { sensor21_, sensor22_ };

            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice2.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(device2Sensors),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

            Assert.IsNotNull(responseObject);
            resultRegSensors = (OperationResult)responseObject;
            AssertSuccess(resultRegSensors);
            AssertDeviceSensorsRegistered(device2Sensors, sampleDevice2);

            //Device 3
            Sensor[] device3Sensors = new Sensor[] {  };
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice2.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(device2Sensors),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

            AssertDeviceSensorsRegistered(device3Sensors, sampleDevice3);
        }

        [TestMethod]
        public void RegisterSensors_DeviceIdMismatch()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Device sampleDevice2 = CreateAndRegisterSampleDevice();
            Device sampleDevice3 = CreateAndRegisterSampleDevice();

            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            //Device 2
            object responseObject;
            Sensor[] device1Sensors = new Sensor[] { sensor11 };
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice2.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

            Assert.IsNotNull(responseObject);
            OperationResult resultRegSensors = (OperationResult)responseObject;
            AssertSuccess(resultRegSensors);
            sensor11.DeviceId = sampleDevice2.Id;
            AssertDeviceSensorsRegistered(device1Sensors.ToArray(), sampleDevice2);
        }
        
        [TestMethod]
        public void RegisterSensors_NullContent()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();

            //Device 1
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice1.Id), null,
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

            Assert.IsNotNull(responseObject);
            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

        [TestMethod]
        public void RegisterSensors_EmptyContent()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();

            //Device 1
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice1.Id), String.Empty,
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

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
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

            Assert.IsNotNull(responseObject);
            OperationResult resultRegSensors = (OperationResult)responseObject;
            AssertFailure(resultRegSensors);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void RegisterSensors_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();

            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            Sensor sensor12 = CreateSampleSensor(sampleDevice1);

            //Device 1
            object responseObject;
            Sensor[] device1Sensors = new Sensor[] { sensor11, sensor12 };
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

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
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

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
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

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
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(device1Sensors),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

            Assert.IsNotNull(responseObject);
            OperationResult resultRegSensors = (OperationResult)responseObject;
            AssertFailure(resultRegSensors);
        }

        #endregion
        
        #region UpdateSensor...

        [TestMethod]
        public void UpdateSensor()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            sensor11.Category += "1";
            sensor11.Description += "2";
            sensor11.IsVirtualSensor = !sensor11.IsVirtualSensor;
            sensor11.PullFrequencyInSeconds += 3;
            sensor11.PullModeCommunicationType = (PullModeCommunicationType)Enum.GetValues(typeof(PullModeCommunicationType)).GetValue((int)sensor11.PullModeCommunicationType % Enum.GetValues(typeof(PullModeCommunicationType)).Length);
            sensor11.PullModeDotNetObjectType += "3.5";
            sensor11.SensorDataRetrievalMode = (SensorDataRetrievalMode)Enum.GetValues(typeof(SensorDataRetrievalMode)).GetValue((int)sensor11.SensorDataRetrievalMode % Enum.GetValues(typeof(SensorDataRetrievalMode)).Length);
            sensor11.SensorValueDataType = (SensorValueDataType)Enum.GetValues(typeof(SensorValueDataType)).GetValue((int)sensor11.SensorValueDataType % Enum.GetValues(typeof(SensorValueDataType)).Length);
            sensor11.ShallSensorDataBePersisted = !sensor11.ShallSensorDataBePersisted;
            sensor11.UnitSymbol += "4";
            sensor11.VirtualSensorDefinition = CreateSampleVirtualSensorDefinition();

            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSingleSensorWithId, sampleDevice1.Id, sensor11.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sensor11),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;

            Sensor[] device1Sensors = new Sensor[] { sensor11 };
            AssertDeviceSensorsRegistered(device1Sensors.ToArray(), sampleDevice1);
        }

        [TestMethod]
        public void UpdateSensor_Null()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSingleSensorWithId, sampleDevice1.Id, sensor11.Id), null,
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void UpdateSensor_NonExistingDeviceI()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSingleSensorWithId, "-NotExistingId-", sensor11.Id), 
                ContentParserFactory.GetParser(MimeTypeJson).Encode(sensor11), MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void UpdateSensor_NonExistingDeviceIdAndSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSingleSensorWithId, sampleDevice1.Id, sensor11.Id),
                ContentParserFactory.GetParser(MimeTypeJson).Encode(sensor11), MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void UpdateSensor_NullDeviceId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSingleSensorWithId, null, sensor11.Id),
                ContentParserFactory.GetParser(MimeTypeJson).Encode(sensor11), MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

        [TestMethod]
        public void UpdateSensor_NullDescription()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);
            sensor11.Description = null;

            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSingleSensorWithId, sampleDevice1.Id, sensor11.Id),
                ContentParserFactory.GetParser(MimeTypeJson).Encode(sensor11), MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

        [TestMethod]
        public void UpdateSensor_NullUnitSymbol()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);
            sensor11.UnitSymbol = null;

            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSingleSensorWithId, sampleDevice1.Id, sensor11.Id),
                ContentParserFactory.GetParser(MimeTypeJson).Encode(sensor11), MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

        #endregion

        #endregion
        
        #region Checks...
        
        #region IsDeviceIdUsed...
        [TestMethod]
        public void IsDeviceIdUsed()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();

            //Not used
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceIsUsed, sampleDevice.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(IsDeviceIdUsedResult), out responseObject));
            Assert.IsNotNull(responseObject);

            IsDeviceIdUsedResult resultUsed = (IsDeviceIdUsedResult)responseObject;
            AssertSuccess(resultUsed);
            Assert.IsFalse(resultUsed.IsUsed);


            //Register
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.SingleDeviceWithId, sampleDevice.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDevice),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;

            //Is used
            AssertSuccess(result);

            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceIsUsed, sampleDevice.Id), MimeTypeJson,
                 HttpRequestTimeoutInMillis, typeof(IsDeviceIdUsedResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultUsed = (IsDeviceIdUsedResult)responseObject;
            AssertSuccess(resultUsed);
            Assert.IsTrue(resultUsed.IsUsed);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsDeviceIdUsed_NullId()
        {
            //Not used
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceIsUsed, null), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(IsDeviceIdUsedResult), out responseObject));
            Assert.IsNotNull(responseObject);

            IsDeviceIdUsedResult resultUsed = (IsDeviceIdUsedResult)responseObject;
            AssertSuccess(resultUsed);
            Assert.IsFalse(resultUsed.IsUsed);
        }

        [TestMethod]
        public void IsDeviceIdUsed_EmptyId()
        {
            //Not used
            object responseObject;
            try{
                Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceIsUsed, String.Empty), MimeTypeJson,
                    HttpRequestTimeoutInMillis, typeof(IsDeviceIdUsedResult), out responseObject));
                Assert.IsNotNull(responseObject);

                IsDeviceIdUsedResult resultUsed = (IsDeviceIdUsedResult)responseObject;
                AssertSuccess(resultUsed);
                Assert.IsFalse(resultUsed.IsUsed);
            }
            catch (WebException exc)
            {

            }
        }

        #endregion

        #region IsSensorIdRegisteredForDevice...

        [TestMethod]
        public void IsSensorIdRegisteredForDevice()
        {
            Device sampleDevice = CreateAndRegisterSampleDevice();

            Sensor sensor11 = CreateSampleSensor(sampleDevice);
            Sensor sensor12 = CreateSampleSensor(sampleDevice);

            //Check
            //Sensor 1 - NO
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceSensorIsRegistered, sampleDevice.Id, sensor11.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(IsSensorIdRegisteredForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            Assert.IsNotNull(responseObject);
            IsSensorIdRegisteredForDeviceResult resultIsRegSensor = (IsSensorIdRegisteredForDeviceResult)responseObject;
            AssertSuccess(resultIsRegSensor);
            Assert.IsFalse(resultIsRegSensor.IsRegistered);

            //Sensor 2 - NO
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceSensorIsRegistered, sampleDevice.Id, sensor11.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(IsSensorIdRegisteredForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            Assert.IsNotNull(responseObject);
            resultIsRegSensor = (IsSensorIdRegisteredForDeviceResult)responseObject;
            AssertSuccess(resultIsRegSensor);
            Assert.IsFalse(resultIsRegSensor.IsRegistered);

            
            //Device 1 sensors - reg sensor1
            Sensor[] device1Sensors = new Sensor[]{sensor11, sensor12};
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(new Sensor[]{sensor11}),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

            Assert.IsNotNull(responseObject);
            OperationResult resultRegSensors = (OperationResult)responseObject;
            AssertSuccess(resultRegSensors);

            //Sensor 1 - YES
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceSensorIsRegistered, sampleDevice.Id, sensor11.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(IsSensorIdRegisteredForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            Assert.IsNotNull(responseObject);
            resultIsRegSensor = (IsSensorIdRegisteredForDeviceResult)responseObject;
            AssertSuccess(resultIsRegSensor);
            Assert.IsTrue(resultIsRegSensor.IsRegistered);

            //Sensor 2 - NO
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceSensorIsRegistered, sampleDevice.Id, sensor12.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(IsSensorIdRegisteredForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            Assert.IsNotNull(responseObject);
            resultIsRegSensor = (IsSensorIdRegisteredForDeviceResult)responseObject;
            AssertSuccess(resultIsRegSensor);
            Assert.IsFalse(resultIsRegSensor.IsRegistered);

            //Device 1 sensors - reg sensor2
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(new Sensor[]{sensor12}),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));

            Assert.IsNotNull(responseObject);
            resultRegSensors = (OperationResult)responseObject;
            AssertSuccess(resultRegSensors);

            //Sensor 1 - YES
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceSensorIsRegistered, sampleDevice.Id, sensor11.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(IsSensorIdRegisteredForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            Assert.IsNotNull(responseObject);
            resultIsRegSensor = (IsSensorIdRegisteredForDeviceResult)responseObject;
            AssertSuccess(resultIsRegSensor);
            Assert.IsTrue(resultIsRegSensor.IsRegistered);

            //Sensor 2 - YES
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceSensorIsRegistered, sampleDevice.Id, sensor11.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(IsSensorIdRegisteredForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            Assert.IsNotNull(responseObject);
            resultIsRegSensor = (IsSensorIdRegisteredForDeviceResult)responseObject;
            AssertSuccess(resultIsRegSensor);
            Assert.IsTrue(resultIsRegSensor.IsRegistered);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void IsSensorIdRegisteredForDevice_NullDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceSensorIsRegistered, null, sensor11.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(IsSensorIdRegisteredForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            Assert.IsNotNull(responseObject);
            IsSensorIdRegisteredForDeviceResult resultIsRegSensor = (IsSensorIdRegisteredForDeviceResult)responseObject;
            AssertSuccess(resultIsRegSensor);
            Assert.IsFalse(resultIsRegSensor.IsRegistered);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void IsSensorIdRegisteredForDevice_EmptyDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceSensorIsRegistered, String.Empty, sensor11.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(IsSensorIdRegisteredForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            Assert.IsNotNull(responseObject);
            IsSensorIdRegisteredForDeviceResult resultIsRegSensor = (IsSensorIdRegisteredForDeviceResult)responseObject;
            AssertSuccess(resultIsRegSensor);
            Assert.IsFalse(resultIsRegSensor.IsRegistered);
        }

        [TestMethod]
        public void IsSensorIdRegisteredForDevice_NullSensorId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceSensorIsRegistered, sampleDevice1.Id, null), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(IsSensorIdRegisteredForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            Assert.IsNotNull(responseObject);
            IsSensorIdRegisteredForDeviceResult resultIsRegSensor = (IsSensorIdRegisteredForDeviceResult)responseObject;
            AssertSuccess(resultIsRegSensor);
            Assert.IsFalse(resultIsRegSensor.IsRegistered);
        }

        [TestMethod]
        //[ExpectedException(typeof(WebException))]
        public void IsSensorIdRegisteredForDevice_EmptySensorId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceSensorIsRegistered, sampleDevice1.Id, String.Empty), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(IsSensorIdRegisteredForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            Assert.IsNotNull(responseObject);
            IsSensorIdRegisteredForDeviceResult resultIsRegSensor = (IsSensorIdRegisteredForDeviceResult)responseObject;
            AssertSuccess(resultIsRegSensor);
            Assert.IsFalse(resultIsRegSensor.IsRegistered);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void IsSensorIdRegisteredForDevice_InvalidDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceSensorIsRegistered, sampleDevice1.Id, sensor11.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(IsSensorIdRegisteredForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            Assert.IsNotNull(responseObject);
            IsSensorIdRegisteredForDeviceResult resultIsRegSensor = (IsSensorIdRegisteredForDeviceResult)responseObject;
            AssertFailure(resultIsRegSensor);
            Assert.IsFalse(resultIsRegSensor.IsRegistered);
        }
        #endregion

        #endregion
        
        #region Sensors, devices, sensor data...

        #region GetDevices...
        [TestMethod]
        public void GetDevices()
        {
            //Create
            Device sampleDevice = CreateAndRegisterSampleDevice();

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + ServerHttpRestUrls.MultipleDevices, MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetDevicesResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetDevicesResult resultDevices = (GetDevicesResult)responseObject;
            AssertSuccess(resultDevices);
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice.Id) != null);
            AssertAreEqual(sampleDevice, resultDevices.Devices.First(x => x.Id == sampleDevice.Id));
        }

        [TestMethod]
        public void GetDevices_Selected()
        {
            //Create
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Device sampleDevice2 = CreateAndRegisterSampleDevice();

            //Get 1st
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + ServerHttpRestUrls.MultipleDevices + GetIdQueryString(new string[] { sampleDevice1.Id }), MimeTypeJson, 
                HttpRequestTimeoutInMillis, typeof(GetDevicesResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetDevicesResult resultDevices = (GetDevicesResult)responseObject;
            AssertSuccess(resultDevices);
            Assert.AreEqual(1, resultDevices.Devices.Count());
            AssertAreEqual(sampleDevice1, resultDevices.Devices[0]);

            //Get 2nd
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + ServerHttpRestUrls.MultipleDevices + GetIdQueryString(new string[] { sampleDevice2.Id }), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetDevicesResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultDevices = (GetDevicesResult)responseObject;
            AssertSuccess(resultDevices);
            Assert.AreEqual(1, resultDevices.Devices.Count());
            AssertAreEqual(sampleDevice2, resultDevices.Devices[0]);

            //Get both
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + ServerHttpRestUrls.MultipleDevices + GetIdQueryString(new string[] { sampleDevice1.Id, sampleDevice2.Id }), MimeTypeJson, 
                HttpRequestTimeoutInMillis, typeof(GetDevicesResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultDevices = (GetDevicesResult)responseObject;
            AssertSuccess(resultDevices);
            Assert.AreEqual(2, resultDevices.Devices.Count());
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice1.Id) != null);
            AssertAreEqual(sampleDevice1, resultDevices.Devices.First(x => x.Id == sampleDevice1.Id));
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice2.Id) != null);
            AssertAreEqual(sampleDevice2, resultDevices.Devices.First(x => x.Id == sampleDevice2.Id));
        }


        [TestMethod]
        public void GetDevices_NullIds()
        {
            //Create
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.MultipleDevices, GetIdQueryString(new string[] { null, null })), MimeTypeJson, 
                HttpRequestTimeoutInMillis, typeof(GetDevicesResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetDevicesResult resultDevices = (GetDevicesResult)responseObject;

            AssertSuccess(resultDevices);
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice1.Id) != null);
            AssertAreEqual(sampleDevice1, resultDevices.Devices.First(x => x.Id == sampleDevice1.Id));
        }

        [TestMethod]
        public void GetDevices_EmptyIds()
        {
            //Create
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.MultipleDevices, GetIdQueryString(new string[] { String.Empty, String.Empty })), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetDevicesResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetDevicesResult resultDevices = (GetDevicesResult)responseObject;

            AssertSuccess(resultDevices);
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice1.Id) != null);
            AssertAreEqual(sampleDevice1, resultDevices.Devices.First(x => x.Id == sampleDevice1.Id));
        }
        #endregion


        #region StoreMultipleSensorData...
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
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData};

            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
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
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }
            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData};

            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice2.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;

            Assert.IsFalse(result.Success);
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
                sampleData1.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }
            MultipleSensorData sampleData2 = new MultipleSensorData();
            sampleData2.SensorId = sensor12.Id;
            sampleData2.Measures = new SensorData[5];
            for (int i = 0; i < sampleData2.Measures.Length; i++)
            {
                sampleData2.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData1, sampleData2};

            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
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
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData };

            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, String.Empty), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void StoreMultipleSensorData_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData };

            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, "-NotExistingId-"), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
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
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

        [TestMethod]
        public void StoreMultipleSensorData_NullSensorDataArrayElement()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }
            sampleData.Measures[1] = null;

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData };
            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

        [TestMethod]
        public void StoreMultipleSensorData_InvalidSensorDataGeneratedWhenField()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }
            sampleData.Measures[0].GeneratedWhen = DateTime.MinValue;

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData };
            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
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
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }
            sampleData.Measures[0].Value = null;

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData };
            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);
        }
        #endregion

        #region StoreSingleSensorData...
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
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorWithIdSensorData, sampleDevice1.Id, sensor11.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleData),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
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
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorWithIdSensorData, String.Empty, sensor11.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleData),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);
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
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorWithIdSensorData, sampleDevice1.Id, String.Empty), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleData),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
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
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorWithIdSensorData, "-NotExistingId-", sensor11.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleData),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
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
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorWithIdSensorData, sampleDevice2.Id, sensor11.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleData),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
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
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorWithIdSensorData, sampleDevice1.Id, "-NotExistingId-"), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleData),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertFailure(result);
        }

        [TestMethod]
        public void StoreSingleSensorData_NullSensorData()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            SensorData[] sampleData = null;

            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorWithIdSensorData, sampleDevice1.Id, sensor11.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleData),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
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
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorWithIdSensorData, sampleDevice1.Id, sensor11.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleData),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

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
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorWithIdSensorData, sampleDevice1.Id, sensor11.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleData),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

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
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorWithIdSensorData, sampleDevice1.Id, sensor11.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleData),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

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
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorWithIdSensorData, sampleDevice1.Id, sensor11.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleData),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);
        }
        #endregion
        
        #region GetSensorsForDevice...
        [TestMethod]
        public void GetSensorsForDevice()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Device sampleDevice2 = CreateAndRegisterSampleDevice();
            Device sampleDevice3 = CreateAndRegisterSampleDevice();

            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            Sensor sensor12 = CreateSampleSensor(sampleDevice1);
            Sensor sensor21 = CreateSampleSensor(sampleDevice2);

            //No sensors
            //Device 1
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice1.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorsForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetSensorsForDeviceResult resultSensorsForDevice = (GetSensorsForDeviceResult)responseObject;
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 0);

            //Device 2
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice2.Id), MimeTypeJson,
                            HttpRequestTimeoutInMillis, typeof(GetSensorsForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultSensorsForDevice = (GetSensorsForDeviceResult)responseObject;
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 0);

            //Device 3
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice3.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorsForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultSensorsForDevice = (GetSensorsForDeviceResult)responseObject;
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 0);

            //Sensors for device 1
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice1.Id),
                ContentParserFactory.GetParser(MimeTypeJson).Encode(new Sensor[] { sensor11, sensor12 }), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            OperationResult resultRegSensors = (OperationResult)responseObject;
            AssertSuccess(resultRegSensors);

            //Device 1
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice1.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorsForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultSensorsForDevice = (GetSensorsForDeviceResult)responseObject;
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 2);
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor11.Id) != null);
            AssertAreEqual(sensor11, resultSensorsForDevice.Sensors.First(x => x.Id == sensor11.Id));
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor12.Id) != null);
            AssertAreEqual(sensor12, resultSensorsForDevice.Sensors.First(x => x.Id == sensor12.Id));


            //Device 2
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice2.Id), MimeTypeJson,
                            HttpRequestTimeoutInMillis, typeof(GetSensorsForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultSensorsForDevice = (GetSensorsForDeviceResult)responseObject;
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 0);

            //Device 3
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice3.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorsForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultSensorsForDevice = (GetSensorsForDeviceResult)responseObject;
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 0);



            //Sensors for device 2
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice2.Id),
                            ContentParserFactory.GetParser(MimeTypeJson).Encode(new Sensor[] { sensor21 }), MimeTypeJson,
                            HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            resultRegSensors = (OperationResult)responseObject;
            AssertSuccess(resultRegSensors);

            //Device 1
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice1.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorsForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultSensorsForDevice = (GetSensorsForDeviceResult)responseObject;
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 2);
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor11.Id) != null);
            AssertAreEqual(sensor11, resultSensorsForDevice.Sensors.First(x => x.Id == sensor11.Id));
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor12.Id) != null);
            AssertAreEqual(sensor12, resultSensorsForDevice.Sensors.First(x => x.Id == sensor12.Id));

            //Device 2
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice2.Id), MimeTypeJson,
                            HttpRequestTimeoutInMillis, typeof(GetSensorsForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultSensorsForDevice = (GetSensorsForDeviceResult)responseObject;
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 1);
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor21.Id) != null);
            AssertAreEqual(sensor21, resultSensorsForDevice.Sensors.First(x => x.Id == sensor21.Id));

            //Device 3
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice3.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorsForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultSensorsForDevice = (GetSensorsForDeviceResult)responseObject;
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 0);
        }

        //[TestMethod]
        public void GetSensorsForDevice_NullDeviceId()
        {
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, null), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorsForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);
            GetSensorsForDeviceResult resultSensorsForDevice = (GetSensorsForDeviceResult)responseObject;
            AssertFailure(resultSensorsForDevice);
        }

        [TestMethod]
        public void GetSensorsForDevice_EmptyDeviceId()
        {
            object responseObject;
            try
            {
                Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, String.Empty), MimeTypeJson,
                    HttpRequestTimeoutInMillis, typeof(GetSensorsForDeviceResult), out responseObject));
                Assert.IsNotNull(responseObject);
                GetSensorsForDeviceResult resultSensorsForDevice = (GetSensorsForDeviceResult)responseObject;
                AssertFailure(resultSensorsForDevice);
            }
            catch (WebException exc)
            {
                Assert.IsTrue(((HttpWebResponse)exc.Response).StatusCode == HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void GetSensorsForDevice_NonExistingDeviceId()
        {
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, "-NotExistingId-"), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorsForDeviceResult), out responseObject));
            Assert.IsNotNull(responseObject);
            GetSensorsForDeviceResult resultSensorsForDevice = (GetSensorsForDeviceResult)responseObject;
            AssertFailure(resultSensorsForDevice);
        }
        #endregion

        #region GetSensor...
        [TestMethod]
        public void GetSensor()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Device sampleDevice2 = CreateAndRegisterSampleDevice();

            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            Sensor sensor12 = CreateSampleSensor(sampleDevice1);
            Sensor sensor21 = CreateSampleSensor(sampleDevice2);

            //Sensors for device 1
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice1.Id),
                ContentParserFactory.GetParser(MimeTypeJson).Encode(new Sensor[] { sensor11, sensor12 }), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            OperationResult resultRegSensors = (OperationResult)responseObject;
            AssertSuccess(resultRegSensors);

            //Sensors for device 2
            Assert.IsTrue(HttpClient.SendPutRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, sampleDevice2.Id),
                            ContentParserFactory.GetParser(MimeTypeJson).Encode(new Sensor[] { sensor21 }), MimeTypeJson,
                            HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            resultRegSensors = (OperationResult)responseObject;
            AssertSuccess(resultRegSensors);


            //Device 1
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdSingleSensorWithId, sampleDevice1.Id, sensor11.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetSensorResult resultGetSensor = (GetSensorResult)responseObject;
            AssertSuccess(resultGetSensor);
            AssertAreEqual(sensor11, resultGetSensor.Sensor);

            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdSingleSensorWithId, sampleDevice1.Id, sensor12.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultGetSensor = (GetSensorResult)responseObject;
            AssertSuccess(resultGetSensor);
            AssertAreEqual(sensor12, resultGetSensor.Sensor);

            //Device 2
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdSingleSensorWithId, sampleDevice2.Id, sensor21.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultGetSensor = (GetSensorResult)responseObject;
            AssertSuccess(resultGetSensor);
            AssertAreEqual(sensor21, resultGetSensor.Sensor);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void GetSensor_NullDeviceId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            //Sensors for device 1
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdSingleSensorWithId, null, sensor11.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetSensorResult resultGetSensor = (GetSensorResult)responseObject;
            AssertFailure(resultGetSensor);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void GetSensor_EmptyDeviceId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            //Sensors for device 1
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdSingleSensorWithId, String.Empty, sensor11.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetSensorResult resultGetSensor = (GetSensorResult)responseObject;
            AssertFailure(resultGetSensor);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void GetSensor_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            //Sensors for device 1
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdSingleSensorWithId, "-NotExistingId-", sensor11.Id), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetSensorResult resultGetSensor = (GetSensorResult)responseObject;
            AssertFailure(resultGetSensor);
        }

        [TestMethod]
        public void GetSensor_NullSensorId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            //Sensors for device 1
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdSingleSensorWithId, sampleDevice1.Id, null), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetSensorResult resultGetSensor = (GetSensorResult)responseObject;
            AssertFailure(resultGetSensor);
        }

        [TestMethod]
        public void GetSensor_EmptySensorId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            //Sensors for device 1
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdSingleSensorWithId, sampleDevice1.Id, String.Empty), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetSensorResult resultGetSensor = (GetSensorResult)responseObject;
            AssertFailure(resultGetSensor);
        }

        [TestMethod]
        public void GetSensor_NonExistingSensorId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            //Sensors for device 1
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdSingleSensorWithId, sampleDevice1.Id, "-NotExisting-"), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(GetSensorResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetSensorResult resultGetSensor = (GetSensorResult)responseObject;
            AssertFailure(resultGetSensor);
        }
        #endregion


        #region GetSensorData(string deviceId, List<string> sensorIdList, DateTime dataStartDateTime, DateTime dataEndDateTime, int maxResults)...
        [TestMethod]
        public void A_TEST_GetSensorData()
        {
            //Store
            object responseObject;

            //NO LIMIT
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData + "?id0=" + "count1", "A192.168.1.1"),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertSuccess(resultGet);
            MultipleSensorData data = resultGet.SensorDataList.FirstOrDefault(x => x.SensorId == "count1");
            Assert.IsNotNull(data);
        }
        
        [TestMethod]
        public void GetSensorData()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData };

            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);
            //Wait until stored
            System.Threading.Thread.Sleep(20000);

            //NO LIMIT
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData + "?id0=" + sensor11.Id, sampleDevice1.Id), 
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertSuccess(resultGet);
            MultipleSensorData data = resultGet.SensorDataList.FirstOrDefault(x => x.SensorId == sensor11.Id);
            Assert.IsNotNull(data);
            Assert.AreEqual(sampleData.Measures.Length, data.Measures.Length);

            int count = sampleData.Measures.Length - 1;
            foreach (SensorData next in data.Measures)
            {
                Assert.IsTrue(next.GeneratedWhen != DateTime.MinValue);
                Assert.IsNotNull(next.Value);
                Assert.AreEqual(count.ToString(), next.Value);
                count--;
            }

            //WITH LIMIT
            int maxLimit = sampleData.Measures.Length - 1;

            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData + "?id0={1}&maxValuesPerSensor={2}",
                sampleDevice1.Id, sensor11.Id, maxLimit),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertSuccess(resultGet);

            data = resultGet.SensorDataList.FirstOrDefault(x => x.SensorId == sensor11.Id);
            Assert.IsNotNull(data);
            Assert.AreEqual(maxLimit, data.Measures.Length);

            count = sampleData.Measures.Length - 1;
            foreach (SensorData next in data.Measures)
            {
                Assert.IsTrue(next.GeneratedWhen != DateTime.MinValue);
                Assert.IsNotNull(next.Value);
                Assert.AreEqual(count.ToString(), next.Value);
                count--;
            }

            //WITH DATES
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData + "?id0={1}&generatedAfter={2}&generatedBefore={3}", 
                sampleDevice1.Id, sensor11.Id, DateTime.Now.Subtract(new TimeSpan(1, 0, 0)).ToString(CultureInfo.InvariantCulture), DateTime.Now.ToString(CultureInfo.InvariantCulture)),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertSuccess(resultGet);
            data = resultGet.SensorDataList.FirstOrDefault(x => x.SensorId == sensor11.Id);
            Assert.IsNotNull(data);
            Assert.AreEqual(sampleData.Measures.Length, data.Measures.Length);

            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData + "?id0={1}&generatedAfter={2}&generatedBefore={3}", 
                sampleDevice1.Id, sensor11.Id, DateTime.Now.ToString(CultureInfo.InvariantCulture), DateTime.Now.Add(new TimeSpan(1, 0, 0)).ToString(CultureInfo.InvariantCulture)),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertSuccess(resultGet);
            Assert.AreEqual(0, resultGet.SensorDataList.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void GetSensorData_EmptyDeviceId()
        {
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData + "?id0=" + "someId", String.Empty),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertFailure(resultGet);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void GetSensorData_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData + "?id0=" + sensor11.Id, "-DoesNotExist-"),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertFailure(resultGet);
        }

        [TestMethod]
        public void GetSensorData_EmptySensorId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData + "?id0=", sampleDevice1.Id),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertSuccess(resultGet);
            Assert.AreEqual(0, resultGet.SensorDataList.Count);
        }

        [TestMethod]
        public void GetSensorData_NoSensorId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice1.Id),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertFailure(resultGet);
        }

        [TestMethod]
        public void GetSensorData_NonExistingSensorId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData + "?id0=-NotExistingId-", sampleDevice1.Id),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertSuccess(resultGet);
            Assert.AreEqual(0, resultGet.SensorDataList.Count);
        }

        [TestMethod]
        public void GetSensorData_StartAfterEnd()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData + "?id0={1}&generatedAfter={2}&generatedBefore={3}",
                sampleDevice1.Id, sensor11.Id, DateTime.Now.ToString(CultureInfo.InvariantCulture), DateTime.Now.Subtract(new TimeSpan(1, 0, 0)).ToString(CultureInfo.InvariantCulture)),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertFailure(resultGet);
        }

        [TestMethod]
        public void GetSensorData_NegativeMaxResults()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData + "?id0={1}&maxValuesPerSensor={2}", sampleDevice1.Id, sensor11.Id, -1),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertFailure(resultGet);
        }
        #endregion

        #region GetSensorDataLatest(string deviceId, List<string> sensorIdList, DateTime dataStartDateTime, DateTime dataEndDateTime, int maxResults)...

        [TestMethod]
        public void GetSensorDataLatest()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData };

            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);
            //NO WAIT until stored
            //System.Threading.Thread.Sleep(10000);

            //NO LIMIT
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorDataLatest + "?id0=" + sensor11.Id, sampleDevice1.Id),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertSuccess(resultGet);
            MultipleSensorData data = resultGet.SensorDataList.FirstOrDefault(x => x.SensorId == sensor11.Id);
            Assert.IsNotNull(data);
            Assert.AreEqual(sampleData.Measures.Length, data.Measures.Length);

            int count = 0;// sampleData.Measures.Length - 1;
            foreach (SensorData next in data.Measures)
            {
                Assert.IsTrue(next.GeneratedWhen != DateTime.MinValue);
                Assert.IsNotNull(next.Value);
                Assert.AreEqual(count.ToString(), next.Value);
                count++;//--;
            }

            //WITH LIMIT
            int maxLimit = sampleData.Measures.Length - 1;

            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorDataLatest + "?id0={1}&maxValuesPerSensor={2}",
                sampleDevice1.Id, sensor11.Id, maxLimit),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertSuccess(resultGet);

            data = resultGet.SensorDataList.FirstOrDefault(x => x.SensorId == sensor11.Id);
            Assert.IsNotNull(data);
            Assert.AreEqual(maxLimit, data.Measures.Length);

            count = 0;// sampleData.Measures.Length - 1;
            foreach (SensorData next in data.Measures)
            {
                Assert.IsTrue(next.GeneratedWhen != DateTime.MinValue);
                Assert.IsNotNull(next.Value);
                Assert.AreEqual(count.ToString(), next.Value);
                count++;//--;
            }
        }

        [TestMethod]
        public void GetSensorDataLatestSingleValue()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString(), CorrelationId = Guid.NewGuid().ToString() };
            }

            MultipleSensorData[] sampleDataList = new MultipleSensorData[] { sampleData };

            //Store
            object responseObject;
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorData, sampleDevice1.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDataList),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(OperationResult), out responseObject));
            Assert.IsNotNull(responseObject);

            OperationResult result = (OperationResult)responseObject;
            AssertSuccess(result);

            //WITH LIMIT
            int maxLimit = 1;

            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorDataLatest + "?id0={1}&maxValuesPerSensor={2}",
                sampleDevice1.Id, sensor11.Id, maxLimit),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertSuccess(resultGet);

            MultipleSensorData data = resultGet.SensorDataList.FirstOrDefault(x => x.SensorId == sensor11.Id);
            Assert.IsNotNull(data);
            Assert.AreEqual(maxLimit, data.Measures.Length);

            int count = 0;// sampleData.Measures.Length - 1;
            foreach (SensorData next in data.Measures)
            {
                Assert.IsTrue(next.GeneratedWhen != DateTime.MinValue);
                Assert.IsNotNull(next.Value);

                // it must be the last value sent
                Assert.AreEqual(sampleData.Measures[sampleData.Measures.Length-1], next.Value);
                count++;//--;
            }
        }



       

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void GetSensorDataLatest_EmptyDeviceId()
        {
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorDataLatest + "?id0=someId", String.Empty),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertFailure(resultGet);
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void GetSensorDataLatest_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorDataLatest + "?id0=" + sensor11.Id, "-DoesNotExist-"),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertFailure(resultGet);
        }

        [TestMethod]
        public void GetSensorDataLatest_EmptySensorId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorDataLatest + "?id0=", sampleDevice1.Id),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertSuccess(resultGet);
            Assert.AreEqual(0, resultGet.SensorDataList.Count);
        }

        [TestMethod]
        public void GetSensorDataLatest_NoSensorId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorDataLatest, sampleDevice1.Id),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertSuccess(resultGet);
        }

        [TestMethod]
        public void GetSensorDataLatest_NonExistingSensorId()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorDataLatest + "?id0=-NotExistingId-", sampleDevice1.Id),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertSuccess(resultGet);
            Assert.AreEqual(0, resultGet.SensorDataList.Count);
        }

        [TestMethod]
        public void GetSensorDataLatest_NegativeMaxResults()
        {
            Device sampleDevice1 = CreateAndRegisterSampleDevice();
            Sensor sensor11 = CreateAndRegisterSampleSensor(sampleDevice1);

            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdSensorsSensorDataLatest + "?id0={1}&maxValuesPerSensor={2}", sampleDevice1.Id, sensor11.Id, -1),
                MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetMultipleSensorDataResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetMultipleSensorDataResult resultGet = (GetMultipleSensorDataResult)responseObject;
            AssertFailure(resultGet);
        }
        #endregion

        #region Various API

        [TestMethod]
        public void GetCorrelationId()
        {
            object responseObject;

            // first id
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + ServerHttpRestUrls.GetNextCorrelationId, MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetCorrelationIdResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetCorrelationIdResult correlationResult = (GetCorrelationIdResult)responseObject;
            AssertSuccess(correlationResult);

            // first id
            Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + ServerHttpRestUrls.GetNextCorrelationId, MimeTypeJson, HttpRequestTimeoutInMillis, typeof(GetCorrelationIdResult), out responseObject));
            Assert.IsNotNull(responseObject);

            GetCorrelationIdResult correlationResult2 = (GetCorrelationIdResult)responseObject;
            AssertSuccess(correlationResult2);

            Assert.AreNotEqual(correlationResult.CorrelationId, correlationResult2.CorrelationId);
        }
        #endregion


        #endregion


        private void AssertDeviceRegistered(Device device, bool expectedResult, bool checkDataForEquality)
        {
            object responseObject;
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceIsUsed, device.Id ), MimeTypeJson,
                HttpRequestTimeoutInMillis, typeof(IsDeviceIdUsedResult), out responseObject));
            Assert.IsNotNull(responseObject);
            IsDeviceIdUsedResult resultDeviceReg = (IsDeviceIdUsedResult)responseObject;
            AssertSuccess(resultDeviceReg);
            Assert.AreEqual(expectedResult, resultDeviceReg.IsUsed);

            if (expectedResult == true)
            {
                Assert.IsTrue(HttpClient.SendGetRequest(ServerUrl + ServerHttpRestUrls.MultipleDevices + GetIdQueryString(new string[] { device.Id }), MimeTypeJson,
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
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.SingleDeviceWithId, sampleDevice.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(sampleDevice),
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
            Assert.IsTrue(HttpClient.SendPutRequest(ServerUrl + String.Format(ServerHttpRestUrls.DeviceWithIdMultipleSensors, device.Id), ContentParserFactory.GetParser(MimeTypeJson).Encode(new Sensor[] { sampleSensor }),
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
            Assert.IsTrue(HttpClient.SendGetRequest(String.Format(ServerUrl + ServerHttpRestUrls.DeviceWithIdMultipleSensors, device.Id), MimeTypeJson,
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
    }
}
