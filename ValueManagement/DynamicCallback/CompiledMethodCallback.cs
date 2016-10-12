using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using log4net;

namespace ValueManagement.DynamicCallback
{
	/// <summary>
	/// Execute a compiled method. This implies that the method has a proper <see cref="Type"/> which is stored in <see cref="ExecutionData"/> and optionally an 
	/// assembly path in <see cref="ExternalFileName"/>
	/// </summary>
	public class CompiledMethodCallback : AbstractCallback
	{
		// our logger
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private Type executionType;

		/// <summary>
		/// Gets the type of the execution.
		/// </summary>
		/// <value>The type of the execution.</value>
		public Type ExecutionType { get { return (executionType); } }

		/// <summary>
		/// Gets or sets the real callback to be called (the instance of the type).
		/// </summary>
		/// <value>The callback.</value>
		private ICompiledMethodExecutionCallback Callback { get; set; }

		/// <summary>
		/// Sets the type of the execution.
		/// </summary>
		/// <param name="type">The type.</param>
		public void SetExecutionType(Type type)
		{
			// and figure out if this is an instance
			if (type.GetInterface(typeof(ICompiledMethodExecutionCallback).Name) == null)
			{
				log.ErrorFormat(Properties.Resources.ErrorInterfaceNotImplemented, typeof(ICompiledMethodExecutionCallback).Name, type.Name);
				throw new InvalidOperationException(Properties.Resources.ErrorInterfaceNotImplemented);
			}

			executionType = type;
		}

		/// <summary>
		/// Executes this instance. Try loading the type (using - if specified - the filename of the assembly)
		/// </summary>
		/// <param name="passInData">The pass in data.</param>
		/// <returns>returns the result of the initial call</returns>
		public override CallbackResultData ExecuteCallback(CallbackPassInData passInData)
		{
			CallbackResultData result = null;

			// always test for it
			if (InitializeEnvironment())
			{
				result = Callback.PeformCallback(passInData);
			}

			return (result);
		}

		/// <summary>
		/// Initializes the execution environment. This will be called only once for any object so global initialization shall be placed here. It is checked if the type can be loaded, implements the proper
		/// interface <see cref="MethodExecutionCallbackInterface"/> and then the type is stored for later execution
		/// </summary>
		/// <returns><c>true</c> if initialization is done correctly, otherwise <c>false</c> </returns>
		protected override bool InitializeExecutionEnvironment()
		{
			try
			{
				CheckParameters();

				// now the real instance is created
				Callback = (ICompiledMethodExecutionCallback)Activator.CreateInstance(ExecutionType);

				IsInitialized = true;

				return (true);
			}
			catch (System.Exception ex)
			{
				log.LogException("InitializeExecutionEnvironment", Properties.Resources.ErrorInitMethodCallEnvironment, ex);
			}

			return (false);
		}

		/// <summary>
		/// Checks the parameters which have to be set for all instances. These are <see cref="SymbolicName"/>, the <see cref="ExecutionType"/> and either <see cref="ExecutionExpression"/>
		/// or <see cref="ExternalFileName"/>.
		/// If any of these requirements are not met and <see cref="InvalidOperationException"/> is thrown
		/// </summary>
		protected override void CheckParameters()
		{
			if ((SymbolicName == null) || (SymbolicName.Length == 0))
			{
				throw new InvalidOperationException(Properties.Resources.NoSymbolicName);
			}

			if (CallbackImplementation != CallbackImplementation.DotNetMethod)
			{
				throw new InvalidOperationException(string.Format(Properties.Resources.ErrorInvalidCallbackType, CallbackImplementation));
			}
			if (ExecutionType == null)
			{
				throw new InvalidOperationException(Properties.Resources.ErrorNoExecutionType);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CompiledMethodExecution"/> class.
		/// </summary>
		/// <param name="symbolicName">Name of the symbolic.</param>
		/// <param name="callbackKind">Kind of the callback.</param>
		protected CompiledMethodCallback(string symbolicName, CallbackType callbackKind)
		{
			SymbolicName = symbolicName;
			CallbackType = callbackKind;
			CallbackImplementation = CallbackImplementation.DotNetMethod;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CompiledMethodExecution"/> class.
		/// </summary>
		/// <param name="symbolicName">Name of the symbolic.</param>
		/// <param name="callbackKind">Kind of the callback.</param>
		/// <param name="callbackType">Type of the callback.</param>
		public CompiledMethodCallback(string symbolicName, CallbackType callbackKind, Type callbackType)
			: this(symbolicName, callbackKind)
		{
			SetExecutionType(callbackType);
		}
	}
}
