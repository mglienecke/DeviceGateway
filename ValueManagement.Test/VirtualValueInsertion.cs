using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using GlobalDataContracts;

using ValueManagement.DynamicCallback;
using System.Reflection;
using CentralServerService;

namespace ValueManagement.Test
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für UnitTest1
    /// </summary>
    //[TestClass]
    public class VirtualValueInsertion
    {
        public VirtualValueInsertion()
        {
            //
            // TODO: Konstruktorlogik hier hinzufügen
            //
        }

        private TestContext testContextInstance;

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
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Verwenden Sie ClassCleanup, um nach Ausführung aller Tests in einer Klasse Code auszuführen.
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen. 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        /// <summary>
        /// Initialize the assembly and the remote client. If there is no remote connection then initialize Log4Net as well
        /// </summary>
        /// <param name="context">The context.</param>
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            CentralServerServiceClient.UseRemoting = true;
        }
        #endregion

        //[TestMethod]
        public void InsertVirtualValue()
        {
            //Create dummy device for virtual sensors
            Device device = new Device();
            device.Description = "Hosting device for virtual sensors";
            device.DeviceIpEndPoint = "127.0.0.1";
            device.Id = "VirtualSensorDevice";
            device.Location = new Location();
            device.Location.Elevation = 0;
            device.Location.Latitude = 0;
            device.Location.Longitude = 0;
            device.Location.Name = "";

            if (CentralServerServiceClient.Instance.Service.IsDeviceIdUsed(device.Id) == false){
                CentralServerServiceClient.Instance.Service.RegisterDevice(device);
            }

            //Create a virtual sensor for calculcating averages
            Sensor sensor = new Sensor();
            sensor.Category = "virtual";
            sensor.Description = "Calculates average";
            sensor.DeviceId = device.Id;
            sensor.Id = "average";
            sensor.IsVirtualSensor = true;
            sensor.SensorValueDataType = SensorValueDataType.Decimal;
            sensor.ShallSensorDataBePersisted = true;
            sensor.VirtualSensorDefinition = new VirtualSensorDefinition();
            sensor.UnitSymbol = "";

            sensor.VirtualSensorDefinition.VirtualSensorCalculationType = VirtualSensorCalculationType.OnChange;
            sensor.VirtualSensorDefinition.VirtualSensorDefinitionType = VirtualSensorDefinitionType.DotNetObject;
            sensor.VirtualSensorDefinition.Definition = typeof(AverageCalculationCallback).AssemblyQualifiedName;

            if (CentralServerServiceClient.Instance.Service.IsSensorIdRegisteredForDevice(sensor.DeviceId, sensor.Id) == false)
            {
                CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[]{sensor});
            }
        }
    }
}
