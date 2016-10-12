using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using GlobalDataContracts;
using System.Threading;
using System.ComponentModel;
using System.Net;
//using Newtonsoft.Json;
using System.IO;
using Common.Server;
using System.Configuration;

namespace CentralServerService
{
    /// <summary>
    /// The class implements a task that scans device sensors with the PULL retrieval mode.
    /// </summary>
    public class DeviceScanningTask
    {
        #region Configuration properties...
        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgSensorScanningErrorThreshold = "SensorScanningErrorThreshold";
        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgDeviceCommunicationHandlerTypeNamePrefix = "DeviceCommunicationHandlerTypeName.";
        /// <summary>
        /// Default configuration property value.
        /// </summary>
        public static readonly string DefaultDeviceCommunicationHandlerTypeName = typeof(HttpRestDeviceCommunicationHandler).AssemblyQualifiedName;
        /// <summary>
        /// Default configuration property value.
        /// </summary>
        public const int DefaultSensorScanningErrorThreshold = 1;
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<string, string> mDeviceIds = new Dictionary<string, string>();
        private readonly Dictionary<string, IEnumerable<Int32>> mDeviceSensorIds = new Dictionary<string, IEnumerable<Int32>>();
        private readonly Dictionary<Int32, Timer> mTimers = new Dictionary<Int32, Timer>();
        private readonly Dictionary<string, IDeviceCommunicationHandler> mDeviceCommHandlers = new Dictionary<string, IDeviceCommunicationHandler>();
        private CentralServerServiceImpl mService;
        private int mSensorScanningErrorThreshold = DefaultSensorScanningErrorThreshold;
        private Dictionary<int, int> mSensorScanningErrorCount = new Dictionary<int, int>();


        /// <summary>
        /// Constructor.
        /// </summary>
        public DeviceScanningTask(CentralServerServiceImpl service)
        {
            mService = service;

            //SensorScanningErrorThreshold
            string valueStr = ConfigurationManager.AppSettings[CfgSensorScanningErrorThreshold];
            if (valueStr != null)
            {
                try
                {
                    mSensorScanningErrorThreshold = Int32.Parse(valueStr);
                }
                catch (Exception exc)
                {
                    log.Fatal(String.Format(Properties.Resources.ExceptionInvalidConfigurationPropertyValue, CfgSensorScanningErrorThreshold, valueStr), exc);
                    throw new ConfigurationErrorsException(String.Format(Properties.Resources.ExceptionInvalidConfigurationPropertyValue, CfgSensorScanningErrorThreshold, valueStr), exc);
                }
            }
        }

        /// <summary>
        /// The method starts the scanning routine.
        /// </summary>
        public void Start()
        {
            foreach (string deviceId in mDeviceIds.Keys){
                StartScanningDeviceSensors(deviceId);
            }
        }

        /// <summary>
        /// The method stops the scanning routine.
        /// </summary>
        public void Stop()
        {
            foreach (string deviceId in mDeviceIds.Keys)
            {
                foreach (int sensorInternalId in mDeviceSensorIds[deviceId])
                {
                    StopScanningSensor(mService.GetSensor(sensorInternalId));
                }
            }
        }

        /// <summary>
        /// The method requests the tasks to add to the scanning routine all PULL-mode sensors in the passed device.
        /// </summary>
        /// <param name="device"></param>
        public void AddDeviceToScanning(Device device)
        {
            //Check paramss
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            //Check if in use
            if (GetService().IsDeviceIdUsed(device.Id))
            {
                mDeviceIds[device.Id] = String.Empty;

                //Get all sensors
                GetSensorsForDeviceResult result = GetService().GetSensorsForDevice(device.Id);
                if (result.Success)
                {
                    mDeviceSensorIds[device.Id] = result.Sensors.ConvertAll<Int32>( x => x.InternalSensorId);
                }
                else
                {
                    throw new ApplicationException(String.Format(Properties.Resources.ExceptionFailedGettingSensorsForDevice, result.ErrorMessages));
                }

                //Start scanning device sensors
                StartScanningDeviceSensors(device.Id);
            }
            else
            {
                throw new ApplicationException(String.Format(Properties.Resources.ExceptionDeviceIsNotInUse, device.Id));
            }
        }

