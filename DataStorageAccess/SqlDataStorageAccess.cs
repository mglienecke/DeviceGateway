using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;

using GlobalDataContracts;
using log4net;

namespace DataStorageAccess
{
	public class SqlDataStorageAccess : DataStorageAccessBase 
	{
		/// <summary>
		/// logger instance
		/// </summary>
		private static log4net.ILog log = log4net.LogManager.GetLogger(typeof(SqlDataStorageAccess));

		/// <summary>
		/// Synchronization lock for Submit calls
		/// </summary>
		private object submitSync = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlDataStorageAccess"/> class 
		/// </summary>
		public SqlDataStorageAccess()
		{

		}

		#region Store...

		/// <summary>
		/// stores a device
		/// </summary>
		/// <param name="device"></param>
		public override void StoreDevice(Device device)
		{
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {
                context.DbDevice.InsertOnSubmit(new DbDevice(device));
                context.SubmitChanges(ConflictMode.FailOnFirstConflict);
            }
		}

		/// <summary>
		/// Updates the device.
		/// </summary>
		/// <param name="device">The device.</param>
		public override void UpdateDevice(Device device)
		{
			// get the old original
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {

                DbDevice dbDev = (from d in context.DbDevice where d.Id == device.Id select d).First();
                dbDev.FillDeviceData(device);

                context.SubmitChanges(ConflictMode.FailOnFirstConflict);
            }
		}

		/// <summary>
		/// Stores the sensor.
		/// </summary>
		/// <param name="sensor">The sensor.</param>
		public override void StoreSensor(Sensor sensor)
		{
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {

                context.DbSensor.InsertOnSubmit(new DbSensor(sensor));
                context.SubmitChanges(ConflictMode.FailOnFirstConflict);

                // load the auto generated internal sensor id 
                sensor.InternalSensorId = (from s in context.DbSensor where s.DeviceId == sensor.DeviceId && s.Id == sensor.Id select s.SensorId).First();
            }
		}

		/// <summary>
		/// Updates the sensor.
		/// </summary>
		/// <param name="sensor">The sensor.</param>
		public override void UpdateSensor(Sensor sensor)
		{
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {

                DbSensor dbSens = (from s in context.DbSensor where s.Id == sensor.Id select s).First();
                dbSens.FillSensorData(sensor);
                context.SubmitChanges(ConflictMode.FailOnFirstConflict);
            }
		}

		#endregion

		/// <summary>
		/// Gets the sensor data.
		/// </summary>
        /// <param name="sensorInternalIdList">The sensor internal id list.</param>
		/// <param name="dataStartDateTime">The data start date time.</param>
		/// <param name="dataEndDateTime">The data end date time.</param>
		/// <param name="maxResults">The max results.</param>
		/// <returns></returns>
        public override List<MultipleSensorData> GetSensorData(List<Int32> sensorInternalIdList, DateTime dataStartDateTime, DateTime dataEndDateTime, int maxResults)
        {
            List<MultipleSensorData> result = new List<MultipleSensorData>();

            // Load all sensor data for the specified sensors that are in the date range
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {

                IQueryable<DbSensorData>[] dataLists = new IQueryable<DbSensorData>[sensorInternalIdList.Count()];
                if (maxResults == 0)
                {
                    dataLists[0] = (from data in context.DbSensorData
                                where sensorInternalIdList.Contains(data.DbSensor.SensorId)
                                   && data.TakenWhen >= dataStartDateTime
                                   && data.TakenWhen < dataEndDateTime
                                orderby data.TakenWhen
                                select data);
                }
                else
                {
                    for (int i = 0; i < sensorInternalIdList.Count(); i++)
                    {
                        Int32 sensorId = sensorInternalIdList[i];
                        dataLists[i] = (from data in context.DbSensorData
                                    where data.DbSensor.SensorId == sensorId
                                       && data.TakenWhen >= dataStartDateTime
                                       && data.TakenWhen < dataEndDateTime
                                    orderby data.TakenWhen
                                    select data).Take(maxResults);
                    }
                }


                result = new List<MultipleSensorData>();

                Dictionary<string, List<SensorData>> tempResult = new Dictionary<string, List<SensorData>>();
                List<SensorData> tempMultipleSensorData;

                if (maxResults == 0)
                {
                    //Traverse the list and create corresponding local object
                    foreach (DbSensorData s in dataLists[0])
                    {
                        if (tempResult.TryGetValue(s.DbSensor.Id, out tempMultipleSensorData) == false)
                        {
                            tempMultipleSensorData = new List<SensorData>();
                            tempResult[s.DbSensor.Id] = tempMultipleSensorData;
                        }

                        tempMultipleSensorData.Add(new SensorData(s.TakenWhen, s.Value, s.CorrelationId));
                    }
                }
                else
                {
                    for (int i = 0; i < sensorInternalIdList.Count(); i++)
                    {
                        //Traverse the list and create corresponding local object
                        foreach (DbSensorData s in dataLists[i])
                        {
                            if (tempResult.TryGetValue(s.DbSensor.Id, out tempMultipleSensorData) == false)
                            {
                                tempMultipleSensorData = new List<SensorData>();
                                tempResult[s.DbSensor.Id] = tempMultipleSensorData;
                            }

                            tempMultipleSensorData.Add(new SensorData(s.TakenWhen, s.Value, s.CorrelationId));
                        }
                    }
                }

                //Fill the result with data
                foreach (var entry in tempResult)
                {
                    result.Add(new MultipleSensorData() { SensorId = entry.Key, Measures = entry.Value.ToArray() });
                }
            }

            return (result);
        }

