using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel.Web;
using GlobalDataContracts;
using System.ServiceModel;
using GatewayServiceContract;

namespace GatewayServiceTest
{
    [TestClass]
    public class RESTfulGatewaysServiceTest:TestBase
    {

        private static IGatewayService Channel
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
            WebHttpBinding binding = new WebHttpBinding(WebHttpSecurityMode.None);
            binding.MaxReceivedMessageSize = Int32.MaxValue;
            WebChannelFactory<IGatewayService> cf = new WebChannelFactory<IGatewayService>(binding, new Uri("http://localhost/GatewayService"));
            Channel = cf.CreateChannel();
            log4net.Config.XmlConfigurator.Configure();
        }

        #region Registration of sensors, devices, triggers, sinks...

        #region SPECIAL...
        ////[TestMethod]
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

        #region RegisterDevice...
        ////[TestMethod]
        public void RegisterDevice()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            OperationResult result = Channel.RegisterDevice(sampleDevice);

            log4net.LogManager.GetLogger(this.GetType()).Info("SEE ME???");

            //Check
            AssertSuccess(result);

            GetDevicesResult resultDevices = Channel.GetDevicesByIds(new string[] { sampleDevice.Id });
            AssertSuccess(resultDevices);
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice.Id) != null);
            AssertAreEqual(sampleDevice, resultDevices.Devices.First(x => x.Id == sampleDevice.Id));
        }

        ////[TestMethod]
        public void RegisterDevice_Null()
        {
            //Create
            OperationResult result = Channel.RegisterDevice(null);
            AssertFailure(result);
        }

        ////[TestMethod]
        public void RegisterDevice_ExistingId()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            OperationResult result = Channel.RegisterDevice(sampleDevice);
            AssertSuccess(result);

            Device sampleDevice1 = CreateSampleDevice();
            sampleDevice1.Id = sampleDevice.Id;
            result = Channel.RegisterDevice(sampleDevice1);
            AssertFailure(result);
        }

        ////[TestMethod]
        public void RegisterDevice_InvalidId()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            sampleDevice.Id += "=>" + "1";
            OperationResult result = Channel.RegisterDevice(sampleDevice);
            AssertFailure(result);
        }

        ////[TestMethod]
        public void RegisterDevice_InvalidData_DescriptionNull()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            sampleDevice.Description = null;
            OperationResult result = Channel.RegisterDevice(sampleDevice);
            AssertFailure(result);
        }

        ////[TestMethod]
        public void RegisterDevice_InvalidData_IdNull()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            sampleDevice.Id = null;
            OperationResult result = Channel.RegisterDevice(sampleDevice);
            AssertFailure(result);
        }
        #endregion

        #region UpdateDevice...
        ////[TestMethod]
        public void UpdateDevice()
        {
            //Create
            Device sampleDevice1 = CreateSampleDevice();
            OperationResult result = Channel.RegisterDevice(sampleDevice1);

            //Check
            AssertSuccess(result);

            Device sampleDevice2 = CreateSampleDevice();
            result = Channel.RegisterDevice(sampleDevice2);

            //Check
            AssertSuccess(result);

            GetDevicesResult resultDevices = Channel.GetDevicesByIds(new string[] { sampleDevice1.Id, sampleDevice2.Id });
            AssertSuccess(resultDevices);
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice1.Id) != null);
            AssertAreEqual(sampleDevice1, resultDevices.Devices.First(x => x.Id == sampleDevice1.Id));
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice2.Id) != null);
            AssertAreEqual(sampleDevice2, resultDevices.Devices.First(x => x.Id == sampleDevice2.Id));

            //Update
            sampleDevice1.Description += "1";
            sampleDevice1.DeviceIpEndPoint += "2";
            sampleDevice1.Location.Elevation += 3;
            sampleDevice1.Location.Latitude += 4;
            sampleDevice1.Location.Longitude += 5;
            sampleDevice1.Location.Name += "6";

            result = Channel.UpdateDevice(sampleDevice1);

            //Check
            resultDevices = Channel.GetDevicesByIds(new string[] { sampleDevice1.Id, sampleDevice2.Id });
            AssertSuccess(resultDevices);
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice1.Id) != null);
            AssertAreEqual(sampleDevice1, resultDevices.Devices.First(x => x.Id == sampleDevice1.Id));
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice2.Id) != null);
            AssertAreEqual(sampleDevice2, resultDevices.Devices.First(x => x.Id == sampleDevice2.Id));
        }

        ////[TestMethod]
        public void UpdateDevice_Null()
        {
            //Create
            OperationResult result = Channel.UpdateDevice(null);
            AssertFailure(result);
        }

        ////[TestMethod]
        public void UpdateDevice_NonExistingId()
        {
            //Update
            Device sampleDevice = CreateSampleDevice();
            OperationResult result = Channel.UpdateDevice(sampleDevice);

            //Check
            AssertFailure(result);
        }

        ////[TestMethod]
        public void UpdateDevice_InvalidData_IdNull()
        {
            //Update
            Device sampleDevice = CreateSampleDevice();
            sampleDevice.Id = null;
            OperationResult result = Channel.UpdateDevice(sampleDevice);
            AssertFailure(result);
        }

        ////[TestMethod]
        public void UpdateDevice_InvalidData_DescriptionNull()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            OperationResult result = Channel.RegisterDevice(sampleDevice);
            AssertSuccess(result);

            //Update
            sampleDevice.Description = null;
            result = Channel.UpdateDevice(sampleDevice);
            AssertFailure(result);
        }
        #endregion

        #region RegisterSensors...
        ////[TestMethod]
        public void RegisterSensors()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Device sampleDevice2 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice2);
            Device sampleDevice3 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice3);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            Sensor sensor12 = CreateSampleSensor(sampleDevice1);
            Sensor sensor21 = CreateSampleSensor(sampleDevice2);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11, sensor12, sensor21 });
            AssertSuccess(resultRegSensors);

            GetSensorsForDeviceResult resultSensorsForDevice = Channel.GetSensorsForDevice(sampleDevice1.Id);
            AssertSuccess(resultSensorsForDevice);

            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 2);
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor11.Id) != null);
            AssertAreEqual(sensor11, resultSensorsForDevice.Sensors.First(x => x.Id == sensor11.Id));
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor12.Id) != null);
            AssertAreEqual(sensor12, resultSensorsForDevice.Sensors.First(x => x.Id == sensor12.Id));

            resultSensorsForDevice = Channel.GetSensorsForDevice(sampleDevice2.Id);
            AssertSuccess(resultSensorsForDevice);

            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 1);
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor21.Id) != null);
            AssertAreEqual(sensor21, resultSensorsForDevice.Sensors.First(x => x.Id == sensor21.Id));

            resultSensorsForDevice = Channel.GetSensorsForDevice(sampleDevice3.Id);
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 0);
        }

        ////[TestMethod]
        public void RegisterSensors_Null()
        {
            OperationResult result = Channel.RegisterSensors(null);
            AssertFailure(result);
        }

        ////[TestMethod]
        public void RegisterSensors_NullElement()
        {
            OperationResult result = Channel.RegisterSensors(new Sensor[] { null });
            AssertFailure(result);
        }

        ////[TestMethod]
        public void RegisterSensors_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertFailure(resultRegSensors);
        }

        ////[TestMethod]
        public void RegisterSensors_InvalidData_NullDeviceId()
        {
            Sensor sensor11 = CreateSampleSensor(null);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertFailure(resultRegSensors);
        }

        ////[TestMethod]
        public void RegisterSensors_InvalidData_NullDescription()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            sensor11.Description = null;

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertFailure(resultRegSensors);
        }

        ////[TestMethod]
        public void RegisterSensors_InvalidData_NullId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            sensor11.Id = null;

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertFailure(resultRegSensors);
        }

        ////[TestMethod]
        public void RegisterSensors_InvalidData_NullUnitSymbol()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            sensor11.UnitSymbol = null;

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertFailure(resultRegSensors);
        }
        #endregion

        #region UpdateSensor...
        ////[TestMethod]
        public void UpdateSensor()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultSensors);

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
            sensor11.VirtualSensorDefinition.Definition += "5";
            sensor11.VirtualSensorDefinition.VirtualSensorCalculationType = (VirtualSensorCalculationType)Enum.GetValues(typeof(VirtualSensorCalculationType)).GetValue((int)sensor11.VirtualSensorDefinition.VirtualSensorCalculationType % Enum.GetValues(typeof(VirtualSensorCalculationType)).Length);
            sensor11.VirtualSensorDefinition.VirtualSensorDefinitionType = (VirtualSensorDefinitionType)Enum.GetValues(typeof(VirtualSensorDefinitionType)).GetValue((int)sensor11.VirtualSensorDefinition.VirtualSensorDefinitionType % Enum.GetValues(typeof(VirtualSensorDefinitionType)).Length);

            resultSensors = Channel.UpdateSensor(sensor11);
            AssertSuccess(resultSensors);

            GetSensorsForDeviceResult resultSensorsForDevice = Channel.GetSensorsForDevice(sampleDevice1.Id);
            AssertSuccess(resultSensorsForDevice);

            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 1);
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor11.Id) != null);
            AssertAreEqual(sensor11, resultSensorsForDevice.Sensors.First(x => x.Id == sensor11.Id));
        }

        ////[TestMethod]
        public void UpdateSensor_Null()
        {
            AssertFailure(Channel.UpdateSensor(null));
        }

        ////[TestMethod]
        public void UpdateSensor_NonExistingDeviceI()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultSensors);

            sensor11.DeviceId += "1";

            resultSensors = Channel.UpdateSensor(sensor11);
            AssertFailure(resultSensors);
        }

        ////[TestMethod]
        public void UpdateSensor_NonExistingDeviceIdAndSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult result = Channel.UpdateSensor(sensor11);
            AssertFailure(result);
        }

        ////[TestMethod]
        public void UpdateSensor_NullDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultSensors);

            sensor11.DeviceId = null;

            resultSensors = Channel.UpdateSensor(sensor11);
            AssertFailure(resultSensors);
        }

        ////[TestMethod]
        public void UpdateSensor_NullDescription()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultSensors);

            sensor11.Description = null;

            resultSensors = Channel.UpdateSensor(sensor11);
            AssertFailure(resultSensors);
        }

        ////[TestMethod]
        public void UpdateSensor_NullUnitSymbol()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultSensors);

            sensor11.UnitSymbol = null;

            resultSensors = Channel.UpdateSensor(sensor11);
            AssertFailure(resultSensors);
        }
        #endregion

        #endregion

        #region Checks...

        #region IsDeviceIdUsed...
        ////[TestMethod]
        public void IsDeviceIdUsed()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            IsDeviceIdUsedResult resultUsed = Channel.IsDeviceIdUsed(sampleDevice.Id);
            AssertSuccess(resultUsed);
            Assert.IsFalse(resultUsed.IsUsed);

            OperationResult result = Channel.RegisterDevice(sampleDevice);

            //Check
            AssertSuccess(result);

            resultUsed = Channel.IsDeviceIdUsed(sampleDevice.Id);
            AssertSuccess(resultUsed);
            Assert.IsTrue(resultUsed.IsUsed);
        }

        ////[TestMethod]
        public void IsDeviceIdUsed_Null()
        {
            AssertFailure(Channel.IsDeviceIdUsed(null));
        }

        #endregion

        #region IsSensorIdRegisteredForDevice...

        ////[TestMethod]
        public void IsSensorIdRegisteredForDevice()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);

            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            Sensor sensor12 = CreateSampleSensor(sampleDevice1);

            IsSensorIdRegisteredForDeviceResult result = Channel.IsSensorIdRegisteredForDevice(sampleDevice1.Id, sensor11.Id);
            AssertSuccess(result);
            Assert.IsFalse(result.IsRegistered);
            result = Channel.IsSensorIdRegisteredForDevice(sampleDevice1.Id, sensor12.Id);
            AssertSuccess(result);
            Assert.IsFalse(result.IsRegistered);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            result = Channel.IsSensorIdRegisteredForDevice(sampleDevice1.Id, sensor11.Id);
            AssertSuccess(result);
            Assert.IsTrue(result.IsRegistered);
            result = Channel.IsSensorIdRegisteredForDevice(sampleDevice1.Id, sensor12.Id);
            AssertSuccess(result);
            Assert.IsFalse(result.IsRegistered);

            resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor12 });
            AssertSuccess(resultRegSensors);

            result = Channel.IsSensorIdRegisteredForDevice(sampleDevice1.Id, sensor11.Id);
            AssertSuccess(result);
            Assert.IsTrue(result.IsRegistered);
            result = Channel.IsSensorIdRegisteredForDevice(sampleDevice1.Id, sensor12.Id);
            AssertSuccess(result);
            Assert.IsTrue(result.IsRegistered);
        }

        ////[TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsSensorIdRegisteredForDevice_NullDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            AssertFailure(Channel.IsSensorIdRegisteredForDevice(null, sensor11.Id));
        }

        ////[TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void IsSensorIdRegisteredForDevice_NullSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            AssertFailure(Channel.IsSensorIdRegisteredForDevice(sampleDevice1.Id, null));
        }

        ////[TestMethod]
        public void IsSensorIdRegisteredForDevice_InvalidDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            IsSensorIdRegisteredForDeviceResult result = Channel.IsSensorIdRegisteredForDevice(sampleDevice1.Id, sensor11.Id);
            AssertSuccess(result);
            Assert.IsFalse(result.IsRegistered);
        }
        #endregion

        #endregion

        #region Sensors, devices, sensor data...
        #region StoreSensorData...
        ////[TestMethod]
        public void StoreSensorData()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString() };
            }

            //Store
            OperationResult resultStoring = Channel.StoreSensorData(sampleDevice1.Id, new MultipleSensorData[] { sampleData });
            AssertSuccess(resultStoring);
        }

        ////[TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StoreSensorData_NullDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString() };
            }

            //Wait until stored
            OperationResult resultStoring = Channel.StoreSensorData(null, new MultipleSensorData[] { sampleData }); ;
            AssertFailure(resultStoring);
        }

        ////[TestMethod]
        public void StoreSensorData_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString() };
            }

            //Wait until stored
            OperationResult resultStoring = Channel.StoreSensorData("-NotExistingId-", new MultipleSensorData[] { sampleData }); ;
            AssertFailure(resultStoring);
        }
        #endregion

        #region GetDevices...
        ////[TestMethod]
        public void GetDevices()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            OperationResult resultReg = Channel.RegisterDevice(sampleDevice);

            //Check
            AssertSuccess(resultReg);

            GetDevicesResult resultDevices = Channel.GetDevices();
            AssertSuccess(resultDevices);
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice.Id) != null);
            AssertAreEqual(sampleDevice, resultDevices.Devices.First(x => x.Id == sampleDevice.Id));
        }

        ////[TestMethod]
        public void GetDevices_Selected()
        {
            //Create
            Device sampleDevice1 = CreateSampleDevice();
            OperationResult resultReg = Channel.RegisterDevice(sampleDevice1);
            //Check
            AssertSuccess(resultReg);

            Device sampleDevice2 = CreateSampleDevice();
            resultReg = Channel.RegisterDevice(sampleDevice2);
            //Check
            AssertSuccess(resultReg);

            //Get 1st
            GetDevicesResult resultDevices = Channel.GetDevicesByIds(new string[] { sampleDevice1.Id });
            AssertSuccess(resultDevices);
            Assert.AreEqual(1, resultDevices.Devices.Count());
            AssertAreEqual(sampleDevice1, resultDevices.Devices[0]);

            //Get 2nd
            resultDevices = Channel.GetDevicesByIds(new string[] { sampleDevice2.Id });
            AssertSuccess(resultDevices);
            Assert.AreEqual(1, resultDevices.Devices.Count());
            AssertAreEqual(sampleDevice2, resultDevices.Devices[0]);

            //Get both
            resultDevices = Channel.GetDevicesByIds(new string[] { sampleDevice1.Id, sampleDevice2.Id });
            AssertSuccess(resultDevices);
            Assert.AreEqual(2, resultDevices.Devices.Count());
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice1.Id) != null);
            AssertAreEqual(sampleDevice1, resultDevices.Devices.First(x => x.Id == sampleDevice1.Id));
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice2.Id) != null);
            AssertAreEqual(sampleDevice2, resultDevices.Devices.First(x => x.Id == sampleDevice2.Id));
        }


        ////[TestMethod]
        public void GetDevices_Null()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            OperationResult resultReg = Channel.RegisterDevice(sampleDevice);

            //Check
            AssertSuccess(resultReg);

            GetDevicesResult resultDevices = Channel.GetDevicesByIds(null);
            AssertSuccess(resultDevices);
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice.Id) != null);
            AssertAreEqual(sampleDevice, resultDevices.Devices.First(x => x.Id == sampleDevice.Id));
        }
        #endregion

        #region GetSensorsForDevice...
        ////[TestMethod]
        public void GetSensorsForDevice()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Device sampleDevice2 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice2);
            Device sampleDevice3 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice3);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            Sensor sensor12 = CreateSampleSensor(sampleDevice1);
            Sensor sensor21 = CreateSampleSensor(sampleDevice2);

            //No sensors
            GetSensorsForDeviceResult resultSensorsForDevice = Channel.GetSensorsForDevice(sampleDevice1.Id);
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 0);

            resultSensorsForDevice = Channel.GetSensorsForDevice(sampleDevice2.Id);
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 0);

            resultSensorsForDevice = Channel.GetSensorsForDevice(sampleDevice3.Id);
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 0);

            //Sensors for device 1
            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11, sensor12 });
            AssertSuccess(resultRegSensors);

            resultSensorsForDevice = Channel.GetSensorsForDevice(sampleDevice1.Id);
            AssertSuccess(resultSensorsForDevice);

            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 2);
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor11.Id) != null);
            AssertAreEqual(sensor11, resultSensorsForDevice.Sensors.First(x => x.Id == sensor11.Id));
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor12.Id) != null);
            AssertAreEqual(sensor12, resultSensorsForDevice.Sensors.First(x => x.Id == sensor12.Id));

            resultSensorsForDevice = Channel.GetSensorsForDevice(sampleDevice2.Id);
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 0);

            resultSensorsForDevice = Channel.GetSensorsForDevice(sampleDevice3.Id);
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 0);

            //Sensors for device 2
            resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor21 });

            resultSensorsForDevice = Channel.GetSensorsForDevice(sampleDevice1.Id);
            AssertSuccess(resultSensorsForDevice);

            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 2);
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor11.Id) != null);
            AssertAreEqual(sensor11, resultSensorsForDevice.Sensors.First(x => x.Id == sensor11.Id));
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor12.Id) != null);
            AssertAreEqual(sensor12, resultSensorsForDevice.Sensors.First(x => x.Id == sensor12.Id));

            resultSensorsForDevice = Channel.GetSensorsForDevice(sampleDevice2.Id);
            AssertSuccess(resultSensorsForDevice);

            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 1);
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor21.Id) != null);
            AssertAreEqual(sensor21, resultSensorsForDevice.Sensors.First(x => x.Id == sensor21.Id));

            resultSensorsForDevice = Channel.GetSensorsForDevice(sampleDevice3.Id);
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count(), 0);
        }

        ////[TestMethod]
        public void GetSensorsForDevice_NullDeviceId()
        {
            GetSensorsForDeviceResult resultSensorsForDevice = Channel.GetSensorsForDevice(null);
            AssertFailure(resultSensorsForDevice);
        }

        ////[TestMethod]
        public void GetSensorsForDevice_NonExistingDeviceId()
        {
            GetSensorsForDeviceResult resultSensorsForDevice = Channel.GetSensorsForDevice("-NotExisting-");
            AssertFailure(resultSensorsForDevice);
        }
        #endregion

        #region GetSensor...
        ////[TestMethod]
        public void GetSensor()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Device sampleDevice2 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice2);
            Device sampleDevice3 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice3);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            Sensor sensor12 = CreateSampleSensor(sampleDevice1);
            Sensor sensor21 = CreateSampleSensor(sampleDevice2);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11, sensor12, sensor21 });
            AssertSuccess(resultRegSensors);

            GetSensorResult resultGetSensor = Channel.GetSensor(sampleDevice1.Id, sensor11.Id);
            AssertSuccess(resultGetSensor);
            AssertAreEqual(sensor11, resultGetSensor.Sensor);

            resultGetSensor = Channel.GetSensor(sampleDevice1.Id, sensor12.Id);
            AssertSuccess(resultGetSensor);
            AssertAreEqual(sensor12, resultGetSensor.Sensor);

            resultGetSensor = Channel.GetSensor(sampleDevice2.Id, sensor21.Id);
            AssertSuccess(resultGetSensor);
            AssertAreEqual(sensor21, resultGetSensor.Sensor);
        }

        ////[TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetSensor_NullDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            GetSensorResult resultGetSensor = Channel.GetSensor(null, sensor11.Id);
            AssertFailure(resultGetSensor);
            Assert.IsNull(resultGetSensor.Sensor);
        }

        ////[TestMethod]
        public void GetSensor_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            GetSensorResult resultGetSensor = Channel.GetSensor("-NotExisting-", sensor11.Id);
            AssertFailure(resultGetSensor);
            Assert.IsNull(resultGetSensor.Sensor);
        }

        ////[TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetSensor_NullSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);


            GetSensorResult resultGetSensor = Channel.GetSensor(sampleDevice1.Id, null);
            AssertFailure(resultGetSensor);
        }

        ////[TestMethod]
        public void GetSensor_NonExistingSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);


            GetSensorResult resultGetSensor = Channel.GetSensor(sampleDevice1.Id, "-NotExisting-");
            AssertFailure(resultGetSensor);
            Assert.IsNull(resultGetSensor.Sensor);
        }
        #endregion

        #region GetSensorData(string deviceId, List<string> sensorIdList, DateTime dataStartDateTime, DateTime dataEndDateTime, int maxResults)...
        ////[TestMethod]
        public void GetSensorData()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString() };
            }

            //Wait until stored
            OperationResult resultStoring = Channel.StoreSensorData(sampleDevice1.Id, new MultipleSensorData[] { sampleData });
            AssertSuccess(resultStoring);
            System.Threading.Thread.Sleep(10000);

            //NO limit
            GetMultipleSensorDataResult result = Channel.GetSensorData(sampleDevice1.Id, new[] { sensor11.Id }, DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), DateTime.Now, 0);
            AssertSuccess(result);
            MultipleSensorData data = result.SensorDataList.FirstOrDefault(x => x.SensorId == sensor11.Id);
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

            //With limit
            int maxLimit = sampleData.Measures.Length - 1;
            result = Channel.GetSensorData(sampleDevice1.Id, new[] { sensor11.Id }, DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), DateTime.Now, maxLimit);
            AssertSuccess(result);
            data = result.SensorDataList.FirstOrDefault(x => x.SensorId == sensor11.Id);
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
        }

        ////[TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetSensorData_NullDeviceId()
        {
            AssertFailure(Channel.GetSensorData(null, new[] { "SomeId" }, DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), DateTime.Now, 0));
        }

        ////[TestMethod]
        public void GetSensorData_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            GetMultipleSensorDataResult result = Channel.GetSensorData("-DoesNotExist-", new[] { sensor11.Id }, DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), DateTime.Now, 0);
            AssertSuccess(result);
            Assert.AreEqual(0, result.SensorDataList.Count());
        }

        ////[TestMethod]
        public void GetSensorData_NullSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);

            AssertFailure(Channel.GetSensorData(sampleDevice1.Id, null, DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), DateTime.Now, 0));
        }

        ////[TestMethod]
        public void GetSensorData_NonExistingSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);

            GetMultipleSensorDataResult result = Channel.GetSensorData(sampleDevice1.Id, new[] { "-NotExistingId-" }, DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), DateTime.Now, 0);
            AssertSuccess(result);
            Assert.AreEqual(0, result.SensorDataList.Count());
        }

        ////[TestMethod]
        public void GetSensorData_StartBeforeEnd()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            AssertFailure(Channel.GetSensorData(sampleDevice1.Id, new[] { sensor11.Id }, DateTime.Now, DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), 0));
        }

        ////[TestMethod]
        public void GetSensorData_NegativeMaxResults()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            AssertFailure(Channel.GetSensorData(sampleDevice1.Id, new[] { sensor11.Id }, DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), DateTime.Now, -1));
        }
        #endregion

        #region GetSensorDataLatest(string deviceId, List<string> sensorIdList, DateTime dataStartDateTime, DateTime dataEndDateTime, int maxResults)...
        [TestMethod]
        public void GetSensorDataLatest()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString() };
            }

            //Wait until stored
            OperationResult resultStoring = Channel.StoreSensorData(sampleDevice1.Id, new MultipleSensorData[] { sampleData }); ;
            AssertSuccess(resultStoring);

            //NO limit
            GetMultipleSensorDataResult result = Channel.GetSensorDataLatest(sampleDevice1.Id, new[] { sensor11.Id }, 0);
            AssertSuccess(result);
            MultipleSensorData data = result.SensorDataList.FirstOrDefault(x => x.SensorId == sensor11.Id);
            Assert.IsNotNull(data);
            Assert.AreEqual(sampleData.Measures.Length, data.Measures.Length);

            int count = 0;
            foreach (SensorData next in data.Measures)
            {
                Assert.IsTrue(next.GeneratedWhen != DateTime.MinValue);
                Assert.IsNotNull(next.Value);
                Assert.AreEqual(count.ToString(), next.Value);
                count++;
            }

            //With limit
            int maxLimit = sampleData.Measures.Length - 1;
            result = Channel.GetSensorDataLatest(sampleDevice1.Id, new[] { sensor11.Id }, maxLimit);
            AssertSuccess(result);
            data = result.SensorDataList.FirstOrDefault(x => x.SensorId == sensor11.Id);
            Assert.IsNotNull(data);
            Assert.AreEqual(maxLimit, data.Measures.Length);

            count = 0;
            foreach (SensorData next in data.Measures)
            {
                Assert.IsTrue(next.GeneratedWhen != DateTime.MinValue);
                Assert.IsNotNull(next.Value);
                Assert.AreEqual(count.ToString(), next.Value);
                count++;
            }
        }

        ////[TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetSensorDataLatest_NullDeviceId()
        {
            AssertFailure(Channel.GetSensorDataLatest(null, new[] { "SomeId" }, 0));
        }

        ////[TestMethod]
        public void GetSensorDataLatest_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            GetMultipleSensorDataResult result = Channel.GetSensorDataLatest("-DoesNotExist-", new[] { sensor11.Id }, 0);
            AssertSuccess(result);
            Assert.AreEqual(0, result.SensorDataList.Count());
        }

        ////[TestMethod]
        public void GetSensorDataLatest_NullSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);

            AssertFailure(Channel.GetSensorDataLatest(sampleDevice1.Id, null, 0));
        }

        ////[TestMethod]
        public void GetSensorDataLatest_NonExistingSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);

            GetMultipleSensorDataResult result = Channel.GetSensorDataLatest(sampleDevice1.Id, new[] { "-NotExistingId-" }, 0);
            AssertSuccess(result);
            Assert.AreEqual(0, result.SensorDataList.Count());
        }

        ////[TestMethod]
        public void GetSensorDataLatest_NegativeMaxResults()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Channel.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = Channel.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            AssertFailure(Channel.GetSensorDataLatest(sampleDevice1.Id, new[] { sensor11.Id }, -1));
        }
        #endregion

        #endregion
    }
}
