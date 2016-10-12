using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalDataContracts;
using ValueManagement.DynamicCallback;

namespace CentralServerService
{
    public class AverageCalculationCallback : ValueManagement.DynamicCallback.ICompiledMethodExecutionCallback
    {
        int mCount = 0;
        decimal mAverage = 0;

        #region ICompiledMethodExecutionCallback members...

        /// <summary>
        /// create the virtual value as the sum of its dependent values
        /// </summary>
        /// <param name="passInData"></param>
        /// <returns>
        /// result data or <c>null</c> if no result is intended
        /// </returns>
        public CallbackResultData PeformCallback(CallbackPassInData passInData)
        {
            CallbackResultData result = new CallbackResultData();
            result.IsValueModified = true;

            // get the value of the bases
            mAverage = (mAverage * mCount + passInData.BaseValueDefinitionList[0].CurrentValue) / ++mCount;

            result.NewValue = new SensorData(mAverage.ToString());
            return (result);
        }

        #endregion
    }
}