        /// <summary>
        /// Gets the latest sensor data.
        /// </summary>
        /// <param name="sensorInternalIdList">The sensor internal id list.</param>
        /// <param name="maxResults">The max results.</param>
        /// <returns></returns>
        public override List<MultipleSensorData> GetSensorDataLatest(List<Int32> sensorInternalIdList, int maxResults)
        {
             List<MultipleSensorData> result = new List<MultipleSensorData>();

            // Load all sensor data for the specified sensors that are in the date range
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {
               
                IQueryable<DbSensorData>[] dataLists = new IQueryable<DbSensorData>[sensorInternalIdList.Count()];

                if (maxResults == 0)
                {
                    dataLists[0] = (from data in context.DbSensorData
                                where sensorInternalIdList.Contains(data.DbSensor.SensorId)
                                orderby data.TakenWhen descending
                                select data);
                }
                else
                {
                    for (int i = 0; i < sensorInternalIdList.Count(); i++)
                    {
                        int sensorId = sensorInternalIdList[i];
                        dataLists[i] = (from data in context.DbSensorData
                                        where data.DbSensor.SensorId == sensorId
                                        orderby data.TakenWhen descending
                                        select data).Take(maxResults);
                    }
                }

                result = new List<MultipleSensorData>();

                Dictionary<string, List<SensorData>> tempResult = new Dictionary<string, List<SensorData>>();
                List<SensorData> tempMultipleSensorData;

                if (maxResults == 0)
                {
                    //Traverse the list and create corresponding local object
                    foreach (DbSensorData s in dataLists[0])
                    {
                        if (tempResult.TryGetValue(s.DbSensor.Id, out tempMultipleSensorData) == false)
                        {
                            tempMultipleSensorData = new List<SensorData>();
                            tempResult[s.DbSensor.Id] = tempMultipleSensorData;
                        }

                        tempMultipleSensorData.Add(new SensorData(s.TakenWhen, s.Value, s.CorrelationId));
                    }
                }
                else
                {
                    for (int i = 0; i < sensorInternalIdList.Count(); i++)
                    {
                        //Traverse the list and create corresponding local object
                        foreach (DbSensorData s in dataLists[i])
                        {
                            if (tempResult.TryGetValue(s.DbSensor.Id, out tempMultipleSensorData) == false)
                            {
                                tempMultipleSensorData = new List<SensorData>();
                                tempResult[s.DbSensor.Id] = tempMultipleSensorData;
                            }

                            tempMultipleSensorData.Add(new SensorData(s.TakenWhen, s.Value, s.CorrelationId));
                        }
                    }
                }

                //Fill the result with data
                foreach (var entry in tempResult)
                {
                    result.Add(new MultipleSensorData() { SensorId = entry.Key, Measures = entry.Value.ToArray() });
                }
            }

            return (result);
        }

		/// <summary>
		/// Stores the sensor data from the list to the database
		/// </summary>
		/// <param name="sensorDataList">The sensor data list.</param>
		/// <returns>the call result</returns>
		public override CallResult StoreSensorData(List<SensorDataForDevice> sensorDataList)
		{
            DateTime start = DateTime.Now;

			CallResult result = new CallResult();

			var writeData = new List<DbSensorData>();

			foreach (SensorDataForDevice writeValue in sensorDataList)
			{
				DbSensorData sensorData = new DbSensorData();
				sensorData.SensorId = writeValue.SensorId;
				sensorData.TakenWhen = writeValue.GeneratedWhen;
				sensorData.Value = writeValue.Value;
			    sensorData.CorrelationId = writeValue.CorrelationId;
				writeData.Add(sensorData);
			}

			// write all in one go
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {

                context.DbSensorData.InsertAllOnSubmit(writeData);
                context.SubmitChanges(ConflictMode.ContinueOnConflict);
            }

			// even if there were errors everything went through
			result.Success = true;

            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            Console.WriteLine(duration);

			return (result);
		}

