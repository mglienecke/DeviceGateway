using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalDataContracts;

namespace ValueManagement.DynamicCallback
{
	/// <summary>
	/// Any data passed to a callback is in the passed in data
	/// </summary>
	[Serializable]
	public class CallbackPassInData
	{
		/// <summary>
		/// Gets or sets the underlying value definition
		/// </summary>
		/// <value>The underlying value definition.</value>
		public ValueDefinition ValueDefinition { get; set; }

		/// <summary>
		/// Gets or sets the current value. This might be different to the value when the call back chain started (which could be still in the value definition) as an interim call back has modified the value
		/// </summary>
		/// <value>The current value.</value>
		public SensorData CurrentValue { get; set; }

		/// <summary>
		/// Gets or sets the kind of the callback.
		/// </summary>
		/// <value>The kind of the callback.</value>
		public CallbackType CallbackKind { get; set; }

		/// <summary>
		/// Gets or sets the base value definition list.
		/// </summary>
		/// <value>The base value definition list.</value>
		public List<ValueDefinition> BaseValueDefinitionList { get; set; }

		/// <summary>
		/// Gets a value indicating whether base definitions are present.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if base definitions are present; otherwise, <c>false</c>.
		/// </value>
		public bool AreBaseDefinitionsPresent { get { return ((BaseValueDefinitionList != null) && (BaseValueDefinitionList.Count > 0)); } }

        /// <summary>
        /// THe method returns a value definition from the global run-time storage by the definition's id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ValueDefinition GetValueDefinition(int id)
        {
            return ValueManager.Instance.ValueDefinitionDictionary[id];
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="CallbackPassInData"/> class.
		/// </summary>
		/// <param name="callbackKind">Kind of the callback.</param>
		/// <param name="definition">The definition.</param>
		/// <param name="data">The data to use for this call back.</param>
		public CallbackPassInData(CallbackType callbackKind, ValueDefinition definition,  SensorData data)
		{
			CallbackKind = callbackKind;
			ValueDefinition = definition;
			CurrentValue = data;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CallbackPassInData"/> class. This is especially intended for virtual value definitions
		/// </summary>
		/// <param name="definition">The definition.</param>
		/// <param name="currentValue">The current value.</param>
		/// <param name="baseValueList">The base value list.</param>
		public CallbackPassInData(ValueDefinition definition, SensorData currentValue, List<ValueDefinition> baseValueList)
		{
			CallbackKind = CallbackType.VirtualValueCalculation;
            CurrentValue = currentValue;
			ValueDefinition = definition;
			BaseValueDefinitionList = baseValueList;
		}
	}
}
