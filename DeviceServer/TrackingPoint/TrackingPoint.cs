using DataStorageAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackingPoint
{
    /// <summary>
    /// The class implements functionality for tracking point creation.
    /// </summary>
    public class TrackingPoint
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(typeof(TrackingPoint));

        #region Configuration constants...
        /// <summary>
        /// Configuration property name. Value: boolean.
        /// </summary>
        public const string CfgPerformTracking = "TrackingPoint.PerformTracking";
        /// <summary>
        /// Default value for the <see cref="CfgPerformTracking"/> configuration property.
        /// </summary>
        public const bool DefaultIsPerformTracking = true;
        /// <summary>
        /// Configuration property name. Value: database connection string, string.
        /// </summary>
        public const string CfgConnectionString = "TrackingPoint.ConnectionString";
        #endregion

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fffffff";
        private const string DefaultName = "default";

        #region Database constants...
        /// <summary>
        /// Predefined DB table name.
        /// </summary>
        public const string DbTableTrackingPoint = "TrackingPoint";
        /// <summary>
        /// Predefined DB column name.
        /// </summary>
        public const string DbColumnAdditionalData = "AdditionalData";
        /// <summary>
        /// Predefined DB column name.
        /// </summary>
        public const string DbColumnTimestamp = "Timestamp";
        /// <summary>
        /// Predefined DB column name.
        /// </summary>
        public const string DbColumnTrackingPoint = "TrackingPoint";

        private const string DbQueryInsertTrackingPointWithTimestamp =
            "INSERT INTO TrackingPoint (TrackingPoint, AdditionalData, Timestamp, Counter, CorrelationId) VALUES (@0, @1, @2, @3, @4)";

        private const string DbQueryInsertTrackingPoint =
            "INSERT INTO TrackingPoint (TrackingPoint, AdditionalData, Timestamp, Counter, CorrelationId) VALUES (@0, @1, SYSDATETIME(), @2, @3)"; 
        #endregion

        /// <summary>
        /// The property contains the default database connection string.
        /// </summary>
        public static string ConnectionString { get; set; }

        /// <summary>
        /// The property contains a flag that defines if the tracking is to be performed.
        /// </summary>
        public static bool PerformTracking { get; set; }

        static TrackingPoint ()
        {
            //Set the default valie
            PerformTracking = DefaultIsPerformTracking;

            //Retrieve the config value, if present: 
            var valueStr = ConfigurationManager.AppSettings[CfgPerformTracking];
            if (!String.IsNullOrEmpty(valueStr))
            {
                PerformTracking = Convert.ToBoolean(valueStr);
            }

            //Retrieve the config value, if present
            valueStr = ConfigurationManager.AppSettings[CfgConnectionString];
            if (!String.IsNullOrEmpty(valueStr))
            {
                ConnectionString = valueStr;
            }
        }

        /// <summary>
        /// The method creates a tracking point record in the tracking point table (<see cref="DbTableTrackingPoint"/>) in the default database.
        /// </summary>
        /// <param name="trackingPoint">The parameter value may not be <c>null</c></param>
        /// <param name="additionalData"></param>
        /// <param name="timestampForTracking"></param>
        /// <param name="counterValue">a value for a counter</param>
        /// <param name="correlationId">the id used to correlate entries</param>
        /// <exception cref="Exception">If the database operation fails.</exception>
        public static void CreateTrackingPoint(string trackingPoint, string additionalData, DateTime timestampForTracking, long counterValue = 0, string correlationId = null)
        {
            //Check params
            if (String.IsNullOrEmpty(ConnectionString))
                throw new InvalidOperationException("The property ConnectionString is not set.");

            CreateTrackingPoint(ConnectionString, trackingPoint, additionalData, timestampForTracking, counterValue, correlationId);
        }

        /// <summary>
        /// The method creates a tracking point record in the tracking point table (<see cref="DbTableTrackingPoint"/>) in the specified database.
        /// </summary>
        /// <param name="connectString">The parameter value may not be <c>null</c></param>
        /// <param name="trackingPoint">The parameter value may not be <c>null</c></param>
        /// <param name="additionalData"></param>
        /// <param name="timestampForTracking"></param>
        /// <param name="counterValue">a value for a counter</param>
        /// <param name="correlationId">the id used to correlate entries</param>
        /// <exception cref="Exception">If the database operation fails.</exception>
        public static void CreateTrackingPoint(string connectString, string trackingPoint, string additionalData, DateTime timestampForTracking, long counterValue, string correlationId)
        {

            //Check params
            if (String.IsNullOrEmpty(connectString))
                throw new ArgumentNullException("connectString");
            if (String.IsNullOrEmpty(trackingPoint))
                throw new ArgumentNullException("trackingPoint");

            if (PerformTracking)
            {
                try
                {
                    //Init DB access
                    var settings = new ConnectionStringSettings(DefaultName, connectString);
                    var sqlAccess = new SqlAccess() {ConnectionStringSettings = settings};

                    var stringDate = timestampForTracking.ToString(DateTimeFormat);

                    //Execute the DB query
                    sqlAccess.ExecuteNonQuery(DbQueryInsertTrackingPointWithTimestamp, trackingPoint, additionalData, stringDate, counterValue, correlationId);
                }
                catch (Exception exc)
                {
                    log.Error("Failed creating a tracking point.", exc);
                }
            }
        }

        /// <summary>
        /// The method creates a tracking point record in the tracking point table (<see cref="DbTableTrackingPoint"/>) in the default database.
        /// </summary>
        /// <param name="trackingPoint">The parameter value may not be <c>null</c></param>
        /// <param name="additionalData"></param>
        /// <param name="counterValue">a value for a counter</param>
        /// <param name="correlationId">the id used to correlate entries</param>
        /// <exception cref="Exception">If the database operation fails.</exception>
        public static void CreateTrackingPoint(string trackingPoint, string additionalData, long counterValue = 0, string correlationId = null)
        {
            //Check params
            if (String.IsNullOrEmpty(ConnectionString))
                throw new InvalidOperationException("The property ConnectionString is not set.");

            CreateTrackingPoint(ConnectionString, trackingPoint, additionalData, counterValue, correlationId);
        }

        /// <summary>
        /// The method creates a tracking point record in the tracking point table (<see cref="DbTableTrackingPoint"/>) in the specified database.
        /// </summary>
        /// <param name="connectString">The parameter value may not be <c>null</c></param>
        /// <param name="trackingPoint">The parameter value may not be <c>null</c></param>
        /// <param name="additionalData"></param>
        /// <param name="counterValue">a value for a counter</param>
        /// <param name="correlationId">the id used to correlate entries</param>
        /// <exception cref="Exception">If the database operation fails.</exception>
        public static void CreateTrackingPoint(string connectString, string trackingPoint, string additionalData, long counterValue, string correlationId)
        {
            //Check params
            if (String.IsNullOrEmpty(connectString))
                throw new ArgumentNullException("connectString");
            if (String.IsNullOrEmpty(trackingPoint))
                throw new ArgumentNullException("trackingPoint");

            if (PerformTracking)
            {
                try
                {
                    //Init DB access
                    var settings = new ConnectionStringSettings(DefaultName, connectString);
                    var sqlAccess = new SqlAccess() {ConnectionStringSettings = settings};

                    //Execute the DB query
                    sqlAccess.ExecuteNonQuery(DbQueryInsertTrackingPoint, trackingPoint, additionalData, counterValue, correlationId);
                }
                catch (Exception exc)
                {
                    log.Error("Failed creating a tracking point.", exc);
                }
            }
        }
    }
}
        