        /// <summary>
        /// The method adds a new dependency between two existing sensors.
        /// </summary>
        /// <param name="baseSensorInternalId"></param>
        /// <param name="dependentSensorInternalId"></param>
        public override void AddSensorDependency(int baseSensorInternalId, int dependentSensorInternalId)
        {
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {
                DbSensorDependency newDependency = new DbSensorDependency();
                newDependency.BaseSensorId = baseSensorInternalId;
                newDependency.DependentSensorId = dependentSensorInternalId;

                context.DbSensorDependency.InsertOnSubmit(newDependency);
                context.SubmitChanges();
            }
        }


        /// <summary>
        /// The method removes an existing dependency between two sensors.
        /// </summary>
        /// <param name="baseSensorInternalId"></param>
        /// <param name="dependentSensorInternalId"></param>
        public override void RemoveSensorDependency(int baseSensorInternalId, int dependentSensorInternalId)
        {
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {
                DbSensorDependency dependencyToRemove = new DbSensorDependency();
                dependencyToRemove.BaseSensorId = baseSensorInternalId;
                dependencyToRemove.DependentSensorId = dependentSensorInternalId;

                context.DbSensorDependency.DeleteOnSubmit(dependencyToRemove);
                context.SubmitChanges();
            }
        }

        /*
		/// <summary>
		/// Stores the device data.
		/// </summary>
		/// <param name="deviceGetter">The device getter.</param>
		/// <param name="sensorGetter">The sensor getter.</param>
		/// <param name="writeValueList">The write value list.</param>
		/// <returns>the call result</returns>
		public override CallResult StoreSensorData(GetDevice deviceGetter, GetSensor sensorGetter, List<InternalSensorValue> writeValueList)
		{
			CallResult result = new CallResult();
            if (writeValueList.Count == 0)
            {
                result.Success = true;
                return result;
            }


			var writeData = new HashSet<DbSensorData>();

			foreach (InternalSensorValue writeValue in writeValueList)
			{
				Sensor sensor = sensorGetter(writeValue.DeviceId, writeValue.SensorId);

				if (sensor != null)
				{
					IEnumerable<KeyValuePair<Int64, object>> valueEnum = writeValue.GetValuesToSave();

					foreach (KeyValuePair<Int64, object> keyVal in valueEnum)
					{
						DbSensorData sensorData = new DbSensorData();
						sensorData.SensorId = sensor.InternalSensorId;
						sensorData.TakenWhen = new DateTime(keyVal.Key);
						
						// TODO: The real value must be taken!
                        sensorData.Value = keyVal.Value == null ? null : Convert.ToString(keyVal.Value);

						writeData.Add(sensorData);
					}
				}
				else
				{
					result.AddErrorFormat(Properties.Resources.ErrorSensorIsNotRegistered, writeValue.SensorId, writeValue.DeviceId);
				}
			}

			// write all in one go
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {

                context.DbSensorData.InsertAllOnSubmit(writeData);
                context.SubmitChanges(ConflictMode.ContinueOnConflict);
            }

			// even if there were errors everything went through
			result.Success = true;
			return (result);
		}
         */

		/// <summary>
		/// Deletes the device.
		/// </summary>
		/// <param name="deviceId">The device id.</param>
		public override void DeleteDevice(string deviceId)	 
		{
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {

                DbDevice dbDev = (from d in context.DbDevice where d.Id == deviceId select d).FirstOrDefault();
                if (dbDev != null)
                {
                    context.DbDevice.DeleteOnSubmit(dbDev);
                    context.SubmitChanges(ConflictMode.FailOnFirstConflict);
                }
            }

		}

		/// <summary>
		/// Deletes the sensor.
		/// </summary>
		/// <param name="sensorId">The sensor id.</param>
		public override void DeleteSensor(string sensorId)
		{
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {

                DbSensor dbSens = (from s in context.DbSensor where s.Id == sensorId select s).FirstOrDefault();
                if (dbSens != null)
                {
                    context.DbSensor.DeleteOnSubmit(dbSens);
                    context.SubmitChanges(ConflictMode.FailOnFirstConflict);
                }
            }
		}

        /// <summary>
        /// The method retrieves all sensor dependencies.
        /// </summary>
        /// <returns></returns>
        public override List<SensorDependency> GetSensorDependencies()
        {
            List<SensorDependency> result = new List<SensorDependency>();
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {
                foreach (DbSensorDependency next in context.DbSensorDependency)
                {
                    result.Add(new SensorDependency() { BaseSensorInternalId = next.BaseSensorId, DependentSensorInternalId = next.DependentSensorId });
                }
            }

            return result;
        }

