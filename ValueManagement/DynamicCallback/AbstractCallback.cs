using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValueManagement.DynamicCallback
{
	/// <summary>
	/// the abstract base class for any execution instance
	/// </summary>
	public abstract class AbstractCallback
	{
		/// <summary>
		/// The property contains the call back's symbolic name. This must be unique among all callbacks
		/// </summary>
		public string SymbolicName { get; set; }

		/// <summary>
		/// Gets or sets the kind of the callback.
		/// </summary>
		/// <value>The kind of the callback.</value>
		public CallbackType CallbackType { get; set; }

		/// <summary>
		/// Gets or sets the type of the execution.
		/// </summary>
		/// <value>The type of the execution.</value>
		public CallbackImplementation CallbackImplementation { get; set; }

		/// <summary>
		/// Gets or sets the execution expression. In case a scripting type (Python, etc.) is used, the script can be directly stored here. For SQL stored procedures the name of the method.
		/// </summary>
		/// <value>The execution script.</value>
		public string ExecutionExpression { get; set; }

		/// <summary>
		/// Gets or sets the name of the external file where a script is located 
		/// </summary>
		/// <value>The name of the external file.</value>
		public string ExternalFileName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is initialized.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is initialized; otherwise, <c>false</c>.
		/// </value>
		public bool IsInitialized { get; set; }

		/// <summary>
		/// Executes this instance. This method has to be overridden in derived classes
		/// </summary>
		/// <param name="data">The data to be passed in.</param>
		/// <returns>the result of the call. In case <c>null</c> is returned no action is taken</returns>
		public abstract CallbackResultData ExecuteCallback(CallbackPassInData data);

		/// <summary>
		/// Initializes the execution environment. This will be called only once for any object so global initialization shall be placed here
		/// </summary>
		/// <returns><c>true</c> if initialization is done correctly, otherwise <c>false</c> </returns>
		protected abstract bool InitializeExecutionEnvironment();

		/// <summary>
		/// Initializes the environment if it is needed
		/// </summary>
		/// <returns><c>true</c> if initialization is done correctly, otherwise <c>false</c> </returns>
		public bool InitializeEnvironment()
		{
			if (IsInitialized == false)
			{
				return (InitializeExecutionEnvironment());
			}

			return (true);
		}

		/// <summary>
		/// Checks the parameters which have to be set for all instances. These are <see cref="SymbolicName"/>, the <see cref="ExecutionType"/> and either <see cref="ExecutionExpression"/> 
		/// or <see cref="ExternalFileName"/>.
		/// If any of these requirements are not met and <see cref="InvalidOperationException"/> is thrown
		/// </summary>
		protected virtual void CheckParameters()
		{
			if ((SymbolicName == null) || (SymbolicName.Length == 0))
			{
				throw new InvalidOperationException(Properties.Resources.NoSymbolicName);
			}

			if (CallbackImplementation == CallbackImplementation.Undefined)
			{
				throw new InvalidOperationException(Properties.Resources.ErrorNoExecutionType);
			}

			if (((ExecutionExpression == null) || (ExecutionExpression.Length == 0)) && ((ExternalFileName == null) || (ExternalFileName.Length == 0)))
			{
				throw new InvalidOperationException(Properties.Resources.NoExecutionDataAndFileName);
			}
		}
	}
}
