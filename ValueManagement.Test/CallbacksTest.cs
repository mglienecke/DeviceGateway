using System;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GlobalDataContracts;
using ValueManagement.DynamicCallback;

namespace ValueManagement.Test
{
    /// <summary>
    /// Test cases for callbacks.
    /// </summary>
    [TestClass]
    public class CallbacksTest
    {
        #region Initialization methods...
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [TestCleanup]
        public void CleanUp()
        {
            Utilities.ClearAllValues();
            Utilities.ClearAllCallbacks();
            Utilities.CalledLevelList.Clear();

            ValueManager.UnsavedValueDict.Clear();
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            log.Debug("Tests initialized");

            try
            {
                Console.WriteLine("Snapshot taken");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Got exception: " + ex.ToString());
            }
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            try
            {
                Console.WriteLine("Snapshot taken");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Got exception: " + ex.ToString());
            }

            log.Debug("Tests terminated");
        }
        #endregion

        #region AddCallbackForValueDefinition...
        [TestMethod]
        public void AddCallbackForValueDefinition()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CompiledMethodCallback("A", DynamicCallback.CallbackType.GeneralCheck, typeof(MultiplyBy3Callback)));

            // assign using implicit conversion
            baseDef.CurrentValue = "100";

            Assert.IsTrue(baseDef.CurrentValue == "300");
        }

        [TestMethod]
        public void AddCallbackForValueDefinition_SymbolicName()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.CallbackDictionary["A"] = new DynamicCallback.CompiledMethodCallback("A", DynamicCallback.CallbackType.GeneralCheck, typeof(MultiplyBy3Callback));
            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, "A");

            // assign using implicit conversion
            baseDef.CurrentValue = "200";

            Assert.IsTrue(baseDef.CurrentValue == "600");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddCallbackForValueDefinition_NullDefinition_NonNullCallback()
        {
            ValueManager.Instance.AddCallbackForValueDefinition(null, new DynamicCallback.CompiledMethodCallback("A", DynamicCallback.CallbackType.GeneralCheck, typeof(ValueCallback)));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddCallbackForValueDefinition_NullDefinition_NonNullSymbolicName()
        {
            ValueManager.Instance.AddCallbackForValueDefinition(null, "A");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddCallbackForValueDefinition_NullCallback()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, (AbstractCallback)null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddCallbackForValueDefinition_NullCallbackSymbolicName()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, (string)null);
        }

        [TestMethod]
        public void AddCallbackForValueDefinition_DuplicateCallback()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CompiledMethodCallback("A", DynamicCallback.CallbackType.GeneralCheck, typeof(ValueCallback)));
            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CompiledMethodCallback("A", DynamicCallback.CallbackType.GeneralCheck, typeof(ValueCallback)));

            // assign using implicit conversion
            baseDef.CurrentValue = "100.12";

            // as we were adding the same callback (name) only one instance was called
            Assert.IsTrue(Utilities.CalledLevelList.Count == 1);
            Assert.IsTrue(Utilities.CalledLevelList[0] == ValueManagement.DynamicCallback.CallbackType.GeneralCheck);
        }

        [TestMethod]
        public void AddCallbackForValueDefinition_ConsequentCallbacks()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CompiledMethodCallback("A", DynamicCallback.CallbackType.GeneralCheck, typeof(MultiplyBy3Callback)));
            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CompiledMethodCallback("B", DynamicCallback.CallbackType.GeneralCheck, typeof(MultiplyBy3Callback)));

            // assign using implicit conversion
            baseDef.CurrentValue = "100";

            // as we were adding the same callback (name) only one instance was called
            Assert.IsTrue(baseDef.CurrentValue == "900");
        }
        #endregion

        #region Software Agents callbacks...

        [TestMethod]
        public void AddCallbackForValueDefinition_SoftwareAgentGraphicDesign_GeneralCheck()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.SoftwareAgentCallback("A", DynamicCallback.CallbackType.GeneralCheck, typeof(MultiplyBy3GraphicActivity)));

            // assign using implicit conversion
            baseDef.CurrentValue = "150";

            Assert.IsTrue(baseDef.CurrentValue == "450");
        }

        [TestMethod]
        public void AddCallbackForValueDefinition_SoftwareAgentCodedActivity_GeneralCheck()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.SoftwareAgentCallback("A", DynamicCallback.CallbackType.GeneralCheck, typeof(MultiplyBy3CodeActivity)));

            // assign using implicit conversion
            baseDef.CurrentValue = "150";

            Assert.IsTrue(baseDef.CurrentValue == "450");
        }


        [TestMethod]
        public void AddCallbackForValueDefinition_SoftwareAgentCodedActivity_AfterChangeCallback()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.SoftwareAgentCallback("A", CallbackType.AfterChangeCallback, typeof(MultiplyBy3CodeActivity)));

            // assign using implicit conversion
            baseDef.CurrentValue = "150";

            // as this is an after change callback the value may not have changed, despite the workflow modifying it internally (as it's the same used for the GeneralCheck)
            Assert.IsTrue(baseDef.CurrentValue == "150");
        }

        #endregion

        #region Compiled method callbacks...
        [TestMethod]
        public void AddCallbackForValueDefinition_CompiledMethod()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CompiledMethodCallback("A", DynamicCallback.CallbackType.GeneralCheck, typeof(MultiplyBy3Callback)));

            // assign using implicit conversion
            baseDef.CurrentValue = "150";

            Assert.IsTrue(baseDef.CurrentValue == "450");
        }

        /// <summary>
        /// Try to add an invalid lambda as well as an invalid method as as callbacks 
        /// </summary>
        [TestMethod]
        public void AddCallbackForValueDefinition_CompiledMethod_Invalid()
        {
            // this lambda takes the right type and returns the right type but doesn't implement the interface correctly
            Func<DynamicCallback.CallbackPassInData, DynamicCallback.CallbackResultData> callback = (DynamicCallback.CallbackPassInData inData) =>
            {
                DynamicCallback.CallbackResultData outData = new DynamicCallback.CallbackResultData();

                return (outData);
            };

            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            try
            {
                ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CompiledMethodCallback("A", DynamicCallback.CallbackType.GeneralCheck, callback.GetType()));
                Assert.Fail("illegal type not detected");
            }
            catch (InvalidOperationException)
            {
                ;
            }
            try
            {
                ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CompiledMethodCallback("B", DynamicCallback.CallbackType.GeneralCheck, typeof(InvalidValueCallback)));
                Assert.Fail("illegal type not detected");
            }
            catch (InvalidOperationException)
            {
                ;
            }

            // assign using implicit conversion
            baseDef.CurrentValue = "100.12";
            Assert.IsTrue(Utilities.CalledLevelList.Count == 0);
        }
        #endregion

        #region C#-interactive callbacks...
        [TestMethod]
        public void AddCallbackForValueDefinition_CSharpInteractive()
        {
            string executionExpression =
@"
using System;
using ValueManagement.DynamicCallback;

public class CSharpInteractiveClass
{
    static public CallbackResultData AdjustSampleRateCallback(CallbackPassInData passInData)
    {
        CallbackResultData result = new CallbackResultData();
		result.IsValueModified = true;
		result.NewValue = passInData.CurrentValue * 5;
		return (result);
    }
}
";

            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CSharpInteractiveCallback("A", DynamicCallback.CallbackType.AdjustSampleRate, executionExpression));

            // assign using implicit conversion
            baseDef.CurrentValue = "50";

            Assert.IsTrue(baseDef.CurrentValue == "250");
        }

        [DeploymentItem("GeneralTriggerMultiplyBy3Script.cs")]
        [TestMethod]
        public void AddCallbackForValueDefinition_CSharpInteractive_File()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CSharpInteractiveCallback("A", DynamicCallback.CallbackType.AdjustSampleRate, null, AppDomain.CurrentDomain.BaseDirectory + "\\" + "GeneralTriggerMultiplyBy3Script.cs"));

            // assign using implicit conversion
            baseDef.CurrentValue = "50";

            Assert.IsTrue(baseDef.CurrentValue == "250");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddCallbackForValueDefinition_CSharpInteractive_Invalid()
        {
            string executionExpression =
@"
using System;
using ValueManagement.DynamicCallback;

public class CSharpInteractiveClass
{
    static public CallbackResultData AdjustSampleRateCallback(CallbackPassInData passInData)
    {
        CallbackResultData result = new Callback_ResultData(); //Invalid class name here!
		result.IsValueModified = true;
		result.NewValue = passInData.CurrentValue * 5;
		return (result);
    }
}
";

            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CSharpInteractiveCallback("A", DynamicCallback.CallbackType.AdjustSampleRate, executionExpression));

            // assign using implicit conversion
            baseDef.CurrentValue = "10";

            Assert.IsTrue(baseDef.CurrentValue == "50");
        }
        #endregion

        #region F#-interactive callbacks...
        [TestMethod]
        public void AddCallbackForValueDefinition_FSharpInteractive()
        {
            string executionExpression =
@"
module public Program

open ValueManagement.DynamicCallback
open GlobalDataContracts
open System

type public FSharpInteractiveClass() =
  static member public DetectAnomalyCallback(data: CallbackPassInData) =
    let result = new CallbackResultData()
    result.IsValueModified <- true
    result.NewValue <- new SensorData()
    result.NewValue.Value <- Convert.ToString(Convert.ToInt32(data.CurrentValue.Value) * 2)
    result
";

            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.FSharpInteractiveCallback("Fs", DynamicCallback.CallbackType.DetectAnomaly, executionExpression));

            // assign using implicit conversion
            baseDef.CurrentValue = "50";

            Assert.IsTrue(baseDef.CurrentValue == "100");
        }

        [DeploymentItem("GeneralTriggerMultiplyBy3Script.fs")]
        [TestMethod]
        public void AddCallbackForValueDefinition_FSharpInteractive_File()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.FSharpInteractiveCallback("Fs", DynamicCallback.CallbackType.DetectAnomaly, null, AppDomain.CurrentDomain.BaseDirectory + "\\" + "GeneralTriggerMultiplyBy3Script.fs"));

            // assign using implicit conversion
            baseDef.CurrentValue = "50";

            Assert.IsTrue(baseDef.CurrentValue == "100");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddCallbackForValueDefinition_FSharpInteractive_Invalid()
        {
            string executionExpression =
@"
module public Program

open ValueManagement.DynamicCallback
open GlobalDataContracts
open System

//Incorrect method name
type public FSharpInteractiveClass() =
//Incorrect method name for the case
  static member public GeneralCheckCallback(data: CallbackPassInData) =
    let result = new CallbackResultData()
    result.IsValueModified <- true
    result.NewValue <- new SensorData()
    result.NewValue.Value <- Convert.ToString(Convert.ToInt32(data.CurrentValue.Value) * 2)
    result
";

            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.FSharpInteractiveCallback("Fs", DynamicCallback.CallbackType.DetectAnomaly, executionExpression));

            // assign using implicit conversion
            baseDef.CurrentValue = "10";

            Assert.IsTrue(baseDef.CurrentValue == "20");
        }
        #endregion

        #region Python script callbacks...
        [TestMethod]
        public void AddCallbackForValueDefinition_PythonScript()
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
def GeneralCheckCallback(callbackPassIn):
    callbackResult = CallbackResultData()
    callbackResult.IsValueModified = True
    callbackResult.NewValue = GlobalDataContracts.SensorData(str(int(callbackPassIn.CurrentValue.Value) * 3))
    return(callbackResult);";

            int startId = Utilities.CreateDefinitions(1);
            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.PythonCallback("P", DynamicCallback.CallbackType.GeneralCheck, executionExpression));

            // assign using implicit conversion
            baseDef.CurrentValue = "15";

            Assert.IsTrue(baseDef.CurrentValue == "45");
        }

        [DeploymentItem("GeneralTriggerMultiplyBy3Script.py")]
        [TestMethod]
        public void AddCallbackForValueDefinition_PythonScript_File()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.PythonCallback("A", DynamicCallback.CallbackType.GeneralCheck, null, AppDomain.CurrentDomain.BaseDirectory + "\\" + "GeneralTriggerMultiplyBy3Script.py"));

            // assign using implicit conversion
            baseDef.CurrentValue = "1000";

            Assert.IsTrue(((int)baseDef.CurrentValue) == 3000);
        }

        [TestMethod]
        //[ExpectedException(typeof(InvalidOperationException))]
        public void AddCallbackForValueDefinition_PythonScript_Invalid()
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
def GeneralCheckCallback(callbackPassIn):
    callbackResult = CallbackResultData()
    callbackResult.IsValueModified = True
    callbackResult.NewValue = GlobalData_Contracts.SensorData(str(int(callbackPassIn.CurrentValue.Value) * 3)) # INVALID NAMESPACE NAME
    return(callbackResult);";

            int startId = Utilities.CreateDefinitions(1);
            ValueDefinition baseDef = ValueManager.Instance[startId];

            //No exception here since the script gets interpreted, not compiled
            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.PythonCallback("P", DynamicCallback.CallbackType.GeneralCheck, executionExpression));

            // assign using implicit conversion
            baseDef.CurrentValue = "15";

            //Ok, the value stays the same, since teh script executes with an error
            Assert.IsTrue(baseDef.CurrentValue.Value == null);
        }

        [TestMethod]
        public void AddCallbackForValueDefinition_PythonScript_UsingHistoricValuesOfBaseValueDefinition()
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


            int idBase = Utilities.GetNextInternalId();
            int idDependent = Utilities.GetNextInternalId();
            ValueDefinition defBase = new ValueDefinition(idBase, idBase.ToString());
            defBase.DefaultValue = "0";
            defBase.ValueTypeCode = TypeCode.Int32;
            defBase.MaxHistoricValues = 20;
            VirtualValueDefinition defAverage = new VirtualValueDefinition(idDependent, idDependent.ToString(), VirtualSensorCalculationType.OnChange);
            defAverage.MaxHistoricValues = 20;
            defAverage.DefaultValue = "0";
            defAverage.ValueTypeCode = TypeCode.Decimal;

            ValueManager.Instance.AddValueDefinition(defBase);
            ValueManager.Instance.AddValueDefinition(defAverage);

            defBase = ValueManager.Instance[idBase];
            defAverage = ValueManager.Instance[idDependent] as VirtualValueDefinition;

            AbstractCallback callback = new DynamicCallback.PythonCallback("RunningMean", DynamicCallback.CallbackType.VirtualValueCalculation, executionExpression);
            ValueManager.Instance.RegisterCallback(callback);
            defAverage.VirtualValueEvaluationCallback = callback.SymbolicName;
            ValueManager.Instance.AddDependency(defBase, defAverage);


            // assign using implicit conversion
            for (int i = 10; i < 30; i++)
                defBase.CurrentValue = i.ToString();

            Assert.IsTrue(Convert.ToDouble(defAverage.CurrentValue, CultureInfo.InvariantCulture) == 19.5d);
        }

        [TestMethod]
        public void AddCallbackForValueDefinition_PythonScript_UsingOtherValueDefinitions()
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
    sum = int(callbackPassIn.GetValueDefinition({0}).CurrentValue.Value) + int(callbackPassIn.GetValueDefinition({1}).CurrentValue.Value)
    callbackResult.NewValue = GlobalDataContracts.SensorData(sum)
    return(callbackResult);";


            int idA = Utilities.GetNextInternalId();
            int idB = Utilities.GetNextInternalId();
            int idSum = Utilities.GetNextInternalId();

            //Set the ids
            executionExpression = String.Format(executionExpression, idA, idB);

            ValueDefinition defA = new ValueDefinition(idA, idA.ToString());
            defA.DefaultValue = "0";
            defA.ValueTypeCode = TypeCode.Int32;
            ValueDefinition defB = new ValueDefinition(idB, idB.ToString());
            defB.DefaultValue = "0";
            defB.ValueTypeCode = TypeCode.Int32;

            VirtualValueDefinition defSum = new VirtualValueDefinition(idSum, idSum.ToString(), VirtualSensorCalculationType.OnChange);
            defSum.MaxHistoricValues = 20;
            defSum.DefaultValue = "0";
            defSum.ValueTypeCode = TypeCode.Decimal;

            ValueManager.Instance.AddValueDefinition(defA);
            ValueManager.Instance.AddValueDefinition(defB);
            ValueManager.Instance.AddValueDefinition(defSum);

            defA = ValueManager.Instance[idA];
            defB = ValueManager.Instance[idB];
            defSum = ValueManager.Instance[idSum] as VirtualValueDefinition;

            AbstractCallback callback = new DynamicCallback.PythonCallback("A+B", DynamicCallback.CallbackType.VirtualValueCalculation, executionExpression);
            ValueManager.Instance.RegisterCallback(callback);
            defSum.VirtualValueEvaluationCallback = callback.SymbolicName;
            ValueManager.Instance.AddDependency(defA, defSum);
            ValueManager.Instance.AddDependency(defB, defSum);

            defA.CurrentValue = 10;
            Assert.IsTrue(defSum.CurrentValue == 10);

            defB.CurrentValue = 11;
            Assert.IsTrue(defSum.CurrentValue == 21);

            defA.CurrentValue = 31;
            Assert.IsTrue(defSum.CurrentValue == 42);
        }
        #endregion

        #region Ruby script callbacks...
        [TestMethod]
        public void AddCallbackForValueDefinition_RubyScript()
        {
            string executionExpression =
 @"
class System::Object
  def initialize
  end
end

require '\ValueManagement.dll'
require 'GlobalDataContracts.dll'

# callback for general check
def GeneralCheckCallback(callbackPassIn)
    callbackResult = ValueManagement::DynamicCallback::CallbackResultData.new
    callbackResult.IsValueModified = true
    callbackResult.NewValue = GlobalDataContracts::SensorData.new String(Integer(callbackPassIn.CurrentValue.Value) * 3)
    
    return(callbackResult);
end";

            int startId = Utilities.CreateDefinitions(1);
            ValueDefinition baseDef = ValueManager.Instance[startId];

            //No exception here since the script gets interpreted, not compiled
            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.RubyCallback("RB", DynamicCallback.CallbackType.GeneralCheck, executionExpression));

            // assign using implicit conversion
            baseDef.CurrentValue = "15";

            //Ok, the value stays the same, since teh script executes with an error
            Assert.IsTrue(baseDef.CurrentValue == "45");
        }

        [DeploymentItem("GeneralTriggerMultiplyBy3Script.rb")]
        [TestMethod]
        public void AddCallbackForValueDefinition_RubyScript_File()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.RubyCallback("A", DynamicCallback.CallbackType.GeneralCheck, null, AppDomain.CurrentDomain.BaseDirectory + "\\" + "GeneralTriggerMultiplyBy3Script.rb"));

            // assign using implicit conversion
            baseDef.CurrentValue = "2000";

            Assert.IsTrue(((int)baseDef.CurrentValue) == 6000);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddCallbackForValueDefinition_RubyScript_Invalid()
        {
            string executionExpression =
@"
require '\ValueManagement.dll'
require 'GlobalDataContracts.dll'

# callback for general check
 def DetectAnomalyCallback(callbackPassIn)
    callbackResult = ValueManagement::DynamicCallback::CallbackResultData.new
    callbackResult.IsValueModified = true
    callbackResult.NewValue = GlobalDataContracts::SensorData.new String(Integer(callbackPassIn.CurrentValue.Value) * 3)
    
    return(callbackResult);
 end";

            int startId = Utilities.CreateDefinitions(1);
            ValueDefinition baseDef = ValueManager.Instance[startId];

            //No exception here since the script gets interpreted, not compiled
            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.RubyCallback("RB", DynamicCallback.CallbackType.GeneralCheck, executionExpression));

            // assign using implicit conversion
            baseDef.CurrentValue = "15";

            //Ok, the value stays the same, since the script executes with an error
            Assert.IsTrue(baseDef.CurrentValue == "15");
        }
        #endregion

        #region SQL callbacks...
        [TestMethod]
        public void AddCallbackForValueDefinition_SQL()
        {
            SqlInteractiveCallback callback = new DynamicCallback.SqlInteractiveCallback("A", DynamicCallback.CallbackType.GeneralCheck,
                ValueManager.Instance.DatabaseConnectionSettingsForCallbacks,
                "SELECT 3 * CONVERT(decimal, @current_value) return_value, 1 is_modified");

            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, callback);

            // assign using implicit conversion
            baseDef.CurrentValue = "50";

            Assert.IsTrue(baseDef.CurrentValue == "150");
        }

        [TestMethod]
        public void AddCallbackForVirtualValue_InteractiveSQL()
        {
            int startId = Utilities.CreateDefinitions(1, true, VirtualSensorCalculationType.OnRequest);
            ValueDefinition baseDef = ValueManager.Instance[startId];


            // register the callback - here a simple single result is enough
            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.SqlInteractiveCallback("SqlVirtualEvalCallback", DynamicCallback.CallbackType.VirtualValueCalculation,
                ValueManager.Instance.DatabaseConnectionSettingsForCallbacks, "SELECT 100"));

            // and set as the handler for evaluation
            (baseDef as VirtualValueDefinition).VirtualValueEvaluationCallback = "SqlVirtualEvalCallback";

            Assert.IsTrue(baseDef.CurrentValue == "100");
        }

        [TestMethod]
        public void AddCallbackForVirtualValueAssumeNull_InteractiveSQL()
        {
            int startId = Utilities.CreateDefinitions(1, true, VirtualSensorCalculationType.OnRequest);
            ValueDefinition baseDef = ValueManager.Instance[startId];


            // register the callback - here a simple single result is enough
            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.SqlInteractiveCallback("SqlVirtualEvalCallback", DynamicCallback.CallbackType.VirtualValueCalculation,
                ValueManager.Instance.DatabaseConnectionSettingsForCallbacks, "SELECT NULL"));

            // and set as the handler for evaluation
            (baseDef as VirtualValueDefinition).VirtualValueEvaluationCallback = "SqlVirtualEvalCallback";

            Assert.IsTrue(baseDef.CurrentValue == string.Empty);
        }

        [TestMethod]
        public void AddCallbackForVirtualValue_StoredProcSQL()
        {
            int startId = Utilities.CreateDefinitions(1, true, VirtualSensorCalculationType.OnRequest);
            ValueDefinition baseDef = ValueManager.Instance[startId];


            // register the callback
            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.SqlProcedureCallback("SqlVirtualEvalCallback", DynamicCallback.CallbackType.VirtualValueCalculation,
                ValueManager.Instance.DatabaseConnectionSettingsForCallbacks.ConnectionString, "[Dynamic].[CalcVirtualValueDemo]"));

            // and set as the handler for evaluation
            (baseDef as VirtualValueDefinition).VirtualValueEvaluationCallback = "SqlVirtualEvalCallback";

            Assert.IsTrue(baseDef.CurrentValue == "100");
        }

        [TestMethod]
        public void AddCallbackForVirtualValueAssumeNull_StoredProcSQL()
        {
            int startId = Utilities.CreateDefinitions(1, true, VirtualSensorCalculationType.OnRequest);
            ValueDefinition baseDef = ValueManager.Instance[startId];


            // register the callback
            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.SqlProcedureCallback("SqlVirtualEvalCallback", DynamicCallback.CallbackType.VirtualValueCalculation,
                ValueManager.Instance.DatabaseConnectionSettingsForCallbacks.ConnectionString, "[Dynamic].[CalcVirtualValueDemoNullResult]"));

            // and set as the handler for evaluation
            (baseDef as VirtualValueDefinition).VirtualValueEvaluationCallback = "SqlVirtualEvalCallback";

            Assert.IsTrue(baseDef.CurrentValue == string.Empty);
        }

        [TestMethod]
        public void AddCallbackForValueDefinition_SQL_Invalid()
        {
            SqlInteractiveCallback callback = new DynamicCallback.SqlInteractiveCallback("A", DynamicCallback.CallbackType.GeneralCheck,
                ValueManager.Instance.DatabaseConnectionSettingsForCallbacks,
                "S_LECT 3 * CONVERT(decimal, @current_value) return_value, 1 is_modified");

            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, callback);

            // assign using implicit conversion
            baseDef.CurrentValue = "50";

            Assert.IsTrue(baseDef.CurrentValue.Value == null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddCallbackForValueDefinition_SQL_InvalidConnectionString()
        {
            SqlInteractiveCallback callback = new DynamicCallback.SqlInteractiveCallback("A", DynamicCallback.CallbackType.GeneralCheck,
                ValueManager.Instance.DatabaseConnectionSettingsForCallbacks.ConnectionString + "INVALID",
                "SELECT 3 * CONVERT(decimal, @current_value) return_value, 1 is_modified");

            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, callback);

            // assign using implicit conversion
            baseDef.CurrentValue = "50";

            Assert.IsTrue(baseDef.CurrentValue == null);
        }
        #endregion

        [TestMethod]
        public void Callback_AdjustSamplingRate()
        {
            string executionExpression =
@"
using System;
using ValueManagement.DynamicCallback;

public class CSharpInteractiveClass
{
    static public CallbackResultData AdjustSampleRateCallback(CallbackPassInData passInData)
    {
        CallbackResultData result = new CallbackResultData();
		result.IsValueModified = false;
        if (Int32.Parse(passInData.CurrentValue.Value) > 100){
            result.SampleRateNeedsAdjustment = true;
            result.NewSampleRate = new TimeSpan(1, 0, 1);   
        }
		return (result);
    }
}
";
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CSharpInteractiveCallback("A", DynamicCallback.CallbackType.AdjustSampleRate, executionExpression));

            //No adjustment
            baseDef.CurrentValue = "50";
            Assert.IsFalse(ValueManager.Instance.AdjustedSamplingRates.ContainsKey(baseDef.InternalId));

            //With adjustment
            baseDef.CurrentValue = "250";

            Assert.IsTrue(ValueManager.Instance.AdjustedSamplingRates.ContainsKey(baseDef.InternalId));
            Assert.AreEqual(new TimeSpan(1, 0, 1), ValueManager.Instance.AdjustedSamplingRates[baseDef.InternalId]);
        }

        // this will deploy the file as well
        [DeploymentItem("SimpleScript.py")]
        [TestMethod]
        public void AddScriptCallback()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.PythonCallback("A", DynamicCallback.CallbackType.GeneralCheck, null, AppDomain.CurrentDomain.BaseDirectory + "\\" + "SimpleScript.py"));

            // assign using implicit conversion
            baseDef.CurrentValue = "1000";

            Assert.IsTrue(((int)baseDef.CurrentValue) == 4000);
        }

        // this will deploy the file as well
        [DeploymentItem("SimpleScript.py")]
        [TestMethod]
        public void AddTwoScriptCallback()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.PythonCallback("B", DynamicCallback.CallbackType.DetectAnomaly, null, AppDomain.CurrentDomain.BaseDirectory + "\\" + "SimpleScript.py"));

            // assign using implicit conversion
            baseDef.CurrentValue = "500";

            // should be 1000 
            Assert.IsTrue(((int)baseDef.CurrentValue) == 1000);

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.PythonCallback("A", DynamicCallback.CallbackType.GeneralCheck, null, AppDomain.CurrentDomain.BaseDirectory + "\\" + "SimpleScript.py"));

            // assign using implicit conversion
            baseDef.CurrentValue = "500";

            // should be 4000 - and not 2000 (the general callback multiplies by 4 - but the value from the previous callback which multiplies by 2 - so 500 * 8 = 4000)
            Assert.IsTrue(((int)baseDef.CurrentValue) == 4000);

            // now if we remove the one callback everything should be back to "normal"
            ValueManager.Instance.RemoveCallbackForValueDefinition(baseDef, "B");

            // assign using implicit conversion
            baseDef.CurrentValue = "500";

            // should be 2000 now again (500 * 4)
            Assert.IsTrue(((int)baseDef.CurrentValue) == 2000);

        }

        [TestMethod]
        public void AddCompiledMethodCallbackForAllLevels()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CompiledMethodCallback("A1", CallbackType.DetectAnomaly, typeof(ValueCallback)));
            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CompiledMethodCallback("A2", CallbackType.AdjustSampleRate, typeof(ValueCallback)));
            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CompiledMethodCallback("A3", CallbackType.GeneralCheck, typeof(ValueCallback)));
            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CompiledMethodCallback("A31", CallbackType.VirtualValueCalculation, typeof(ValueCallback)));
            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CompiledMethodCallback("A4", CallbackType.BeforeStore, typeof(ValueCallback)));
            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CompiledMethodCallback("A5", CallbackType.AfterStore, typeof(ValueCallback)));

            // assign using implicit conversion
            baseDef.CurrentValue = "100.12";

            // the saving might have been run already or not
            Assert.IsTrue(Utilities.CalledLevelList.Count >= ValueManager.Instance.PhaseOne.Count());

            int index = 0;
            foreach (ValueManagement.DynamicCallback.CallbackType kind in ValueManager.Instance.PhaseOne)
            {
                Assert.IsTrue(Utilities.CalledLevelList[index] == ValueManager.Instance.PhaseOne[index]);
                index++;
            }
        }

        [DeploymentItem("SimpleScript.py")]
        [TestMethod]
        public void AddMixedCallbacks()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.PythonCallback("A", DynamicCallback.CallbackType.GeneralCheck, null, AppDomain.CurrentDomain.BaseDirectory + "\\" + "SimpleScript.py"));

            // assign using implicit conversion
            baseDef.CurrentValue = "1000";
            Assert.IsTrue(((int)baseDef.CurrentValue) == 4000);

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CompiledMethodCallback("A3", CallbackType.GeneralCheck, typeof(MultiplyBy3Callback)));

            // this will trigger 2 callbacks - the question is just which one runs first. Due to multiplying it doesn't matter
            baseDef.CurrentValue = "1000";

            // the value must be 1000 * 4 * 3
            Assert.IsTrue(baseDef.CurrentValue == (1000 * 4 * 3));
        }

        /// <summary>
        /// Test to compute a virtual value based on two base values. The result is dependent value = base 1 + base 2
        /// </summary>
        [TestMethod]
        public void AddVirtualDefinitionWithCompiledCallback()
        {
            int startId = Utilities.CreateDefinitions(3, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition baseDef2 = ValueManager.Instance[startId + 1];
            ValueDefinition depDef = ValueManager.Instance[startId + 2];

            // make #2 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef);
            ValueManager.Instance.AddDependency(baseDef2, depDef);

            // register the callback
            ValueManager.Instance.AddCallbackForValueDefinition(depDef, new DynamicCallback.CompiledMethodCallback("VirtualEvalCallback", DynamicCallback.CallbackType.VirtualValueCalculation, typeof(VirtualValueEvaluationCallback)));

            // and set as the handler for evaluation
            (depDef as VirtualValueDefinition).VirtualValueEvaluationCallback = "VirtualEvalCallback";

            // define the base values
            baseDef.CurrentValue = "100";
            Assert.IsTrue(depDef.CurrentValue == 200);

            baseDef2.CurrentValue = "100";
            Assert.IsTrue(depDef.CurrentValue == 400);

            // No history since it is a virtual value
            Assert.IsTrue(depDef.GetHistoricValues().Length == 0);
        }

        /// <summary>
        /// Test to compute a virtual value based on two base values. The result is dependent value = base 1 + base 2
        /// </summary>
        [TestMethod]
        public void AddVirtualDefinitionWithCompiledCallbackAndLateEvaluation()
        {
            int startId = Utilities.CreateDefinitions(3, true, VirtualSensorCalculationType.OnRequest);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition baseDef2 = ValueManager.Instance[startId + 1];
            ValueDefinition depDef = ValueManager.Instance[startId + 2];

            // make #2 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef);
            ValueManager.Instance.AddDependency(baseDef2, depDef);

            // register the callback
            ValueManager.Instance.AddCallbackForValueDefinition(depDef, new DynamicCallback.CompiledMethodCallback("VirtualEvalCallback", DynamicCallback.CallbackType.VirtualValueCalculation, typeof(VirtualValueEvaluationCallback)));

            // and set as the handler for evaluation
            (depDef as VirtualValueDefinition).VirtualValueEvaluationCallback = "VirtualEvalCallback";

            // define the base values
            baseDef.CurrentValue = "100";
            baseDef2.CurrentValue = "100";

            // use implicit conversion
            Assert.IsTrue(depDef.CurrentValue == 400);

            // there shall not be any value as it was NULL initially and only changed once when the request for the value was done -> but it did not save any
            Assert.IsTrue(depDef.GetHistoricValues().Length == 0);
        }

        [DeploymentItem("SimpleScript.py")]
        [TestMethod]
        public void AddVirtualDefinitionWithScriptCallback()
        {
            int startId = Utilities.CreateDefinitions(3, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition baseDef2 = ValueManager.Instance[startId + 1];
            ValueDefinition depDef = ValueManager.Instance[startId + 2];

            // make #2 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef);
            ValueManager.Instance.AddDependency(baseDef2, depDef);

            // register the callback
            ValueManager.Instance.AddCallbackForValueDefinition(depDef, new DynamicCallback.PythonCallback("VirtualEvalCallback", DynamicCallback.CallbackType.VirtualValueCalculation, null, AppDomain.CurrentDomain.BaseDirectory + "\\" + "SimpleScript.py"));

            // and set as the handler for evaluation
            (depDef as VirtualValueDefinition).VirtualValueEvaluationCallback = "VirtualEvalCallback";

            // define the base values
            baseDef.CurrentValue = "100";
            Assert.IsTrue(depDef.CurrentValue == 200);

            baseDef2.CurrentValue = "100";
            Assert.IsTrue(depDef.CurrentValue == 400);

            // No history since it is a virtual value
            Assert.IsTrue(depDef.GetHistoricValues().Length == 0);
        }

        [TestMethod]
        public void TestSaveValueData()
        {
            int startId = Utilities.CreateDefinitions(Utilities.AvgValueDefinitionCount);

            // store in each variable
            for (int x = 0; x < Utilities.AvgValueDefinitionCount; x++)
            // for (int x = 0; x < 5; x++)
            {
                // 10 values -> 9 are historic, 1 is current
                for (int i = 0; i < Utilities.ValueCount; i++)
                // for (int i = 0; i < 2; i++)
                {
                    ValueManager.Instance[startId + x].CurrentValue = new SensorData(i.ToString());
                }
            }

            // now we should have values in the dictionary of unsaved values
            // Assert.IsTrue(ValueManager.UnsavedValueDict.Keys.Count == Utilities.AvgValueDefinitionCount);

            // and each entry has a depth of x
            // Assert.IsTrue(ValueManager.UnsavedValueDict.All(pair => pair.Value.Count == Utilities.ValueCount));

            System.Threading.ManualResetEvent writingOccurredSem = new System.Threading.ManualResetEvent(false);

            ValueManager.DataWasWritten += (object o, EventArgs ea) =>
            {
                writingOccurredSem.Set();
            };

            // and we can start writing
            Assert.IsTrue(ValueManager.Instance.StartWriteToStorageThread());

            // give the other thread some time to start up (as otherwise the close might come too fast)
            writingOccurredSem.WaitOne();

            // now stop the thread
            ValueManager.Instance.StopWriteToStorageThread();

            // and there should be no entries left
            Assert.IsTrue(ValueManager.UnsavedValueDict.Count == 0);
        }
    }
}

