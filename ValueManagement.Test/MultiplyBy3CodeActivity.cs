using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using ValueManagement.DynamicCallback;

namespace ValueManagement.Test
{
    /// <summary>
    /// Sample CodeActivity which operates only by the provided In / Out parameters
    /// </summary>
    public sealed class MultiplyBy3CodeActivity : CodeActivity
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
            result.NewValue = inData.CurrentValue * 3;

            Result.Set(context, result);
        }
    }

    /// <summary>
    /// Sample CodeActivity which operates only by the provided In / Out parameters and performs no action
    /// </summary>
    public sealed class NopCodeActivity : CodeActivity
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
            ;
        }
    }
}
