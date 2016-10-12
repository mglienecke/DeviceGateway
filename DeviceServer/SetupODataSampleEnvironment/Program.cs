 using System.Configuration;
using System.IO;
using System.Net;
using CentralServerService;
using GlobalDataContracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValueManagement.DynamicCallback;
using Common.Server.CNDEP;
using Newtonsoft.Json;
using Common.Server;

namespace SetupODataSampleEnvironment
{
    class Program
    {
        private const string MimeTypeJson = "application/json";
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

        private static CndepUdpClient CndepUdpClient
        {
            get;
            set;
        }
        #endregion

        static void Main(string[] args)
        {
            CndepServerAddress = ConfigurationManager.AppSettings["CNDEPServer"];
            CndepServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["CNDEPPort"]);
            CndepContentType = MimeTypeJson;
            CndepRequestRetryCount = 1;
            CndepRequestTimeout = 5000;
            CndepUdpClient = new CndepUdpClient(CndepServerPort, Dns.GetHostEntry(CndepServerAddress).AddressList[0]);

            while (true)
            {
                Console.WriteLine(@"Enter '1' to generate test environment, 
enter '2' for virtual sensors test data generation, 
enter '3' for actuator test data generation (the DeviceServer.Simulator must be started to see the values delivered),
enter '4' to quit.");
                string readLine = Console.ReadLine();

                switch (readLine)
                {
                    case "1":
                        SetupTestEnvironment();
                        break;
                    case "2":
                        GenerateVirtualSensorTestData();
                        break;
                    case "3":
                        GenerateActuatorTestData();
                        break;
                    case "4":
                        break;
                    default:
                        continue;
                }
                break;
            }

            Console.WriteLine("Press Enter to quit...");
            Console.ReadLine();
            Environment.Exit(0);
            
        }

