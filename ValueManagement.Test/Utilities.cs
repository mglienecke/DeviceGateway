using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GlobalDataContracts;

using ValueManagement.DynamicCallback;

namespace ValueManagement.Test
{
	internal class Utilities
	{
		public static int InternalId { get; set; }
		public const int MaxValueDefinitionCount = 100000;
		public const int AvgValueDefinitionCount = 1000;
		public const int ValueCount = 10;

		public static List<DynamicCallback.CallbackType> CalledLevelList = new List<DynamicCallback.CallbackType>();
		internal static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Utilities));

		static Utilities()
		{
			InternalId = 1;
		}

        internal static int GetNextInternalId() { return InternalId++; }

		/// <summary>
		/// Clears all values.
		/// </summary>
		internal static void ClearAllValues()
		{
			List<ValueDefinition> defList = ValueManager.Instance.ValueDefinitionDictionary.Values.ToList();
			foreach (ValueDefinition def in defList)
			{
				ValueManager.Instance.RemoveValueDefinition(def);
			}

			Assert.IsTrue(ValueManager.Instance.ValueDefinitionDictionary.Count == 0);
		}

		internal static void ClearAllCallbacks()
		{
			ValueManager.Instance.ValueCallbackDictionary.Clear();
			ValueManager.Instance.CallbackDictionary.Clear();
		}

		/// <summary>
		/// Creates the specified amount of definitions.
		/// </summary>
		/// <param name="maxCount">The max count.</param>
		/// <returns>the starting id</returns>
		internal static int CreateDefinitions(int maxCount)
		{
			return (CreateDefinitions(maxCount, false, VirtualSensorCalculationType.Undefined));
		}

		/// <summary>
		/// Creates the definitions.
		/// </summary>
		/// <param name="maxCount">The max count.</param>
		/// <param name="isVirtualValue">if set to <c>true</c> [is virtual value].</param>
		/// <returns>the starting id</returns>
		internal static int CreateDefinitions(int maxCount, bool isVirtualValue, GlobalDataContracts.VirtualSensorCalculationType calculationType)
		{
			int startId = InternalId;

			// create the variables
			for (int x = 0; x < maxCount; x++)
			{
				int id = InternalId++;
				ValueDefinition def = isVirtualValue ? new VirtualValueDefinition(id, id.ToString(), calculationType) : new ValueDefinition(id, id.ToString());

				// set the counters to its maximum
				//def.MaxHistoricValues = -1;
                def.MaxHistoricValues = 200;

				ValueManager.Instance.AddValueDefinition(def);
			}

			Assert.IsTrue(ValueManager.Instance.ValueDefinitionDictionary.Count == maxCount);

			return (startId);
		}
	}
}
