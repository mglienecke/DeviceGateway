using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ValueManagement.DynamicCallback;

using log4net;
using GlobalDataContracts;

namespace ValueManagement
{
	/// <summary>
	/// defines a virtual value
	/// </summary>
	public class VirtualValueDefinition : ValueDefinition
	{
		// our logger
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// the default cycle time is 5 minutes
		/// </summary>
		public static readonly TimeSpan DefaultCycleTime = new TimeSpan(0, 5, 0);

        #region Constructors...
        /// <summary>
		/// Initializes a new instance of the <see cref="VirtualValueDefinition"/> class.
		/// </summary>
		/// <param name="internalId">The internal id.</param>
		/// <param name="name">The name.</param>
		/// <param name="calculationType">Type of the calculation for virtual values.</param>
        public VirtualValueDefinition(int internalId, string name, VirtualSensorCalculationType calculationType)
			: base(internalId, name)
		{
			VirtualValueCalculationType = calculationType;
			CycleTime = DefaultCycleTime;
            //Historic values are not kept for virtual value definitions.
            KeepHistoricValues = false;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VirtualValueDefinition"/> class.
		/// </summary>
		/// <param name="internalId">The internal id.</param>
		/// <param name="name">The name.</param>
		/// <param name="calculationType">Type of the calculation for virtual values.</param>
		/// <param name="callbackName">Name of the callback for the value evaluation.</param>
		public VirtualValueDefinition(int internalId, string name, VirtualSensorCalculationType calculationType, string callbackName)
			: this(internalId, name, calculationType)
		{
			VirtualValueEvaluationCallback = callbackName;
		}
        #endregion

        #region Properties...
        /// <summary>
        /// Gets a value indicating whether this instance is a virtual value. Yes, we are
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a virtual value; otherwise, <c>false</c>.
        /// </value>
        public override bool IsVirtualValue { get { return true; } }

        /// <summary>
        /// Gets or sets the type of the virtual value calculation.
        /// </summary>
        /// <value>The type of the virtual value calculation.</value>
        public VirtualSensorCalculationType VirtualValueCalculationType { get; set; }

        /// <summary>
        /// Gets or sets the cycle time.
        /// </summary>
        /// <value>The cycle time.</value>
        public TimeSpan CycleTime { get; set; }

        /// <summary>
        /// Gets or sets the last value calculation.
        /// </summary>
        /// <value>The last value calculation.</value>
        public DateTime LastValueCalculation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a recalculation is needed. Usually this is set after an underlying value changes but no direct update was done
        /// </summary>
        /// <value>
        /// 	<c>true</c> if a recalculation is needed; otherwise, <c>false</c>.
        /// </value>
        public bool IsRecalculationNeeded { get; set; }

        /// <summary>
        /// Gets or sets the virtual value evaluation callback.
        /// </summary>
        /// <value>The virtual value evaluation callback.</value>
        public string VirtualValueEvaluationCallback { get; set; }

        /// <summary>
        /// Gets a value indicating whether the virtual evaluation callback is defined.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the virtual evaluation callback is defined; otherwise, <c>false</c>.
        /// </value>
        public bool IsVirtualEvaluationCallbackDefined { 
            get { 
                return ((VirtualValueEvaluationCallback != null) && (VirtualValueEvaluationCallback.Length > 0)); 
            } 
        }

        /// <summary>
        /// Gets or sets the current value. Getting starts the calculation process if the callback is defined
        /// </summary>
        /// <value>The current value.</value>
        public override SensorData CurrentValue
        {
            get
            {
                // if there is a need for a recalculation this is done right now whatever the classical approach for the recalculation might be
                //if (IsRecalculationNeeded && IsVirtualEvaluationCallbackDefined)
                if (VirtualValueCalculationType == VirtualSensorCalculationType.OnRequest && IsVirtualEvaluationCallbackDefined)
                {
                    CalculateVirtualValue();
                }
                return base.CurrentValue;
            }
            set
            {
                base.CurrentValue = value;
            }
        }
        #endregion

		/// <summary>
		/// Calculates the virtual value. If the callback is defined and the key is present then the callback is performed
		/// </summary>
		public void CalculateVirtualValue()
		{
			// Perform the recalculation
			if (ValueManager.Instance.CallbackDictionary.ContainsKey(VirtualValueEvaluationCallback))
			{
				// get the call back
				AbstractCallback callback = ValueManager.Instance.CallbackDictionary[VirtualValueEvaluationCallback];

				// we can only use virtual value callback types now
				if (callback.CallbackType != CallbackType.VirtualValueCalculation)
				{
					// so log the error and skip the entry
					log.ErrorFormat(Properties.Resources.ErrorInvalidCallbackTypeForVirtualValues, callback.SymbolicName, callback.CallbackType);
					return;
				}

				// and call - the result is the value of the virtual value
				try
				{
					CallbackResultData result = callback.ExecuteCallback(new CallbackPassInData(this, base.CurrentValue, ValueManager.Instance.GetBaseValueDefinitions(this).ToList()));
					if (result != null && result.IsCancelled == false)
					{
					    if (result.IsValueModified)
					    {
					        // setting the value might trigger the next chain of call backs
					        CurrentValue = result.NewValue;

					        // store the recalculation timestamp as well as reset the flag that it is needed
					        LastValueCalculation = DateTime.Now;
					        IsRecalculationNeeded = false;
					    }
					    else
					    {
                            // store the recalculation timestamp as well as reset the flag that it is needed
                            LastValueCalculation = DateTime.Now;
                            IsRecalculationNeeded = false; 
					    }
					}
				}
				catch (System.Exception ex)
				{
                    log.LogException("CalculateVirtualValue", Properties.Resources.ErrorExecuteCallback, ex);
				}
			}
			else
			{
				log.ErrorFormat(Properties.Resources.ErrorVirtualEvalCallbackKeyNotFound, VirtualValueEvaluationCallback);
			}
		}
	}
}
