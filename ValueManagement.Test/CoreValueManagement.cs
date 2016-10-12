using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using GlobalDataContracts;

using ValueManagement.DynamicCallback;
using System.Reflection;

namespace ValueManagement.Test
{
	[TestClass]
	public class CoreValueManagement
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

		/// <summary>
		/// Initialize the assembly and initialize Log4Net as well
		/// </summary>
		/// <param name="context">The context.</param>
		[AssemblyInitialize()]
		public static void AssemblyInit(TestContext context)
		{
			log4net.Config.XmlConfigurator.Configure();
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

        #region AddValueDefinition...
        [TestMethod]
        public void AddValueDefinition()
        {
            int id = Utilities.InternalId++;
            ValueDefinition def = new ValueDefinition(id, id.ToString());

            ValueManager.Instance.AddValueDefinition(def);
            Assert.IsTrue(ValueManager.Instance[id].InternalId == def.InternalId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddValueDefinition_Null()
        {
            ValueManager.Instance.AddValueDefinition(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddValueDefinition_ExistingId()
        {

            int id = Utilities.InternalId++;
            ValueDefinition def = new ValueDefinition(id, id.ToString());

            ValueManager.Instance.AddValueDefinition(def);
            Assert.IsTrue(ValueManager.Instance[id].InternalId == def.InternalId);

            ValueManager.Instance.AddValueDefinition(def);
        }
        #endregion

        #region RemoveValueDefinition...
        [TestMethod]
        public void RemoveValueDefinition()
        {
            int id = Utilities.InternalId++;
            ValueDefinition def = new ValueDefinition(id, id.ToString());

            ValueManager.Instance.AddValueDefinition(def);
            Assert.IsTrue(ValueManager.Instance[id].InternalId == def.InternalId);
            ValueManager.Instance.RemoveValueDefinition(def);

            Assert.IsFalse(ValueManager.Instance.IsValueDefinitionPresent(def.InternalId));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveValueDefinition_Null()
        {
            ValueManager.Instance.RemoveValueDefinition(null);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void RemoveValueDefinition_NotExistingId()
        {
            int id = Utilities.InternalId++;
            ValueDefinition def = new ValueDefinition(id, id.ToString());

            ValueManager.Instance.RemoveValueDefinition(def);
        }

        #endregion

        #region IsValueDefinitionPresent...
        [TestMethod]
        public void IsValueDefinitionPresent()
        {
            int id1 = Utilities.InternalId++;
            ValueDefinition def1 = new ValueDefinition(id1, id1.ToString());

            int id2 = Utilities.InternalId++;
            ValueDefinition def2 = new ValueDefinition(id2, id2.ToString());

            Assert.IsFalse(ValueManager.Instance.IsValueDefinitionPresent(def1.InternalId));
            Assert.IsFalse(ValueManager.Instance.IsValueDefinitionPresent(def2.InternalId));

            //Add 1st
            ValueManager.Instance.AddValueDefinition(def1);

            Assert.IsTrue(ValueManager.Instance.IsValueDefinitionPresent(def1.InternalId));
            Assert.IsFalse(ValueManager.Instance.IsValueDefinitionPresent(def2.InternalId));

            //Add 2nd
            ValueManager.Instance.AddValueDefinition(def2);

            Assert.IsTrue(ValueManager.Instance.IsValueDefinitionPresent(def1.InternalId));
            Assert.IsTrue(ValueManager.Instance.IsValueDefinitionPresent(def2.InternalId));

            //Remove 1st
            ValueManager.Instance.RemoveValueDefinition(def1);

            Assert.IsFalse(ValueManager.Instance.IsValueDefinitionPresent(def1.InternalId));
            Assert.IsTrue(ValueManager.Instance.IsValueDefinitionPresent(def2.InternalId));

            //Remove 2nd
            ValueManager.Instance.RemoveValueDefinition(def2);

            Assert.IsFalse(ValueManager.Instance.IsValueDefinitionPresent(def1.InternalId));
            Assert.IsFalse(ValueManager.Instance.IsValueDefinitionPresent(def2.InternalId));
        }

        [TestMethod]
        public void IsValueDefinitionPresent_NotExistingId()
        {
            Assert.IsFalse(ValueManager.Instance.IsValueDefinitionPresent(-1));
        }
        #endregion

        #region GetValueDefinition...
        [TestMethod]
        public void GetValueDefinition()
        {
            int id1 = Utilities.InternalId++;
            ValueDefinition def1 = new ValueDefinition(id1, id1.ToString());

            int id2 = Utilities.InternalId++;
            ValueDefinition def2 = new ValueDefinition(id2, id2.ToString());

            //Add
            ValueManager.Instance.AddValueDefinition(def1);
            ValueManager.Instance.AddValueDefinition(def2);

            def1.CurrentValue = 1;
            def2.CurrentValue = 2;

            AssertAreEqual(def1, ValueManager.Instance[def1.InternalId]);
            AssertAreEqual(def2, ValueManager.Instance[def2.InternalId]);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetValueDefinition_NotExistingId()
        {
            ValueDefinition def1 = ValueManager.Instance[-1];
        }
        #endregion 

        #region Set ValueDefinition CurrentValue...
        [TestMethod]
        public void SetValueDefinitionCurrentValue()
        {
            int id1 = Utilities.InternalId++;
            ValueDefinition def1 = new ValueDefinition(id1, id1.ToString());
            int id2 = Utilities.InternalId++;
            ValueDefinition def2 = new ValueDefinition(id2, id2.ToString());

            ValueManager.Instance.AddValueDefinition(def1);
            ValueManager.Instance.AddValueDefinition(def2);

            ValueManager.Instance[def1.InternalId].CurrentValue = 1;
            Assert.AreEqual("1", ValueManager.Instance[def1.InternalId].CurrentValue.Value);
            ValueManager.Instance[def2.InternalId].CurrentValue = 2;
            Assert.AreEqual("2", ValueManager.Instance[def2.InternalId].CurrentValue.Value);
            
            Assert.AreEqual("1", ValueManager.Instance[def1.InternalId].CurrentValue.Value);
        }


        [TestMethod]
        public void SetValueDefinitionCurrentValue_HistoricalData()
        {
            int id1 = Utilities.InternalId++;
            ValueDefinition def1 = new ValueDefinition(id1, id1.ToString());
            int id2 = Utilities.InternalId++;
            ValueDefinition def2 = new ValueDefinition(id2, id2.ToString());

            ValueManager.Instance.AddValueDefinition(def1);
            ValueManager.Instance.AddValueDefinition(def2);

            Assert.AreEqual(0, def1.GetHistoricValues().Length);
            Assert.AreEqual(0, def2.GetHistoricValues().Length);

            ValueManager.Instance[def1.InternalId].CurrentValue = 1;
            Assert.AreEqual("1", ValueManager.Instance[def1.InternalId].CurrentValue.Value);
            Assert.AreEqual(0, def1.GetHistoricValues().Length);
            Assert.AreEqual(0, def2.GetHistoricValues().Length);

            ValueManager.Instance[def1.InternalId].CurrentValue = 2;
            Assert.AreEqual("2", ValueManager.Instance[def1.InternalId].CurrentValue.Value);
            Assert.AreEqual(1, def1.GetHistoricValues().Length);
            Assert.AreEqual("1", def1.GetHistoricValues()[0].Value);
            Assert.AreEqual(0, def2.GetHistoricValues().Length);


            for (int i = 0; i < 10; i++)
                ValueManager.Instance[def2.InternalId].CurrentValue = i;

            Assert.AreEqual(1, def1.GetHistoricValues().Length);
            Assert.AreEqual(9, def2.GetHistoricValues().Length);
            for (int i = 0; i < 9; i++)
                Assert.AreEqual(i.ToString(), def2.GetHistoricValues()[i].Value);
        }

        [TestMethod]
        public void SetValueDefinitionCurrentValue_HistoricalData_MaxLimited()
        {
            int id1 = Utilities.InternalId++;
            ValueDefinition def1 = new ValueDefinition(id1, id1.ToString());
            ValueManager.Instance.AddValueDefinition(def1);
            def1.MaxHistoricValues = 5;


            for (int i = 0; i < 10; i++)
                ValueManager.Instance[def1.InternalId].CurrentValue = i;

            Assert.AreEqual(5, def1.GetHistoricValues().Length);
            for (int i = 4; i < 9; i++)
                Assert.AreEqual(i.ToString(), def1.GetHistoricValues()[i - 4].Value);
        }
        
        [TestMethod]
		public void SetValueDataWithoutCallback()
		{
			int startId = Utilities.CreateDefinitions(Utilities.AvgValueDefinitionCount);

			// store in each variable
			for (int x = 0; x < Utilities.AvgValueDefinitionCount; x++)
			{
				// 10 values -> 9 are historic, 1 is current
				for (int i = 0; i < Utilities.ValueCount; i++)
				{
					ValueManager.Instance[startId + x].CurrentValue = new SensorData(i.ToString());
				}
				
				// check the historic values
                Assert.IsTrue(ValueManager.Instance[startId + x].GetHistoricValues().Length == (Utilities.ValueCount - 1));
			}
		}
        #endregion

        #region AddDependency...
        [TestMethod]
        public void AddDependency()
        {
            int startId = Utilities.CreateDefinitions(2, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId + 1];


            // make #2 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef);

            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Contains(baseDef));
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddDependency_Null()
        {
            int startId = Utilities.CreateDefinitions(1, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddDependency(baseDef, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddDependency_BaseNull()
        {
            int startId = Utilities.CreateDefinitions(1, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition depDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddDependency(null, depDef);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddDependency_NotRegisteredValueDefinition()
        {
            int startId = Utilities.CreateDefinitions(1, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = new VirtualValueDefinition(baseDef.InternalId+1, Guid.NewGuid().ToString(), VirtualSensorCalculationType.OnChange);

            // make #2 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef);
            Assert.Fail("Addded a dependency with a non-registered dependent value definition.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddDependency_BaseNotRegisteredValueDefinition()
        {
            int startId = Utilities.CreateDefinitions(1, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition depDef = ValueManager.Instance[startId];
            ValueDefinition baseDef = new VirtualValueDefinition(depDef.InternalId + 1, Guid.NewGuid().ToString(), VirtualSensorCalculationType.OnChange);

            ValueManager.Instance.AddDependency(baseDef, depDef);
            Assert.Fail("Addded a dependency with a non-registered base value definition.");
        }

        /// <summary>
        /// Test adding dependencies for non-virtual values which should fail with an exception
        /// </summary>
        [TestMethod]
        public void AddDependency_NonVirtualValueDependency()
        {
            int startId = Utilities.CreateDefinitions(2);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId + 1];

            try
            {
                ValueManager.Instance.AddDependency(baseDef, depDef);
                Assert.Fail("The dependency has to be a virtual value definition");
            }
            catch (ValueDefinitionSetupException)
            {
                ;
            }
        }

        /// <summary>
        /// Test adding self dependencies for values which should fail with an exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ValueManagement.ValueDefinitionSetupException))]
        public void AddDependency_SelfDependency()
        {
            int startId = Utilities.CreateDefinitions(1, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.AddDependency(baseDef, baseDef);
            Assert.Fail("The dependency check for self failed");
        }

        [TestMethod]
        public void AddAndRemoveManyDependencies()
        {
            int startId = Utilities.CreateDefinitions(Utilities.AvgValueDefinitionCount, true, VirtualSensorCalculationType.OnChange);

            // now all (except the first entry) are dependent on the first entry
            for (int i = 1; i < Utilities.AvgValueDefinitionCount; i++)
            {
                ValueManager.Instance.AddDependency(ValueManager.Instance[startId], ValueManager.Instance[startId + i]);
            }

            // check that all definitions are dependent 
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(ValueManager.Instance[startId]).Count == (Utilities.AvgValueDefinitionCount - 1));

            // and that all have the same base
            for (int i = 1; i < Utilities.AvgValueDefinitionCount; i++)
            {
                HashSet<ValueDefinition> defSet = ValueManager.Instance.GetBaseValueDefinitions(ValueManager.Instance[startId + i]);
                Assert.IsTrue(defSet.Count == 1);
                Assert.IsTrue(defSet.Contains(ValueManager.Instance[startId]));
            }
        }

        /// <summary>
        /// Test the adding and removing of one dependency
        /// </summary>
        [TestMethod]
        public void AddDependency_CyclicDependency_ABA()
        {
            int startId = Utilities.CreateDefinitions(2, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId + 1];

            // make #2 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef);

            // and now #1 on #2 -> cycle
            try
            {
                ValueManager.Instance.AddDependency(depDef, baseDef);
                Assert.Fail("Cycle not detected");
            }
            catch (CyclicReferenceException)
            {
                ;
            }

            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Contains(baseDef));
            // this should not work
            Assert.IsFalse(ValueManager.Instance.GetBaseValueDefinitions(baseDef).Contains(depDef));

            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef));
            // this should not work
            Assert.IsFalse(ValueManager.Instance.GetDependentValueDefinitions(depDef).Contains(baseDef));
        }

        /// <summary>
        /// Test the adding of one cycle dependency
        /// </summary>
        [TestMethod]
        public void AddDependency_CyclicDependency_ABCA()
        {
            int startId = Utilities.CreateDefinitions(3, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId + 1];
            ValueDefinition depDefLev2 = ValueManager.Instance[startId + 2];

            // make #2 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef);
            ValueManager.Instance.AddDependency(depDef, depDefLev2);

            // this will now form the cycle Base -> Dep -> L2 -> Base
            try
            {
                ValueManager.Instance.AddDependency(depDefLev2, baseDef);
                Assert.Fail("Cycle not detected");
            }
            catch (CyclicReferenceException)
            {
                ;
            }

            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Contains(baseDef));
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDefLev2).Contains(depDef));
            // this should not work
            Assert.IsFalse(ValueManager.Instance.GetBaseValueDefinitions(baseDef).Contains(depDefLev2));

            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef));
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(depDef).Contains(depDefLev2));
            // this should not work
            Assert.IsFalse(ValueManager.Instance.GetDependentValueDefinitions(depDefLev2).Contains(baseDef));


            // this should clear the dependency
            ValueManager.Instance.RemoveValueDefinition(ValueManager.Instance[startId]);
            Assert.IsFalse(ValueManager.Instance.GetBaseValueDefinitions(depDef).Contains(baseDef));
            Assert.IsFalse(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef));
        }

        /// <summary>
        /// Test the adding and removing of 2 cyclic dependencies in parallel graphs of the tree
        /// </summary>
        [TestMethod]
        public void AddDependency_CyclicDependency__ABCA_ADCA()
        {
            int startId = Utilities.CreateDefinitions(5, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId + 1];
            ValueDefinition depDefLev2 = ValueManager.Instance[startId + 2];
            ValueDefinition depDef2 = ValueManager.Instance[startId + 3];
            ValueDefinition depDef2Lev2 = ValueManager.Instance[startId + 4];

            // make #2 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef);
            ValueManager.Instance.AddDependency(depDef, depDefLev2);
            ValueManager.Instance.AddDependency(baseDef, depDef2);
            ValueManager.Instance.AddDependency(depDef2, depDef2Lev2);

            // this will now form the cycle Base -> Dep -> L2 -> Base
            try
            {
                ValueManager.Instance.AddDependency(depDefLev2, baseDef);
                Assert.Fail("Cycle not detected");
            }
            catch (CyclicReferenceException)
            {
                ;
            }

            // this would be another cycle Base -> Dep2 -> D2L2 -> Base
            try
            {
                ValueManager.Instance.AddDependency(depDef2Lev2, baseDef);
                Assert.Fail("Cycle not detected");
            }
            catch (CyclicReferenceException)
            {
                ;
            }

            // but this should be fine
            ValueManager.Instance.AddDependency(depDef, depDef2Lev2);
        }
        #endregion

        #region RemoveDependency...
        [TestMethod]
        public void RemoveDependency()
        {
            int startId = Utilities.CreateDefinitions(3, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId + 1];
            ValueDefinition depDef1 = ValueManager.Instance[startId + 2];


            // make #2 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef);
            // make #3 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef1);

            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Contains(baseDef));
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef));
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef1).Contains(baseDef));
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef1));