        private static void SetupTestEnvironment()
        {
            try
            {
                //CREATE SAMPLE DEVICES if they are not already created
                //Device 1
                var deviceA = CreateTestDevice("A", 1, ConfigurationManager.AppSettings["DeviceIpAddress"]);
                RegisterDevice(deviceA);


                //CREATE SAMPLE SENSORS for the sample devices if they are not already created
                //Create data sensors
                //Device 1 sensors
                //Sensor 1
                var sensorA1 = CreateTestPushSensor(deviceA, "sensor_A1");
                sensorA1.ShallSensorDataBePersisted = true;
                RegisterSensor(sensorA1);

                ////Sensor 2
                //var sensorA2 = CreateTestPushSensor(deviceA, "sensor_A2");
                //RegisterSensor(sensorA2);

                //Create virtual sensors
                //SUM VALUE
                var vSensorSumA1 = CreateTestVirtualSensor(deviceA, "sum1");
                vSensorSumA1.VirtualSensorDefinition.VirtualSensorCalculationType = VirtualSensorCalculationType.OnChange;
                vSensorSumA1.VirtualSensorDefinition.VirtualSensorDefinitionType = VirtualSensorDefinitionType.CSharpExpression;
                vSensorSumA1.VirtualSensorDefinition.Definition = @"
using System;
using System.Linq;
using ValueManagement.DynamicCallback;

public class CSharpInteractiveClass
{
    static decimal Sum = 0;
    static public CallbackResultData VirtualValueCalculationCallback(CallbackPassInData passInData)
    {
        CallbackResultData result = new CallbackResultData();
		result.IsValueModified = true;
        Sum = Convert.ToDecimal(passInData.BaseValueDefinitionList.ElementAt(0).CurrentValue.Value) + Sum;
		result.NewValue = new GlobalDataContracts.SensorData(){Value = (Sum).ToString(), GeneratedWhen = DateTime.Now, CorrelationId = passInData.BaseValueDefinitionList.ElementAt(0).CurrentValue.CorrelationId};
		return (result);
    }
}";
                RegisterSensor(vSensorSumA1);

                //The sensor must be dependent from the sensor sensor_A1.
                RegisterSensorDependency(sensorA1, vSensorSumA1);

                //AVERAGE VALUE
                var vSensorAvgA1 = CreateTestVirtualSensor(deviceA, "average1");
                vSensorAvgA1.VirtualSensorDefinition.VirtualSensorCalculationType = VirtualSensorCalculationType.OnChange;
                vSensorAvgA1.VirtualSensorDefinition.VirtualSensorDefinitionType = VirtualSensorDefinitionType.CSharpExpression;
                vSensorAvgA1.VirtualSensorDefinition.Definition = @"
using System;
using System.Linq;
using ValueManagement.DynamicCallback;

public class CSharpInteractiveClass
{
    static decimal Sum = 0;
    static decimal Count = 0;
    static public CallbackResultData VirtualValueCalculationCallback(CallbackPassInData passInData)
    {
        CallbackResultData result = new CallbackResultData();
		result.IsValueModified = true;
        Sum = Convert.ToDecimal(passInData.BaseValueDefinitionList.ElementAt(0).CurrentValue.Value) + Sum;
        Count++;
		result.NewValue = (Sum/Count).ToString();
		return (result);
    }
}";
                RegisterSensor(vSensorAvgA1);

                //The sensor must be dependent from the sensor sensor_A1.
                RegisterSensorDependency(sensorA1, vSensorAvgA1);

                //MINIMUM VALUE
                var vSensorMinA1 = CreateTestVirtualSensor(deviceA, "min1");
                vSensorMinA1.VirtualSensorDefinition.VirtualSensorCalculationType = VirtualSensorCalculationType.OnChange;
                vSensorMinA1.VirtualSensorDefinition.VirtualSensorDefinitionType = VirtualSensorDefinitionType.CSharpExpression;
                vSensorMinA1.VirtualSensorDefinition.Definition = @"
using System;
using System.Linq;
using ValueManagement.DynamicCallback;

public class CSharpInteractiveClass
{
    static public CallbackResultData VirtualValueCalculationCallback(CallbackPassInData passInData)
    {
        CallbackResultData result = new CallbackResultData();
        var newValue = Convert.ToDecimal(passInData.BaseValueDefinitionList.ElementAt(0).CurrentValue.Value);

        if (passInData.CurrentValue.Value == null)
           passInData.CurrentValue.Value = passInData.ValueDefinition.DefaultValue;

        if (newValue < Convert.ToDecimal(passInData.CurrentValue.Value)){
            result.NewValue = newValue;
            result.IsValueModified = true;
        }
        else{
            result.NewValue = passInData.CurrentValue;
            result.IsValueModified = false;
        }

		return (result);
    }
}";
                vSensorMinA1.DefaultValue = "100";
                RegisterSensor(vSensorMinA1);

                //The sensor must be dependent from the sensor sensor_A1.
                RegisterSensorDependency(sensorA1, vSensorMinA1);

                //MAXIMUM VALUE
                var vSensorMaxA1 = CreateTestVirtualSensor(deviceA, "max1");
                vSensorMaxA1.VirtualSensorDefinition.VirtualSensorCalculationType = VirtualSensorCalculationType.OnChange;
                vSensorMaxA1.VirtualSensorDefinition.VirtualSensorDefinitionType = VirtualSensorDefinitionType.CSharpExpression;
                vSensorMaxA1.VirtualSensorDefinition.Definition = @"
using System;
using System.Linq;
using ValueManagement.DynamicCallback;

public class CSharpInteractiveClass
{
    static public CallbackResultData VirtualValueCalculationCallback(CallbackPassInData passInData)
    {
        CallbackResultData result = new CallbackResultData();
        var newValue = Convert.ToDecimal(passInData.BaseValueDefinitionList.ElementAt(0).CurrentValue.Value);

        if (passInData.CurrentValue.Value == null)
           passInData.CurrentValue.Value = passInData.ValueDefinition.DefaultValue;

        if (newValue > Convert.ToDecimal(passInData.CurrentValue.Value)){
            result.NewValue = newValue;
            result.IsValueModified = true;
        }
        else{
            result.NewValue = passInData.CurrentValue;
            result.IsValueModified = false;
        }

		return (result);
    }
}";
                vSensorMaxA1.DefaultValue = "0";
                RegisterSensor(vSensorMaxA1);

                //The sensor must be dependent from the sensor sensor_A1.
                RegisterSensorDependency(sensorA1, vSensorMaxA1);

                Console.WriteLine("Test environmnt has been set up.");
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }

        private static Device CreateTestDevice(string name, int increment, string ipAddress)
        {
            var device = new Device();
            device.Id = name + ipAddress;
            device.Description = "Simulated test device: " + name;
            device.Location = new Location();
            device.Location.Name = "Test site";
            device.Location.Elevation = 100 + increment;
            device.Location.Latitude = 200 + increment;
            device.Location.Longitude = 300 + increment;
            device.DeviceIpEndPoint = ipAddress;

            return device;
        }

        private static Sensor CreateTestPushSensor(Device device, string name)
        {
            var result = new Sensor();
            result.Category = "Test push sensor";
            result.Description = "Test simulated sensor: " + name;
            result.DeviceId = device.Id;
            result.Id = name;
            result.IsVirtualSensor = false;
            result.PullFrequencyInSeconds = 10;
            result.PullModeCommunicationType = PullModeCommunicationType.CNDEP;
            result.PullModeDotNetObjectType = null;
            result.SensorDataRetrievalMode = SensorDataRetrievalMode.Push;
            result.SensorValueDataType = SensorValueDataType.Decimal;
            result.ShallSensorDataBePersisted = false;
            result.PersistDirectlyAfterChange = false;
            result.UnitSymbol = "U";
            result.VirtualSensorDefinition = null;
            result.DefaultValue = "0";

            return result;
        }

        private static Sensor CreateTestVirtualSensor(Device device, string name)
        {
            var result = new Sensor();
            result.Category = "Test virtual sensor";
            result.Description = "Test simulated virtual sensor: " + name;
            result.DeviceId = device.Id;
            result.Id = name;
            result.IsVirtualSensor = true;
            result.PullFrequencyInSeconds = 10;
            result.SensorDataRetrievalMode = SensorDataRetrievalMode.Push;
            result.SensorValueDataType = SensorValueDataType.Decimal;
            result.ShallSensorDataBePersisted = true;
            result.PersistDirectlyAfterChange = true;
            result.UnitSymbol = "U";
            result.VirtualSensorDefinition = new VirtualSensorDefinition();
            result.DefaultValue = "0";

            return result;
        }

        private static void RegisterDevice(Device device1)
        {
            if (!CentralServerServiceClient.Instance.Service.IsDeviceIdUsed(device1.Id))
            {
                var result = CentralServerServiceClient.Instance.Service.RegisterDevice(device1);
                Debug.Assert(result.Success, result.ErrorMessages);
                Debug.Assert(CentralServerServiceClient.Instance.Service.IsDeviceIdUsed(device1.Id));

                Console.WriteLine("Test device {0} successfully created.", device1.Id);
            }
            else
            {
                Console.WriteLine("Test device {0} already exists.", device1.Id);
            }
        }

        private static void RegisterSensor(Sensor sensor)
        {
            if (!CentralServerServiceClient.Instance.Service.IsSensorIdRegisteredForDevice(sensor.DeviceId, sensor.Id))
            {
                var result = CentralServerServiceClient.Instance.Service.RegisterSensors(new[] { sensor });
                Debug.Assert(result.Success, result.ErrorMessages);

                Debug.Assert(CentralServerServiceClient.Instance.Service.IsSensorIdRegisteredForDevice(sensor.DeviceId, sensor.Id));

                Console.WriteLine("Test sensor {0} of device {1} successfully created.", sensor.DeviceId, sensor.Id);
            }
            else
            {
                Console.WriteLine("Test sensor {0} of device {1} already exists.", sensor.DeviceId, sensor.Id);
            }
        }

        private static void RegisterSensorDependency(Sensor baseSensor, Sensor dependentSensor)
        {
            var baseSensorRetrieved = CentralServerServiceClient.Instance.Service.GetSensor(baseSensor.DeviceId, baseSensor.Id);
            var dependentSensorRetrieved = CentralServerServiceClient.Instance.Service.GetSensor(dependentSensor.DeviceId, dependentSensor.Id);

            var dependentSensors =
                CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(baseSensorRetrieved.Sensor.InternalSensorId);
            if (dependentSensors.Exists(x => x.Id == dependentSensor.Id && x.DeviceId == dependentSensor.DeviceId))
            {
                Console.WriteLine("Dependency between sensors {0} and {1} already exists.", baseSensor.Id, dependentSensor.Id);
            }
            else
            {
                CentralServerServiceClient.Instance.Service.AddSensorDependency(baseSensorRetrieved.Sensor.InternalSensorId,
                                                                                dependentSensorRetrieved.Sensor.InternalSensorId);
                Console.WriteLine("Dependency between sensors {0} and {1} successfully created.", baseSensor.Id, dependentSensor.Id);

                Debug.Assert(
                    CentralServerServiceClient.Instance.Service.GetDependentSensorDependencies(baseSensorRetrieved.Sensor.InternalSensorId)
                                              .Exists(x => x.Id == dependentSensor.Id && x.DeviceId == dependentSensor.DeviceId));
            }
        }

        private static void GenerateVirtualSensorTestData()
        {
            SensorData[] sampleData = new SensorData[10];
            Random random = new Random();
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = random.Next(1, 100).ToString(), CorrelationId = Guid.NewGuid().ToString() + Guid.NewGuid().ToString()};
            }

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = "sensor_A1", Measures = sampleData } }, DeviceId = "A" + ConfigurationManager.AppSettings["DeviceIpAddress"] };
            //StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = "3", Measures = sampleData } }, DeviceId = "192.168.1.5-1" };
            Debug.Assert(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
                CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Debug.Assert(responseObject != null);
            Debug.Assert(responseObject is OperationResult);
            Debug.Assert(((OperationResult)responseObject).Success, ((OperationResult)responseObject).ErrorMessages);

            Console.WriteLine("{0} data entries has been written for the sensor {1} von device {2}", sampleData.Length, "sensor_A1", request.DeviceId);
            //Console.WriteLine("{0} data entries has been written for the sensor {1} von device {2}", sampleData.Length, "3", "192.168.1.5-1");
        }

        private static void GenerateActuatorTestData()
        {
            SensorData[] sampleData = new SensorData[10];
            Random random = new Random();
            for (int i = 0; i < sampleData.Length; i++)
            {
                sampleData[i] = new SensorData() { GeneratedWhen = DateTime.Now.Subtract(new TimeSpan(0, 0, i)), Value = random.Next(1, 100).ToString(),
                    CorrelationId = i.ToString()};
            }

            //Store
            object responseObject;
            OperationResult resultTemplate = new OperationResult();

            //StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = "sensor_A1", Measures = sampleData } }, DeviceId = "A192.168.1.1" };
            StoreSensorDataRequest request = new StoreSensorDataRequest() { Data = new MultipleSensorData[] { new MultipleSensorData() { SensorId = "actuator", Measures = sampleData } }, DeviceId = "SimulatorDevice_1" };
            Debug.Assert(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
                CndepContentType, resultTemplate, out responseObject), responseObject.ToString());
            Debug.Assert(responseObject != null);
            Debug.Assert(responseObject is OperationResult);
            Debug.Assert(((OperationResult)responseObject).Success, ((OperationResult)responseObject).ErrorMessages);

            //Console.WriteLine("{0} data entries has been written for the sensor {1} von device {2}", sampleData.Length, "sensor_A1", "A192.168.1.1");
            Console.WriteLine("{0} data entries has been written for the sensor {1} von device {2}", sampleData.Length, "actuator", "localhost-1");
        }

        private static bool SendCndepRequest(string serverAddress, int serverPort, byte command, byte function, string requestData, string contentType, object resultObjectTemplate, out object result, params object[] parameters)
        {
            try
            {
                using (CndepUdpClient udpClient = new CndepUdpClient(serverPort, Dns.GetHostAddresses(serverAddress)[0]))
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
                result = String.Format("UDP CNDEP request failed. Error: {0}", exc.Message);
                return false;
            }
        }

        private static byte mSessionId;
        private static byte GetSessionId()
        {
            //lock (this)
            //{
                return mSessionId++;
            //}
        }
    }
}
