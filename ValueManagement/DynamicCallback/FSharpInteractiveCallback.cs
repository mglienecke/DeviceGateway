﻿using System;
using System.Linq;
using System.Reflection;
using System.IO;
using GlobalDataContracts;
using FSharp.Compiler.CodeDom;

using System.CodeDom.Compiler;

namespace ValueManagement.DynamicCallback
{
    /// <summary>
    /// The class implements the callback mechanism for F# code that get compiled during the runtime.
    /// The code is expected to be in the following form:
    /// <code>
    /// open ValueManagement.DynamicCallback
    /// open GlobalDataContracts
    /// open System
    /// 
    /// type FSharpInteractiveClass() =
    /// static member GeneralCheckCallback(data: CallbackPassInData) =
    ///     let result = new CallbackResultData()
    ///     result.IsValueModified <- true
    ///     result.NewValue <- new SensorData()
    ///     result.NewValue.Value <- Convert.ToString(Convert.ToInt32(data.CurrentValue.Value) * 3)
    ///     result
    /// </code>
    /// </summary>
	public class FSharpInteractiveCallback : AbstractCallback
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// the suffix after a callback name
		/// </summary>
		public const string CallbackSuffix = "Callback";

        public const string FSharpInteractiveClassName = "Program+FSharpInteractiveClass";
		
		/// <summary>
		/// Gets or sets the F# callback method function 
		/// </summary>
		/// <value>The F# callback.</value>
        protected MethodInfo ExecutionMethod { get; set; }

		/// <summary>
		/// Initializes the execution environment. After creating the runtime and scope, the script / file is loaded and compiled so that is is ready for execution later
		/// </summary>
		/// <returns><c>true</c> if initialization is done correctly, otherwise <c>false</c> </returns>
		protected override bool InitializeExecutionEnvironment()
		{
			try
			{
				CheckParameters();

                //Setup the compiler
                CompilerParameters compilerParams = new CompilerParameters();
                string outputDirectory = Directory.GetCurrentDirectory();

                compilerParams.GenerateInMemory = true;
                compilerParams.TreatWarningsAsErrors = false;
                compilerParams.GenerateExecutable = false;
                compilerParams.CompilerOptions = "/optimize";

                //Get references to the necessary assemblies
                SensorData sensor = new SensorData(DateTime.Now, "", null);

			    Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                Assembly globalDataContractsAssembly = loadedAssemblies.Where(x => x.FullName.StartsWith("GlobalDataContracts,")).First();
                Assembly valueManagementAssembly = loadedAssemblies.Where(x => x.FullName.StartsWith("ValueManagement,")).First();

                string[] references = { "System.dll", valueManagementAssembly.Location, globalDataContractsAssembly.Location };
                compilerParams.ReferencedAssemblies.AddRange(references);

                FSharpCodeProvider provider = new FSharpCodeProvider();
                CompilerResults compile;

				// Compile the code
				if (ExternalFileName != null)
				{
					if (File.Exists(ExternalFileName))
					{
                        compile = provider.CompileAssemblyFromFile(compilerParams, ExternalFileName);
					}
					else
					{
						throw new InvalidOperationException(string.Format(Properties.Resources.ScriptFileDoesNotExist, ExternalFileName));
					}
				}
				else
				{
					// construct the source from the expression
                    compile = provider.CompileAssemblyFromSource(compilerParams, ExecutionExpression);
				}

                //Check for errors
                if (compile.Errors.HasErrors)
                {
                    string text = Properties.Resources.ErrorCompileError;
                    foreach (CompilerError ce in compile.Errors)
                    {
                        text += "\r\n" + ce.ToString();
                    }
                    throw new InvalidOperationException(text);
                }

                //Get the method by reflection
                Module module = compile.CompiledAssembly.GetModules()[0];
                Type callbackType = null;

                if (module != null)
                {
                    callbackType = module.GetType(FSharpInteractiveClassName);
                }
                else
                {
                    throw new InvalidOperationException(Properties.Resources.ErrorFSharpInteractiveCallbackIsNotCompiled);
                }

                // the name of the callback is always the name of the kind + the suffix
                string callbackMethodName = CallbackType.ToString() + CallbackSuffix;

                if (callbackType != null)
                {
                    ExecutionMethod = callbackType.GetMethod(callbackMethodName);
                }
                else{
                    throw new InvalidOperationException(String.Format(Properties.Resources.ErrorFSharpInteractiveCallbackTypeIsNotFound, FSharpInteractiveClassName));
                }

                if (ExecutionMethod == null)
                {
                    throw new InvalidOperationException(string.Format(Properties.Resources.ErrorFSharpInteractiveCallbackMethodIsNotFound, callbackMethodName));
                }

				IsInitialized = true;

				return (true);
			}
			catch (System.Exception ex)
			{
				log.LogException("InitializeExecutionEnvironment", Properties.Resources.ErrorInitFSharpInteractiveEnvironment, ex);
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
			CallbackResultData result = null;

			// Execute the source and retrieve the result
			try
			{
				dynamic scriptResult = ExecutionMethod.Invoke(null, new object[]{data});
				result = (scriptResult != null) && (scriptResult is CallbackResultData) ? (scriptResult as CallbackResultData) : null;
				return (result);
			}
			catch (System.Exception ex)
			{
				log.ErrorFormat(Properties.Resources.ErrorCallingFSharpInteractiveCallback, ExecutionMethod.Name, ex.ToString());
				throw;
			}
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CompiledMethodExecution"/> class.
		/// </summary>
		/// <param name="symbolicName">Name of the symbolic.</param>
		/// <param name="callbackType">Type of the callback.</param>
		/// <param name="callbackKind">Kind of the callback.</param>
		protected FSharpInteractiveCallback(string symbolicName, CallbackType callbackKind)
		{
			SymbolicName = symbolicName;
			CallbackType = callbackKind;
			CallbackImplementation = CallbackImplementation.FSharpInteractive;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CompiledMethodExecution"/> class.
		/// </summary>
		/// <param name="symbolicName">Name of the symbolic.</param>
		/// <param name="callbackType">Type of the callback.</param>
		/// <param name="callbackKind">Kind of the callback.</param>
		/// <param name="executionExpression">The execution expression.</param>
		public FSharpInteractiveCallback(string symbolicName, CallbackType callbackKind, string executionExpression)
			: this(symbolicName, callbackKind)
		{
			ExecutionExpression = executionExpression;

			if (InitializeEnvironment() == false)
			{
                throw new InvalidOperationException(string.Format(Properties.Resources.ErrorInitFSharpInteractiveEnvironment, "general fault"));
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
        public FSharpInteractiveCallback(string symbolicName, CallbackType callbackKind, string executionExpression, string externalFileName)
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
