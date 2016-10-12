using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

using System.Data;

using log4net;
using GlobalDataContracts;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Configuration;

namespace ValueManagement.DynamicCallback
{
    /// <summary>
    /// The class implements the callback mechanism for SQL code.
    /// Two parameters can be passed into the code: <c>@current_value</c> <c>and @sensor_internal_id</c>
    /// Sample code:
    /// <code>
    /// SELECT 3 * CONVERT(decimal, @current_value) return_value, 1 is_modified
    /// </code>
    /// Expected return fields are: return_value, is_modified, is_cancelled, is_sample_rate_adjusted, new_sample_rate_in_seconds.
    /// The return_value and is_modified fields are required. 
    /// </summary>
    public class SqlInteractiveCallback : AbstractCallback
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static readonly string DefaultProviderName = typeof(System.Data.SqlClient.SqlConnection).AssemblyQualifiedName;

        protected const string ParameterNameCurrentValue = "@current_value";
        protected const string ParameterNameSensorId = "@sensor_internal_id";

        private const string FieldNameReturnValue = "return_value";
        private const string FieldNameIsCancelled = "is_cancelled";
        private const string FieldNameIsModified = "is_modified";
        private const string FieldNameIsSampleRateAdjusted = "is_sample_rate_adjusted";
        private const string FieldNameNewSampleRateInSeconds = "new_sample_rate_in_seconds";

        private int mCommandTimeout;
        private Type mConnectionType;

        #region Properties...
        /// <summary>
        /// The property contains the settings for the database connection to be used.
        /// </summary>
        /// <value>The property value may not be <c>null</c></value>
        public ConnectionStringSettings ConnectionSettings { get; set; }

        protected Type ConnectionType
        {
            get
            {
                return mConnectionType;
            }
        }

        /// <summary>
        /// The property contains the DB command execution timeout (in seconds). Zero value means there is no timeout.
        /// </summary>
        public int CommandTimeout
        {
            get
            {
                return mCommandTimeout;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                else
                {
                    mCommandTimeout = value;
                }
            }
        }
        #endregion

        /// <summary>
        /// Initializes the execution environment. After creating the runtime and scope, the script / file is loaded and compiled so that is is ready for execution later
        /// </summary>
        /// <returns><c>true</c> if initialization is done correctly, otherwise <c>false</c> </returns>
        protected override bool InitializeExecutionEnvironment()
        {
            try
            {
                CheckParameters();

                //Check if the connection can be opened
                using (IDbConnection connection = OpenDbConnection())
                {
                    IsInitialized = true;
                }

                return (true);
            }
            catch (System.Exception ex)
            {
                log.LogException("InitializeExecutionEnvironment", Properties.Resources.ErrorInitSqlExecutionEnvironment, ex);
                throw new InvalidOperationException(Properties.Resources.ErrorInitSqlExecutionEnvironment, ex);
            }
        }