        /// <summary>
        /// The method requests the task to stop scanning all sensors in the passed device.
        /// </summary>
        /// <param name="device"></param>
        public void RemoveDeviceFromScanning(Device device)
        {
            //Check paramss
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            //Log
            log.Info(String.Format(Properties.Resources.InfoStoppingScanningDeviceSensors, device.Id));

            //Check if listed and remove
            mDeviceIds.Remove(device.Id);

            IEnumerable<Int32> sensors;
            if (mDeviceSensorIds.TryGetValue(device.Id, out sensors) == true){
                foreach (Int32 sensorIntId in sensors)
                {
                    StopScanningSensor(mService.GetSensor(sensorIntId));
                }
            }

            mDeviceSensorIds.Remove(device.Id);
        }

        /// <summary>
        /// The method initiates scanning of the specified sensor.
        /// </summary>
        /// <param name="sensor"></param>
        /// <param name="pullFrequencyInMillis"></param>
        public void PutSensorToScanning(Sensor sensor, Int32 pullFrequencyInMillis)
        {
            //Check if scanning for the sensor is already running
            StopScanningSensor(sensor);

            //Start scanning for the sensor if it's required
            if (sensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.Pull))
            {
                //Log
                log.Info(String.Format(Properties.Resources.InfoStartingScanningSensor, sensor.DeviceId, sensor.Id));

                //Error count to zero
                mSensorScanningErrorCount[sensor.InternalSensorId] = 0;

                //Start the timer
                mTimers[sensor.InternalSensorId] = (new Timer(InitiateSensorScan, sensor, (pullFrequencyInMillis), (pullFrequencyInMillis)));
            }
        }

        /// <summary>
        /// The method stops scanning of the specified sensor.
        /// </summary>
        /// <param name="sensor"></param>
        public void StopScanningSensor(Sensor sensor)
        {
            Timer timer;
            lock (mTimers)
            {
                if (mTimers.TryGetValue(sensor.InternalSensorId, out timer))
                {
                    //Log
                    log.Info(String.Format(Properties.Resources.InfoStoppingScanningSensor, sensor.DeviceId, sensor.Id));

                    //Stop
                    timer.Change(Int32.MaxValue, Timeout.Infinite);
                    timer.Dispose();
                    mTimers.Remove(sensor.InternalSensorId);
                }
            }
        }

