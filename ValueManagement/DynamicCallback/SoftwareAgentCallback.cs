using System;
using System.Activities;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using log4net;

namespace ValueManagement.DynamicCallback
{
    /// <summary>
    /// Execute a software agent
    /// </summary>
    public class SoftwareAgentCallback : AbstractCallback
    {
        /// <summary>
        /// the name for passed in data in the dictionary
        /// </summary>
        public const string SoftwareAgentPassInDataName = "PassInData";

        /// <summary>
        /// the name for the result in the dictionary
        /// </summary>
        public const string SoftwareAgentResultName = "Result";

        /// <summary>
        /// the type of the In argument to the software agent
        /// </summary>
        public static readonly Type SoftwareAgentInDataType = typeof (InArgument<CallbackPassInData>);

        /// <summary>
        /// the type of the Out argument to the software agent
        /// </summary>
        public static readonly Type SoftwareAgentOutDataType = typeof(OutArgument<CallbackResultData>);

        // our logger
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Type softwareAgentType;

        /// <summary>
        /// Gets the type of the software agent
        /// </summary>
        /// <value>The type of the software agent.</value>
        public Type SoftwareAgentType { get { return (softwareAgentType); } }

        /// <summary>
        /// Gets or sets the real callback to be called (the instance of the type).
        /// </summary>
        /// <value>The callback to the software agent.</value>
        private Activity SoftwareAgent { get; set; }

        /// <summary>
        /// Sets the type of the execution.
        /// </summary>
        /// <param name="type">The type.</param>
        public void SetSoftwareAgentType(Type type)
        {
            string error = null;

            do
            {
                // and figure out if this is an instance
                if (type.IsSubclassOf(typeof(Activity)) == false)
                {
                    error = string.Format(Properties.Resources.ErrorTypeIsNoSoftwareAgent, type.Name);
                    break;
                }

                // Load all properties
                PropertyInfo[] properties = type.GetProperties();

                // check the inbond property first
                PropertyInfo inProp = properties.FirstOrDefault(p => p.Name == SoftwareAgentPassInDataName);
                if (inProp == null)
                {
                    error = string.Format(Properties.Resources.ErrorPropertyIsMissingInType, type.Name, SoftwareAgentPassInDataName);
                    break;
                }
                if (inProp.PropertyType != SoftwareAgentInDataType)
                {
                    error = string.Format(Properties.Resources.ErrorPropertyIsOfWrongType, type.Name, SoftwareAgentPassInDataName, inProp.PropertyType.Name, SoftwareAgentInDataType.Name);
                    break;
                }

                // then the outbound property
                PropertyInfo outProp = properties.FirstOrDefault(p => p.Name == SoftwareAgentResultName);
                if (outProp == null)
                {
                    error = string.Format(Properties.Resources.ErrorPropertyIsMissingInType, type.Name, SoftwareAgentResultName);
                    break;
                }
                if (outProp.PropertyType != SoftwareAgentOutDataType)
                {
                    error = string.Format(Properties.Resources.ErrorPropertyIsOfWrongType, type.Name, SoftwareAgentResultName, outProp.PropertyType.Name, SoftwareAgentOutDataType.Name);
                    break;
                }
            }
            while (false);

            if (error != null)
            {
                log.Error(error);
                throw new InvalidOperationException(error);
            }

            softwareAgentType = type;
        }

        /// <summary>
        /// Executes this software agent by activating it through the <see cref="WorkflowInvoker"/>
        /// </summary>
        /// <param name="passInData">The pass in data.</param>
        /// <returns>returns the result of the initial call</returns>
        public override CallbackResultData ExecuteCallback(CallbackPassInData passInData)
        {
            CallbackResultData resultData = null;

            // always test for it
            if (InitializeEnvironment())
            {
                // the dictionary for the passed in data
                Dictionary<string, object> arguments = new Dictionary<string, object>();
                arguments.Add(SoftwareAgentPassInDataName, passInData);

                // crea the invoker and call the workflow
                WorkflowInvoker invoker2 = new WorkflowInvoker(SoftwareAgent);
                IDictionary<string, object> result = invoker2.Invoke(arguments);

                // process the results
                resultData = result[SoftwareAgentResultName] as CallbackResultData;

                // This check is only relevant for Non Callback after value change
                if ((CallbackType != CallbackType.AfterChangeCallback) && resultData == null)
                {
                    log.ErrorFormat(Properties.Resources.ErrorSoftwareAgentProducedNoResult, SoftwareAgentType.Name);
                }
            }

            return (resultData);
        }

        /// <summary>
        /// Initializes the execution environment. This will be called only once for any object so global initialization shall be placed here. It is checked if the type can be loaded, is a descendent of 
        /// <see cref="Activity"/> and then the type is stored for later execution
        /// </summary>
        /// <returns><c>true</c> if initialization is done correctly, otherwise <c>false</c> </returns>
        protected override bool InitializeExecutionEnvironment()
        {
            try
            {
                CheckParameters();

                // now the real instance is created
                SoftwareAgent = (Activity)Activator.CreateInstance(SoftwareAgentType);

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
        /// Checks the parameters which have to be set for all instances. These are <see cref="AbstractCallback.SymbolicName"/>, the <see cref="SoftwareAgentType"/> and <see cref="SoftwareAgentType"/>.
        /// If any of these requirements are not met and <see cref="InvalidOperationException"/> is thrown
        /// </summary>
        protected override void CheckParameters()
        {
            if (string.IsNullOrEmpty(SymbolicName))
            {
                throw new InvalidOperationException(Properties.Resources.NoSymbolicName);
            }

            if (CallbackImplementation != CallbackImplementation.SoftwareAgent)
            {
                throw new InvalidOperationException(string.Format(Properties.Resources.ErrorInvalidCallbackType, CallbackImplementation));
            }
            if (SoftwareAgentType == null)
            {
                throw new InvalidOperationException(Properties.Resources.ErrorNoExecutionType);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoftwareAgentCallback"/> class.
        /// </summary>
        /// <param name="symbolicName">Name of the symbolic.</param>
        /// <param name="callbackKind">Kind of the callback.</param>
        protected SoftwareAgentCallback(string symbolicName, CallbackType callbackKind)
        {
            SymbolicName = symbolicName;
            CallbackType = callbackKind;
            CallbackImplementation = CallbackImplementation.SoftwareAgent;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SoftwareAgentCallback"/> class.
        /// </summary>
        /// <param name="symbolicName">Name of the symbolic.</param>
        /// <param name="callbackKind">Kind of the callback.</param>
        /// <param name="softwareAgentType">Type of the software agent.</param>
        public SoftwareAgentCallback(string symbolicName, CallbackType callbackKind, Type softwareAgentType)
            : this(symbolicName, callbackKind)
        {
            SetSoftwareAgentType(softwareAgentType);
        }
    }
}
