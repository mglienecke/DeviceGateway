using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

using Microsoft.CSharp;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;

using log4net;
using System.CodeDom.Compiler;
using GlobalDataContracts;

namespace ValueManagement.DynamicCallback
{
    /// <summary>
    /// The class implements the callback mechanism for C# code that get compiled during the runtime.
    /// The code is expected to be in the following form:
    /// <code>
    /// using System;
    /// using ValueManagement.DynamicCallback;
    /// using GlobalDataContracts;
    /// 
    /// public class CSharpInteractiveClass
    /// {
    ///     public static CallbackResultData GeneralCheckCallback(CallbackPassInData data)
    ///     {
    ///        //implementation code
    ///     }
    /// }
    /// </code>
    /// </summary>
	public class CSharpInteractiveCallback : AbstractCallback
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// the suffix after a callback name
		/// </summary>
		public const string CallbackSuffix = "Callback";

        public const string CSharpInteractiveClassName = "CSharpInteractiveClass";
		
		/// <summary>
		/// Gets or sets the C# callback method function 
		/// </summary>
		/// <value>The C# callback.</value>
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

                Assembly globalDataContractsAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("GlobalDataContracts,")).First();
                Assembly valueManagementAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("ValueManagement,")).First();

                string[] references = { "System.dll", "System.Linq.dll", "System.Core.dll", valueManagementAssembly.Location, globalDataContractsAssembly.Location };
                compilerParams.ReferencedAssemblies.AddRange(references);

                CSharpCodeProvider provider = new CSharpCodeProvider();
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
                    callbackType = module.GetType("CSharpInteractiveClass");
                }
                else
                {
                    throw new InvalidOperationException(Properties.Resources.ErrorCSharpInteractiveCallbackIsNotCompiled);
                }

                // the name of the callback is always the name of the kind + the suffix
                string callbackMethodName = CallbackType.ToString() + CallbackSuffix;

                if (callbackType != null)
                {
                    ExecutionMethod = callbackType.GetMethod(callbackMethodName);
                }
                else
                {
                    throw new InvalidOperationException(String.Format(Properties.Resources.ErrorCSharpInteractiveCallbackTypeIsNotFound, CSharpInteractiveClassName));
                }

                if (ExecutionMethod == null)
                {
                    throw new InvalidOperationException(string.Format(Properties.Resources.ErrorCSharpInteractiveCallbackMethodIsNotFound, callbackMethodName));
                }

                IsInitialized = true;

                return (true);
            }
            catch (InvalidOperationException exc)
            {
                log.LogException("InitializeExecutionEnvironment", Properties.Resources.ErrorInitCSharpInteractiveEnvironment, exc);
                throw;
            }
            catch (System.Exception ex)
            {
                log.LogException("InitializeExecutionEnvironment", Properties.Resources.ErrorInitCSharpInteractiveEnvironment, ex);
                throw new InvalidOperationException(Properties.Resources.ErrorInitCSharpInteractiveEnvironment, ex);
            }
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
				log.ErrorFormat(Properties.Resources.ErrorCallingCSharpInteractiveCallback, ExecutionMethod.Name, ex.ToString());
				throw;
			}
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="CompiledMethodExecution"/> class.
		/// </summary>
		/// <param name="symbolicName">Name of the symbolic.</param>
		/// <param name="callbackType">Type of the callback.</param>
		/// <param name="callbackKind">Kind of the callback.</param>
		protected CSharpInteractiveCallback(string symbolicName, CallbackType callbackKind)
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
		public CSharpInteractiveCallback(string symbolicName, CallbackType callbackKind, string executionExpression)
			: this(symbolicName, callbackKind)
		{
			ExecutionExpression = executionExpression;

			if (InitializeEnvironment() == false)
			{
				throw new InvalidOperationException(Properties.Resources.ErrorCallingCSharpInteractiveCallback);
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
        public CSharpInteractiveCallback(string symbolicName, CallbackType callbackKind, string executionExpression, string externalFileName)
			: this(symbolicName, callbackKind)
		{
			// always ignored
			ExecutionExpression = null;
			ExternalFileName = externalFileName;

			if (InitializeEnvironment() == false)
			{
				throw new InvalidOperationException(Properties.Resources.ErrorCreatingCSharpInteractiveCallback);
			}
		}
	}
}
