using System;
using System.Collections.Generic;
using System.Collections;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Data;
using System.Data.Sql;
using System.Data.SqlTypes;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

using GlobalDataContracts;
using log4net;

namespace DataStorageAccess
{
	public class SqlDirectDataStorageAccess : DataStorageAccessBase 
	{
		/// <summary>
		/// logger instance
		/// </summary>
        private static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SqlDirectDataStorageAccess));

        public static string CfgSqlDirectDataStorageAccessParameters = "DataStorageAccess.Properties.Settings.ExperimentsConnectionString1";

		/// <summary>
		/// Synchronization lock for Submit calls
		/// </summary>
		private object submitSync = new object();

        private SqlAccess mSqlAccess = new SqlAccess();

        #region SQL names and queries...
        private const string SqlCreateDevice = "EXEC CreateDevice @0, @1, @2, @3, @4, @5, @6";
        private const string SqlUpdateDevice = "EXEC UpdateDevice @0, @1, @2, @3, @4, @5, @6";
        private const string SqlCreateSensor = "EXEC CreateSensor @0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16, @17, @18, @19";
        private const string SqlUpdateSensor = "EXEC UpdateSensor @0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16, @17, @18";

        private const string SqlColumnSensorId = "Id";
        private const string SqlColumnSensorDeviceId = "DeviceId";
        private const string SqlColumnSensorSensorId = "SensorId";
        private const string SqlColumnSensorDescription = "Description";
        private const string SqlColumnSensorUnitSymbol = "UnitSymbol";
        private const string SqlColumnSensorSensorValueDataType = "SensorValueDataType";
        private const string SqlColumnSensorSensorDataRetrievalMode = "SensorDataRetrievalMode";
        private const string SqlColumnSensorShallSensorDataBePersisted = "ShallSensorDataBePersisted";
        private const string SqlColumnSensorPersistDirectlyAfterChange = "PersistDirectlyAfterChange";
        private const string SqlColumnSensorIsVirtualSensor = "IsVirtualSensor";
        private const string SqlColumnSensorSensorCategory = "SensorCategory";
        private const string SqlColumnSensorSensorDataCalculationMode = "SensorDataCalculationMode";
        private const string SqlColumnSensorVirtualSensorDefinitionType = "VirtualSensorDefinitionType";
        private const string SqlColumnSensorVirtualSensorDefininition = "VirtualSensorDefininition";
        private const string SqlColumnSensorPullModeCommunicationType = "PullModeCommunicationType";
        private const string SqlColumnSensorPushModeCommunicationType = "PushModeCommunicationType";
        private const string SqlColumnSensorPullModeDotNetType = "PullModeDotNetType";
        private const string SqlColumnSensorPullFrequencyInSec = "PullFrequencyInSec";
        private const string SqlColumnSensorDefaultValue = "DefaultValue";
	    private const string SqlColumnSensorIsSynchronousPushToActuator = "IsSynchronousPushToActuator";
        private const string SqlColumnSensorIsActuator = "IsActuator";

        private const string SqlColumnDeviceId = "Id";
        private const string SqlColumnDeviceDescription = "Description";
        private const string SqlColumnDeviceIpEndPoint = "IpEndPoint";
        private const string SqlColumnDeviceLocationName = "LocationName";
        private const string SqlColumnDeviceLatitude = "Latitude";
        private const string SqlColumnDeviceLongitude = "Longitude";
        private const string SqlColumnDeviceElevation = "Elevation";

        private const string SqlGetSensorsForDevice = "SELECT * FROM DbSensor WHERE DeviceId = @0";
        private const string SqlGetDevices = "SELECT * FROM DbDevice";

        private const string SqlGetSensorData = "SELECT DeviceId, Id, DbSensor.SensorId, TakenWhen, Value, CorrelationId FROM DbSensorData INNER JOIN DbSensor ON DbSensorData.SensorId = DbSensor.SensorId WHERE DbSensor.SensorId = @0 AND TakenWhen >= @1 AND TakenWhen <= @2 ORDER BY TakenWhen";
        private const string SqlGetSensorDataMaxLimited = "SELECT TOP {0} DeviceId, Id, DbSensor.SensorId, TakenWhen, Value, CorrelationId FROM DbSensorData INNER JOIN DbSensor ON DbSensorData.SensorId = DbSensor.SensorId WHERE DbSensor.SensorId = @0 AND TakenWhen >= @1 AND TakenWhen <= @2 ORDER BY TakenWhen";
        private const string SqlGetSensorDataLatest = "SELECT DeviceId, Id, DbSensor.SensorId, TakenWhen, Value, CorrelationId FROM DbSensorData INNER JOIN DbSensor ON DbSensorData.SensorId = DbSensor.SensorId WHERE DbSensor.SensorId = @0 ORDER BY TakenWhen DESC";
        private const string SqlGetSensorDataLatestMaxLimited = "SELECT TOP {0} DeviceId, Id, DbSensor.SensorId, TakenWhen, Value, CorrelationId FROM DbSensorData INNER JOIN DbSensor ON DbSensorData.SensorId = DbSensor.SensorId WHERE DbSensor.SensorId = @0 ORDER BY TakenWhen DESC";

        private const string SqlTableSensorData = "DbSensorData";
        private const string SqlColumnSensorDataSensorId = "SensorId";
        private const string SqlColumnSensorDataTakenWhen = "TakenWhen";
        private const string SqlColumnSensorDataValue = "Value";
        private const string SqlColumnSensorDataCorrelationId = "CorrelationId";

        private const int ArgumentIndexWriteValueId = 0;
        private const int ArgumentIndexWriteValueTakenWhen = 1;
        private const int ArgumentIndexWriteValueValue = 2;
        private const int ArgumentIndexWriteValueCorrelationId = 3;

        private const string SqlAddSensorDependency    = "EXEC CreateSensorDependency @0, @1";
        private const string SqlRemoveSensorDependency = "EXEC DeleteSensorDependency @0, @1";
        private const string SqlGetSensorDependencies = "SELECT BaseSensorId, DependentSensorId FROM DbSensorDependency";
        private const string SqlGetSensorDependenciesByBase = "SELECT BaseSensorId, DependentSensorId FROM DbSensorDependency WHERE BaseSensorId = @0";
        private const string SqlGetSensorDependenciesByDependent = "SELECT BaseSensorId, DependentSensorId FROM DbSensorDependency WHERE DependentSensorId = @0";
        private const string SqlColumnSensorDependencyBaseSensorId      = "BaseSensorId";
        private const string SqlColumnSensorDependencyDependentSensorId = "DependentSensorId";

	    private const string SqlSelectNextValueForCorrelationId = "SELECT NEXT VALUE FOR CorrelationSequence";

        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fffffff";

        #endregion

        /// <summary>
		/// Initializes a new instance of the <see cref="SqlColumnDataStorageAccess"/> class 
		/// </summary>
        public SqlDirectDataStorageAccess()
		{
             mSqlAccess.ConnectionStringSettings = ConfigurationManager.ConnectionStrings[CfgSqlDirectDataStorageAccessParameters];
    	}

    	#region Store...

		/// <summary>
		/// stores a device
		/// </summary>
		/// <param name="device"></param>
		public override void StoreDevice(Device device)
		{
            object locationName = null;
            object latitude = null;
            object longitude = null;
            object elevation = null;

            if (device.Location != null)
            {
                locationName = device.Location.Name;
                latitude = device.Location.Latitude;
                longitude =  device.Location.Longitude;
                elevation = device.Location.Elevation;
            }

            mSqlAccess.ExecuteNonQuery(SqlCreateDevice, device.Id, device.Description, device.DeviceIpEndPoint, locationName, latitude, longitude, elevation);
		}

		/// <summary>
		/// Updates the device.
		/// </summary>
		/// <param name="device">The device.</param>
		public override void UpdateDevice(Device device)
		{
            object locationName = null;
            object latitude = null;
            object longitude = null;
            object elevation = null;

            if (device.Location != null)
            {
                locationName = device.Location.Name;
                latitude = device.Location.Latitude;
                longitude = device.Location.Longitude;
                elevation = device.Location.Elevation;
            }

            mSqlAccess.ExecuteNonQuery(SqlUpdateDevice, device.Id, device.Description, device.DeviceIpEndPoint, locationName, latitude, longitude, elevation);
		}

		/// <summary>
		/// Stores the sensor.
		/// </summary>
		/// <param name="sensor">The sensor.</param>
        public override void StoreSensor(Sensor sensor)
        {
            VirtualSensorDefinitionType virtualSensorDefinitionType = VirtualSensorDefinitionType.Undefined;
            VirtualSensorCalculationType virtualSensorDefinitionCalculationType = VirtualSensorCalculationType.Undefined;
            string virtualSensorDefinition = null;

            if (sensor.VirtualSensorDefinition != null)
            {
                virtualSensorDefinitionType = sensor.VirtualSensorDefinition.VirtualSensorDefinitionType;
                virtualSensorDefinitionCalculationType = sensor.VirtualSensorDefinition.VirtualSensorCalculationType;
                virtualSensorDefinition = sensor.VirtualSensorDefinition.Definition;
            }

            sensor.InternalSensorId = Convert.ToInt32(mSqlAccess.ExecuteScalar(SqlCreateSensor,
                sensor.Id,
                sensor.DeviceId,
                sensor.Description,
                sensor.UnitSymbol,
                sensor.SensorValueDataType,
                sensor.SensorDataRetrievalMode,
                sensor.ShallSensorDataBePersisted,
                sensor.PersistDirectlyAfterChange,
                sensor.IsVirtualSensor,
                sensor.Category,
                virtualSensorDefinitionCalculationType,
                virtualSensorDefinitionType,
                virtualSensorDefinition,
                sensor.PullModeCommunicationType,
                sensor.PullModeDotNetObjectType,
                sensor.PullFrequencyInSeconds,
                sensor.DefaultValue,
                sensor.IsSynchronousPushToActuator,
                sensor.IsActuator,
                sensor.PushModeCommunicationType
                ));
        }


		/// <summary>
		/// Updates the sensor.
		/// </summary>
		/// <param name="sensor">The sensor.</param>
		public override void UpdateSensor(Sensor sensor)
		{
            VirtualSensorDefinitionType virtualSensorDefinitionType = VirtualSensorDefinitionType.Undefined;
            VirtualSensorCalculationType virtualSensorDefinitionCalculationType = VirtualSensorCalculationType.Undefined;
            string virtualSensorDefinition = null;

            if (sensor.VirtualSensorDefinition != null)
            {
                virtualSensorDefinitionType = sensor.VirtualSensorDefinition.VirtualSensorDefinitionType;
                virtualSensorDefinitionCalculationType = sensor.VirtualSensorDefinition.VirtualSensorCalculationType;
                virtualSensorDefinition = sensor.VirtualSensorDefinition.Definition;
            }

            mSqlAccess.ExecuteNonQuery(SqlUpdateSensor,
                sensor.InternalSensorId,
                sensor.Description,
                sensor.UnitSymbol,
                sensor.SensorValueDataType,
                sensor.SensorDataRetrievalMode,
                sensor.ShallSensorDataBePersisted,
                sensor.PersistDirectlyAfterChange,
                sensor.IsVirtualSensor,
                sensor.Category,
                virtualSensorDefinitionCalculationType,
                virtualSensorDefinitionType,
                virtualSensorDefinition,
                sensor.PullModeCommunicationType,
                sensor.PullModeDotNetObjectType,
                sensor.PullFrequencyInSeconds,
                sensor.DefaultValue,
                sensor.IsSynchronousPushToActuator,
                sensor.IsActuator,
                sensor.PushModeCommunicationType);
		}

		#endregion

		/// <summary>
		/// Gets the sensor data.
		/// </summary>
        /// <param name="sensorInternalIdList">The sensor internal id list.</param>
		/// <param name="dataStartDateTime">The data start date time.</param>
		/// <param name="dataEndDateTime">The data end date time.</param>
		/// <param name="maxResultsPerSensor">The max results per sensor.</param>
		/// <returns></returns>
        public override List<MultipleSensorData> GetSensorData(List<Int32> sensorInternalIdList, DateTime dataStartDateTime, DateTime dataEndDateTime, int maxResultsPerSensor)
        {
            List<MultipleSensorData> result = new List<MultipleSensorData>();

            int count = sensorInternalIdList.Count;
            string[] sqlStatements = new string[count];
            object[][] parameters = new object[count][];

            //Create the batch
            for (int i = 0; i < count; i++){
                sqlStatements[i] = maxResultsPerSensor == 0 ? SqlGetSensorData : String.Format(SqlGetSensorDataMaxLimited, maxResultsPerSensor);
                parameters[i] = new object[] { sensorInternalIdList[i], dataStartDateTime, dataEndDateTime };
            }

            return ExtractMultipleSensorData(mSqlAccess.ExecuteBatchQuery(sqlStatements, parameters));
        }

        /// <summary>
        /// Gets the latest sensor data.
        /// </summary>
        /// <param name="sensorIdList">The sensor internal id list.</param>
        /// <param name="maxResultsPerSensor">The max results.</param>
        /// <returns></returns>
        public override List<MultipleSensorData> GetSensorDataLatest(List<Int32> sensorInternalIdList, int maxResultsPerSensor)
        {
            List<MultipleSensorData> result = new List<MultipleSensorData>();

            int count = sensorInternalIdList.Count;
            string[] sqlStatements = new string[count];
            object[][] parameters = new object[count][];

            //Create the batch
            for (int i = 0; i < count; i++)
            {
                sqlStatements[i] = maxResultsPerSensor == 0 ? SqlGetSensorDataLatest : String.Format(SqlGetSensorDataLatestMaxLimited, maxResultsPerSensor);
                parameters[i] = new object[] { sensorInternalIdList[i] };
            }

            return ExtractMultipleSensorData(mSqlAccess.ExecuteBatchQuery(sqlStatements, parameters));
        }

		/// <summary>
		/// Stores the sensor data from the list to the database
		/// </summary>
		/// <param name="sensorDataList">The sensor data list.</param>
		/// <returns>the call result</returns>
		public override CallResult StoreSensorData(List<SensorDataForDevice> sensorDataList)
		{
			CallResult result = new CallResult();

			var writeData = new List<DbSensorData>();

			// write all in one go
            try
            {
                DumpDataToDatabase(sensorDataList);
                result.Success = true;
            }
            catch (Exception exc)
            {
                log.Error(Properties.Resources.ErrorFailedStoringSensorData, exc);
                result.Success = false;
                result.AddError(Properties.Resources.ErrorFailedStoringSensorData);
            }

			return (result);
		}

        /// <summary>
        /// The method adds a new dependency between two existing sensors.
        /// </summary>
        /// <param name="baseSensorInternalId"></param>
        /// <param name="dependentSensorInternalId"></param>
        public override void AddSensorDependency(int baseSensorInternalId, int dependentSensorInternalId)
        {
            mSqlAccess.ExecuteScalar(SqlAddSensorDependency,
                baseSensorInternalId,
                dependentSensorInternalId);
        }


        /// <summary>
        /// The method removes an existing dependency between two sensors.
        /// </summary>
        /// <param name="baseSensorInternalId"></param>
        /// <param name="dependentSensorInternalId"></param>
        public override void RemoveSensorDependency(int baseSensorInternalId, int dependentSensorInternalId)
        {
            mSqlAccess.ExecuteScalar(SqlRemoveSensorDependency,
                baseSensorInternalId,
                dependentSensorInternalId);
        }

        /// <summary>
        /// The method retrieves all sensor dependencies.
        /// </summary>
        /// <returns></returns>
        public override List<SensorDependency> GetSensorDependencies()
        {
            return ExtractSensorDependencies(mSqlAccess.ExecuteQuery(SqlGetSensorDependencies));
        }

        /// <summary>
        /// The method retrieves all sensor dependencies where the sensor with the passed internal id is the dependent one.
        /// </summary>
        /// <returns></returns>
        public override List<SensorDependency> GetBaseSensorDependencies(int dependentSensorInternalId)
        {
            return ExtractSensorDependencies(mSqlAccess.ExecuteQuery(SqlGetSensorDependenciesByDependent, dependentSensorInternalId));
        }

        /// <summary>
        /// The method retrieves all sensor dependencies where the sensor with the passed internal id is the base one.
        /// </summary>
        /// <returns></returns>
        public override List<SensorDependency> GetDependentSensorDependencies(int baseSensorInternalId)
        {
            return ExtractSensorDependencies(mSqlAccess.ExecuteQuery(SqlGetSensorDependenciesByBase, baseSensorInternalId));
        }

		/// <summary>
		/// Deletes the device.
		/// </summary>
		/// <param name="deviceId">The device id.</param>
		public override void DeleteDevice(string deviceId)	 
		{
            throw new NotImplementedException();
		}

		/// <summary>
		/// Deletes the sensor.
		/// </summary>
		/// <param name="sensorId">The sensor id.</param>
		public override void DeleteSensor(string sensorId)
		{
            throw new NotImplementedException();
		}


	    public override string GetCorrelationId()
	    {
	        return Convert.ToString(mSqlAccess.ExecuteScalar(SqlSelectNextValueForCorrelationId));
	    }


	    #region Load...

		/// <summary>
		/// Loads the sensors for a given device
		/// </summary>
		/// <param name="deviceId"></param>
		/// <returns>
		/// a list of sensors for the device specified
		/// </returns>
		public override List<Sensor> LoadSensorsForDevice(string deviceId)
		{
            return ExtractSensors(mSqlAccess.ExecuteQuery(SqlGetSensorsForDevice, deviceId));
        }

		/// <summary>
		/// Loads the devices.
		/// </summary>
		/// <returns></returns>
		public override List<Device> LoadDevices()
		{
            return ExtractDevices(mSqlAccess.ExecuteQuery(SqlGetDevices));
		}

		#endregion

        #region Private methods...
        private void DumpDataToDatabase(List<SensorDataForDevice> parameters)
        {
            using (SqlConnection connection =
                   new SqlConnection(mSqlAccess.ConnectionStringSettings.ConnectionString))
            {
                connection.Open();

                // Create a table with some rows. 
                DataTable dataToDump = CreateDumpDataTable(parameters);

                // Create the SqlBulkCopy object. 
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = SqlTableSensorData;
                    bulkCopy.ColumnMappings.Add(ArgumentIndexWriteValueId, SqlColumnSensorDataSensorId);
                    bulkCopy.ColumnMappings.Add(ArgumentIndexWriteValueTakenWhen, SqlColumnSensorDataTakenWhen);
                    bulkCopy.ColumnMappings.Add(ArgumentIndexWriteValueValue, SqlColumnSensorDataValue);
                    bulkCopy.ColumnMappings.Add(ArgumentIndexWriteValueCorrelationId, SqlColumnSensorDataCorrelationId);

                    //Dump to the SQL server
                    bulkCopy.WriteToServer(dataToDump);
                }
            }
        }

        private DataTable CreateDumpDataTable(List<SensorDataForDevice> parameters)
        {
            DataTable result = new DataTable();

            //Create columns
            DataColumn columnId = new DataColumn();
            columnId.DataType = typeof(Int32);
            columnId.ColumnName = SqlColumnSensorDataSensorId;
            result.Columns.Add(columnId);

            DataColumn columnTimestamp = new DataColumn();
            // columnTimestamp.DataType = typeof(DateTime);
            columnTimestamp.DataType = typeof(string);
            columnTimestamp.ColumnName = SqlColumnSensorDataTakenWhen;
            result.Columns.Add(columnTimestamp);

            DataColumn columnValue = new DataColumn();
            columnValue.DataType = typeof(String);
            columnValue.ColumnName = SqlColumnSensorDataValue;
            result.Columns.Add(columnValue);

            DataColumn columnCorrelationId = new DataColumn();
            columnCorrelationId.DataType = typeof(String);
            columnCorrelationId.ColumnName = SqlColumnSensorDataCorrelationId;
            result.Columns.Add(columnCorrelationId);

            //Fill the table
            foreach (SensorDataForDevice nextVariableData in parameters)
            {
                DataRow row = result.NewRow();

                row[SqlColumnSensorDataSensorId] = nextVariableData.SensorId;
                row[SqlColumnSensorDataTakenWhen] = nextVariableData.GeneratedWhen.ToString(DateTimeFormat);
                row[SqlColumnSensorDataValue] = nextVariableData.Value;
                row[SqlColumnSensorDataCorrelationId] = nextVariableData.CorrelationId;

                result.Rows.Add(row);
            }

            return result;
        }

        private List<MultipleSensorData> ExtractMultipleSensorData(Dictionary<string, object>[] data)
        {
            List<MultipleSensorData> result = new List<MultipleSensorData>();

            //Traverse the list and create corresponding local object
            Dictionary<string, List<SensorData>> tempResult = new Dictionary<string, List<SensorData>>();
            Dictionary<string, Int32> lookupInternalId = new Dictionary<string, Int32>();
            List<SensorData> tempMultipleSensorData;
            foreach (Dictionary<string, object> next in data)
            {

                string nextId = Convert.ToString(next[SqlColumnSensorId]);
                lookupInternalId[nextId] = Convert.ToInt32(next[SqlColumnSensorSensorId]);

                if (tempResult.TryGetValue(nextId, out tempMultipleSensorData) == false)
                {
                    tempMultipleSensorData = new List<SensorData>();
                    tempResult[nextId] = tempMultipleSensorData;
                }

                tempMultipleSensorData.Add(new SensorData(Convert.ToDateTime(next[SqlColumnSensorDataTakenWhen]), Convert.ToString(next[SqlColumnSensorDataValue]),
                    Convert.ToString(next[SqlColumnSensorDataCorrelationId])));
            }

            //Fill the result with data
            foreach (var entry in tempResult)
            {
                result.Add(new MultipleSensorData() { SensorId = entry.Key, InternalSensorId = lookupInternalId[entry.Key], Measures = entry.Value.ToArray() });
            }

            return result;
        }

        private List<Device> ExtractDevices(Dictionary<string, object>[] data)
        {
            List<Device> result = new List<Device>();

            // traverse the list and create the real objects
            foreach (Dictionary<string, object> next in data)
            {
                Device device = new Device();

                try
                {
                    device.Id = Convert.ToString(next[SqlColumnDeviceId]);
                    device.Description = Convert.ToString(next[SqlColumnDeviceDescription]);
                    device.DeviceIpEndPoint = Convert.ToString(next[SqlColumnDeviceIpEndPoint]);
                    //string deviceLocationName = next.GetString(SqlColumnDeviceLocationName, null);
                    //if (deviceLocationName != null)
                    //{
                    //    device.Location = new Location();
                    //    device.Location.Name = deviceLocationName;
                    //    device.Location.Elevation = next.GetDouble(SqlColumnDeviceElevation, 0);
                    //    device.Location.Latitude = next.GetDouble(SqlColumnDeviceLatitude, 0);
                    //    device.Location.Longitude = next.GetDouble(SqlColumnDeviceLongitude, 0);
                    //}

                    result.Add(device);
                }
                catch (Exception x)
                {
                    // just log and go on
                    log.ErrorFormat("Exception during extracting device: {0} occured: {1}", device.Id, x.ToString());
                }
            }

            return result;
        }

        private List<Sensor> ExtractSensors(Dictionary<string, object>[] data)
        {
            List<Sensor> result = new List<Sensor>();

            // traverse the list and create the real objects
            foreach (Dictionary<string, object> next in data)
            {
                Sensor sensor = new Sensor();

                try
                {
                    sensor.Id = Convert.ToString(next[SqlColumnSensorId]);
                    sensor.InternalSensorId = Convert.ToInt32(next[SqlColumnSensorSensorId]);
                    sensor.DeviceId = Convert.ToString(next[SqlColumnSensorDeviceId]);
                    sensor.Description = Convert.ToString(next[SqlColumnSensorDescription]);
                    sensor.IsVirtualSensor = Convert.ToBoolean(next[SqlColumnSensorIsVirtualSensor]);
                    sensor.SensorDataRetrievalMode = (SensorDataRetrievalMode)Convert.ToInt32(next[SqlColumnSensorSensorDataRetrievalMode]);
                    sensor.SensorValueDataType = (SensorValueDataType)Convert.ToInt32(next[SqlColumnSensorSensorValueDataType]);
                    sensor.PullModeDotNetObjectType = next.GetString(SqlColumnSensorPullModeDotNetType, null);
                    sensor.Category = next.ContainsKey(SqlColumnSensorSensorCategory) ? Convert.ToString(next[SqlColumnSensorSensorCategory]) : null;
                    sensor.PullFrequencyInSeconds = Convert.ToInt32(next[SqlColumnSensorPullFrequencyInSec]);
                    sensor.PullModeCommunicationType = (PullModeCommunicationType)Convert.ToInt32(next[SqlColumnSensorPullModeCommunicationType]);
                    sensor.PushModeCommunicationType = (PullModeCommunicationType)Convert.ToInt32(next[SqlColumnSensorPushModeCommunicationType]);
                    sensor.DefaultValue = Convert.ToString(next[SqlColumnSensorDefaultValue]);
                    sensor.IsSynchronousPushToActuator = Convert.ToBoolean(next[SqlColumnSensorIsSynchronousPushToActuator]);
                    sensor.IsActuator = Convert.ToBoolean(next[SqlColumnSensorIsActuator]);

                    sensor.ShallSensorDataBePersisted = Convert.ToBoolean(next[SqlColumnSensorShallSensorDataBePersisted]);
                    sensor.PersistDirectlyAfterChange = Convert.ToBoolean(next[SqlColumnSensorPersistDirectlyAfterChange]);
                    sensor.UnitSymbol = Convert.ToString(next[SqlColumnSensorUnitSymbol]);
                    if (sensor.IsVirtualSensor)
                    {
                        sensor.VirtualSensorDefinition = new VirtualSensorDefinition();
                        sensor.VirtualSensorDefinition.VirtualSensorCalculationType = (VirtualSensorCalculationType)Convert.ToInt32(next[SqlColumnSensorSensorDataCalculationMode]);
                        sensor.VirtualSensorDefinition.VirtualSensorDefinitionType = (VirtualSensorDefinitionType)Convert.ToInt32(next[SqlColumnSensorVirtualSensorDefinitionType]);
                        sensor.VirtualSensorDefinition.Definition = next.GetString(SqlColumnSensorVirtualSensorDefininition, null);
                    }
                }
                catch (Exception x)
                {
                    log.ErrorFormat("Exception during extracting sensor: {0} for device: {1} occured: {2}", sensor.Id, sensor.DeviceId, x.ToString());
                    continue;
                }

                result.Add(sensor);
            }

            return result;
        }

        private List<SensorDependency> ExtractSensorDependencies(Dictionary<string, object>[] data)
        {
            List<SensorDependency> result = new List<SensorDependency>();

            foreach (Dictionary<string, object> next in data)
            {
                SensorDependency sensorDependency = new SensorDependency();
                sensorDependency.BaseSensorInternalId = Convert.ToInt32(next[SqlColumnSensorDependencyBaseSensorId]);
                sensorDependency.DependentSensorInternalId = Convert.ToInt32(next[SqlColumnSensorDependencyDependentSensorId]);
                
                result.Add(sensorDependency);
            }

            return result;
        }
        #endregion
    }
}
