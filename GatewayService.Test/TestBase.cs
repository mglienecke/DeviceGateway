using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalDataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GatewayServiceTest
{
    public class TestBase
    {
        internal const string TestDescription = "Test Device";

        internal static int Counter = 0;

        #region Private methods...
        internal void AssertSuccess(OperationResult result)
        {
            //Assert.IsTrue(result.ErrorMessages == null, result.ErrorMessages);
            Assert.IsTrue(result.Success);
        }

        internal void AssertFailure(OperationResult result)
        {
            Assert.IsFalse(String.IsNullOrEmpty(result.ErrorMessages), result.ErrorMessages);
            Assert.IsFalse(result.Success, result.ErrorMessages);
        }

        internal void AssertAreEqual(Sensor expected, Sensor received)
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
            //Assert.AreEqual(expected.InternalSensorId, received.InternalSensorId);
            Assert.AreEqual(expected.IsVirtualSensor, received.IsVirtualSensor);
            Assert.AreEqual(expected.PersistDirectlyAfterChange, received.PersistDirectlyAfterChange);
            Assert.AreEqual(expected.PullFrequencyInSeconds, received.PullFrequencyInSeconds);
            Assert.AreEqual(expected.PullModeCommunicationType, received.PullModeCommunicationType);
            Assert.AreEqual(expected.PushModeCommunicationType, received.PushModeCommunicationType);
            Assert.AreEqual(expected.PullModeDotNetObjectType, received.PullModeDotNetObjectType);
            Assert.AreEqual(expected.SensorDataRetrievalMode, received.SensorDataRetrievalMode);
            Assert.AreEqual(expected.SensorValueDataType, received.SensorValueDataType);
            Assert.AreEqual(expected.ShallSensorDataBePersisted, received.ShallSensorDataBePersisted);
            Assert.AreEqual(expected.UnitSymbol, received.UnitSymbol);
            AssertAreEqual(expected.VirtualSensorDefinition, received.VirtualSensorDefinition);
        }

        internal void AssertAreEqual(Device expected, Device received)
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

        internal void AssertAreEqual(Location expected, Location received)
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

        internal void AssertAreEqual(VirtualSensorDefinition expected, VirtualSensorDefinition received)
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

        internal Device CreateSampleDevice()
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

        internal Sensor CreateSampleSensor(Device device)
        {
            Sensor result = new Sensor();
            result.Category = "C" + Counter++;
            result.Description = "Desc" + Counter++;
            result.DeviceId = device == null ? null : device.Id;
            result.Id = Guid.NewGuid().ToString();
            result.InternalSensorId = 0;
            result.IsVirtualSensor = false;// Counter++ % 2 == 0;
            result.PullFrequencyInSeconds = Counter++;
            result.PullModeCommunicationType = (PullModeCommunicationType)Enum.GetValues(typeof(PullModeCommunicationType)).GetValue(Counter++ % Enum.GetValues(typeof(PullModeCommunicationType)).Length);
            result.PullModeDotNetObjectType = "Type" + Counter++;
            result.SensorDataRetrievalMode = (SensorDataRetrievalMode)Enum.GetValues(typeof(SensorDataRetrievalMode)).GetValue(Counter++ % Enum.GetValues(typeof(SensorDataRetrievalMode)).Length);
            result.SensorValueDataType = (SensorValueDataType)Enum.GetValues(typeof(SensorValueDataType)).GetValue(Counter++ % Enum.GetValues(typeof(SensorValueDataType)).Length);
            result.ShallSensorDataBePersisted = true;//Counter++ % 2 == 0; 
            result.PersistDirectlyAfterChange = Counter++ % 2 == 0;
            result.UnitSymbol = "U" + Counter++;
            result.IsSynchronousPushToActuator = Counter++ % 2 == 0;
            result.IsActuator = false;// Counter++ % 2 == 0;

            if (result.IsVirtualSensor)
                result.VirtualSensorDefinition = CreateSampleVirtualSensorDefinition();

            return result;
        }

        internal Sensor CreateSampleSensor(Device device, bool withVirtualDefinition)
        {
            Sensor result = new Sensor();
            result.IsVirtualSensor = withVirtualDefinition;
            if (result.IsVirtualSensor)
                result.VirtualSensorDefinition = CreateSampleVirtualSensorDefinition();

            return result;
        }

        internal VirtualSensorDefinition CreateSampleVirtualSensorDefinition()
        {
            VirtualSensorDefinition definition = new VirtualSensorDefinition();
            definition.Definition = String.Format(
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
    callbackResult.NewValue = GlobalDataContracts.SensorData(str(int(callbackPassIn.CurrentValue.Value) * {0}))
    return(callbackResult);", 
                            //Counter++);
                            1);

            definition.VirtualSensorCalculationType = VirtualSensorCalculationType.OnChange;
            definition.VirtualSensorDefinitionType = VirtualSensorDefinitionType.IronPhyton;

            return definition;
        }
        #endregion
    }
}
