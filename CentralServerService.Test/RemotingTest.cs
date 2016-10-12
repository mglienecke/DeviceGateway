using System;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Remoting;
using log4net;

using CentralServerService;
using GlobalDataContracts;
using System.Threading;
using ValueManagement;

namespace CentralServerServiceTest
{
    [TestClass]
    public class RemotingTest
    {
        private const string TestDescription = "Test Device";

        private static int Counter = 0;

        /// <summary>
        /// Initialize the assembly and the remote client. If there is no remote connection then initialize Log4Net as well
        /// </summary>
        /// <param name="context">The context.</param>
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            CentralServerServiceClient.UseRemoting = useRemoteConnection;

            if (useRemoteConnection == false)
            {
                log4net.Config.XmlConfigurator.Configure();
            }
        }

        [TestMethod]
        public void TestServerConnection()
        {
            if (useRemoteConnection && (RemotingServices.IsTransparentProxy(CentralServerServiceClient.Instance.Service) == false))
            {
                Assert.Fail("no .NET Remoting connection");
            }
        }

        #region ICentralServerService

        #region Registration of sensors, devices, triggers, sinks...

        #region RegisterDevice...
        [TestMethod]
        public void RegisterDevice()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            OperationResult result = CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);

            log4net.LogManager.GetLogger(this.GetType()).Info("SEE ME???");

            //Check
            AssertSuccess(result);

            GetDevicesResult resultDevices = CentralServerServiceClient.Instance.Service.GetDevices(new string[] { sampleDevice.Id });
            AssertSuccess(resultDevices);
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice.Id) != null);
            AssertAreEqual(sampleDevice, resultDevices.Devices.First(x => x.Id == sampleDevice.Id));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterDevice_Null()
        {
            //Create
            OperationResult result = CentralServerServiceClient.Instance.Service.RegisterDevice(null);
            AssertFailure(result);
        }

        [TestMethod]
        public void RegisterDevice_ExistingId()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            OperationResult result = CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);
            AssertSuccess(result);

            Device sampleDevice1 = CreateSampleDevice();
            sampleDevice1.Id = sampleDevice.Id;
            result = CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            AssertFailure(result);
        }

        [TestMethod]
        public void RegisterDevice_InvalidId()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            sampleDevice.Id += CentralServerServiceImpl.DeviceSensorSeparator + "1";
            OperationResult result = CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);
            AssertFailure(result);
        }

        [TestMethod]
        public void RegisterDevice_InvalidData_DescriptionNull()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            sampleDevice.Description = null;
            OperationResult result = CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);
            AssertFailure(result);
        }

        [TestMethod]
        public void RegisterDevice_InvalidData_IdNull()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            sampleDevice.Id = null;
            OperationResult result = CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);
            AssertFailure(result);
        }
        #endregion

        #region UpdateDevice...
        [TestMethod]
        public void UpdateDevice()
        {
            //Create
            Device sampleDevice1 = CreateSampleDevice();
            OperationResult result = CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);

            //Check
            AssertSuccess(result);

            Device sampleDevice2 = CreateSampleDevice();
            result = CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice2);

            //Check
            AssertSuccess(result);

            GetDevicesResult resultDevices = CentralServerServiceClient.Instance.Service.GetDevices(new string[] { sampleDevice1.Id, sampleDevice2.Id });
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

            result = CentralServerServiceClient.Instance.Service.UpdateDevice(sampleDevice1);

            //Check
            resultDevices = CentralServerServiceClient.Instance.Service.GetDevices(new string[] { sampleDevice1.Id, sampleDevice2.Id });
            AssertSuccess(resultDevices);
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice1.Id) != null);
            AssertAreEqual(sampleDevice1, resultDevices.Devices.First(x => x.Id == sampleDevice1.Id));
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice2.Id) != null);
            AssertAreEqual(sampleDevice2, resultDevices.Devices.First(x => x.Id == sampleDevice2.Id));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateDevice_Null()
        {
            //Create
            OperationResult result = CentralServerServiceClient.Instance.Service.UpdateDevice(null);
        }

        [TestMethod]
        public void UpdateDevice_NonExistingId()
        {
            //Update
            Device sampleDevice = CreateSampleDevice();
            OperationResult result = CentralServerServiceClient.Instance.Service.UpdateDevice(sampleDevice);

            //Check
            AssertFailure(result);
        }

        [TestMethod]
        public void UpdateDevice_InvalidData_IdNull()
        {
            //Update
            Device sampleDevice = CreateSampleDevice();
            sampleDevice.Id = null;
            OperationResult result = CentralServerServiceClient.Instance.Service.UpdateDevice(sampleDevice);
            AssertFailure(result);
        }

        [TestMethod]
        public void UpdateDevice_InvalidData_DescriptionNull()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            OperationResult result = CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);
            AssertSuccess(result);

            //Update
            sampleDevice.Description = null;
            result = CentralServerServiceClient.Instance.Service.UpdateDevice(sampleDevice);
            AssertFailure(result);
        }
        #endregion

        #region RegisterSensors...
        [TestMethod]
        public void RegisterSensors()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Device sampleDevice2 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice2);
            Device sampleDevice3 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice3);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            Sensor sensor12 = CreateSampleSensor(sampleDevice1);
            Sensor sensor21 = CreateSampleSensor(sampleDevice2);

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11, sensor12, sensor21 });
            AssertSuccess(resultRegSensors);

            GetSensorsForDeviceResult resultSensorsForDevice = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(sampleDevice1.Id);
            AssertSuccess(resultSensorsForDevice);

            Assert.AreEqual(resultSensorsForDevice.Sensors.Count, 2);
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor11.Id) != null);
            AssertAreEqual(sensor11, resultSensorsForDevice.Sensors.First(x => x.Id == sensor11.Id));
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor12.Id) != null);
            AssertAreEqual(sensor12, resultSensorsForDevice.Sensors.First(x => x.Id == sensor12.Id));

            resultSensorsForDevice = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(sampleDevice2.Id);
            AssertSuccess(resultSensorsForDevice);

            Assert.AreEqual(resultSensorsForDevice.Sensors.Count, 1);
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor21.Id) != null);
            AssertAreEqual(sensor21, resultSensorsForDevice.Sensors.First(x => x.Id == sensor21.Id));

            resultSensorsForDevice = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(sampleDevice3.Id);
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count, 0);
        }

        [TestMethod]
        public void RegisterSensors_WithPythonScript()
        {
            string executionExpression =
 @"
import clr
import ValueManagement.DynamicCallback
from ValueManagement.DynamicCallback import *
clr.AddReference('GlobalDataContracts')
import GlobalDataContracts
from GlobalDataContracts import *

# callback for general check
def VirtualValueCalculationCallback(callbackPassIn):
    callbackResult = CallbackResultData()
    callbackResult.IsValueModified = True
    historicValues = callbackPassIn.BaseValueDefinitionList[0].HistoricValues
    if historicValues.Count > 0:
        sum = 0
        for i in range (0, historicValues.Count):
            sum = sum + float(historicValues[i].Value)
        sum = sum + float(callbackPassIn.BaseValueDefinitionList[0].CurrentValue.Value)
        callbackResult.NewValue = GlobalDataContracts.SensorData(str(sum / (historicValues.Count + 1)))
    else:
        callbackResult.NewValue = GlobalDataContracts.SensorData(""0"")
    return(callbackResult);";

            Device sampleDevice1 = CreateSampleDevice();
            OperationResult result = CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Assert.IsTrue(result.Success);

            Sensor baseSensor = new Sensor();
            baseSensor.Category = "Test";
            baseSensor.Description = "Base sensor";
            baseSensor.UnitSymbol = "?";
            baseSensor.DeviceId = sampleDevice1.Id;
            baseSensor.Id = Guid.NewGuid().ToString();
            baseSensor.DefaultValue = "0";
            baseSensor.IsVirtualSensor = false;
            baseSensor.PullModeCommunicationType = PullModeCommunicationType.REST;
            baseSensor.SensorDataRetrievalMode = SensorDataRetrievalMode.Push;
            baseSensor.SensorValueDataType = SensorValueDataType.Decimal;
            baseSensor.ShallSensorDataBePersisted = false;
            baseSensor.PersistDirectlyAfterChange = false;

            Sensor sampleSensor = new Sensor();
            sampleSensor.Category = "Test";
            sampleSensor.Description = "Sample sensor";
            sampleSensor.UnitSymbol = "?";
            sampleSensor.DeviceId = sampleDevice1.Id;
            sampleSensor.Id = Guid.NewGuid().ToString();
            sampleSensor.InternalSensorId = 0;
            sampleSensor.DefaultValue = "0";
            sampleSensor.IsVirtualSensor = true;
            sampleSensor.SensorValueDataType = SensorValueDataType.Decimal;
            sampleSensor.VirtualSensorDefinition = new VirtualSensorDefinition();
            sampleSensor.VirtualSensorDefinition.Definition = executionExpression;
            sampleSensor.VirtualSensorDefinition.VirtualSensorCalculationType = VirtualSensorCalculationType.OnRequest;
            sampleSensor.VirtualSensorDefinition.VirtualSensorDefinitionType = VirtualSensorDefinitionType.IronPhyton;

            //Register
            result = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { baseSensor, sampleSensor });
            Assert.IsTrue(result.Success);

            //Get back (with internal ids)
            GetSensorResult resultSensor = CentralServerServiceClient.Instance.Service.GetSensor(sampleDevice1.Id, baseSensor.Id);
            Assert.IsTrue(resultSensor.Success);
            baseSensor = resultSensor.Sensor;

            resultSensor = CentralServerServiceClient.Instance.Service.GetSensor(sampleDevice1.Id, sampleSensor.Id);
            Assert.IsTrue(resultSensor.Success);
            sampleSensor = resultSensor.Sensor;

            //Setup sensor dependency
            CentralServerServiceClient.Instance.Service.AddSensorDependency(baseSensor.InternalSensorId, sampleSensor.InternalSensorId);

            //Push data
            MultipleSensorData sensorData = new MultipleSensorData();
            sensorData.SensorId = baseSensor.Id;
            sensorData.Measures = new SensorData[20];
            // assign using implicit conversion
            for (int i = 10; i < 30; i++)
                sensorData.Measures[i - 10] = new SensorData(DateTime.Now.Ticks, i.ToString(), null);

            CentralServerServiceClient.Instance.Service.StoreSensorData(sampleDevice1.Id, sensorData);

            //Get data
            GetMultipleSensorDataResult resultGet = CentralServerServiceClient.Instance.Service.GetSensorDataLatest(sampleDevice1.Id, new List<string>(new string[] { sampleSensor.Id }), 0);
            Assert.IsTrue(resultGet.Success);
            Assert.AreEqual(1, resultGet.SensorDataList.Count);
            Assert.AreEqual(1, resultGet.SensorDataList[0].Measures.Length);
            Assert.AreEqual(19.5m, Convert.ToDecimal(resultGet.SensorDataList[0].Measures[0].Value, CultureInfo.InvariantCulture));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSensors_Null()
        {
            OperationResult result = CentralServerServiceClient.Instance.Service.RegisterSensors(null);
            AssertFailure(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSensors_NullElement()
        {
            OperationResult result = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { null });
            AssertFailure(result);
        }

        [TestMethod]
        public void RegisterSensors_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertFailure(resultRegSensors);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSensors_InvalidData_NullDeviceId()
        {
            Sensor sensor11 = CreateSampleSensor(null);

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertFailure(resultRegSensors);
        }

        [TestMethod]
        public void RegisterSensors_InvalidData_NullDescription()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            sensor11.Description = null;

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertFailure(resultRegSensors);
        }

        [TestMethod]
        public void RegisterSensors_InvalidData_NullId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            sensor11.Id = null;

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertFailure(resultRegSensors);
        }

        [TestMethod]
        public void RegisterSensors_InvalidData_NullUnitSymbol()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            sensor11.UnitSymbol = null;

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertFailure(resultRegSensors);
        }
        #endregion

        #region UpdateSensor...
        [TestMethod]
        public void UpdateSensor()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
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

            resultSensors = CentralServerServiceClient.Instance.Service.UpdateSensor(sensor11);
            AssertSuccess(resultSensors);

            GetSensorsForDeviceResult resultSensorsForDevice = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(sampleDevice1.Id);
            AssertSuccess(resultSensorsForDevice);

            Assert.AreEqual(resultSensorsForDevice.Sensors.Count, 1);
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor11.Id) != null);
            AssertAreEqual(sensor11, resultSensorsForDevice.Sensors.First(x => x.Id == sensor11.Id));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateSensor_Null()
        {
            CentralServerServiceClient.Instance.Service.UpdateSensor(null);
        }

        [TestMethod]
        public void UpdateSensor_NonExistingDeviceI()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultSensors);

            sensor11.DeviceId += "1";

            resultSensors = CentralServerServiceClient.Instance.Service.UpdateSensor(sensor11);
            AssertFailure(resultSensors);
        }

        [TestMethod]
        public void UpdateSensor_NonExistingDeviceIdAndSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult result = CentralServerServiceClient.Instance.Service.UpdateSensor(sensor11);
            AssertFailure(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateSensor_NullDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultSensors);

            sensor11.DeviceId = null;

            resultSensors = CentralServerServiceClient.Instance.Service.UpdateSensor(sensor11);
            AssertFailure(resultSensors);
        }

        [TestMethod]
        public void UpdateSensor_NullDescription()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultSensors);

            sensor11.Description = null;

            resultSensors = CentralServerServiceClient.Instance.Service.UpdateSensor(sensor11);
            AssertFailure(resultSensors);
        }

        [TestMethod]
        public void UpdateSensor_NullUnitSymbol()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultSensors);

            sensor11.UnitSymbol = null;

            resultSensors = CentralServerServiceClient.Instance.Service.UpdateSensor(sensor11);
            AssertFailure(resultSensors);
        }
        #endregion

        #region RegisterDataSink...
        /*
        //[TestMethod]
        public void RegisterDataSink_AfterValueArrived()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            CentralServerServiceClient.Instance.Service.RegisterDataSink(sensor11, DataSinkType.AfterValueArrived, 
                new EventHandler<DataSinkEventArgs>(
                    delegate(object sender, DataSinkEventArgs args)
                    {
                        args.ChangedValue = new SensorData(DateTime.Now, args.OriginalValue.Value + " Changed");
                    }
                ));

            List<MultipleSensorData> data = new List<MultipleSensorData>();
            data.Add(new MultipleSensorData());
            data[0].SensorId = sensor11.Id;
            data[0].Measures = new SensorData[] { new SensorData(DateTime.Now, "TestVal") };
            OperationResult res = CentralServerServiceClient.Instance.Service.StoreSensorData(sampleDevice1.Id, data);
            AssertSuccess(res);

            GetMultipleSensorDataResult resultGet = CentralServerServiceClient.Instance.Service.GetSensorDataLatest(sampleDevice1.Id, new List<string>(new []{sensor11.Id}), 1);
            AssertSuccess(resultGet);
            MultipleSensorData resultMultSensorData = resultGet.SensorDataList[0];
            Assert.AreEqual(sensor11.Id, resultMultSensorData.SensorId);
            Assert.AreEqual(1, resultMultSensorData.Measures.Length);
            SensorData resultSensorData =  resultMultSensorData.Measures[0];
            Assert.AreEqual("TestVal" + " Changed", resultSensorData.Value);
        }

        //[TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterDataSink_NullSensor()
        {
            CentralServerServiceClient.Instance.Service.RegisterDataSink(null, DataSinkType.AfterValueArrived, new EventHandler<DataSinkEventArgs>(
                delegate(object sender, DataSinkEventArgs args){
                }
                ));
        }

        //[TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterDataSink_InvalidSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            CentralServerServiceClient.Instance.Service.RegisterDataSink(sensor11, DataSinkType.AfterValueArrived, new EventHandler<DataSinkEventArgs>(
                delegate(object sender, DataSinkEventArgs args)
                {
                }
                ));
        }

        //[TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterDataSink_NullHandler()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
 
            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            CentralServerServiceClient.Instance.Service.RegisterDataSink(sensor11, DataSinkType.AfterValueArrived, null);
        }
         */
        #endregion

        #endregion

        #region Checks...

        #region IsDeviceIdUsed...
        [TestMethod]
        public void IsDeviceIdUsed()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.IsDeviceIdUsed(sampleDevice.Id));

            OperationResult result = CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);

            //Check
            AssertSuccess(result);

            Assert.IsTrue(CentralServerServiceClient.Instance.Service.IsDeviceIdUsed(sampleDevice.Id));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsDeviceIdUsed_Null()
        {
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.IsDeviceIdUsed(null));
        }

        #endregion

        #region IsSensorIdRegisteredForDevice...

        [TestMethod]
        public void IsSensorIdRegisteredForDevice()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);

            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            Sensor sensor12 = CreateSampleSensor(sampleDevice1);

            Assert.IsFalse(CentralServerServiceClient.Instance.Service.IsSensorIdRegisteredForDevice(sampleDevice1.Id, sensor11.Id));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.IsSensorIdRegisteredForDevice(sampleDevice1.Id, sensor12.Id));

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            Assert.IsTrue(CentralServerServiceClient.Instance.Service.IsSensorIdRegisteredForDevice(sampleDevice1.Id, sensor11.Id));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.IsSensorIdRegisteredForDevice(sampleDevice1.Id, sensor12.Id));

            resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor12 });
            AssertSuccess(resultRegSensors);

            Assert.IsTrue(CentralServerServiceClient.Instance.Service.IsSensorIdRegisteredForDevice(sampleDevice1.Id, sensor11.Id));
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.IsSensorIdRegisteredForDevice(sampleDevice1.Id, sensor12.Id));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsSensorIdRegisteredForDevice_NullDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            Assert.IsFalse(CentralServerServiceClient.Instance.Service.IsSensorIdRegisteredForDevice(null, sensor11.Id));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsSensorIdRegisteredForDevice_NullSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            Assert.IsFalse(CentralServerServiceClient.Instance.Service.IsSensorIdRegisteredForDevice(sampleDevice1.Id, null));
        }

        [TestMethod]
        public void IsSensorIdRegisteredForDevice_InvalidDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            Assert.IsFalse(CentralServerServiceClient.Instance.Service.IsSensorIdRegisteredForDevice(sampleDevice1.Id, sensor11.Id));
        }
        #endregion

        #endregion

        #region Sensors, devices, sensor data...
        #region StoreSensorData...
        [TestMethod]
        public void StoreSensorData()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            sensor11.SensorValueDataType = SensorValueDataType.Int;

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData()
                    {
                        GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)),
                        Value = i.ToString(),
                        CorrelationId = Guid.NewGuid().ToString()
                    };
            }

            //Wait until stored
            OperationResult resultStoring = WaitForStoringDone(() =>
            {
                return CentralServerServiceClient.Instance.Service.StoreSensorData(sampleDevice1.Id, new List<MultipleSensorData>(new MultipleSensorData[] { sampleData })); ;
            },
                true);
            AssertSuccess(resultStoring);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StoreSensorData_NullDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData()
                    {
                        GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)),
                        Value = i.ToString(),
                        CorrelationId = Guid.NewGuid().ToString()
                    };
            }

            //Wait until stored
            OperationResult resultStoring = WaitForStoringDone(() =>
            {
                return CentralServerServiceClient.Instance.Service.StoreSensorData(null, new List<MultipleSensorData>(new MultipleSensorData[] { sampleData })); ;
            },
                true);
            AssertFailure(resultStoring);
        }

        [TestMethod]
        public void StoreSensorData_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData()
                    {
                        GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)),
                        Value = i.ToString(),
                        CorrelationId = Guid.NewGuid().ToString()
                    };
            }

            //Wait until stored
            OperationResult resultStoring = WaitForStoringDone(() =>
            {
                return CentralServerServiceClient.Instance.Service.StoreSensorData("-NotExistingId-", new List<MultipleSensorData>(new MultipleSensorData[] { sampleData })); ;
            },
                true);
            AssertFailure(resultStoring);
        }
        #endregion

        #region GetDevices...
        [TestMethod]
        public void GetDevices()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            OperationResult resultReg = CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);

            //Check
            AssertSuccess(resultReg);

            GetDevicesResult resultDevices = CentralServerServiceClient.Instance.Service.GetDevices();
            AssertSuccess(resultDevices);
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice.Id) != null);
            AssertAreEqual(sampleDevice, resultDevices.Devices.First(x => x.Id == sampleDevice.Id));
        }

        [TestMethod]
        public void GetDevices_Selected()
        {
            //Create
            Device sampleDevice1 = CreateSampleDevice();
            OperationResult resultReg = CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            //Check
            AssertSuccess(resultReg);

            Device sampleDevice2 = CreateSampleDevice();
            resultReg = CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice2);
            //Check
            AssertSuccess(resultReg);

            //Get 1st
            GetDevicesResult resultDevices = CentralServerServiceClient.Instance.Service.GetDevices(new string[] { sampleDevice1.Id });
            AssertSuccess(resultDevices);
            Assert.AreEqual(1, resultDevices.Devices.Count);
            AssertAreEqual(sampleDevice1, resultDevices.Devices[0]);

            //Get 2nd
            resultDevices = CentralServerServiceClient.Instance.Service.GetDevices(new string[] { sampleDevice2.Id });
            AssertSuccess(resultDevices);
            Assert.AreEqual(1, resultDevices.Devices.Count);
            AssertAreEqual(sampleDevice2, resultDevices.Devices[0]);

            //Get both
            resultDevices = CentralServerServiceClient.Instance.Service.GetDevices(new string[] { sampleDevice1.Id, sampleDevice2.Id });
            AssertSuccess(resultDevices);
            Assert.AreEqual(2, resultDevices.Devices.Count);
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice1.Id) != null);
            AssertAreEqual(sampleDevice1, resultDevices.Devices.First(x => x.Id == sampleDevice1.Id));
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice2.Id) != null);
            AssertAreEqual(sampleDevice2, resultDevices.Devices.First(x => x.Id == sampleDevice2.Id));
        }

        [TestMethod]
        public void GetDevices_Null()
        {
            //Create
            Device sampleDevice = CreateSampleDevice();
            OperationResult resultReg = CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);

            //Check
            AssertSuccess(resultReg);

            GetDevicesResult resultDevices = CentralServerServiceClient.Instance.Service.GetDevices(null);
            AssertSuccess(resultDevices);
            Assert.IsTrue(resultDevices.Devices.FirstOrDefault(x => x.Id == sampleDevice.Id) != null);
            AssertAreEqual(sampleDevice, resultDevices.Devices.First(x => x.Id == sampleDevice.Id));
        }
        #endregion

        #region GetSensorsForDevice...
        [TestMethod]
        public void GetSensorsForDevice()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Device sampleDevice2 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice2);
            Device sampleDevice3 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice3);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            Sensor sensor12 = CreateSampleSensor(sampleDevice1);
            Sensor sensor21 = CreateSampleSensor(sampleDevice2);

            //No sensors
            GetSensorsForDeviceResult resultSensorsForDevice = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(sampleDevice1.Id);
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count, 0);

            resultSensorsForDevice = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(sampleDevice2.Id);
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count, 0);

            resultSensorsForDevice = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(sampleDevice3.Id);
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count, 0);

            //Sensors for device 1
            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11, sensor12 });
            AssertSuccess(resultRegSensors);

            resultSensorsForDevice = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(sampleDevice1.Id);
            AssertSuccess(resultSensorsForDevice);

            Assert.AreEqual(resultSensorsForDevice.Sensors.Count, 2);
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor11.Id) != null);
            AssertAreEqual(sensor11, resultSensorsForDevice.Sensors.First(x => x.Id == sensor11.Id));
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor12.Id) != null);
            AssertAreEqual(sensor12, resultSensorsForDevice.Sensors.First(x => x.Id == sensor12.Id));

            resultSensorsForDevice = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(sampleDevice2.Id);
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count, 0);

            resultSensorsForDevice = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(sampleDevice3.Id);
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count, 0);

            //Sensors for device 2
            resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor21 });

            resultSensorsForDevice = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(sampleDevice1.Id);
            AssertSuccess(resultSensorsForDevice);

            Assert.AreEqual(resultSensorsForDevice.Sensors.Count, 2);
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor11.Id) != null);
            AssertAreEqual(sensor11, resultSensorsForDevice.Sensors.First(x => x.Id == sensor11.Id));
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor12.Id) != null);
            AssertAreEqual(sensor12, resultSensorsForDevice.Sensors.First(x => x.Id == sensor12.Id));

            resultSensorsForDevice = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(sampleDevice2.Id);
            AssertSuccess(resultSensorsForDevice);

            Assert.AreEqual(resultSensorsForDevice.Sensors.Count, 1);
            Assert.IsTrue(resultSensorsForDevice.Sensors.FirstOrDefault(x => x.Id == sensor21.Id) != null);
            AssertAreEqual(sensor21, resultSensorsForDevice.Sensors.First(x => x.Id == sensor21.Id));

            resultSensorsForDevice = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(sampleDevice3.Id);
            AssertSuccess(resultSensorsForDevice);
            Assert.AreEqual(resultSensorsForDevice.Sensors.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSensorsForDevice_NullDeviceId()
        {
            GetSensorsForDeviceResult resultSensorsForDevice = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(null);
            AssertFailure(resultSensorsForDevice);
        }

        [TestMethod]
        public void GetSensorsForDevice_NonExistingDeviceId()
        {
            GetSensorsForDeviceResult resultSensorsForDevice = CentralServerServiceClient.Instance.Service.GetSensorsForDevice("-NotExisting-");
            AssertFailure(resultSensorsForDevice);
        }
        #endregion

        #region GetSensor...
        [TestMethod]
        public void GetSensor()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Device sampleDevice2 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice2);
            Device sampleDevice3 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice3);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            Sensor sensor12 = CreateSampleSensor(sampleDevice1);
            Sensor sensor21 = CreateSampleSensor(sampleDevice2);

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11, sensor12, sensor21 });
            AssertSuccess(resultRegSensors);

            GetSensorResult resultGetSensor = CentralServerServiceClient.Instance.Service.GetSensor(sampleDevice1.Id, sensor11.Id);
            AssertSuccess(resultGetSensor);
            AssertAreEqual(sensor11, resultGetSensor.Sensor);

            resultGetSensor = CentralServerServiceClient.Instance.Service.GetSensor(sampleDevice1.Id, sensor12.Id);
            AssertSuccess(resultGetSensor);
            AssertAreEqual(sensor12, resultGetSensor.Sensor);

            resultGetSensor = CentralServerServiceClient.Instance.Service.GetSensor(sampleDevice2.Id, sensor21.Id);
            AssertSuccess(resultGetSensor);
            AssertAreEqual(sensor21, resultGetSensor.Sensor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSensor_NullDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            GetSensorResult resultGetSensor = CentralServerServiceClient.Instance.Service.GetSensor(null, sensor11.Id);
            AssertSuccess(resultGetSensor);
            Assert.IsNull(resultGetSensor.Sensor);
        }

        [TestMethod]
        public void GetSensor_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            GetSensorResult resultGetSensor = CentralServerServiceClient.Instance.Service.GetSensor("-NotExisting-", sensor11.Id);
            AssertFailure(resultGetSensor);
            Assert.IsNull(resultGetSensor.Sensor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSensor_NullSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);


            GetSensorResult resultGetSensor = CentralServerServiceClient.Instance.Service.GetSensor(sampleDevice1.Id, null);
            AssertSuccess(resultGetSensor);
        }

        [TestMethod]
        public void GetSensor_NonExistingSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);


            GetSensorResult resultGetSensor = CentralServerServiceClient.Instance.Service.GetSensor(sampleDevice1.Id, "-NotExisting-");
            AssertFailure(resultGetSensor);
            Assert.IsNull(resultGetSensor.Sensor);
        }
        #endregion

        #region GetSensorData(string deviceId, List<string> sensorIdList, DateTime dataStartDateTime, DateTime dataEndDateTime, int maxResults)...
        [TestMethod]
        public void GetSensorData()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            sensor11.SensorValueDataType = SensorValueDataType.Int;
            sensor11.ShallSensorDataBePersisted = true;
            sensor11.IsVirtualSensor = false;

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = i.ToString() };
            }

            //Wait until stored
            OperationResult resultStoring = WaitForStoringDone(() =>
            {
                return CentralServerServiceClient.Instance.Service.StoreSensorData(sampleDevice1.Id, new List<MultipleSensorData>(new MultipleSensorData[] { sampleData })); ;
            },
                true);
            AssertSuccess(resultStoring);
            System.Threading.Thread.Sleep(7000);

            //NO limit
            GetMultipleSensorDataResult result = CentralServerServiceClient.Instance.Service.GetSensorData(sampleDevice1.Id, new List<string>(new[] { sensor11.Id }), DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), DateTime.Now, 0);
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
            result = CentralServerServiceClient.Instance.Service.GetSensorData(sampleDevice1.Id, new List<string>(new[] { sensor11.Id }), DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), DateTime.Now, maxLimit);
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSensorData_NullDeviceId()
        {
            CentralServerServiceClient.Instance.Service.GetSensorData(null, new List<string>(new[] { "SomeId" }), DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), DateTime.Now, 0);
        }

        [TestMethod]
        public void GetSensorData_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            GetMultipleSensorDataResult result = CentralServerServiceClient.Instance.Service.GetSensorData("-DoesNotExist-", new List<string>(new[] { sensor11.Id }), DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), DateTime.Now, 0);
            AssertFailure(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSensorData_NullSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);

            CentralServerServiceClient.Instance.Service.GetSensorData(sampleDevice1.Id, null, DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), DateTime.Now, 0);
        }

        [TestMethod]
        public void GetSensorData_NonExistingSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);

            GetMultipleSensorDataResult result = CentralServerServiceClient.Instance.Service.GetSensorData(sampleDevice1.Id, new List<string>(new[] { "-NotExistingId-" }), DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), DateTime.Now, 0);
            AssertSuccess(result);
            Assert.AreEqual(0, result.SensorDataList.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetSensorData_StartBeforeEnd()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            CentralServerServiceClient.Instance.Service.GetSensorData(sampleDevice1.Id, new List<string>(new[] { sensor11.Id }), DateTime.Now, DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetSensorData_NegativeMaxResults()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            CentralServerServiceClient.Instance.Service.GetSensorData(sampleDevice1.Id, new List<string>(new[] { sensor11.Id }), DateTime.Now.Subtract(new TimeSpan(1, 0, 0)), DateTime.Now, -1);
        }
        #endregion

        #region GetSensorDataLatest(string deviceId, List<string> sensorIdList, DateTime dataStartDateTime, DateTime dataEndDateTime, int maxResults)...
        [TestMethod]
        public void GetSensorDataLatest()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);
            sensor11.SensorValueDataType = SensorValueDataType.Int;
            sensor11.IsVirtualSensor = false;

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            MultipleSensorData sampleData = new MultipleSensorData();
            sampleData.SensorId = sensor11.Id;
            sampleData.Measures = new SensorData[5];
            for (int i = 0; i < sampleData.Measures.Length; i++)
            {
                sampleData.Measures[i] = new SensorData()
                    {
                        GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)),
                        Value = i.ToString(),
                        CorrelationId = Guid.NewGuid().ToString()
                    };
            }

            //Wait until stored
            OperationResult resultStoring = WaitForStoringDone(() =>
            {
                return CentralServerServiceClient.Instance.Service.StoreSensorData(sampleDevice1.Id, new List<MultipleSensorData>(new MultipleSensorData[] { sampleData })); ;
            },
                true);
            AssertSuccess(resultStoring);

            //NO limit
            GetMultipleSensorDataResult result = CentralServerServiceClient.Instance.Service.GetSensorDataLatest(sampleDevice1.Id, new List<string>(new[] { sensor11.Id }), 0);
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
                Assert.IsTrue(!String.IsNullOrEmpty(next.CorrelationId));
                count++;
            }

            //With limit
            int maxLimit = sampleData.Measures.Length - 1;
            result = CentralServerServiceClient.Instance.Service.GetSensorDataLatest(sampleDevice1.Id, new List<string>(new[] { sensor11.Id }), maxLimit);
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
                Assert.IsTrue(!String.IsNullOrEmpty(next.CorrelationId));
                count++;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSensorDataLatest_NullDeviceId()
        {
            CentralServerServiceClient.Instance.Service.GetSensorDataLatest(null, new List<string>(new[] { "SomeId" }), 0);
        }

        [TestMethod]
        public void GetSensorDataLatest_NonExistingDeviceId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            GetMultipleSensorDataResult result = CentralServerServiceClient.Instance.Service.GetSensorDataLatest("-DoesNotExist-", new List<string>(new[] { sensor11.Id }), 0);
            AssertFailure(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSensorDataLatest_NullSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);

            CentralServerServiceClient.Instance.Service.GetSensorDataLatest(sampleDevice1.Id, null, 0);
        }

        [TestMethod]
        public void GetSensorDataLatest_NonExistingSensorId()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);

            GetMultipleSensorDataResult result = CentralServerServiceClient.Instance.Service.GetSensorDataLatest(sampleDevice1.Id, new List<string>(new[] { "-NotExistingId-" }), 0);
            AssertSuccess(result);
            Assert.AreEqual(0, result.SensorDataList.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetSensorDataLatest_NegativeMaxResults()
        {
            Device sampleDevice1 = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice1);
            Sensor sensor11 = CreateSampleSensor(sampleDevice1);

            OperationResult resultRegSensors = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor11 });
            AssertSuccess(resultRegSensors);

            CentralServerServiceClient.Instance.Service.GetSensorDataLatest(sampleDevice1.Id, new List<string>(new[] { sensor11.Id }), -1);
        }
        #endregion

        #region AddSensorDependency...
        [TestMethod]
        public void AddSensorDependency()
        {
            //Create
            Device sampleDevice = CreateAndRegisterSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);
            Sensor sensorB = CreateAndRegisterSampleSensor(sampleDevice, true);
            Sensor sensorD = CreateAndRegisterSampleSensor(sampleDevice, true);
            Sensor sensorN = CreateAndRegisterSampleSensor(sampleDevice, true);

            //Check
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensorD.InternalSensorId).Any(x => x.InternalSensorId == sensorB.InternalSensorId));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensorB.InternalSensorId).Any(x => x.InternalSensorId == sensorD.InternalSensorId));

            //Add
            CentralServerServiceClient.Instance.Service.AddSensorDependency(sensorB.InternalSensorId, sensorD.InternalSensorId);

            //Check
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensorD.InternalSensorId).Any(x => x.InternalSensorId == sensorB.InternalSensorId));
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensorB.InternalSensorId).Any(x => x.InternalSensorId == sensorD.InternalSensorId));
            //independent...
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensorN.InternalSensorId).Any(x => x.InternalSensorId == sensorB.InternalSensorId));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensorB.InternalSensorId).Any(x => x.InternalSensorId == sensorN.InternalSensorId));
        }

        [TestMethod]
        [ExpectedException(typeof(ValueDefinitionSetupException))]
        public void AddSensorDependency_DependentNotVirtual()
        {
            //Create
            Device sampleDevice = CreateAndRegisterSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);
            Sensor sensorB = CreateAndRegisterSampleSensor(sampleDevice, true);
            Sensor sensorD = CreateAndRegisterSampleSensor(sampleDevice, false);

            //Check
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensorD.InternalSensorId).Any(x => x.InternalSensorId == sensorB.InternalSensorId));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensorB.InternalSensorId).Any(x => x.InternalSensorId == sensorD.InternalSensorId));

            //Add
            CentralServerServiceClient.Instance.Service.AddSensorDependency(sensorB.InternalSensorId, sensorD.InternalSensorId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddSensorDependency_InvalidBaseSensorId()
        {
            //Create
            Sensor sensorD = CreateAndRegisterSampleSensor(CreateAndRegisterSampleDevice(), true);

            //Add
            CentralServerServiceClient.Instance.Service.AddSensorDependency(Int32.MinValue, sensorD.InternalSensorId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddSensorDependency_InvalidDependentSensorId()
        {
            //Create
            Sensor sensorB = CreateAndRegisterSampleSensor(CreateAndRegisterSampleDevice(), true);

            //Add
            CentralServerServiceClient.Instance.Service.AddSensorDependency(sensorB.InternalSensorId, Int32.MinValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ValueDefinitionSetupException))]
        public void AddSensorDependency_SelfDependency()
        {
            //Create
            Sensor sensorB = CreateAndRegisterSampleSensor(CreateAndRegisterSampleDevice(), true);

            //Add
            CentralServerServiceClient.Instance.Service.AddSensorDependency(sensorB.InternalSensorId, sensorB.InternalSensorId);
        }

        [TestMethod]
        [ExpectedException(typeof(CyclicReferenceException))]
        public void AddSensorDependency_CircularDependency()
        {
            //Create
            Sensor sensor1 = CreateAndRegisterSampleSensor(CreateAndRegisterSampleDevice(), true);
            Sensor sensor2 = CreateAndRegisterSampleSensor(CreateAndRegisterSampleDevice(), true);
            Sensor sensor3 = CreateAndRegisterSampleSensor(CreateAndRegisterSampleDevice(), true);

            //Check 1-2
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor2.InternalSensorId).Any(x => x.InternalSensorId == sensor1.InternalSensorId));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor1.InternalSensorId).Any(x => x.InternalSensorId == sensor2.InternalSensorId));

            //Add 1-2
            CentralServerServiceClient.Instance.Service.AddSensorDependency(sensor1.InternalSensorId, sensor2.InternalSensorId);

            //Check 1-2
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor2.InternalSensorId).Any(x => x.InternalSensorId == sensor1.InternalSensorId));
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor1.InternalSensorId).Any(x => x.InternalSensorId == sensor2.InternalSensorId));

            //Check 2-3
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor3.InternalSensorId).Any(x => x.InternalSensorId == sensor2.InternalSensorId));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor2.InternalSensorId).Any(x => x.InternalSensorId == sensor3.InternalSensorId));

            //Add 2-3
            CentralServerServiceClient.Instance.Service.AddSensorDependency(sensor2.InternalSensorId, sensor3.InternalSensorId);

            //Check 2-3
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor3.InternalSensorId).Any(x => x.InternalSensorId == sensor2.InternalSensorId));
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor2.InternalSensorId).Any(x => x.InternalSensorId == sensor3.InternalSensorId));

            //Add 3-1 - failure expected
            //Add 3-1
            CentralServerServiceClient.Instance.Service.AddSensorDependency(sensor3.InternalSensorId, sensor1.InternalSensorId);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void AddSensorDependency_ExistingDependency()
        {
            //Create
            Sensor sensorB = CreateAndRegisterSampleSensor(CreateAndRegisterSampleDevice(), true);
            Sensor sensorD = CreateAndRegisterSampleSensor(CreateAndRegisterSampleDevice(), true);

            //Check
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensorD.InternalSensorId).Any(x => x.InternalSensorId == sensorB.InternalSensorId));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensorB.InternalSensorId).Any(x => x.InternalSensorId == sensorD.InternalSensorId));

            //Add
            CentralServerServiceClient.Instance.Service.AddSensorDependency(sensorB.InternalSensorId, sensorD.InternalSensorId);
            //Double!
            CentralServerServiceClient.Instance.Service.AddSensorDependency(sensorB.InternalSensorId, sensorD.InternalSensorId);
        }
        #endregion

        #region RemoveSensorDependency...
        [TestMethod]
        public void RemoveSensorDependency()
        {
            //Create
            Device sampleDevice = CreateAndRegisterSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);
            Sensor sensor1 = CreateAndRegisterSampleSensor(sampleDevice, true);
            Sensor sensor2 = CreateAndRegisterSampleSensor(sampleDevice, true);
            Sensor sensor3 = CreateAndRegisterSampleSensor(sampleDevice, true);

            //Check
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor2.InternalSensorId).Any(x => x.InternalSensorId == sensor1.InternalSensorId));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor1.InternalSensorId).Any(x => x.InternalSensorId == sensor2.InternalSensorId));

            //Add 1-2
            CentralServerServiceClient.Instance.Service.AddSensorDependency(sensor1.InternalSensorId, sensor2.InternalSensorId);

            //Check 1-2
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor2.InternalSensorId).Any(x => x.InternalSensorId == sensor1.InternalSensorId));
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor1.InternalSensorId).Any(x => x.InternalSensorId == sensor2.InternalSensorId));

            //Add 2-3
            CentralServerServiceClient.Instance.Service.AddSensorDependency(sensor2.InternalSensorId, sensor3.InternalSensorId);

            //Check 2-3
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor3.InternalSensorId).Any(x => x.InternalSensorId == sensor2.InternalSensorId));
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor2.InternalSensorId).Any(x => x.InternalSensorId == sensor3.InternalSensorId));

            //Remove 1-2
            CentralServerServiceClient.Instance.Service.RemoveSensorDependency(sensor1.InternalSensorId, sensor2.InternalSensorId);

            //Check 1-2
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor2.InternalSensorId).Any(x => x.InternalSensorId == sensor1.InternalSensorId));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor1.InternalSensorId).Any(x => x.InternalSensorId == sensor2.InternalSensorId));

            //Check 2-3
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor3.InternalSensorId).Any(x => x.InternalSensorId == sensor2.InternalSensorId));
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor2.InternalSensorId).Any(x => x.InternalSensorId == sensor3.InternalSensorId));

            //Remove 2-3
            CentralServerServiceClient.Instance.Service.RemoveSensorDependency(sensor2.InternalSensorId, sensor3.InternalSensorId);

            //Check 1-2
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor2.InternalSensorId).Any(x => x.InternalSensorId == sensor1.InternalSensorId));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor1.InternalSensorId).Any(x => x.InternalSensorId == sensor2.InternalSensorId));

            //Check 2-3
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor3.InternalSensorId).Any(x => x.InternalSensorId == sensor2.InternalSensorId));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor2.InternalSensorId).Any(x => x.InternalSensorId == sensor3.InternalSensorId));
        }

        [TestMethod]
        public void RemoveSensorDependency_NotExistingDependency()
        {
            //Create
            Device sampleDevice = CreateAndRegisterSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);
            Sensor sensor1 = CreateAndRegisterSampleSensor(sampleDevice, true);
            Sensor sensor2 = CreateAndRegisterSampleSensor(sampleDevice, true);
            Sensor sensor3 = CreateAndRegisterSampleSensor(sampleDevice, true);

            //Check
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor2.InternalSensorId).Any(x => x.InternalSensorId == sensor1.InternalSensorId));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor1.InternalSensorId).Any(x => x.InternalSensorId == sensor2.InternalSensorId));

            //Remove  1-2
            CentralServerServiceClient.Instance.Service.RemoveSensorDependency(sensor1.InternalSensorId, sensor2.InternalSensorId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RemoveSensorDependency_NotExistingBaseSensor()
        {
            //Create
            Device sampleDevice = CreateAndRegisterSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);
            Sensor sensor1 = CreateAndRegisterSampleSensor(sampleDevice, true);

            //Remove  1-2
            CentralServerServiceClient.Instance.Service.RemoveSensorDependency(Int32.MinValue, sensor1.InternalSensorId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RemoveSensorDependency_NotExistingDependentSensor()
        {
            //Create
            Device sampleDevice = CreateAndRegisterSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);
            Sensor sensor1 = CreateAndRegisterSampleSensor(sampleDevice, true);

            //Remove  1-2
            CentralServerServiceClient.Instance.Service.RemoveSensorDependency(sensor1.InternalSensorId, Int32.MinValue);
        }
        #endregion


        #region GetSensorBaseDependencies...
        [TestMethod]
        public void GetBaseSensorDependencies()
        {
            //Create
            Device sampleDevice = CreateAndRegisterSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);
            Sensor sensor1 = CreateAndRegisterSampleSensor(sampleDevice, true);
            Sensor sensor2 = CreateAndRegisterSampleSensor(sampleDevice, true);
            Sensor sensor3 = CreateAndRegisterSampleSensor(sampleDevice, true);

            //Check
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor2.InternalSensorId).Any(x => x.InternalSensorId == sensor1.InternalSensorId));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor3.InternalSensorId).Any(x => x.InternalSensorId == sensor1.InternalSensorId));

            //Add 1-2
            CentralServerServiceClient.Instance.Service.AddSensorDependency(sensor1.InternalSensorId, sensor2.InternalSensorId);

            //Check
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor2.InternalSensorId).Any(x => x.InternalSensorId == sensor1.InternalSensorId));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor3.InternalSensorId).Any(x => x.InternalSensorId == sensor1.InternalSensorId));

            //Add 1-3
            CentralServerServiceClient.Instance.Service.AddSensorDependency(sensor1.InternalSensorId, sensor3.InternalSensorId);

            //Check
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor3.InternalSensorId).Any(x => x.InternalSensorId == sensor1.InternalSensorId));
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(sensor2.InternalSensorId).Any(x => x.InternalSensorId == sensor1.InternalSensorId));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetBaseSensorDependencies_NotExistingDependentSensorId()
        {
            CentralServerServiceClient.Instance.Service.GetBaseSensorDependencies(Int32.MinValue);
        }
        #endregion

        #region GetSensorDependentDependencies...
        [TestMethod]
        public void GetDependentSensorDependencies()
        {
            //Create
            Device sampleDevice = CreateAndRegisterSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);
            Sensor sensor1 = CreateAndRegisterSampleSensor(sampleDevice, true);
            Sensor sensor2 = CreateAndRegisterSampleSensor(sampleDevice, true);
            Sensor sensor3 = CreateAndRegisterSampleSensor(sampleDevice, true);

            //Check
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor1.InternalSensorId).Any(x => x.InternalSensorId == sensor2.InternalSensorId));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor1.InternalSensorId).Any(x => x.InternalSensorId == sensor3.InternalSensorId));

            //Add 1-2
            CentralServerServiceClient.Instance.Service.AddSensorDependency(sensor1.InternalSensorId, sensor2.InternalSensorId);

            //Check
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor1.InternalSensorId).Any(x => x.InternalSensorId == sensor2.InternalSensorId));
            Assert.IsFalse(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor1.InternalSensorId).Any(x => x.InternalSensorId == sensor3.InternalSensorId));

            //Add 1-3
            CentralServerServiceClient.Instance.Service.AddSensorDependency(sensor1.InternalSensorId, sensor3.InternalSensorId);

            //Check
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor1.InternalSensorId).Any(x => x.InternalSensorId == sensor2.InternalSensorId));
            Assert.IsTrue(CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(sensor1.InternalSensorId).Any(x => x.InternalSensorId == sensor3.InternalSensorId));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetBaseSensorDependencies_NotExistingBaseSensorId()
        {
            CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(Int32.MinValue);
        }
        #endregion
        #endregion

        #region Various API

        [TestMethod]
        public void GetNextCorrellationId()
        {
            GetCorrelationIdResult result = CentralServerServiceClient.Instance.Service.GetNextCorrelationId();
            string correlationId = result.CorrelationId;
            result = CentralServerServiceClient.Instance.Service.GetNextCorrelationId();
            Assert.IsTrue(result.CorrelationId != correlationId);
        }

        #endregion


        #endregion

        //[TestMethod]
        public void MassiveSensorCreate()
        {
            Stopwatch stopwatch;

            int[][] testCounts = new int[][] { new []{50, 20}, new []{100, 20}, new []{200, 20}, new []{400, 20},
                new []{50, 50}, new []{50, 100}, new []{50, 200}, new []{50, 400}};

            Console.WriteLine("-----Testing massive devices and sensors creation----");
            for (int index = 0; index < testCounts.Length; index++)
            {
                OperationResult result;
                Device sampleDevice;
                Sensor[] sensors;
                long totalMemoryStart = GC.GetTotalMemory(true) / 1024;
                double startProcessorTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount;
                PerformanceCounter counterRead = new PerformanceCounter("Process", "IO Read Bytes/sec", "QTAgent32");
                counterRead.NextValue();
                PerformanceCounter counterWrite = new PerformanceCounter("Process", "IO Write Bytes/sec", "QTAgent32");
                counterWrite.NextValue();
                stopwatch = Stopwatch.StartNew();

                for (int i = 0; i < testCounts[index][0]; i++)
                {
                    sampleDevice = CreateSampleDevice();
                    result = CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);
                    AssertSuccess(result);

                    sensors = new Sensor[testCounts[index][1]];
                    for (int j = 0; j < sensors.Length; j++)
                    {
                        sensors[j] = CreateSampleSensor(sampleDevice);
                    }

                    result = CentralServerServiceClient.Instance.Service.RegisterSensors(sensors);
                    AssertSuccess(result);
                }

                long millisElapsed = stopwatch.ElapsedMilliseconds;
                double endProcessorTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount;
                long totalMemoryEnd = GC.GetTotalMemory(true) / 1024;

                Console.WriteLine("Devices: {0}; Sensors: {1}", testCounts[index][0], testCounts[index][1]);
                Console.WriteLine("     Elapsed time:       {0} millis", millisElapsed);
                Console.WriteLine("     Processor load:     {0} %", 100 * (endProcessorTime - startProcessorTime) / millisElapsed);
                Console.WriteLine("     Memory consumption: {0} KB", totalMemoryEnd - totalMemoryStart);
                Console.WriteLine("     I/O Read bytes:     {0} KB/s", (int)counterRead.NextValue() / 1024);
                Console.WriteLine("     I/O Write bytes:    {0} KB/s", (int)counterWrite.NextValue() / 1024);
            }

            Console.WriteLine("-------------------------------");
        }

        //[TestMethod]
        public void MassiveInsertIntSensorData()
        {
            Device device = GetFirstDevice();
            if (device == null)
            {
                device = CreateDevice();
            }
            Assert.IsNotNull(device);

            // get an integer sensor type 
            Sensor sensInt = CreateSensor(device, Guid.NewGuid().ToString(), true, SensorDataRetrievalMode.Push, "Symb", 10, SensorValueDataType.Int);
            Sensor sensBit = CreateSensor(device, Guid.NewGuid().ToString(), true, SensorDataRetrievalMode.Push, "Symb", 10, SensorValueDataType.Bit);
            Sensor sensLong = CreateSensor(device, Guid.NewGuid().ToString(), true, SensorDataRetrievalMode.Push, "Symb", 10, SensorValueDataType.Long);
            Sensor sensString = CreateSensor(device, Guid.NewGuid().ToString(), true, SensorDataRetrievalMode.Push, "Symb", 10, SensorValueDataType.String);
            Sensor sensDecimal = CreateSensor(device, Guid.NewGuid().ToString(), true, SensorDataRetrievalMode.Push, "Symb", 10, SensorValueDataType.Decimal);
            Assert.IsNotNull(sensInt);
            Assert.IsNotNull(sensBit);
            Assert.IsNotNull(sensLong);
            Assert.IsNotNull(sensString);
            Assert.IsNotNull(sensDecimal);


            MultipleSensorData dataInt = new MultipleSensorData();
            MultipleSensorData dataDecimal = new MultipleSensorData();
            MultipleSensorData dataBit = new MultipleSensorData();
            MultipleSensorData dataString = new MultipleSensorData();
            MultipleSensorData dataLong = new MultipleSensorData();

            int[] testCounts = new int[] { 100, 200, 500, 1000, 2000, 4000, 8000 };

            for (int index = 0; index < testCounts.Length; index++)
            {
                dataInt.SensorId = sensInt.Id;
                dataInt.Measures = new SensorData[testCounts[index]];
                dataDecimal.SensorId = sensDecimal.Id;
                dataDecimal.Measures = new SensorData[testCounts[index]];
                dataLong.SensorId = sensLong.Id;
                dataLong.Measures = new SensorData[testCounts[index]];
                dataBit.SensorId = sensBit.Id;
                dataBit.Measures = new SensorData[testCounts[index]];
                dataString.SensorId = sensString.Id;
                dataString.Measures = new SensorData[testCounts[index]];

                // generate N values
                for (int i = 0; i < testCounts[index]; i++)
                {
                    SensorData x = new SensorData();
                    x.GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i));
                    x.Value = i.ToString();

                    dataInt.Measures[i] = x;
                    dataDecimal.Measures[i] = x;
                    dataString.Measures[i] = x;
                    dataLong.Measures[i] = x;

                    SensorData bitData = new SensorData();
                    bitData.GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i));
                    bitData.Value = ((i % 2) == 0) ? "1" : "0";
                    dataBit.Measures[i] = bitData;
                }

                OperationResult res = WaitForStoringDone(() =>
                {
                    return CentralServerServiceClient.Instance.Service.StoreSensorData(device.Id, new[] { dataInt, dataLong, dataBit, dataDecimal, dataString }.ToList());
                },
                    true);
                Assert.IsTrue(res.Success);
                Thread.Sleep(2000);

                /*
                Sensor[] sensVect = new Sensor[5];
                MultipleSensorData[] sensDatVect = new MultipleSensorData[5];

                for (int i = 0; i < sensVect.Length; i++)
                {
                    sensVect[i] = CreateSensor(device, Guid.NewGuid().ToString(), true, SensorDataRetrievalMode.Push, "Symb", 10, SensorValueDataType.Int);

                    sensDatVect[i] = new MultipleSensorData();
                    sensDatVect[i].SensorId = sensVect[i].Id;
                    sensDatVect[i].Measures = new SensorData[10000];

                    for (int count = 0; count < 10000; count++)
                    {
                        SensorData x = new SensorData();
                        x.GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, count));
                        x.Value = count.ToString();

                        sensDatVect[i].Measures[i] = x;
                    }
                }
                

                res = WaitForStoringDone(() =>
                {
                    return (CentralServerServiceClient.Instance.Service.StoreSensorData(device.Id, sensDatVect.ToList()));
                },
                    true);
                Assert.IsTrue(res.Success); */
            }
        }

        //[TestMethod]
        public void MassiveRetrieveIntSensorData()
        {
            Device device = GetFirstDevice();
            if (device == null)
            {
                device = CreateDevice();
            }
            Assert.IsNotNull(device);

            // get an integer sensor type 
            Sensor sensInt = CreateSensor(device, Guid.NewGuid().ToString(), true, SensorDataRetrievalMode.Push, "Symb", 10, SensorValueDataType.Int);
            Sensor sensBit = CreateSensor(device, Guid.NewGuid().ToString(), true, SensorDataRetrievalMode.Push, "Symb", 10, SensorValueDataType.Bit);
            Sensor sensLong = CreateSensor(device, Guid.NewGuid().ToString(), true, SensorDataRetrievalMode.Push, "Symb", 10, SensorValueDataType.Long);
            Sensor sensString = CreateSensor(device, Guid.NewGuid().ToString(), true, SensorDataRetrievalMode.Push, "Symb", 10, SensorValueDataType.String);
            Sensor sensDecimal = CreateSensor(device, Guid.NewGuid().ToString(), true, SensorDataRetrievalMode.Push, "Symb", 10, SensorValueDataType.Decimal);
            Assert.IsNotNull(sensInt);
            Assert.IsNotNull(sensBit);
            Assert.IsNotNull(sensLong);
            Assert.IsNotNull(sensString);
            Assert.IsNotNull(sensDecimal);


            MultipleSensorData dataInt = new MultipleSensorData();
            MultipleSensorData dataDecimal = new MultipleSensorData();
            MultipleSensorData dataBit = new MultipleSensorData();
            MultipleSensorData dataString = new MultipleSensorData();
            MultipleSensorData dataLong = new MultipleSensorData();

            dataInt.SensorId = sensInt.Id;
            dataInt.Measures = new SensorData[10000];
            dataDecimal.SensorId = sensDecimal.Id;
            dataDecimal.Measures = new SensorData[10000];
            dataLong.SensorId = sensLong.Id;
            dataLong.Measures = new SensorData[10000];
            dataBit.SensorId = sensBit.Id;
            dataBit.Measures = new SensorData[10000];
            dataString.SensorId = sensString.Id;
            dataString.Measures = new SensorData[10000];

            // generate 10,00 values initially
            for (int i = 0; i < 10000; i++)
            {
                SensorData x = new SensorData();
                x.GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i));
                x.Value = i.ToString();

                dataInt.Measures[i] = x;
                dataDecimal.Measures[i] = x;
                dataString.Measures[i] = x;
                dataLong.Measures[i] = x;

                SensorData bitData = new SensorData();
                bitData.GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i));
                bitData.Value = ((i % 2) == 0) ? "1" : "0";
                dataBit.Measures[i] = bitData;
            }

            OperationResult res = WaitForStoringDone(() =>
            {
                return CentralServerServiceClient.Instance.Service.StoreSensorData(device.Id, new[] { dataInt, dataLong, dataBit, dataDecimal, dataString }.ToList());
            },
                true);
            AssertSuccess(res);

            int[] testCounts = new int[] { 100, 250, 500, 1000, 2000, 4000, 8000 };

            Console.WriteLine("------- SENSOR TESTING DATA RETRIEVAL---------------------");
            Stopwatch stopwatch;
            double startProcessorTime;
            long totalMemoryStart;
            GetMultipleSensorDataResult result;
            for (int index = 0; index < testCounts.Length; index++)
            {
                totalMemoryStart = GC.GetTotalMemory(true) / 1024;
                startProcessorTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount;
                PerformanceCounter counterRead = new PerformanceCounter("Process", "IO Read Bytes/sec", "QTAgent32");
                counterRead.NextValue();
                PerformanceCounter counterWrite = new PerformanceCounter("Process", "IO Write Bytes/sec", "QTAgent32");
                counterWrite.NextValue();
                stopwatch = Stopwatch.StartNew();

                result = CentralServerServiceClient.Instance.Service.GetSensorDataLatest(device.Id, new List<string>(new[] { sensBit.Id, sensDecimal.Id, sensInt.Id, sensLong.Id, sensString.Id }), testCounts[index]);
                AssertSuccess(result);
                Assert.IsTrue(result.SensorDataList.Count == 5);
                Assert.IsTrue(result.SensorDataList[0].Measures.Length == testCounts[index]);

                long millisElapsed = stopwatch.ElapsedMilliseconds;
                double endProcessorTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount;
                long totalMemoryEnd = GC.GetTotalMemory(true) / 1024;

                Console.WriteLine("Results count: {0}", testCounts[index]);
                Console.WriteLine("     Elapsed time:       {0} millis", millisElapsed);
                Console.WriteLine("     Processor load:     {0} %", 100 * (endProcessorTime - startProcessorTime) / millisElapsed);
                Console.WriteLine("     Memory consumption: {0} KB", totalMemoryEnd - totalMemoryStart);
                Console.WriteLine("     I/O Read bytes:     {0} KB/s", (int)counterRead.NextValue() / 1024);
                Console.WriteLine("     I/O Write bytes:    {0} KB/s", (int)counterWrite.NextValue() / 1024);
            }

            Console.WriteLine("----------------------------");
        }

        #region Old methods...
        /*


		[TestMethod]
		public void TestRegisterAndUpdateDevice()
		{
			List<Device> deviceList = CentralServerServiceClient.Instance.Service.GetDevices().Devices;
			
			// create a unique device id
			string deviceId = Guid.NewGuid().ToString();
			
			Device dev = new Device();

			// create the device
			Device testEntry = deviceList.FirstOrDefault(entry => entry.Id == deviceId);
			if (testEntry == null)
			{
				// test the creation
				testEntry = new Device();
				testEntry.Id = deviceId;
				testEntry.Description = TestDescription;
				testEntry.Location = new Location();
				testEntry.Location.Latitude = 51.4772; 
				testEntry.Location.Longitude = 0;
				testEntry.Location.Name = "Greenwich Test Device";
				
				CentralServerServiceClient.Instance.Service.RegisterDevice(testEntry);

				// check that the device was properly created
				Assert.IsNotNull(CentralServerServiceClient.Instance.Service.GetDevices().Devices.FirstOrDefault(devTy => devTy.Id == deviceId));
			}

			// now the update -> this should have no impact but produce an error
			testEntry.Description = deviceId;
			CallResult result = CentralServerServiceClient.Instance.Service.RegisterDevice(testEntry);
			// check for the error and that no change has been done
			Assert.IsFalse(result.Success);
			Assert.IsNotNull(CentralServerServiceClient.Instance.Service.GetDevices().Devices.FirstOrDefault(devTy => devTy.Description == TestDescription));

			// check that the device was properly updated
			result = CentralServerServiceClient.Instance.Service.UpdateDevice(testEntry);
			Assert.IsTrue(result.Success);
			Assert.IsNotNull(CentralServerServiceClient.Instance.Service.GetDevices().Devices.FirstOrDefault(devTy => devTy.Description == deviceId));

			// and back to the proper value
			testEntry.Description = TestDescription;
			result = CentralServerServiceClient.Instance.Service.UpdateDevice(testEntry);

			// check that the device was properly updated
			Assert.IsTrue(result.Success);
			Assert.IsNotNull(CentralServerServiceClient.Instance.Service.GetDevices().Devices.FirstOrDefault(devTy => devTy.Description == testEntry.Description));
		}

		[TestMethod]
		public void TestRegisterAndUpdateSensor()
		{
			Device device = GetFirstDevice();
			if (device == null)
			{
				device = CreateDevice();
			}

			Assert.IsNotNull(device);

			Sensor sensor = CreateSensor(device, Guid.NewGuid().ToString(), true, SensorDataRetrievalMode.Push, "Symb", 10, SensorValueDataType.Int);
			Assert.IsNotNull(sensor);

			string newDescription = "Change description";
			sensor.Description = newDescription;
			CallResult result = CentralServerServiceClient.Instance.Service.RegisterSensors((new []{ sensor}).ToList());
			Assert.IsFalse(result.Success);

			GetSensorResult callRes = CentralServerServiceClient.Instance.Service.GetSensor(device.Id, sensor.Id);
			Assert.IsTrue(callRes.Success);

			if (useRemoteConnection)
			{
				// this test will fail for local (non remote) connection as the object is modified
				Assert.IsFalse(callRes.Sensor.Description == newDescription);
			}

			result = CentralServerServiceClient.Instance.Service.UpdateSensor(sensor);
			Assert.IsTrue(result.Success);

			callRes = CentralServerServiceClient.Instance.Service.GetSensor(device.Id, sensor.Id);
			Assert.IsTrue(callRes.Success);
			Assert.IsTrue(callRes.Sensor.Description == newDescription);
		}

		[TestMethod]
		public void InsertIntSensorData()
		{
			Device device = GetFirstDevice();
			if (device == null)
			{
				device = CreateDevice();
			}
			Assert.IsNotNull(device);

			// get an integer sensor type 
			Sensor sens = GetDeviceSensor(device.Id, SensorValueDataType.Int);
			if (sens == null)
			{
				sens = CreateSensor(device, Guid.NewGuid().ToString(), true, SensorDataRetrievalMode.Push, "Symb", 10, SensorValueDataType.Int);
			}

			Assert.IsNotNull(sens);

			MultipleSensorData data = new MultipleSensorData();
			data.SensorId = sens.Id;
			data.Measures = new SensorData[100];

			// generate 100 values initially
			for (int i = 0; i < 100; i++)
			{
				SensorData x = new SensorData();
				x.GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i));
				x.Value = i.ToString();
				data.Measures[i] = x;
			}

			CallResult res = WaitForStoringDone(() =>
				{
					return (CentralServerServiceClient.Instance.Service.StoreSensorData(device.Id, new[] { data }.ToList()));
				}, 
				false);

            // generate 100 values more
            for (int i = 0; i < 100; i++)
            {
                SensorData x = new SensorData();
                x.GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i));
                x.Value = i.ToString();
                data.Measures[i] = x;
            }

            res = WaitForStoringDone(() =>
            {
                return (CentralServerServiceClient.Instance.Service.StoreSensorData(device.Id, new[] { data }.ToList()));
            },
                false);

            Thread.Sleep(5000);

			Assert.IsTrue(res.Success);
		}
        */

        /*
		[TestMethod]
		public void MassiveCreateSensorAndData()
		{
			Device device = GetFirstDevice();
			if (device == null)
			{
				device = CreateDevice();
			}
			Assert.IsNotNull(device);

			
			int maxCount = 10000;
	
			MultipleSensorData[] sensDatVect = new MultipleSensorData[maxCount];

			for (int sensCount = 0; sensCount < maxCount; sensCount++)
			{
				Sensor testSens = CreateSensor(device, "TestSensor" + DateTime.Now.Ticks.ToString(), true, SensorDataRetrievalMode.Push, "What", 10, SensorValueDataType.Int);
				Assert.IsNotNull(testSens);
				

				sensDatVect[sensCount] = new MultipleSensorData();
				sensDatVect[sensCount].SensorId = testSens.Id;
				sensDatVect[sensCount].Measures = new SensorData[100];

				// generate 500 values initially
				for (int i = 1; i < 100; i++)
				{
					SensorData x = new SensorData();
					x.GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i));
					x.Value = i.ToString();

					sensDatVect[sensCount].Measures[i] = x;
				}

				Console.WriteLine("{0} Sensors created out of {1}", sensCount, maxCount);
			}

			Console.WriteLine("starting to write the data at: " + DateTime.Now.ToString());
			CallResult res = WaitForStoringDone(() =>
				{
					return (CentralServerServiceClient.Instance.Service.StoreSensorData(device.Id, sensDatVect.ToList()));
				},
				true);

			Console.WriteLine("finished writing the data at: " + DateTime.Now.ToString());
			Assert.IsTrue(res.Success);
		}
         */
        #endregion

        #region Private methods...
        private static bool useRemoteConnection = true;

        /// <summary>
        /// Gets the first device.
        /// </summary>
        /// <returns></returns>
        private Device GetFirstDevice()
        {
            GetDevicesResult result = CentralServerServiceClient.Instance.Service.GetDevices();
            return (result.Devices.Count > 0 ? result.Devices[0] : null);
        }

        /// <summary>
        /// Gets the first sensor.
        /// </summary>
        /// <returns></returns>
        private Sensor GetFirstSensor()
        {
            Sensor sens = null;

            Device dev = GetFirstDevice();
            if (dev != null)
            {
                sens = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(dev.Id).Sensors[0];
            }
            return (sens);
        }

        /// <summary>
        /// Creates the sensor.
        /// </summary>
        /// <param name="persistData">if set to <c>true</c> [persist data].</param>
        /// <param name="retrievalMode">The retrieval mode.</param>
        /// <param name="symbol">The symbol.</param>
        /// <param name="unitType">Type of the unit.</param>
        /// <returns></returns>
        private Sensor CreateSensor(Device device, string sensorId, bool persistData, SensorDataRetrievalMode retrievalMode, string symbol,
            int scanningRate, SensorValueDataType valueDataType)
        {
            if (device == null)
            {
                device = CreateDevice();
            }

            Assert.IsNotNull(device);

            Sensor sensor = new Sensor();
            sensor.Id = sensorId;
            sensor.Description = TestDescription;
            sensor.DeviceId = device.Id;
            sensor.IsVirtualSensor = false;
            sensor.SensorDataRetrievalMode = retrievalMode;
            sensor.ShallSensorDataBePersisted = persistData;
            sensor.UnitSymbol = symbol;
            sensor.PullFrequencyInSeconds = scanningRate;
            sensor.PullModeCommunicationType = PullModeCommunicationType.SOAP;
            sensor.SensorValueDataType = valueDataType;

            List<Sensor> sensorList = new List<Sensor>();
            sensorList.Add(sensor);

            return (CentralServerServiceClient.Instance.Service.RegisterSensors(sensorList).Success ? sensor : null);
        }

        /// <summary>
        /// Creates the device.
        /// </summary>
        /// <returns></returns>
        private Device CreateDevice()
        {
            Device device = new Device();
            device.Id = Guid.NewGuid().ToString();
            device.Description = TestDescription;
            device.Location = new Location();
            device.Location.Name = "Nowhere";
            device.DeviceIpEndPoint = "IP:" + Guid.NewGuid().ToString();

            return (CentralServerServiceClient.Instance.Service.RegisterDevice(device).Success ? device : null);
        }

        /// <summary>
        /// Creates the device.
        /// </summary>
        /// <returns></returns>
        private Device CreateAndRegisterSampleDevice()
        {
            Device sampleDevice = CreateSampleDevice();
            CentralServerServiceClient.Instance.Service.RegisterDevice(sampleDevice);
            return sampleDevice;
        }

        private Sensor CreateAndRegisterSampleSensor(Device device, bool createVirtualSensor)
        {
            Sensor sensor = CreateSampleSensor(device);
            if (createVirtualSensor)
            {
                sensor.IsVirtualSensor = true;
            }
            else
            {
                sensor.IsVirtualSensor = false;
                sensor.VirtualSensorDefinition = null;
            }

            CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[] { sensor });
            return CentralServerServiceClient.Instance.Service.GetSensor(device.Id, sensor.Id).Sensor;
        }

        /// <summary>
        /// get a sensor for a device which fits the data type we are looking for
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="searchType"></param>
        /// <returns></returns>
        public Sensor GetDeviceSensor(string deviceId, SensorValueDataType searchType)
        {
            GetSensorsForDeviceResult callResult = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(deviceId);
            return (callResult.Success ? callResult.Sensors.FirstOrDefault(devSens => devSens.SensorValueDataType == searchType) : null);
        }

        /// <summary>
        /// Waits for storing done and cancels the task if necessary
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="cancelServiceIfNotRemote">if set to <c>true</c> [cancel service if not remote].</param>
        /// <returns></returns>
        private OperationResult WaitForStoringDone(Func<OperationResult> activity, bool cancelServiceIfNotRemote)
        {
            bool valuesStored = false;

            if (useRemoteConnection == false)
            {
                CentralServerServiceClient.Instance.Service.StoringIsDone += (sender, args) =>
                {
                    valuesStored = true;
                };
            }

            OperationResult res = activity();

            if ((useRemoteConnection == false) && cancelServiceIfNotRemote)
            {
                while (valuesStored == false)
                {
                    System.Threading.Thread.Sleep(500);
                }

                //CentralServerServiceClient.Instance.Service.CancelService();
            }

            return (res);
        }

        private void AssertSuccess(OperationResult result)
        {
            Assert.IsTrue(result.ErrorMessages == null);
            Assert.IsTrue(result.Success);
        }

        private void AssertFailure(OperationResult result)
        {
            Assert.IsTrue(result.ErrorMessages != null && result.ErrorMessages.Length > 0);
            Assert.IsFalse(result.Success);
        }

        private void AssertAreEqual(Sensor expected, Sensor received)
        {
            if (expected == received)
            {
                return;
            }

            if (expected == null || received == null)
            {
                Assert.Fail();
            }

            Assert.AreEqual(expected.Description, received.Description);
            Assert.AreEqual(expected.Id, received.Id);
            Assert.AreEqual(expected.Category, received.Category);
            Assert.AreEqual(expected.DeviceId, received.DeviceId);
            Assert.AreEqual(expected.DefaultValue, received.DefaultValue);
            //Assert.AreEqual(expected.InternalSensorId, received.InternalSensorId);
            Assert.AreEqual(expected.IsVirtualSensor, received.IsVirtualSensor);
            Assert.AreEqual(expected.PersistDirectlyAfterChange, received.PersistDirectlyAfterChange);
            Assert.AreEqual(expected.PullFrequencyInSeconds, received.PullFrequencyInSeconds);
            Assert.AreEqual(expected.PullModeCommunicationType, received.PullModeCommunicationType);
            Assert.AreEqual(expected.PullModeDotNetObjectType, received.PullModeDotNetObjectType);
            Assert.AreEqual(expected.SensorDataRetrievalMode, received.SensorDataRetrievalMode);
            Assert.AreEqual(expected.SensorValueDataType, received.SensorValueDataType);
            Assert.AreEqual(expected.ShallSensorDataBePersisted, received.ShallSensorDataBePersisted);
            Assert.AreEqual(expected.UnitSymbol, received.UnitSymbol);
            AssertAreEqual(expected.VirtualSensorDefinition, received.VirtualSensorDefinition);
        }

        private void AssertAreEqual(Device expected, Device received)
        {
            if (expected == received)
            {
                return;
            }

            if (expected == null || received == null)
            {
                Assert.Fail();
            }

            Assert.AreEqual(expected.Description, received.Description);
            Assert.AreEqual(expected.DeviceIpEndPoint, received.DeviceIpEndPoint);
            Assert.AreEqual(expected.Id, received.Id);
            AssertAreEqual(expected.Location, received.Location);
        }

        private void AssertAreEqual(Location expected, Location received)
        {
            if (expected == received)
            {
                return;
            }

            if (expected == null || received == null)
            {
                Assert.Fail();
            }

            Assert.AreEqual(expected.Elevation, received.Elevation);
            Assert.AreEqual(expected.Latitude, received.Latitude);
            Assert.AreEqual(expected.Longitude, received.Longitude);
            Assert.AreEqual(expected.Name, received.Name);
        }

        private void AssertAreEqual(VirtualSensorDefinition expected, VirtualSensorDefinition received)
        {
            if (expected == received)
            {
                return;
            }

            if (expected == null || received == null)
            {
                Assert.Fail();
            }

            Assert.AreEqual(expected.Definition, received.Definition);
            Assert.AreEqual(expected.VirtualSensorCalculationType, received.VirtualSensorCalculationType);
            Assert.AreEqual(expected.VirtualSensorDefinitionType, received.VirtualSensorDefinitionType);
        }

        private Device CreateSampleDevice()
        {
            Device result = new Device();
            result.Description = "Descr " + Guid.NewGuid().ToString();
            result.DeviceIpEndPoint = "IP:" + Guid.NewGuid().ToString();
            result.Id = Guid.NewGuid().ToString();
            result.Location = new Location();
            result.Location.Longitude = Counter++ % 360;
            result.Location.Latitude = Counter++ % 360;
            result.Location.Elevation = Counter++;
            result.Location.Name = "Loc" + Guid.NewGuid().ToString();

            return result;
        }

        private Sensor CreateSampleSensor(Device device)
        {
            Sensor result = new Sensor();
            result.Category = "C" + Counter++;
            result.Description = "Desc" + Counter++;
            result.DeviceId = device == null ? null : device.Id;
            result.Id = Guid.NewGuid().ToString();
            result.InternalSensorId = 0;
            result.IsVirtualSensor = Counter++ % 2 == 0;
            result.PullFrequencyInSeconds = Counter++;
            result.PullModeCommunicationType = (PullModeCommunicationType)Enum.GetValues(typeof(PullModeCommunicationType)).GetValue(Counter++ % Enum.GetValues(typeof(PullModeCommunicationType)).Length);
            result.PullModeDotNetObjectType = "Type" + Counter++;
            result.SensorDataRetrievalMode = (SensorDataRetrievalMode)Enum.GetValues(typeof(SensorDataRetrievalMode)).GetValue(Counter++ % Enum.GetValues(typeof(SensorDataRetrievalMode)).Length);
            result.SensorValueDataType = (SensorValueDataType)Enum.GetValues(typeof(SensorValueDataType)).GetValue(Counter++ % Enum.GetValues(typeof(SensorValueDataType)).Length);
            result.ShallSensorDataBePersisted = Counter++ % 2 == 0;
            result.PersistDirectlyAfterChange = Counter++ % 2 == 0;
            result.UnitSymbol = "U" + Counter++;
            result.VirtualSensorDefinition = new VirtualSensorDefinition();
            result.VirtualSensorDefinition.Definition = "Defimition" + Guid.NewGuid().ToString();
            result.VirtualSensorDefinition.VirtualSensorCalculationType = (VirtualSensorCalculationType)Enum.GetValues(typeof(VirtualSensorCalculationType)).GetValue(Counter++ % Enum.GetValues(typeof(VirtualSensorCalculationType)).Length);
            result.VirtualSensorDefinition.VirtualSensorDefinitionType = VirtualSensorDefinitionType.Undefined;
            result.DefaultValue = DateTime.Now.Ticks.ToString();
            result.IsSynchronousPushToActuator = Counter++ % 2 == 0;
            result.IsActuator = Counter++ % 2 == 0;

            return result;
        }

        #endregion
    }
}
