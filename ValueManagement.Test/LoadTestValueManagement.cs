// #undef USE_ASSERT
#define USE_ASSERT

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Common.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Threading;
using System.Diagnostics;

using GlobalDataContracts;

using ValueManagement.DynamicCallback;
using System.Data;

namespace ValueManagement.Test
{
    [TestClass]
    public class LoadTestValueManagement
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const int LoadTestIterations = 10000;

        public const int SimpleTestIterations = 200;
        public const int ComplexTestIterations = 100;

        public const string AppSettingResultFileName = "ResultFileName";

        private static StreamWriter writer;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            //ValueManager.StartWriteToStorageThread();
            ExecutionDurationDumper.Clear();
            string finalFileName = string.Format(ConfigurationManager.AppSettings[AppSettingResultFileName], DateTime.Now.Ticks.ToString());
            writer = new StreamWriter(finalFileName);
            context.WriteLine("Testdata can be found in: {0}", finalFileName);
        }
        [ClassCleanup]
        public static void ClassCleanup()
        {
            foreach (string section in ExecutionDurationDumper.Sections)
            {
                writer.WriteLine(section);
                foreach (long duration in ExecutionDurationDumper.GetExecutionDurationsForSection(section))
                {
                    writer.WriteLine("{0}", duration);
                }
                writer.WriteLine("--------------------");
            }
            writer.Flush();
            writer.Close();
        }

        [TestCleanup]
        public void CleanUp()
        {
            Utilities.ClearAllValues();
            Utilities.ClearAllCallbacks();
            Utilities.CalledLevelList.Clear();
        }

        [TestMethod]
        public void Test1()
        {
            for (int i = 0; i < SimpleTestIterations; i++)
            {
                MeasureCompiledTriggerExecution();
                CleanUp();
                MeasurePythonScriptTriggerExecution();
                CleanUp();
                MeasureSqlStoredProcTriggerExecution();
                CleanUp();
                MeasureSqlScriptTriggerExecution();
                CleanUp();
            }
        }

        [TestMethod]
        public void Test2()
        {
            for (int i = 0; i < SimpleTestIterations; i++)
            {
                MeasureVirtualValueMethodCallTiming();
                CleanUp();
                MeasurePythonScriptVirtualValueCallTiming();
                CleanUp();
            }
        }


        [TestMethod]
        public void Test4()
        {
            for (int i = 0; i < ComplexTestIterations; i++)
            {
                MultiThreadReadWriteCSharpInteractiveTriggerTest();
                CleanUp();
                // MultiThreadReadWriteFSharpInteractiveTriggerTest();
                // CleanUp();
                MultiThreadReadWriteMethodTriggerTest();
                CleanUp();
                MultiThreadReadWriteScriptTriggerPythonTest();
                CleanUp();
                MultiThreadReadWriteSqlInteractiveTriggerTest();
                CleanUp();
                MultiThreadReadWriteSqlProcedureTriggerTest();
                CleanUp();
            }
        }

        [TestMethod]
        public void Test6_GeneralCheck()
        {
            for (int i = 0; i < SimpleTestIterations; i++)
            {
                MeasureSoftwareAgentExecution();
                CleanUp();
            }
        }


        [TestMethod]
        public void Test6_AfterValueChangeTrigger()
        {
            for (int i = 0; i < SimpleTestIterations; i++)
            {
                MeasureSoftwareAgentAfterChangeTriggerExecution();
                CleanUp();
            }
        }

        [TestMethod]
        public void Test6_VirtualValue()
        {
            for (int i = 0; i < SimpleTestIterations; i++)
            {
                MeasurSoftwareAgentVirtualValueCallTiming();
                CleanUp();
            }
        }
        [TestMethod]
        public void Test6_ReadWrite()
        {
            for (int i = 0; i < ComplexTestIterations; i++)
            {
                MultiThreadReadWriteSoftwareAgentTriggerTest();
                CleanUp();
            }
        }


        [TestMethod]
        public void MeasureCompiledTriggerExecution()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.CompiledMethodCallback("A", DynamicCallback.CallbackType.GeneralCheck, typeof(MultiplyBy3Callback)));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // perform 100000 iterations
            for (int i = 0; i < LoadTestIterations; i++)
            {
                baseDef.CurrentValue = "200";

#if USE_ASSERT
                Assert.IsTrue(baseDef.CurrentValue == 600);
#endif
            }
            watch.Stop();
            Console.WriteLine("{0:N} Compiled C# method callback iterations took {1} µseconds time", LoadTestIterations, watch.ElapsedMicroSeconds());
            watch.AddExecutionDurationToList(System.Reflection.MethodBase.GetCurrentMethod().ToString());
        }


        [TestMethod]
        public void MeasureSoftwareAgentExecution()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.SoftwareAgentCallback("A", DynamicCallback.CallbackType.GeneralCheck, typeof(MultiplyBy3CodeActivity)));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // perform 100000 iterations
            for (int i = 0; i < LoadTestIterations; i++)
            {
                baseDef.CurrentValue = "200";

#if USE_ASSERT
                Assert.IsTrue(baseDef.CurrentValue == 600);
#endif
            }
            watch.Stop();
            Console.WriteLine("{0:N} software agent callback iterations took {1} µseconds time", LoadTestIterations, watch.ElapsedMicroSeconds());
            watch.AddExecutionDurationToList(System.Reflection.MethodBase.GetCurrentMethod().ToString());
        }

        [TestMethod]
        public void MeasureSoftwareAgentAfterChangeTriggerExecution()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.SoftwareAgentCallback("A", DynamicCallback.CallbackType.AfterChangeCallback, typeof(NopCodeActivity)));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // perform 100000 iterations
            for (int i = 0; i < LoadTestIterations; i++)
            {
                baseDef.CurrentValue = "200";

#if USE_ASSERT
                Assert.IsTrue(baseDef.CurrentValue == 200);
#endif
            }
            watch.Stop();
            Console.WriteLine("{0:N} software agent after value change callback iterations took {1} µseconds time", LoadTestIterations, watch.ElapsedMicroSeconds());
            watch.AddExecutionDurationToList(System.Reflection.MethodBase.GetCurrentMethod().ToString());
        }


        [TestMethod]
        public void MeasureVirtualValueMethodCallTiming()
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

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // perform 100000 iterations
            for (int i = 0; i < LoadTestIterations; i++)
            {
                // define the base values
                baseDef.CurrentValue = "100";
#if USE_ASSERT
                Assert.IsTrue(depDef.CurrentValue == ((i == 0) ? 200 : 400));
#endif

                baseDef2.CurrentValue = "100";
#if USE_ASSERT
                Assert.IsTrue(depDef.CurrentValue == 400);
#endif
            }

            watch.Stop();
            Console.WriteLine("{0:N} Compiled C# virtual value evaluations in MethodCall took {1} µseconds time", LoadTestIterations, watch.ElapsedMicroSeconds());
            watch.AddExecutionDurationToList(System.Reflection.MethodBase.GetCurrentMethod().ToString());
        }

        [DeploymentItem("SimpleScript.py")]
        [TestMethod]
        public void MeasurePythonScriptTriggerExecution()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.PythonCallback("A", DynamicCallback.CallbackType.GeneralCheck, null, AppDomain.CurrentDomain.BaseDirectory + "\\" + "SimpleScript.py"));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // perform 100000 iterations
            for (int i = 0; i < LoadTestIterations; i++)
            {
                baseDef.CurrentValue = "200";

#if USE_ASSERT
                Assert.IsTrue(baseDef.CurrentValue == 800);
#endif
            }

            watch.Stop();
            Console.WriteLine("{0:N} Python script callback iterations took {1} µseconds time", LoadTestIterations, watch.ElapsedMicroSeconds());
            watch.AddExecutionDurationToList(System.Reflection.MethodBase.GetCurrentMethod().ToString());
        }

        
        [TestMethod]
        public void MeasureSqlScriptTriggerExecution()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.SqlInteractiveCallback("A", DynamicCallback.CallbackType.GeneralCheck,
                ValueManager.Instance.DatabaseConnectionSettingsForCallbacks,
                "SELECT 4 * CONVERT(decimal, @current_value) return_value, 1 is_modified"));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // perform 100000 iterations
            for (int i = 0; i < LoadTestIterations; i++)
            {
                baseDef.CurrentValue = "200";

#if USE_ASSERT
                Assert.IsTrue(baseDef.CurrentValue == 800);
#endif
            }

            watch.Stop();
            Console.WriteLine("{0:N} SQL script callback iterations took {1} µseconds time", LoadTestIterations, watch.ElapsedMicroSeconds());
            watch.AddExecutionDurationToList(System.Reflection.MethodBase.GetCurrentMethod().ToString());
        }

        [TestMethod]
        public void MeasureSqlStoredProcTriggerExecution()
        {
            int startId = Utilities.CreateDefinitions(1);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueManager.Instance.AddCallbackForValueDefinition(baseDef, new DynamicCallback.SqlProcedureCallback("A", DynamicCallback.CallbackType.GeneralCheck,
               "Data Source=.;Initial Catalog=Experiments;Persist Security Info=True;User ID=EXPERIMENT;Password=EXPERIMENT",
               "dbo.MultiplyBy3"));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // perform 100000 iterations
            for (int i = 0; i < LoadTestIterations; i++)
            {
                baseDef.CurrentValue = "200";

#if USE_ASSERT
                Assert.IsTrue(baseDef.CurrentValue == 600);
#endif
            }

            watch.Stop();
            Console.WriteLine("{0:N} SQL stored proc callback iterations took {1} µseconds time", LoadTestIterations, watch.ElapsedMicroSeconds());
            watch.AddExecutionDurationToList(System.Reflection.MethodBase.GetCurrentMethod().ToString());
        }



        [DeploymentItem("SimpleScript.py")]
        [TestMethod]
        public void MeasurePythonScriptVirtualValueCallTiming()
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

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // perform 100000 iterations
            for (int i = 0; i < LoadTestIterations; i++)
            {
                // define the base values
                baseDef.CurrentValue = "100";
#if USE_ASSERT
                Assert.IsTrue(depDef.CurrentValue == ((i == 0) ? 200 : 400));
#endif

                baseDef2.CurrentValue = "100";
#if USE_ASSERT
                Assert.IsTrue(depDef.CurrentValue == 400);
#endif
            }

            watch.Stop();
            Console.WriteLine("{0:N} Python script virtual value evaluations took {1} µseconds time", LoadTestIterations, watch.ElapsedMicroSeconds());
            watch.AddExecutionDurationToList(System.Reflection.MethodBase.GetCurrentMethod().ToString());
        }


        [TestMethod]
        public void MeasurSoftwareAgentVirtualValueCallTiming()
        {
            int startId = Utilities.CreateDefinitions(3, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition baseDef2 = ValueManager.Instance[startId + 1];
            ValueDefinition depDef = ValueManager.Instance[startId + 2];

            // make #2 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef);
            ValueManager.Instance.AddDependency(baseDef2, depDef);

            // register the callback
            ValueManager.Instance.AddCallbackForValueDefinition(depDef, new DynamicCallback.SoftwareAgentCallback("VirtualEvalCallback", DynamicCallback.CallbackType.VirtualValueCalculation, typeof(VirtualValueEvaluationCodeActivity)));

            // and set as the handler for evaluation
            (depDef as VirtualValueDefinition).VirtualValueEvaluationCallback = "VirtualEvalCallback";

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // perform 100000 iterations
            for (int i = 0; i < LoadTestIterations; i++)
            {
                // define the base values
                baseDef.CurrentValue = "100";
#if USE_ASSERT
                Assert.IsTrue(depDef.CurrentValue == ((i == 0) ? 200 : 400));
#endif

                baseDef2.CurrentValue = "100";
#if USE_ASSERT
                Assert.IsTrue(depDef.CurrentValue == 400);
#endif
            }

            watch.Stop();
            Console.WriteLine("{0:N} Software Agent virtual value evaluations took {1} µseconds time", LoadTestIterations, watch.ElapsedMicroSeconds());
            watch.AddExecutionDurationToList(System.Reflection.MethodBase.GetCurrentMethod().ToString());
        }

        public const int SuperHighValueDefinitionCount = 25000;
        public const int SuperHighValueCount = 100;

        // #if false
        // the method below is only used for heavy-duty load testing

        [TestMethod]
        public void TestMaxLoad()
        {
            try
            {
                Console.WriteLine("Snapshot taken");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Got exception: " + ex.ToString());
            }


            // create the values
            for (int x = 0; x < SuperHighValueDefinitionCount; x++)
            {
                try
                {
                    // plus data
                    int id = Utilities.InternalId++;
                    List<SensorData> valList = new List<SensorData>();
                    for (int count = 0; count < SuperHighValueCount; count++)
                    {
                        SensorData data = new SensorData();
                        data.Value = x.ToString();
                        data.GeneratedWhen = DateTime.Now;

                        valList.Add(data);
                    }

                    // add to the dictionary
                    ValueDefinition def = new ValueDefinition(id, id.ToString(), valList);
                    ValueManager.Instance.AddValueDefinition(def);
                }
                catch (OutOfMemoryException)
                {
                    System.GC.Collect();
                    Console.WriteLine("GC run at iteration {0}", x);
                }
            }


            // and now check that it is true
            Assert.IsTrue(ValueManager.Instance.ValueDefinitionDictionary.Keys.Count == SuperHighValueDefinitionCount);

            try
            {
                Console.WriteLine("Snapshot taken");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Got exception: " + ex.ToString());
            }

        }
        // #endif

        //public const int VariableCount = 100000;
        public const int VariableCount = 100;
        public const int ReadThreadCount = 5;
        public const int WriteThreadCount = 5;
        //public const int IterationsPerThread = 50000;
        public const int IterationsPerThread = 500;
        public const int SetValueConst = 100;

        [TestMethod]
        public void MultiThreadReadWriteCSharpInteractiveTriggerTest()
        {
            string executionExpression =
@"
using System;
using ValueManagement.DynamicCallback;

public class CSharpInteractiveClass
{
    static public CallbackResultData GeneralCheckCallback(CallbackPassInData passInData)
    {
        CallbackResultData result = new CallbackResultData();
		result.IsValueModified = true;
		result.NewValue = passInData.CurrentValue * 3;
		return (result);
    }
}
";
            MultiThreadReadWriteTriggerTest(new DynamicCallback.CSharpInteractiveCallback("A", DynamicCallback.CallbackType.GeneralCheck,
                executionExpression), "DynamicCSharp");
        }

        [TestMethod]
        public void MultiThreadReadWriteFSharpInteractiveTriggerTest()
        {
            string executionExpression =
@"
module public Program

open ValueManagement.DynamicCallback
open GlobalDataContracts
open System

type public FSharpInteractiveClass() =
  static member public GeneralCheckCallback(data: CallbackPassInData) =
    let result = new CallbackResultData()
    result.IsValueModified <- true
    result.NewValue <- new SensorData()
    result.NewValue.Value <- Convert.ToString(Convert.ToInt32(data.CurrentValue.Value) * 3)
    result
";
            MultiThreadReadWriteTriggerTest(new DynamicCallback.FSharpInteractiveCallback("A", DynamicCallback.CallbackType.GeneralCheck,
                executionExpression), "InteractiveFSharp");
        }


        [TestMethod]
        public void MultiThreadReadWriteSqlInteractiveTriggerTest()
        {
            SqlInteractiveCallback callback = new DynamicCallback.SqlInteractiveCallback("A", DynamicCallback.CallbackType.GeneralCheck,
                "Data Source=.;Initial Catalog=Experiments;Persist Security Info=True;User ID=EXPERIMENT;Password=EXPERIMENT",
                "SELECT 3 * CONVERT(decimal, @current_value) return_value, 1 is_modified");
            MultiThreadReadWriteTriggerTest(callback, "InteractiveSQL");
        }

        [TestMethod]
        public void MultiThreadReadWriteSqlProcedureTriggerTest()
        {
            SqlProcedureCallback callback = new DynamicCallback.SqlProcedureCallback("A", DynamicCallback.CallbackType.GeneralCheck,
                "Data Source=.;Initial Catalog=Experiments;Persist Security Info=True;User ID=EXPERIMENT;Password=EXPERIMENT",
                "dbo.MultiplyBy3");
            MultiThreadReadWriteTriggerTest(callback, "StoredProcSQL");
        }

        [TestMethod]
        public void MultiThreadReadWriteMethodTriggerTest()
        {
            MultiThreadReadWriteTriggerTest(new DynamicCallback.CompiledMethodCallback("A", DynamicCallback.CallbackType.GeneralCheck, typeof(MultiplyBy3Callback)), "CompiledCSharp");
        }

        [TestMethod]
        public void MultiThreadReadWriteSoftwareAgentTriggerTest()
        {
            MultiThreadReadWriteTriggerTest(new DynamicCallback.SoftwareAgentCallback("A", DynamicCallback.CallbackType.GeneralCheck, typeof(MultiplyBy3CodeActivity)), "SoftwareAgentCompiled");
        }


        [DeploymentItem("GeneralTriggerMultiplyBy3Script.py")]
        [TestMethod]
        public void MultiThreadReadWriteScriptTriggerPythonTest()
        {
            MultiThreadReadWriteTriggerTest(new DynamicCallback.PythonCallback("A", DynamicCallback.CallbackType.GeneralCheck, null, AppDomain.CurrentDomain.BaseDirectory + "\\" + "GeneralTriggerMultiplyBy3Script.py"), "Python");
        }

       
        /// <summary>
        /// Create 10000 variables and then from x threads read and y threads write using assigned triggers
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="exeuctionTimingSectionName">the name of the execution timing section in the ExecutionDurationDumper</param>
        private void MultiThreadReadWriteTriggerTest(AbstractCallback callback, string exeuctionTimingSectionName)
        {
            try
            {
                int startId = Utilities.CreateDefinitions(VariableCount);

                ValueDefinition baseDef = ValueManager.Instance[startId];


                CountdownEvent countdownEvent = new CountdownEvent(ReadThreadCount + WriteThreadCount);

                for (int varCounter = 0; varCounter < VariableCount; varCounter++)
                {
                    // associate the general check method
                    ValueManager.Instance.AddCallbackForValueDefinition(ValueManager.Instance[startId + varCounter], callback);
                }

                Stopwatch watch = new Stopwatch();
                watch.Start();

                for (int writeCount = 0; writeCount < WriteThreadCount; writeCount++)
                {
                    new Thread((object o) =>
                    {
                        Thread.Yield();

                        Stopwatch writeThreadWatch = new Stopwatch();
                        writeThreadWatch.Start();

                        // in x threads set all variables to a constant value
                        for (int index = 0; index < VariableCount; index++)
                        {
                            // which is either 0 for the uneven threads or the value constant for the even threads
                            ValueManager.Instance[startId + index].CurrentValue = ((writeCount % 2 == 0) ? SetValueConst : 0);
                        }

                        writeThreadWatch.Stop();

                        Console.WriteLine("Write-Thread {0} took {1} µsec time", (int)o, writeThreadWatch.ElapsedMicroSeconds());
                        writeThreadWatch.AddExecutionDurationToList(exeuctionTimingSectionName + "_Write");

                        // signal the caller
                        countdownEvent.Signal();
                    }).Start(writeCount);
                }


                // in y threads read the variables
                for (int readCount = 0; readCount < ReadThreadCount; readCount++)
                {
                    new Thread((object o) =>
                    {
                        // take a random index
                        Random rnd = new Random();

                        Thread.Yield();

                        Stopwatch readThreadWatch = new Stopwatch();
                        readThreadWatch.Start();

                        // and then iterate n times to read the value
                        for (int iteration = 0; iteration < IterationsPerThread; iteration++)
                        {
                            int varIndex = rnd.Next(VariableCount);
                            int value = -1;
                            try
                            {
                                value = ValueManager.Instance[startId + varIndex].CurrentValue;
                            }
                            catch (Exception exc)
                            {
                            }

                            // either the value is not set or the set value times 3 due to the trigger (which is executed synchronously in the lock
                            Assert.IsTrue((value == 0) || (value == (SetValueConst * 3)), "Value was {0} instead of 0 or {1}", value, SetValueConst * 3);
                        }

                        readThreadWatch.Stop();
                        Console.WriteLine("Read-Thread {0} took {1} µsec time", (int)o, readThreadWatch.ElapsedMicroSeconds());
                        readThreadWatch.AddExecutionDurationToList(exeuctionTimingSectionName + "_Read");

                        // Signal the caller that we are ready
                        countdownEvent.Signal();

                    }).Start(readCount);
                }

                // wait for all threads to finish
                countdownEvent.Wait();

                for (int varCounter = 0; varCounter < VariableCount; varCounter++)
                {
                    // make sure that there are enough historic entries
                    Assert.IsTrue(ValueManager.Instance[startId + varCounter].GetHistoricValues().Length == WriteThreadCount - 1);
                }

                Console.WriteLine("Total execution took {0} µsec time", watch.ElapsedMicroSeconds());
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
        }
    }
}