        protected IDbConnection OpenDbConnection()
        {
            //Setup the SQL execution
            //Create instance

            IDbConnection instance;
            try
            {
                mConnectionType = Type.GetType(ConnectionSettings.ProviderName, true, true);

                instance = (IDbConnection)(Activator.CreateInstance(mConnectionType));
            }
            catch (Exception exc)
            {
                throw new Exception(String.Format(Properties.Resources.ExceptionFailedCreateDbConnectionInstance, ConnectionSettings.ProviderName, exc.Message), exc);
            }

            //Open connection
            try
            {
                instance.ConnectionString = ConnectionSettings.ConnectionString;
                instance.Open();
            }
            catch (Exception exc)
            {
                throw new Exception(String.Format(Properties.Resources.ExceptionFailedToOpenDbConnection, exc.Message), exc);
            }

            return instance;
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
            // Execute the source and retrieve the result
            try
            {
                List<Dictionary<string, object>> resultRaw = ExecuteQuery(ExecutionExpression, data);

                if (resultRaw.Count < 1)
                {
                    throw new Exception(Properties.Resources.ExceptionNoResultsReturnedFromSqlCallbackQuery);
                }

                CallbackResultData result = new CallbackResultData();

                if (data.CallbackKind != CallbackType.VirtualValueCalculation)
                {
                    //Is modified
                    object temp;
                    if (resultRaw[0].TryGetValue(FieldNameIsModified, out temp))
                    {
                        if (Convert.IsDBNull(temp))
                            result.IsValueModified = true;
                        else
                            result.IsValueModified = Convert.ToBoolean(temp);
                    }

                    //Is cancelled
                    if (resultRaw[0].TryGetValue(FieldNameIsCancelled, out temp))
                    {
                        if (Convert.IsDBNull(temp))
                            result.IsCancelled = false;
                        else
                            result.IsCancelled = Convert.ToBoolean(temp);
                    }

                    //Sample rate needs adjustment
                    if (resultRaw[0].TryGetValue(FieldNameIsSampleRateAdjusted, out temp))
                    {
                        if (Convert.IsDBNull(temp))
                            result.SampleRateNeedsAdjustment = false;
                        else
                            result.SampleRateNeedsAdjustment = Convert.ToBoolean(temp);
                    }

                    //New sample rate
                    if (result.SampleRateNeedsAdjustment)
                    {
                        result.NewSampleRate = new TimeSpan(0, 0, Convert.ToInt32(resultRaw[0][FieldNameNewSampleRateInSeconds]));
                    }


                    //Get the new value if any
                    if (result.IsValueModified && !result.IsCancelled)
                    {

                        result.NewValue = new SensorData();
                        result.NewValue.Value = Convert.ToString(resultRaw[0][FieldNameReturnValue]);
                        result.NewValue.GeneratedWhen = DateTime.Now;
                        //Pass the correlation id
                        if (data.CurrentValue != null) result.NewValue.CorrelationId = data.CurrentValue.CorrelationId;
                    }
                    else
                    {
                        result.NewValue = data.CurrentValue;
                    }
                }
                else
                {
                    // For virtual values assume just a scalar result and take the first result value 
                    result.NewValue = new SensorData();
                    if (resultRaw[0].Values.Count > 0)
                    {
                        result.NewValue.Value = Convert.ToString(resultRaw[0].Values.First());
                    }
                    result.NewValue.GeneratedWhen = DateTime.Now;
                    //Pass the correlation id
                    if (data.CurrentValue != null) result.NewValue.CorrelationId = data.CurrentValue.CorrelationId;
                }

                return result;
            }
            catch (System.Exception ex)
            {
                log.ErrorFormat(Properties.Resources.ErrorCallingSqlInteractiveCallback, ex.ToString());
                throw;
            }
        }