            // this should clear the dependency 1-2
            ValueManager.Instance.RemoveDependency(baseDef, depDef);
            Assert.IsFalse(ValueManager.Instance.GetBaseValueDefinitions(depDef).Contains(baseDef));
            Assert.IsFalse(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef));
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef1).Contains(baseDef));
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef1));

            // this should clear the dependency 1-3
            ValueManager.Instance.RemoveDependency(baseDef, depDef1);
            Assert.IsFalse(ValueManager.Instance.GetBaseValueDefinitions(depDef).Contains(baseDef));
            Assert.IsFalse(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef));
            Assert.IsFalse(ValueManager.Instance.GetBaseValueDefinitions(depDef1).Contains(baseDef));
            Assert.IsFalse(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveDependency_BaseNull()
        {
            int startId = Utilities.CreateDefinitions(2, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId + 1];

            // make #2 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef);

            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Contains(baseDef));
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef));

            // this should clear the dependency 1-2
            ValueManager.Instance.RemoveDependency(null, depDef);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveDependency_DependentNull()
        {
            int startId = Utilities.CreateDefinitions(2, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId + 1];


            // make #2 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef);

            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Contains(baseDef));
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef));

            // this should clear the dependency 1-2
            ValueManager.Instance.RemoveDependency(baseDef, null);
        }

        [TestMethod]
        public void RemoveDependency_NotExistingDependency()
        {
            int startId = Utilities.CreateDefinitions(2, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId + 1];

            Assert.IsFalse(ValueManager.Instance.GetBaseValueDefinitions(depDef).Contains(baseDef));
            Assert.IsFalse(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef));

            // this should clear the dependency 1-2
            ValueManager.Instance.RemoveDependency(baseDef, depDef);
        }

        [TestMethod]
        public void RemoveDependency_NotExistingBase()
        {
            int startId = Utilities.CreateDefinitions(1, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = new VirtualValueDefinition(startId + 1, Guid.NewGuid().ToString(), VirtualSensorCalculationType.OnChange);
            ValueDefinition depDef = ValueManager.Instance[startId];

            ValueManager.Instance.RemoveDependency(baseDef, depDef);
        }

        [TestMethod]
        public void RemoveDependency_NotExistingDependent()
        {
            int startId = Utilities.CreateDefinitions(1, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition depDef = new VirtualValueDefinition(startId + 1, Guid.NewGuid().ToString(), VirtualSensorCalculationType.OnChange);
            ValueDefinition baseDef = ValueManager.Instance[startId];

            ValueManager.Instance.RemoveDependency(baseDef, depDef);
        }
        #endregion

        #region IsDependentDefinition...
        [TestMethod]
        public void IsDependentDefinition()
        {
            int startId = Utilities.CreateDefinitions(2, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId + 1];

            //Not dependent
            Assert.IsFalse(ValueManager.Instance.IsDependentDefinition(baseDef));
            Assert.IsFalse(ValueManager.Instance.IsDependentDefinition(depDef));

            // make #2 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef);

            //Dependent
            Assert.IsFalse(ValueManager.Instance.IsDependentDefinition(baseDef));
            Assert.IsTrue(ValueManager.Instance.IsDependentDefinition(depDef));

            ValueManager.Instance.RemoveDependency(baseDef, depDef);

            //Not dependent
            Assert.IsFalse(ValueManager.Instance.IsDependentDefinition(baseDef));
            Assert.IsFalse(ValueManager.Instance.IsDependentDefinition(depDef));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsDependentDefinition_Null()
        {
            int startId = Utilities.CreateDefinitions(2, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId + 1];

            //Not dependent
            Assert.IsFalse(ValueManager.Instance.IsDependentDefinition(null));
        }
        #endregion

        #region IsBaseDefinition...
        [TestMethod]
        public void IsBaseDefinition()
        {
            int startId = Utilities.CreateDefinitions(2, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId + 1];

            //Not dependent
            Assert.IsFalse(ValueManager.Instance.IsBaseDefinition(baseDef));
            Assert.IsFalse(ValueManager.Instance.IsBaseDefinition(depDef));

            // make #2 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef);

            //Dependent
            Assert.IsTrue(ValueManager.Instance.IsBaseDefinition(baseDef));
            Assert.IsFalse(ValueManager.Instance.IsBaseDefinition(depDef));

            ValueManager.Instance.RemoveDependency(baseDef, depDef);

            //Not dependent
            Assert.IsFalse(ValueManager.Instance.IsBaseDefinition(baseDef));
            Assert.IsFalse(ValueManager.Instance.IsBaseDefinition(depDef));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsBaseDefinition_Null()
        {
            int startId = Utilities.CreateDefinitions(2, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId + 1];

            //Not dependent
            Assert.IsFalse(ValueManager.Instance.IsBaseDefinition(null));
        }
        #endregion

        #region GetDependentValueDefinitions...
        [TestMethod]
        public void GetDependentValueDefinitions()
        {
            int startId = Utilities.CreateDefinitions(3, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId + 1];
            ValueDefinition depDef1 = ValueManager.Instance[startId + 2];

            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(depDef).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(depDef1).Count == 0);

            // make #2 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef);

            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Count == 1);
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef));

            // make #3 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef1);

            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Count == 2);
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(depDef).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(depDef1).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef));
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef1));

            // remove dependency #2
            ValueManager.Instance.RemoveDependency(baseDef, depDef);

            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Count == 1);
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(depDef).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(depDef1).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef1));

            // remove dependency #3
            ValueManager.Instance.RemoveDependency(baseDef, depDef1);

            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(depDef).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(depDef1).Count == 0);

            // make dependency #3 - #2 - #1
            ValueManager.Instance.AddDependency(baseDef, depDef);
            ValueManager.Instance.AddDependency(depDef, depDef1);

            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Count == 1);
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(depDef).Count == 1);
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(depDef1).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(baseDef).Contains(depDef));
            Assert.IsTrue(ValueManager.Instance.GetDependentValueDefinitions(depDef).Contains(depDef1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDependentValueDefinitions_Null()
        {
            int startId = Utilities.CreateDefinitions(2, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId];

            ValueManager.Instance.GetDependentValueDefinitions(null);
        }
        #endregion

        #region GetBaseValueDefinitions...
        [TestMethod]
        public void GetBaseValueDefinitions()
        {
            int startId = Utilities.CreateDefinitions(3, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId + 1];
            ValueDefinition depDef1 = ValueManager.Instance[startId + 2];

            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(baseDef).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef1).Count == 0);

            // make #2 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef);

            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(baseDef).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Count == 1);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef1).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Contains(baseDef));

            // make #3 dependent on #1
            ValueManager.Instance.AddDependency(baseDef, depDef1);

            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(baseDef).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Count == 1);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef1).Count == 1);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Contains(baseDef));
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Contains(baseDef));

            // remove dependency #2
            ValueManager.Instance.RemoveDependency(baseDef, depDef);

            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(baseDef).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef1).Count == 1);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef1).Contains(baseDef));

            // remove dependency #3
            ValueManager.Instance.RemoveDependency(baseDef, depDef1);

            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(baseDef).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef1).Count == 0);

            // make dependency #3 - #2 - #1
            ValueManager.Instance.AddDependency(baseDef, depDef);
            ValueManager.Instance.AddDependency(depDef, depDef1);

            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(baseDef).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Count == 1);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef1).Count == 1);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Contains(baseDef));
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef1).Contains(depDef));

            //Remove
            ValueManager.Instance.RemoveDependency(baseDef, depDef);
            ValueManager.Instance.RemoveDependency(depDef, depDef1);

            //Male #2 - #1 and #2 - #3
            ValueManager.Instance.AddDependency(baseDef, depDef);
            ValueManager.Instance.AddDependency(depDef1, depDef);

            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(baseDef).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Count == 2);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef1).Count == 0);
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Contains(baseDef));
            Assert.IsTrue(ValueManager.Instance.GetBaseValueDefinitions(depDef).Contains(depDef1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetBaseValueDefinitions_Null()
        {
            int startId = Utilities.CreateDefinitions(2, true, VirtualSensorCalculationType.OnChange);

            ValueDefinition baseDef = ValueManager.Instance[startId];
            ValueDefinition depDef = ValueManager.Instance[startId];

            ValueManager.Instance.GetBaseValueDefinitions(null);
        }
        #endregion

        #region RegisterSensorAsValueDefinition...
        [TestMethod]
        public void RegisterSensorAsValueDefinition()
        {
            Sensor sensor = CreateTestSensor(false);
            ValueDefinition definition = ValueManager.Instance.RegisterSensorAsValueDefinition(sensor);
            Assert.IsNotNull(definition);
            AssertAreEqual(sensor, definition);

            ValueManager.Instance[definition.InternalId].CurrentValue = "100";
            AssertAreEqual("100", definition.CurrentValue.Value);
        }

        [TestMethod]
        public void RegisterSensorAsValueDefinition_VirtualSensor()
        {
            Sensor sensor = CreateTestSensor(true);
            ValueDefinition definition = ValueManager.Instance.RegisterSensorAsValueDefinition(sensor);
            Assert.IsNotNull(definition);
            AssertAreEqual(sensor, definition);

            ValueManager.Instance[definition.InternalId].CurrentValue = "100";
            Assert.AreEqual("100", definition.CurrentValue.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterSensorAsValueDefinition_Null()
        {
            ValueManager.Instance.RegisterSensorAsValueDefinition(null);
        }
        #endregion

        [TestMethod]
        public void AddManyValueDefinitions()
        {
            // create the values
            for (int x = 0; x < Utilities.MaxValueDefinitionCount; x++)
            {
                // plus data
                int id = Utilities.InternalId++;
                List<SensorData> valList = new List<SensorData>();
                for (int count = 0; count < Utilities.ValueCount; count++)
                {
                    SensorData data = new SensorData();
                    data.Value = x.ToString();
                    data.GeneratedWhen = DateTime.Now;
                    data.CorrelationId = Guid.NewGuid().ToString();

                    valList.Add(data);
                }

                // add to the dictionary
                ValueDefinition def = new ValueDefinition(id, id.ToString(), valList);
                ValueManager.Instance.AddValueDefinition(def);
            }

            // and now check that it is true
            Assert.IsTrue(ValueManager.Instance.ValueDefinitionDictionary.Keys.Count == Utilities.MaxValueDefinitionCount);
        }

		[TestMethod]
		public void TestMaxHistoricAndUnsavedValues()
		{
			int startId = Utilities.CreateDefinitions(1);

			ValueManager.Instance[startId].MaxHistoricValues = (Utilities.ValueCount / 2);
			
			// store in each variable
			for (int x = 0; x < Utilities.ValueCount; x++)
			{
				ValueManager.Instance[startId].CurrentValue = x;
			}

			// check the historic values
            Assert.IsTrue(ValueManager.Instance[startId].GetHistoricValues().Length < Utilities.ValueCount);
            Assert.IsTrue(ValueManager.Instance[startId].GetHistoricValues().Length == ValueManager.Instance[startId].MaxHistoricValues);
		}

        #region Private methods...
        private void AssertAreEqual(Sensor sensor, ValueDefinition definition)
        {
            if (sensor == null || definition == null)
                Assert.Fail();

            Assert.AreEqual(sensor.InternalSensorId, definition.InternalId);
            Assert.AreEqual(sensor.Description, definition.Description);
            Assert.AreEqual(sensor.SensorValueDataType, (SensorValueDataType)definition.ValueTypeCode);
            Assert.AreEqual(sensor.UnitSymbol, definition.ValueUnitOfMeasure);
            Assert.AreEqual(sensor.ShallSensorDataBePersisted, definition.ShallSensorDataBePersisted);
            Assert.AreEqual(sensor.Id, definition.Name);


            if (sensor.IsVirtualSensor){
                VirtualValueDefinition virtualDefinition = (VirtualValueDefinition)definition;
                Assert.AreEqual(sensor.VirtualSensorDefinition.VirtualSensorCalculationType, virtualDefinition.VirtualValueCalculationType);
                if (sensor.VirtualSensorDefinition.VirtualSensorDefinitionType != VirtualSensorDefinitionType.Undefined)
                   Assert.AreEqual(1, ValueManager.Instance.ValueCallbackDictionary[virtualDefinition].Count);
            }
        }

        private void AssertAreEqual(ValueDefinition expected, ValueDefinition received)
        {
            if (expected == received)
            {
                return;
            }
            if (expected == null || received == null)
            {
                Assert.Fail();
            }

            Assert.AreEqual(expected.CurrentValue, received.CurrentValue);
            Assert.AreEqual(expected.Description, received.Description);
            Assert.AreEqual(expected.GetHistoricValues(), received.GetHistoricValues());
            Assert.AreEqual(expected.InternalId, received.InternalId);
            Assert.AreEqual(expected.IsVirtualValue, received.IsVirtualValue);
            Assert.AreEqual(expected.MaxHistoricValues, received.MaxHistoricValues);
            Assert.AreEqual(expected.Name, received.Name);
            Assert.AreEqual(expected.ValueTypeCode, received.ValueTypeCode);
            Assert.AreEqual(expected.ValueUnitOfMeasure, received.ValueUnitOfMeasure);
        }

        private void AssertAreEqual(SensorData expected, SensorData received)
        {
            if (expected == received)
            {
                return;
            }
            if (expected == null || received == null)
            {
                Assert.Fail();
            }

            Assert.AreEqual(expected.GeneratedWhen, received.GeneratedWhen);
            Assert.AreEqual(expected.Value, received.Value);
            Assert.AreEqual(expected.CorrelationId, received.CorrelationId);
        }

        private Sensor CreateTestSensor(bool virtualSensor)
        {
            Sensor sensor = new Sensor();
            sensor.Category = "Category" + Guid.NewGuid().ToString();
            sensor.Description = "Description" + Guid.NewGuid().ToString();
            sensor.DeviceId = Guid.NewGuid().ToString();
            sensor.Id = Guid.NewGuid().ToString();
            sensor.InternalSensorId = Utilities.InternalId++;
            sensor.IsVirtualSensor = virtualSensor;
            sensor.PersistDirectlyAfterChange = Utilities.InternalId % 2 == 0;
            sensor.PullFrequencyInSeconds = Utilities.InternalId++;
            sensor.PullModeCommunicationType = (PullModeCommunicationType)(Utilities.InternalId++ % Enum.GetNames(typeof(PullModeCommunicationType)).Length);
            sensor.PullModeDotNetObjectType = "x.x.x,x";
            sensor.SensorDataRetrievalMode = (SensorDataRetrievalMode)(Utilities.InternalId++ % Enum.GetNames(typeof(SensorDataRetrievalMode)).Length);
            sensor.SensorValueDataType = (SensorValueDataType)(Utilities.InternalId++ % Enum.GetNames(typeof(SensorValueDataType)).Length);
            sensor.ShallSensorDataBePersisted = Utilities.InternalId % 2 == 0;
            sensor.UnitSymbol = "AbbA";
            if (virtualSensor)
            {
                sensor.VirtualSensorDefinition = new VirtualSensorDefinition();
                sensor.VirtualSensorDefinition.Definition = "ValueManagement.Test.MultiplyBy3Callback, ValueManagement.Test";
                sensor.VirtualSensorDefinition.VirtualSensorCalculationType = VirtualSensorCalculationType.OnChange;
                sensor.VirtualSensorDefinition.VirtualSensorDefinitionType = VirtualSensorDefinitionType.DotNetObject;
            }

            return sensor;
        }
        #endregion
    }
}