        private void StartScanningDeviceSensors(string deviceId)
        {
            //Log
            log.Info(String.Format(Properties.Resources.InfoStartingScanningDeviceSensors, deviceId));

            foreach (Int32 sensorIntId in mDeviceSensorIds[deviceId])
            {
                Sensor sensor = mService.GetSensor(sensorIntId);
                if (sensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.Pull) && !sensor.IsVirtualSensor)
                {
                    PutSensorToScanning(sensor, sensor.PullFrequencyInSeconds*1000);
                }
            }
        }

        private void InitiateSensorScan(object state)
        {
            try
            {
                Sensor sensor = (Sensor)state;

                //Check if the sensor is still in the regs, return if not
                if (mTimers.ContainsKey(sensor.InternalSensorId))
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ScanSensor), sensor);
                }
            }
            catch (Exception exc){
                log.Error("Sensor scanning initiation thread failed.", exc);
            }
        }

        private void ScanSensor(object state)
        {
            Sensor sensor = (Sensor)state;

            //Cancel scanning if the sensor's time is no longer active
            if (mTimers.ContainsKey(sensor.InternalSensorId) == false)
            {
                return;
            }

            Device device = mService.GetDevice(sensor.DeviceId);

            IDeviceCommunicationHandler handler;
            try
            {
                handler = GetDeviceCommunicationHandler(sensor);
            }
            catch (Exception exc)
            {
                log.Error(String.Format(Properties.Resources.ErrorFailedGettingCommunicationHandlerForSensor, sensor.DeviceId, sensor.Id), exc);
                StopScanningSensor(sensor);
                return;
            }

            //Proceed if the hander is defined
            if (handler == null)
            {
                log.Info(String.Format("SCANNING SENSOR: {0}===>{1}. DEVICE COMMUNICATION HANDER IS UNDEFINED ", device.Id, sensor.Id));
            }
            else
            {
                try
                {
                    log.Info(String.Format("SCANNING SENSOR: {0}===>{1}", device.Id, sensor.Id));

                    SensorData currentSensorData = handler.GetSensorCurrentData(device, sensor);

                    //Store what we have
                    if (currentSensorData != null)
                    {
                        MultipleSensorData multSensorData = new MultipleSensorData();
                        multSensorData.SensorId = sensor.Id;
                        multSensorData.Measures = new SensorData[] { currentSensorData };

                        StoreSensorDataResult result = GetService().StoreSensorData(device.Id, multSensorData);
                        if (result.Success)
                        {
                            ClearSensorScanningErrorCount(sensor);
                        }
                        else
                        {
                            log.Error(result.ErrorMessages);

                            //Check if the sensor's error threshold has been exceeded
                            if (MarkSensorScanningErrorAndCheckThresholdExceeded(sensor.InternalSensorId))
                            {
                                StopScanningSensor(sensor);
                            }
                        }

                        //Adjust scanning rates if needed
                        if (result.AdjustedScanPeriods != null)
                        {
                            foreach (SensorScanPeriod next in result.AdjustedScanPeriods)
                            {
                                //If the sensor still valid
                                if (mTimers.ContainsKey(next.InternalSensorId))
                                {
                                    //Restart scanning with the new period
                                    PutSensorToScanning(sensor, next.ScanPeriodInMillis);
                                }
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    log.Error(String.Format(Properties.Resources.ErrorFailedScanningSensorData, sensor.DeviceId, sensor.Id), exc);


                    //Check if the sensor is still getting scanned. If not, then it has been already put off. No need to log and stop, 
                    //it has been already done.
                    if (mTimers.ContainsKey(sensor.InternalSensorId))
                    {
                        log.Error(String.Format(Properties.Resources.ErrorFailedScanningSensor, device.Id, sensor.Id, exc.Message), exc);

                        //Check if the sensor's error threshold has been exceeded
                        if (MarkSensorScanningErrorAndCheckThresholdExceeded(sensor.InternalSensorId))
                        {
                            StopScanningSensor(sensor);
                        }
                    }
                }
            }
        }

        private bool MarkSensorScanningErrorAndCheckThresholdExceeded(Int32 sensorIntId)
        {
            //Retrieve and increment
            int errorCount = ++mSensorScanningErrorCount[sensorIntId];
            
            //Keep
            mSensorScanningErrorCount[sensorIntId] = errorCount;

            //Check
            return errorCount > mSensorScanningErrorThreshold;
        }

        private void ClearSensorScanningErrorCount(Sensor sensor)
        {
            //Clear
            mSensorScanningErrorCount[sensor.InternalSensorId] = 0;
        }

        private IDeviceCommunicationHandler GetDeviceCommunicationHandler(Sensor sensor)
        {
            //Check for undefined
            if (sensor.PullModeCommunicationType == PullModeCommunicationType.Undefined)
                return null;

            IDeviceCommunicationHandler result;

            //Is it a standard comm handler or a custom .NET comm handler?
            if (sensor.PullModeDotNetObjectType == null || sensor.PullModeDotNetObjectType.Trim().Length == 0)
            {
                return DeviceCommunicationHandlerFactory.GetDeviceCommunicationHandler(sensor.PullModeCommunicationType.ToString());
            }
            else
            {
                //Already instantiated?
                if (mDeviceCommHandlers.TryGetValue(sensor.PullModeDotNetObjectType, out result))
                {
                    return result;
                }

                //Instantiate
                try
                {
                    result = (IDeviceCommunicationHandler)Utilities.CreateObjectByTypeName(sensor.PullModeDotNetObjectType);
                    //Store if reusable
                    if (result.IsReusableAndThreadSafe)
                    {
                        lock (mDeviceCommHandlers)
                        {
                            mDeviceCommHandlers[sensor.PullModeDotNetObjectType] = result;
                        }
                    }
                    return result;
                }
                catch (Exception exc)
                {
                    throw new Exception(String.Format(Properties.Resources.ExceptionFailedCreatingIDeviceCommunicationHandlerInstance, exc.Message), exc);
                }
            }
        }

        private ICentralServerService GetService()
        {
            if (mService == null)
            {
                return CentralServerServiceClient.Instance.Service;
            }
            else
            {
                return mService;
            }
        }
    }
}
