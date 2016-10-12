using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

using Microsoft.Scripting.Hosting;
using IronRuby.Hosting;
using Microsoft.Scripting;

using log4net;
using IronRuby;

namespace ValueManagement.DynamicCallback
{
    /// <summary>
    /// The class implements the callback mechanism for Ruby scripting.
    /// </summary>
	public class RubyCallback : AbstractCallback
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The language definition for Ruby
		/// </summary>
		public const string RubyLanguage = "ruby";

		/// <summary>
		/// the suffix after a callback name
		/// </summary>
		public const string CallbackSuffix = "Callback";

		private ScriptRuntime runtime;
		
		/// <summary>
		/// Gets the runtime environment as a Singleton. If no runtime is set up a new runtime is created
		/// </summary>
		/// <value>The runtime.</value>
		protected ScriptRuntime Runtime
		{
			get
			{
				lock (this)
				{
					if (runtime == null)
					{
						switch (CallbackImplementation)
						{
							case CallbackImplementation.Ruby:
                                ScriptRuntimeSetup setup = new ScriptRuntimeSetup();
                                setup.DebugMode = true;
                                setup.LanguageSetups.Add(Ruby.CreateRubySetup());
								runtime = Ruby.CreateRuntime(setup);
								break;
							default:
								throw new InvalidOperationException(string.Format(Properties.Resources.ErrorCallbackTypeNotSupported, CallbackImplementation));
						}

						// just make sure all types are available in the context
						runtime.LoadAssembly(Assembly.GetExecutingAssembly()); 
						runtime.LoadAssembly(Assembly.GetCallingAssembly());
					}
				}

				return (runtime);
			}
		}

		/// <summary>
		/// Gets the engine.
		/// </summary>
		/// <returns></returns>
		protected ScriptEngine Engine
		{
			get { return(Runtime.GetEngine(RubyLanguage)); }
		}

		/// <summary>
		/// Gets or sets the source of the script
		/// </summary>
		/// <value>The source.</value>
		protected ScriptSource Source { get; set; }

		/// <summary>
		/// Gets or sets the python callback function 
		/// </summary>
		/// <value>The python callback.</value>
		protected Func<object, object> RubyCallbackFunc { get; set; }

		/// <summary>
		/// Initializes the execution environment. After creating the runtime and scope, the script / file is loaded and compiled so that is is ready for execution later
		/// </summary>
		/// <returns><c>true</c> if initialization is done correctly, otherwise <c>false</c> </returns>
		protected override bool InitializeExecutionEnvironment()
		{
			try
			{
				CheckParameters();

				// if the script is stored externally and the file is a valid file 
				if (ExternalFileName != null)
				{
					if (File.Exists(ExternalFileName))
					{
						// construct the source from the file
						Source = Engine.CreateScriptSourceFromFile(ExternalFileName);
					}
					else
					{
						throw new InvalidOperationException(string.Format(Properties.Resources.ScriptFileDoesNotExist, ExternalFileName));
					}
				}
				else
				{
					// construct the source from the expression
					Source = Engine.CreateScriptSourceFromString(this.ExecutionExpression);
				}

				// now create the scope the script shall run in
				ScriptScope scope = Runtime.CreateScope();
				Source.Execute(scope);

				// and try to get the function which is called
				Func<object, object> myCallback;

				// the name of the callback is always the name of the kind + the suffix
				string callbackName = CallbackType.ToString() + CallbackSuffix;

				if (scope.TryGetVariable<Func<object, object>>(callbackName, out myCallback))
				{
					RubyCallbackFunc = myCallback;
				}
				else
				{
					throw new InvalidOperationException(string.Format(Properties.Resources.ErrorRubyCallbackIsNotDefined, callbackName));
				}
				IsInitialized = true;


				return (true);
			}
			catch (System.Exception ex)
			{
				log.LogException("InitializeExecutionEnvironment", Properties.Resources.ErrorInitScriptEnvironment, ex);
			}

			return (false);
		}

		/// <summary>
		/// Executes this instance.
		/// </summary>
		/// <param name="data">The data to be passed in.</param>
		/// <returns>
		/// the result of the call. In case <c>null</c> is returned no action is taken
		/// </returns>
		public override CallbackResultData ExecuteCallback(CallbackPassInData data)
		{
			if (Runtime == null)
			{
				throw new InvalidOperationException(Properties.Resources.ErrorNoScriptRuntime);
			}

			CallbackResultData result = null;

			// Execute the source and retrieve the result
			try
			{
				dynamic scriptResult = RubyCallbackFunc(data);
				result = (scriptResult != null) && (scriptResult is CallbackResultData) ? (scriptResult as CallbackResultData) : null;
				return (result);
			}
			catch (System.Exception ex)
			{
				log.ErrorFormat(Properties.Resources.ErrorCallingRubyCallback, RubyCallbackFunc.Method.ToString(), ex.ToString());
				throw;
			}
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CompiledMethodExecution"/> class.
		/// </summary>
		/// <param name="symbolicName">Name of the symbolic.</param>
		/// <param name="callbackType">Type of the callback.</param>
		/// <param name="callbackKind">Kind of the callback.</param>
		protected RubyCallback(string symbolicName, CallbackType callbackKind)
		{
			SymbolicName = symbolicName;
			CallbackType = callbackKind;
			CallbackImplementation = CallbackImplementation.Ruby;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CompiledMethodExecution"/> class.
		/// </summary>
		/// <param name="symbolicName">Name of the symbolic.</param>
		/// <param name="callbackType">Type of the callback.</param>
		/// <param name="callbackKind">Kind of the callback.</param>
		/// <param name="executionExpression">The execution expression.</param>
		public RubyCallback(string symbolicName, CallbackType callbackKind, string executionExpression)
			: this(symbolicName, callbackKind)
		{
			ExecutionExpression = executionExpression;

			if (InitializeEnvironment() == false)
			{
				throw new InvalidOperationException(Properties.Resources.ErrorCreatingRubyCallback);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CompiledMethodExecution"/> class. This version assumes that the executionExpression is empty or null as the externalFileName will be used
		/// </summary>
		/// <param name="symbolicName">Name of the symbolic.</param>
		/// <param name="callbackType">Type of the callback.</param>
		/// <param name="callbackKind">Kind of the callback.</param>
		/// <param name="executionExpression">The execution expression.</param>
		/// <param name="externalFileName">Name of the external file.</param>
		public RubyCallback(string symbolicName, CallbackType callbackKind, string executionExpression, string externalFileName)
			: this(symbolicName, callbackKind)
		{
			// always ignored
			ExecutionExpression = null;
			ExternalFileName = externalFileName;

			if (InitializeEnvironment() == false)
			{
				throw new InvalidOperationException(Properties.Resources.ErrorCreatingRubyCallback);
			}
		}
	}
}