        /// <summary>
        /// The method retrieves all sensor dependencies where the sensor with the passed internal id is the dependent one.
        /// </summary>
        /// <returns></returns>
        public override List<SensorDependency> GetBaseSensorDependencies(int dependentSensorInternalId)
        {
            List<SensorDependency> result = new List<SensorDependency>();
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {
                var foundDependencies = from n in context.DbSensorDependency
                                        where n.DependentSensorId == dependentSensorInternalId
                                        select n;
                foreach (DbSensorDependency next in foundDependencies)
                {
                    result.Add(new SensorDependency() { BaseSensorInternalId = next.BaseSensorId, DependentSensorInternalId = next.DependentSensorId });
                }
            }

            return result;
        }

        /// <summary>
        /// The method retrieves all sensor dependencies where the sensor with the passed internal id is the base one.
        /// </summary>
        /// <returns></returns>
        public override List<SensorDependency> GetDependentSensorDependencies(int baseSensorInternalId)
        {
            List<SensorDependency> result = new List<SensorDependency>();
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {
                var foundDependencies = from n in context.DbSensorDependency
                                        where n.BaseSensorId == baseSensorInternalId
                                        select n;
                foreach (DbSensorDependency next in foundDependencies)
                {
                    result.Add(new SensorDependency() { BaseSensorInternalId = next.BaseSensorId, DependentSensorInternalId = next.DependentSensorId });
                }
            }

            return result;
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
            List<Sensor> result;
			// load all the sensors for a given device and create the resulting structure
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {

                var sensorList = from s in context.DbSensor
                                 where s.DeviceId == deviceId
                                 select s;

                result = new List<Sensor>(sensorList.Count());

                // traverse the list and create the real objects
                foreach (DbSensor s in sensorList)
                {
                    Sensor sensor = new Sensor();

                    sensor.Id = s.Id;
                    sensor.InternalSensorId = s.SensorId;
                    sensor.DeviceId = s.DeviceId;
                    sensor.Description = s.Description;
                    sensor.IsVirtualSensor = s.IsVirtualSensor;
                    sensor.SensorDataRetrievalMode = (SensorDataRetrievalMode)s.SensorDataRetrievalMode;
                    sensor.SensorValueDataType = (SensorValueDataType)s.SensorValueDataType;
                    sensor.PullModeDotNetObjectType = s.PullModeDotNetType;
                    sensor.Category = s.SensorCategory;
                    sensor.PullFrequencyInSeconds = s.PullFrequencyInSec;
                    sensor.PullModeCommunicationType = (PullModeCommunicationType)s.PullModeCommunicationType;
                    sensor.DefaultValue = s.DefaultValue;
                    sensor.IsSynchronousPushToActuator = s.IsSynchronousPushToActuator;
                    sensor.IsActuator = s.IsActuator;

                    sensor.ShallSensorDataBePersisted = s.ShallSensorDataBePersisted;
                    sensor.UnitSymbol = s.UnitSymbol;
                    if (sensor.IsVirtualSensor)
                    {
                        sensor.VirtualSensorDefinition = new VirtualSensorDefinition();
                        sensor.VirtualSensorDefinition.VirtualSensorCalculationType = (VirtualSensorCalculationType)s.SensorDataCalculationMode;
                        sensor.VirtualSensorDefinition.VirtualSensorDefinitionType = (VirtualSensorDefinitionType)s.VirtualSensorDefinitionType;
                        sensor.VirtualSensorDefinition.Definition = s.VirtualSensorDefininition;
                    }

                    result.Add(sensor);
                }
            }

			return (result);
		}

		/// <summary>
		/// Loads the devices.
		/// </summary>
		/// <returns></returns>
		public override List<Device> LoadDevices()
		{
			// load all devices and create the device types
            using (SqlStorageLinqDataClassesDataContext context = new SqlStorageLinqDataClassesDataContext())
            {

                var devices = from d in context.DbDevice
                              orderby d.Id
                              select new Device
                              {
                                  Id = d.Id,
                                  Description = d.Description,
                                  Location = new Location
                                  {
                                      Name = d.LocationName,
                                      Latitude = d.Latitude.HasValue ? Convert.ToDouble(d.Latitude.Value) : 0,
                                      Longitude = d.Longitude.HasValue ? Convert.ToDouble(d.Longitude.Value) : 0,
                                      Elevation = d.Elevation.HasValue ? Convert.ToDouble(d.Elevation.Value) : 0
                                  }
                              };

                return (devices.ToList());
            }
		}

		#endregion

        public override string GetCorrelationId()
        {
            throw new NotSupportedException();
        }

	}
}
