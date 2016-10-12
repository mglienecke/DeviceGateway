using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValueManagement.DynamicCallback
{
	/// <summary>
	/// this interface must be implemented by any method which wants to be used as a call back
	/// </summary>
	public interface ICompiledMethodExecutionCallback
	{
		/// <summary>
		/// accept the parameters coming in and deliver a result
		/// </summary>
		/// <param name="passInData"></param>
		/// <returns>result data or <c>null</c> if no result is intended</returns>
		CallbackResultData PeformCallback(CallbackPassInData passInData);
	}
}