        private List<Dictionary<string, object>> ExecuteQuery(string sqlStatement, CallbackPassInData data)
        {
            using (IDbConnection connection = OpenDbConnection())
            {
                //Create command
                using (IDbCommand command = connection.CreateCommand())
                {
                    command.Parameters.Clear();


                    //Current value
                    IDbDataParameter currentValue = command.CreateParameter();
                    currentValue.ParameterName = ParameterNameCurrentValue;
                    currentValue.Value = (object)data.CurrentValue.Value ?? DBNull.Value;
                    currentValue.DbType = DbType.String;
                    currentValue.Size = data.CurrentValue.Value == null ? 1 : data.CurrentValue.Value.Length;
                    currentValue.Direction = ParameterDirection.Input;
                    command.Parameters.Add(currentValue);

                    //Sensor id
                    IDbDataParameter sensorInternalId = command.CreateParameter();
                    sensorInternalId.ParameterName = ParameterNameSensorId;
                    sensorInternalId.Value = data.ValueDefinition.InternalId;
                    sensorInternalId.DbType = DbType.Int32;
                    sensorInternalId.Direction = ParameterDirection.Input;
                    command.Parameters.Add(sensorInternalId);


                    command.CommandText = sqlStatement;

                    //Set the timeout if needed 
                    command.CommandTimeout = CommandTimeout;

                    //Execute
                    using (IDataReader reader = command.ExecuteReader())
                    {
                        List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();

                        //Read all 
                        do
                        {
                            if (reader.Read())
                            {
                                int fieldCount = reader.FieldCount;
                                do
                                {
                                    Dictionary<string, object> packet = new Dictionary<string, object>();

                                    for (int i = 0; i < fieldCount; i++)
                                    {
                                        string nextName = reader.GetName(i);
                                        object nextValue = reader.GetValue(i);
                                        packet[nextName] = nextValue;
                                    }

                                    results.Add(packet);
                                }
                                while (reader.Read());
                            }
                        }
                        while (reader.NextResult());

                        return results;
                    }
                }
            }
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="CompiledMethodExecution"/> class.
        /// </summary>
        /// <param name="symbolicName">Name of the symbolic.</param>
        /// <param name="callbackKind">Kind of the callback.</param>
        protected SqlInteractiveCallback(string symbolicName, CallbackType callbackKind)
        {
            SymbolicName = symbolicName;
            CallbackType = callbackKind;
            CallbackImplementation = CallbackImplementation.SqlInteractive;
            ConnectionSettings = new ConnectionStringSettings();
            ConnectionSettings.ProviderName = DefaultProviderName;
            CommandTimeout = 5;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="symbolicName">Name of the symbolic.</param>
        /// <param name="callbackKind">Kind of the callback.</param>
        /// <param name="sqlExpression">The execution expression.</param>
        /// <param name="connectionString">database connection string</param>
        public SqlInteractiveCallback(string symbolicName, CallbackType callbackKind, string connectionString, string sqlExpression)
            : this(symbolicName, callbackKind)
        {
            // checks
            if (sqlExpression == null || sqlExpression.Length == 0)
            {
                throw new ArgumentNullException("sqlExpression");
            }
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }

            ExecutionExpression = sqlExpression;
            ConnectionSettings.ConnectionString = connectionString;

            if (InitializeEnvironment() == false)
            {
                throw new InvalidOperationException(Properties.Resources.ErrorCallingSqlInteractiveCallback);
            }
        }

        /// <summary>
        /// Constructor.
        /// <param name="symbolicName">Name of the symbolic.</param>
        /// <param name="callbackKind">Kind of the callback.</param>
        /// <param name="sqlExpression">The execution expression.</param>
        /// <param name="settings">database connection string</param>
        public SqlInteractiveCallback(string symbolicName, CallbackType callbackKind, ConnectionStringSettings settings, string sqlExpression)
            : this(symbolicName, callbackKind)
        {
            // checks
            if (sqlExpression == null || sqlExpression.Length == 0)
            {
                throw new ArgumentNullException("sqlExpression");
            }
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            ExecutionExpression = sqlExpression;
            ConnectionSettings.ConnectionString = settings.ConnectionString;
            ConnectionSettings.ProviderName = settings.ProviderName;

            //Upgrade the settings if the provider name is not set
            if (ConnectionSettings.ProviderName == null || ConnectionSettings.ProviderName.Length == 0)
            {
                ConnectionSettings.ProviderName = DefaultProviderName;
            }

            if (InitializeEnvironment() == false)
            {
                throw new InvalidOperationException(Properties.Resources.ErrorCreatingSqlInteractiveCallback);
            }
        }

        /// <summary>
        /// Checks the parameters which have to be set for all instances. These are <see cref="SymbolicName"/>, the <see cref="ExecutionType"/> and either <see cref="ExecutionExpression"/> 
        /// or <see cref="ExternalFileName"/>.
        /// If any of these requirements are not met and <see cref="InvalidOperationException"/> is thrown
        /// </summary>
        protected override void CheckParameters()
        {
            base.CheckParameters();

            if ((ConnectionSettings.ConnectionString == null) || (ConnectionSettings.ConnectionString.Length == 0))
            {
                throw new InvalidOperationException(Properties.Resources.NoConnectionString);
            }

            if ((ConnectionSettings.ProviderName == null) || (ConnectionSettings.ProviderName.Length == 0))
            {
                throw new InvalidOperationException(Properties.Resources.NoConnectionTypeFullName);
            }
        }
    }
}
