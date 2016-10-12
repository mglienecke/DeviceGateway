using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ValueManagement.DynamicCallback;
using GlobalDataContracts;

namespace ValueManagement.Test
{
	public class ValueCallback : ValueManagement.DynamicCallback.ICompiledMethodExecutionCallback
	{

		#region ICompiledMethodExecutionCallback Member

		/// <summary>
		/// accept the parameters coming in and deliver a result
		/// </summary>
		/// <param name="passInData"></param>
		/// <returns>
		/// result data or <c>null</c> if no result is intended
		/// </returns>
		public DynamicCallback.CallbackResultData PeformCallback(DynamicCallback.CallbackPassInData passInData)
		{
			Utilities.CalledLevelList.Add(passInData.CallbackKind);
			return (null);
		}

		#endregion
	}

    /// <summary>
    /// The class implements a test callback that multiplies the passed in value by three.
    /// </summary>
	public class MultiplyBy3Callback : ValueManagement.DynamicCallback.ICompiledMethodExecutionCallback
	{

		#region ICompiledMethodExecutionCallback Members...

		/// <summary>
		/// accept the parameters coming in and deliver a result which is 3 times the input
		/// </summary>
		/// <param name="passInData"></param>
		/// <returns>
		/// result data or <c>null</c> if no result is intended
		/// </returns>
		public DynamicCallback.CallbackResultData PeformCallback(DynamicCallback.CallbackPassInData passInData)
		{
			CallbackResultData result = new CallbackResultData();
			result.IsValueModified = true;
			result.NewValue = passInData.CurrentValue * 3;
			return (result);
		}

		#endregion
	}

	public class VirtualValueEvaluationCallback : ValueManagement.DynamicCallback.ICompiledMethodExecutionCallback
	{

		#region ICompiledMethodExecutionCallback Member

		/// <summary>
		/// create the virtual value as the sum of its dependent values
		/// </summary>
		/// <param name="passInData"></param>
		/// <returns>
		/// result data or <c>null</c> if no result is intended
		/// </returns>
		public DynamicCallback.CallbackResultData PeformCallback(DynamicCallback.CallbackPassInData passInData)
		{
			CallbackResultData result = new CallbackResultData();
			result.IsValueModified = true;

			int value = 0;

			// get the value of the bases
			foreach (ValueDefinition baseDefs in passInData.BaseValueDefinitionList)
			{
				value += baseDefs.CurrentValue;
			}

			// and multiply as a test
			value = value * 2;

			result.NewValue = new SensorData(value.ToString());
			return (result);
		}

		#endregion
	}

	public class InvalidValueCallback
	{
		/// <summary>
		/// Simple callback function
		/// </summary>
		/// <param name="inData"></param>
		/// <returns></returns>
		public DynamicCallback.CallbackResultData PeformCallback(DynamicCallback.CallbackPassInData inData)
		{
			throw new InvalidOperationException("this code should never be reached");
		}
	}



}
