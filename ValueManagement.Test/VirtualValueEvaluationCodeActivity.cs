using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using ValueManagement.DynamicCallback;
using GlobalDataContracts;

namespace ValueManagement.Test
{
    /// <summary>
    /// Sample CodeActivity which operates only by the provided In / Out parameters
    /// </summary>
    public sealed class VirtualValueEvaluationCodeActivity : CodeActivity
    {
        /// <summary>
        /// the input data
        /// </summary>
        public InArgument<CallbackPassInData> PassInData { get; set; }

        /// <summary>
        /// the output data
        /// </summary>
        public OutArgument<CallbackResultData> Result { get; set; }

        /// <summary>
        /// the method which just multiplies the input by 3
        /// </summary>
        /// <param name="context"></param>
        protected override void Execute(CodeActivityContext context)
        {
            var inData = PassInData.Get(context);
            CallbackResultData result = new CallbackResultData();

            result.IsValueModified = true;


            int value = 0;

            // get the value of the bases
            foreach (ValueDefinition baseDefs in inData.BaseValueDefinitionList)
            {
                value += baseDefs.CurrentValue;
            }

            // and multiply as a test
            value = value * 2;

            result.NewValue = new SensorData(value.ToString());

            Result.Set(context, result);
        }
    }
}
