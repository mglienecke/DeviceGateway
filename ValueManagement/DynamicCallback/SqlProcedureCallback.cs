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
    /// The class implements the callback mechanism for SQL procedure call. 
    /// The procedure MUST have all listed parameters.
    /// Code sample:
    /// <code>
    /// EXEC dbo.MultiplyBy3 @current_value, @sensor__internal_id, @return_value OUT, @is_cancelled OUT, @is_modified OUT, @is_sample_rate_adjusted OUT, @new_sample_rate_in_seconds OUT
    /// </code>
    /// </summary>
    public class SqlProcedureCallback : SqlInteractiveCallback
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int MaxSensorValueSize = 8000;
        private const string ParameterNameReturnValue = "@return_value";
        private const string ParameterNameIsCancelled = "@is_cancelled";
        private const string ParameterNameIsModified = "@is_modified";
        private const string ParameterNameIsSampleRateAdjusted = "@is_sample_rate_adjusted";
        private const string ParameterNameNewSampleRateInSeconds = "@new_sample_rate_in_seconds";

        #region Constructors...
        /// <summary>
        /// Initializes a new instance of the <see cref="CompiledMethodExecution"/> class.
        /// </summary>
        /// <param name="symbolicName">Name of the symbolic.</param>
        /// <param name="callbackType">Type of the callback.</param>
        /// <param name="callbackKind">Kind of the callback.</param>
        protected SqlProcedureCallback(string symbolicName, CallbackType callbackKind) :
            base(symbolicName, callbackKind)
        {
            CallbackImplementation = CallbackImplementation.SqlStoredProcedure;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CompiledMethodExecution"/> class.
        /// </summary>
        /// <param name="symbolicName">Name of the symbolic.</param>
        /// <param name="callbackKind">Kind of the callback.</param>
        /// <param name="procedureName">The name of the stored procedure.</param>
        /// <param name="connectionString">database connection string</param>
        public SqlProcedureCallback(string symbolicName, CallbackType callbackKind, string connectionString, string procedureName)
            : this(symbolicName, callbackKind)
        {
            // checks
            if (procedureName == null || procedureName.Length == 0)
            {
                throw new ArgumentNullException("procedureName");
            }
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }

            ExecutionExpression = procedureName;
            ConnectionSettings.ConnectionString = connectionString;

            if (InitializeEnvironment() == false)
            {
                throw new InvalidOperationException(Properties.Resources.ErrorCallingSqlStoredProcedureCallback);
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
        }
        #endregion

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
                // only used for virtual values
                List<Dictionary<string, object>> results = null;

                using (IDbConnection connection = OpenDbConnection())
                {
                    //Create command
                    using (IDbCommand command = connection.CreateCommand())
                    {
                        InitializeDbCommandStoredProcedure(data.CallbackKind, command, this.ExecutionExpression, CommandTimeout, data.CurrentValue.Value, data.ValueDefinition.InternalId);
                        command.CommandType = CommandType.StoredProcedure;

                        // Execute - either as Non-Query for anything except a virtual value calculation and as a query for it
                        if (data.CallbackKind != CallbackType.VirtualValueCalculation)
                        {
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            using (IDataReader reader = command.ExecuteReader())
                            {
                                results = new List<Dictionary<string, object>>();

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
                            }
                        }


                        CallbackResultData result = new CallbackResultData();

                        // For Non-Virtual-Value Calculation use this approach
                        if (data.CallbackKind != CallbackType.VirtualValueCalculation)
                        {
                            //Is modified
                            object temp = ((IDataParameter)command.Parameters[ParameterNameIsModified]).Value;
                            if (Convert.IsDBNull(temp))
                                result.IsValueModified = true;
                            else
                                result.IsValueModified = Convert.ToBoolean(temp);

                            //Is cancelled
                            temp = ((IDataParameter)command.Parameters[ParameterNameIsCancelled]).Value;
                            if (Convert.IsDBNull(temp))
                                result.IsCancelled = false;
                            else
                                result.IsCancelled = Convert.ToBoolean(temp);

                            //Sample rate needs adjustment
                            temp = ((IDataParameter)command.Parameters[ParameterNameIsSampleRateAdjusted]).Value;
                            if (Convert.IsDBNull(temp))
                                result.SampleRateNeedsAdjustment = false;
                            else
                                result.SampleRateNeedsAdjustment = Convert.ToBoolean(temp);

                            //New sample rate
                            if (result.SampleRateNeedsAdjustment)
                            {
                                result.NewSampleRate = new TimeSpan(0, 0, Convert.ToInt32(((IDataParameter)command.Parameters[ParameterNameNewSampleRateInSeconds]).Value));
                            }

                            //Get the new value if any
                            if (result.IsValueModified && !result.IsCancelled)
                            {
                                result.NewValue = new SensorData();
                                result.NewValue.Value = Convert.ToString(((IDataParameter)command.Parameters[ParameterNameReturnValue]).Value);
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
                            // otherwise (virtual value) just use the very first result
                            result.NewValue = new SensorData();
                            if (results != null && results.Count > 0)
                            {
                                result.NewValue.Value = Convert.ToString(results[0].Values.First());
                            }
                            result.NewValue.GeneratedWhen = DateTime.Now;
                            //Pass the correlation id
                            if (data.CurrentValue != null) result.NewValue.CorrelationId = data.CurrentValue.CorrelationId;
                        }

                        return result;
                    }
                }
            }
            catch (System.Exception ex)
            {
                log.ErrorFormat(Properties.Resources.ErrorCallingSqlInteractiveCallback, ex.ToString());
                throw;
            }
        }

        private void InitializeDbCommandStoredProcedure(CallbackType callbackType, IDbCommand command, string sqlProcedureName, int commandTimeout,
            string value, int sensorId)
        {
            command.Parameters.Clear();

            // These parameters are used as OUT only for non virtual values
            if (callbackType != CallbackType.VirtualValueCalculation)
            {
                //Return value
                IDbDataParameter returnValueParam = command.CreateParameter();
                returnValueParam.ParameterName = ParameterNameReturnValue;
                returnValueParam.Value = null;
                returnValueParam.DbType = DbType.String;
                returnValueParam.Size = MaxSensorValueSize;
                returnValueParam.Direction = ParameterDirection.Output;
                command.Parameters.Add(returnValueParam);

                //Is cancelled
                IDbDataParameter isCancelledParam = command.CreateParameter();
                isCancelledParam.ParameterName = ParameterNameIsCancelled;
                isCancelledParam.Value = null;
                isCancelledParam.DbType = DbType.Boolean;
                isCancelledParam.Direction = ParameterDirection.Output;
                command.Parameters.Add(isCancelledParam);

                //Is modified
                IDbDataParameter isValueModifiedParam = command.CreateParameter();
                isValueModifiedParam.ParameterName = ParameterNameIsModified;
                isValueModifiedParam.Value = null;
                isValueModifiedParam.DbType = DbType.Boolean;
                isValueModifiedParam.Direction = ParameterDirection.Output;
                command.Parameters.Add(isValueModifiedParam);

                //Needs new sample rate
                IDbDataParameter isSampleRateAdjusted = command.CreateParameter();
                isSampleRateAdjusted.ParameterName = ParameterNameIsSampleRateAdjusted;
                isSampleRateAdjusted.Value = null;
                isSampleRateAdjusted.DbType = DbType.Boolean;
                isSampleRateAdjusted.Direction = ParameterDirection.Output;
                command.Parameters.Add(isSampleRateAdjusted);

                //new sample rate
                IDbDataParameter newSampleRate = command.CreateParameter();
                newSampleRate.ParameterName = ParameterNameNewSampleRateInSeconds;
                newSampleRate.Value = null;
                newSampleRate.DbType = DbType.Int32;
                newSampleRate.Direction = ParameterDirection.Output;
                command.Parameters.Add(newSampleRate);
            }

            //Current value
            IDbDataParameter currentValue = command.CreateParameter();
            currentValue.ParameterName = ParameterNameCurrentValue;
            currentValue.Value = (object) value ?? DBNull.Value;
            currentValue.DbType = DbType.String;
            currentValue.Size = value == null ? 1 : value.Length;
            currentValue.Direction = ParameterDirection.Input;
            command.Parameters.Add(currentValue);

            //Sensor id
            IDbDataParameter sensorInternalId = command.CreateParameter();
            sensorInternalId.ParameterName = ParameterNameSensorId;
            sensorInternalId.Value = sensorId;
            sensorInternalId.DbType = DbType.Int32;
            sensorInternalId.Direction = ParameterDirection.Input;
            command.Parameters.Add(sensorInternalId);

            command.CommandText = sqlProcedureName;

            //Set the timeout
            command.CommandTimeout = commandTimeout;
        }
    }
}
