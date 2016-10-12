using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Globalization;

using log4net;

namespace DataStorageAccess
{
    public class SqlAccess
    {
        /// <summary>
        /// logger instance
        /// </summary>
        private static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SqlAccess));

        private static readonly Dictionary<string, object>[] NoDataResults = new Dictionary<string, object>[0];

        public SqlAccess()
        {
        }

        private ConnectionStringSettings mConnectionStringSettings;

        /// <summary>
        /// The property contains the SQL access settings.
        /// </summary>
        public ConnectionStringSettings ConnectionStringSettings
        {
            get
            {
                return mConnectionStringSettings;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ConnectionStringSettings");
                }
                mConnectionStringSettings = value;

                if (mConnectionStringSettings.ProviderName.IsNotNullOrEmpty())
                {
                    ConnectionType = Type.GetType(ConnectionStringSettings.ProviderName);
                }
            }
        }

        /// <summary>
        /// The property allows setting a timeout value for executing SQL commands.
        /// </summary>
        public Int32 CommandTimeout
        {
            get;
            set;
        }

        private static Type mConnectionType = typeof(System.Data.SqlClient.SqlConnection);

        /// <summary>
        /// The property contains the type of the SQL connection. Normally it gets derived from the connection string.
        /// </summary>
        public Type ConnectionType
        {
            get
            {
                return mConnectionType;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ConnectionType");
                }
                mConnectionType = value;
            }
        }

        /// <summary>
        /// Execute a scalar SQL statement
        /// </summary>
        /// <param name="sqlText">SQL string with parameter placeholders
        /// like {0}</param>
        /// <param name="parameters"></param>
        /// <returns>null if nothing is returned</returns>
        /// <exception cref="SqlExecutorException">if statement execution fails</exception>
        public object ExecuteScalar(string sqlText, params object[] parameters)
        {
            //Get 
            Exception exception = null;
            bool exceptionThrown = false;
            object result = null;

            //Create command
            IDbCommand command = CreateCommand();

            try
            {
                InitializeCommand(command, sqlText, CommandTimeout, parameters);

                //Execute
                result = command.ExecuteScalar();
            }
            catch (Exception exc)
            {
                exception = exc;
                exceptionThrown = true;
            }
            finally
            {
                //Release the command
                ReleaseCommand(command);

                //Rethrow f an exception has happened
                if (exceptionThrown)
                {
                    if (exception != null)
                    {
                        throw new Exception(Properties.Resources.ExceptionSqlQueryFailed,
                            exception);
                    }
                    else
                    {
                        throw new Exception(Properties.Resources.ExceptionSqlQueryFailed);
                    }
                }
            }

            //Check if null
            if (Convert.IsDBNull(result))
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Execute an SQL query.
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Dictionary<string, object>[] ExecuteQuery(string sqlText, params object[] parameters)
        {
            //Create command
            IDbCommand command = CreateCommand();

            Dictionary<string, object>[] results = NoDataResults;
            Exception exception = null;
            bool exceptionThrown = false;

            try
            {
                InitializeCommand(command, sqlText, CommandTimeout, parameters);

                //Execute command and collect results
                results = ResultsToDictionaryArray(command.ExecuteReader());
            }
            catch (Exception exc)
            {
                exception = exc;
                exceptionThrown = true;
            }
            finally
            {
                //Release the command
                ReleaseCommand(command);

                //Rethrow f an exception has happened
                if (exceptionThrown)
                {
                    if (exception != null)
                    {
                        log.Error(Properties.Resources.ExceptionSqlQueryFailed, exception);
                        throw new Exception(Properties.Resources.ExceptionSqlQueryFailed, exception); ;
                    }
                    else
                    {
                        log.Error(Properties.Resources.ExceptionSqlQueryFailed);
                    }
                }
            }

            return results;

        }

        /// <summary>
        /// Execute a non-query command.
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public int ExecuteNonQuery(string sqlText, params object[] parameters)
        {
            //Get 
            bool exceptionThrown = false;
            Exception exception = null;
            int rowsAffected = 0;

            //Create command
            IDbCommand command = CreateCommand();

            try
            {
                InitializeCommand(command, sqlText, CommandTimeout, parameters);

                //Execute command
                rowsAffected = command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                exception = exc;
                exceptionThrown = true;
            }
            finally
            {
                //Release the command
                ReleaseCommand(command);

                //Rethrow f an exception has happened
                if (exceptionThrown)
                {
                    if (exception != null)
                    {
                        log.Error(Properties.Resources.ExceptionSqlQueryFailed, exception);
                        throw new Exception(Properties.Resources.ExceptionSqlQueryFailed, exception); ;
                    }
                    else
                    {
                        log.Error(Properties.Resources.ExceptionSqlQueryFailed);
                    }
                }
            }

            return rowsAffected;
        }

        /// <summary>
        /// Execute a batch SQL query.
        /// </summary>
        /// <param name="sqlStrings">SQL strings.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="SqlExecutorException">if query execution fails</exception>
        public Dictionary<string, object>[] ExecuteBatchQuery(string[] sqlStrings, object[][] parameters)
        {
            //Get 
            //Check array lengths
            if (sqlStrings.Length != parameters.Length)
            {
                throw new ArgumentException(Properties.Resources.ExceptionArrayLengthsMustBeEqual);
            }

            IDbConnection connection = null;
            List<Dictionary<string, object>> resultsList = new List<Dictionary<string, object>>();

            //Open SQL connection
            using (connection = OpenConnection())
            {
                IDbTransaction transaction = connection.BeginTransaction();

                try
                {
                    for (int i = 0; i < sqlStrings.Length; i++)
                    {
                        //Prepare Db command.
                        IDbCommand command = connection.CreateCommand();
                        InitializeCommand(command, sqlStrings[i], CommandTimeout, parameters[i]);
                        command.Transaction = transaction;

                        //Execute command and collect results
                        resultsList.AddRange(ResultsToDictionaryArray(command.ExecuteReader()));
                    }

                    //Commit
                    transaction.Commit();
                }
                catch (Exception exc)
                {
                    //Rollback
                    Rollback(transaction);

                    throw new Exception(Properties.Resources.ExceptionSqlQueryFailed,
                           exc);
                }
            }

            return resultsList.ToArray();
        }

        private IDbConnection OpenConnection()
        {
            //Create instance
            IDbConnection instance;
            try
            {
                instance = (IDbConnection)(Activator.CreateInstance(ConnectionType));
            }
            catch (Exception exc)
            {
                log.Fatal(Properties.Resources.ExceptionFailedCreateDatabasebConnectionInstance, exc);
                throw new Exception(Properties.Resources.ExceptionFailedCreateDatabasebConnectionInstance, exc);
            }


            try
            {
                instance.ConnectionString = ConnectionStringSettings.ConnectionString;
                instance.Open();
            }
            catch (Exception exc)
            {

                log.Fatal(Properties.Resources.ExceptionFailedToOpenDatabaseConnection, exc);
                throw new Exception(Properties.Resources.ExceptionFailedToOpenDatabaseConnection, exc);
            }

            return instance;
        }

        private void InitializeCommand(IDbCommand command, string sqlText, int commandTimeout, object[] parameters)
        {
            command.Parameters.Clear();
            //FormatString-style or DB-Parameters style?
            if (sqlText.Contains("{0}"))
            {
                //Format and create command
                command.CommandText = FormatParameterizedString(sqlText, parameters);
            }
            else
            {
                //Create and set parameters
                Regex regex = new Regex(@"@(?<Name>\d(\d)*)(\((?<DataType>([A-Za-z0-9]+))(,( )*(?<Size>([1-9][0-9]+))?)\))?");
                MatchCollection matches = regex.Matches(sqlText);
                if (matches.Count != parameters.Length)
                {
                    throw new ArgumentException("The numbers of passed parameters and parameters declared in the SQL statement do not match.");
                }

                StringBuilder result = new StringBuilder(sqlText);
                foreach (Match match in matches)
                {
                    IDbDataParameter nextParam = command.CreateParameter();
                    nextParam.ParameterName = match.Groups["Name"].Value;
                    int index = Convert.ToInt32(nextParam.ParameterName);
                    nextParam.Value = parameters[index] == null ? DBNull.Value : parameters[index];
                    //Got type?
                    if (match.Groups["DataType"].Value != null && (match.Groups["DataType"].Value.Length > 0))
                    {
                        nextParam.DbType = (DbType)Enum.Parse(typeof(DbType), match.Groups["DataType"].Value);
                        //Got size?
                        if (match.Groups["Size"].Value != null && match.Groups["Size"].Value.Length > 0)
                        {
                            nextParam.Size = Convert.ToInt32(match.Groups["Size"].Value);
                        }

                        //Clean up from the string
                        string metaDataPart = match.Value.Substring(match.Value.IndexOf('('));
                        result.Replace(metaDataPart, String.Empty);
                    }
                    command.Parameters.Add(nextParam);
                }

                command.CommandText = result.ToString();
            }

            //Set the timeout if needed 
            command.CommandTimeout = commandTimeout;
        }

        private string FormatParameterizedString(string sqlString, object[] parameters)
        {
            //Copy parameter array
            object[] copiedParams = new object[parameters.Length];
            parameters.CopyTo(copiedParams, 0);

            //Prepare parameters
            string NullPlaceholder = "-NuLl-";
            for (int i = 0; i < copiedParams.Length; i++)
            {
                if (copiedParams[i] == null)
                {
                    copiedParams[i] = NullPlaceholder;
                }
            }

            //Format the string
            sqlString = String.Format(CultureInfo.InvariantCulture,
                sqlString, copiedParams);

            //Handle the situation when some string parameters are in '' and some
            //are not.
            sqlString = sqlString.Replace("'" + NullPlaceholder + "'", "NULL");
            sqlString = sqlString.Replace(NullPlaceholder, "NULL");


            return sqlString;
        }

        private void ReleaseCommand(IDbCommand command)
        {
            IDbConnection conn = command.Connection;
            command.Dispose();
            ReleaseConnection(conn);
        }

        private void ReleaseConnection(IDbConnection connection)
        {
            try
            {
                connection.Close();
            }
            catch (Exception exc)
            {
                log.Warn(Properties.Resources.ErrorFailedClosingDatabaseConnection, exc);
            }
        }

        private IDbCommand CreateCommand()
        {
            //Open SQL connection
            IDbConnection connection = OpenConnection();

            try
            {
                //Create command
                return connection.CreateCommand();
            }
            catch
            {
                ReleaseConnection(connection);
                throw;
            }
        }

        private IDbCommand CreateCommandWithTransaction()
        {
            //Open SQL connection
            IDbConnection connection = OpenConnection();

            try
            {
                //Create command
                IDbCommand command = connection.CreateCommand();

                //Start transaction
                command.Transaction = connection.BeginTransaction();

                return command;
            }
            catch (InvalidOperationException exc)
            {
                ReleaseConnection(connection);
                throw new Exception(Properties.Resources.ExceptionFailedCreateDbCommandWithTransaction,
                    exc);
            }
        }

        private void Rollback(IDbTransaction transaction)
        {
            //Rollback
            try
            {
                transaction.Rollback();
            }
            catch (Exception rollbackExc)
            {
                log.Error(Properties.Resources.ErrorRollbackFailed, rollbackExc);
            }
        }

        /// <summary>
        /// Read all rows from the reader and put them to <see cref="DataRowDictionary"/>  objects. 
        /// Close the reader.
        /// </summary>
        /// <param name="reader"></param>
        private Dictionary<string, object>[] ResultsToDictionaryArray(IDataReader reader)
        {
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            try
            {
                do
                {
                    int retrievedRowCount = 0;
                    int rowLimit = Int32.MaxValue;

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

                            retrievedRowCount++;
                        }
                        while (reader.Read() && (retrievedRowCount < rowLimit));
                    }
                }
                while (reader.NextResult());
            }
            finally
            {
                reader.Close();
            }

            return results.ToArray();
        }


    }

    /// <summary>
    /// The class declares several usefult extenstion methods.
    /// </summary>
    internal static class Utilities
    {        
        public static String GetString(this Dictionary<string, object> data, string key, string defaultValue)
        {
            object result;
            if (data.TryGetValue(key, out result))
            {
                if (DBNull.Value.Equals(result))
                {
                    return defaultValue;
                }
                else
                {
                    return Convert.ToString(result);
                }
            }
            else
            {
                return defaultValue;
            }
        }

        public static double GetDouble(this Dictionary<string, object> data, string key, double defaultValue)
        {
            object result;
            if (data.TryGetValue(key, out result))
            {
                if (DBNull.Value.Equals(result))
                {
                    return defaultValue;
                }
                else
                {
                    return Convert.ToDouble(result);
                }
            }
            else
            {
                return defaultValue;
            }
        }

        public static bool IsNotNullOrEmpty(this string str)
        {
            return str != null && str.Length > 0;
        }
    }
}
