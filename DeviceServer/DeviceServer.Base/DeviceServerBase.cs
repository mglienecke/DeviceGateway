using System;
using System.Collections;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT;
using Microsoft.SPOT.Time;
using NetMf.CommonExtensions;
using Microsoft.SPOT.Net.NetworkInformation;
using netduino.helpers.Helpers;
//using System.Net;
using System.Threading;
using System.Reflection;
using Microsoft.SPOT.IO;
using System.IO;

namespace DeviceServer.Base
{
    /// <summary>
    /// The class declares constants for the device-server communications.
    /// </summary>
    public sealed class CommunicationConstants
    {
        #region Content type constants...
        internal const string ContentTypeApplicationJson = "application/json";
        internal const string ContentTypeTextXml = "text/xml";
        internal const string ContentTypeTextHtml = "text/html";
        internal const string ContentTypeTextPlain = "text/plain";
        #endregion

        #region Server communication handler types...
        /// <summary>
        /// Communication type name
        /// </summary>
        public const string ServerCommTypeHttpRest     = "HttpRest";
        /// <summary>
        /// Communication type name
        /// </summary>
        public const string ServerCommTypeWcfSoap      = "WcfSoap";
        /// <summary>
        /// Communication type name
        /// </summary>
        public const string ServerCommTypeUdpCndep     = "UdpCndep";
        /// <summary>
        /// Communication type name
        /// </summary>
        public const string ServerCommTypeDotNetObject = "DotNetObject";
        /// <summary>
        /// Communication type name
        /// </summary>
        public const string ServerCommTypeUndefined    = "Undefined";
        #endregion

        internal static readonly Hashtable CommTypeNames = new Hashtable();
        internal const char PathSplitChar = '/';
        internal const int SensorIdPos = 2;

        static CommunicationConstants()
        {
            CommTypeNames[PullModeCommunicationType.REST] = ServerCommTypeHttpRest;
            CommTypeNames[PullModeCommunicationType.CNDEP] = ServerCommTypeUdpCndep;
            CommTypeNames[PullModeCommunicationType.SOAP] = ServerCommTypeWcfSoap;
            CommTypeNames[PullModeCommunicationType.DotNetObject] = ServerCommTypeDotNetObject;
            //Just a hardcoded default
            CommTypeNames[PullModeCommunicationType.Undefined] = ServerCommTypeHttpRest;
        }

        /// <summary>
        /// The method retrieves comm type name for the passed communication type constant.
        /// </summary>
        /// <returns></returns>
        internal static string GetCommTypeName(PullModeCommunicationType commType)
        {
            return CommTypeNames[commType] as string;
        }
    }

    /// <summary>
    /// The class provides a base for specific device server implementations by providing all necessary 
    /// functionality for registering the device and its sensors, scanning the sensors and communicating with the gateway service. 
    /// The subclass implementations must care about setting up their actual sensor definitions and adding them to the <see cref="DeviceServerBase.Sensors"/> list.
    /// </summary>
    public abstract class DeviceServerBase
    {
        internal static readonly DateTime TimeZero = new DateTime(1700, 1, 1);

        private ExtendedWeakReference mConfigRef;
        private static DeviceConfig mConfig;
        private static DeviceServerBase mInstance;
        private readonly LimitedQueue mErrorLog = new LimitedQueue(10);
        private static TextDisplayApplication mTextDisplayApplication;

        private readonly ArrayList mSensors = new ArrayList();
        private readonly SensorScanTimeComparer mSensorScanTimeComparer = new SensorScanTimeComparer();
        private readonly ArrayList mSensorScanTimes = new ArrayList();

        private readonly BlockingQueue mSendQueue = new BlockingQueue(50);

        private Thread mScanThread;
        private bool mDoRunScanThread;
        private Thread mSendThread;
        private bool mDoRunSendThread;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected DeviceServerBase()
        {
            //RemovableMedia.Insert += new InsertEventHandler(RemovableMedia_Insert);
        }

        void RemovableMedia_Insert(object sender, MediaEventArgs e)
        {
            //Debug.Print("Media Insert:");
            //PrintVolumeInfo(e.Volume);
        }

        #region Properties...
        /// <summary>
        /// The property contains the data of the hosting device.
        /// </summary>
        public Device Device { get; protected set; }

        /// <summary>
        /// The property getter returns configuration data object of the device.
        /// </summary>
        public DeviceConfig Config
        {
            get
            {
                return mConfig;
            }
        }

        /// <summary>
        /// The property contains the current list of the device's sensors.
        /// </summary>
        public ArrayList Sensors
        {
            get
            {
                return mSensors;
            }
        }

        /// <summary>
        /// The property getter returns the server's error log.
        /// The elements are of type <see cref="ErrorLogEntry"/>.
        /// </summary>
        public ICollection ErrorLog
        {
            get
            {
                return mErrorLog;
            }
        }

        /// <summary>
        /// The property returns URL of the GatewayService.
        /// </summary>
        protected string GatewayServiceUrl
        {
            get
            {
                return Properties.Resources.GetString(Properties.Resources.StringResources.CfgGatewayServiceUrl);
            }
        }

        /// <summary>
        /// The property contains the server-side timestamp to be used as a landmark when calculating timestamps for sensor values.
        /// </summary>
        public DateTime ServerTimeMark { get; set; }

        /// <summary>
        /// The property contains difference in ticks between the local device time and the server time.
        /// </summary>
        public long ServerTimeOffset { get; set; }

        /// <summary>
        /// The property contains flag defining if the sensors should use server time.
        /// </summary>
        public bool UseServerTime { get; set; }
        #endregion

        #region Methods for specific subclass implementations...
        /// <summary>
        /// The method initializes sensors for a specific implementation.
        /// </summary>
        protected abstract void LoadSensors();

        /// <summary>
        /// The method initializes device for a specific implementation.
        /// </summary>
        protected abstract void LoadDevice();

        /// <summary>
        /// The method returns default device config.
        /// </summary>
        /// <returns></returns>
        protected virtual DeviceConfig GetDefaultDeviceConfig()
        {
            return new DeviceConfig();
        }
        #endregion

        /// <summary>
        /// THe property contains reference to the actual implementation instance that has been launched.
        /// </summary>
        internal protected static DeviceServerBase RunningInstance
        {
            get
            {
                return mInstance;
            }
        }

        /// <summary>
        /// The methods corrects the current server time corresponding to the passed local time.
        /// </summary>
        /// <param name="localTime"></param>
        /// <returns></returns>
        internal protected DateTime GetServerTime(DateTime localTime)
        {
            return localTime.AddTicks(DeviceServerBase.RunningInstance.ServerTimeOffset);
        }

        /// <summary>
        /// The method initializes and starts the passed device server instance.
        /// </summary>
        /// <param name="server"></param>
        protected static void Run(DeviceServerBase server)
        {
            mInstance = server;

            mInstance.LoadConfigData();
            mInstance.Init();
            mInstance.Start();
        }

        /// <summary>
        /// The method starts the text-displaying WPF application.
        /// </summary>
        /// <param name="startText"></param>
        protected static void StartTextDisplayApplication(string startText)
        {
            mTextDisplayApplication = new TextDisplayApplication(startText);
            mTextDisplayApplication.StartApplication();
        }

        private void Init()
        {
            //Load device
            LoadDevice();

            //Load sensor data
            LoadSensors();

            //Refresh device registration if needed
            if (mConfig.IsRefreshDeviceRegOnStartup)
            {
                RefreshDeviceRegistration();
            }

            //Init the sensor data
            InitSensors();

            //Init the server communication handler
            try{
                ServerCommunicationHandlerFactory.GetServerCommunicationHandler(mConfig.ServerCommType);
            }
            catch (Exception exc){
                string errorMessage = StringUtility.Format("Device server initializion failed. Communication type: {0}; Error: {1}" + mConfig.ServerCommType, exc.ToString());
                Debug.Print(errorMessage);
                DisplayText(errorMessage);
            }

            //Init the sensor data exchange communication handlers
            string[] sensorDataExchangeCommTypes = mConfig.SensorDataExchangeCommTypes.Split(new char[] { DeviceConfig.ListSeparatorChar });
            foreach (string commType in sensorDataExchangeCommTypes)
            {
                try
                {
                    ServerCommunicationHandlerFactory.GetSensorDataExchangeCommunicationHandler(commType);
                }
                catch (Exception exc)
                {
                    string errorMessage = StringUtility.Format("Sensor data exchange handler initializion failed. Communication type: {0}; Error: {1}" + commType, exc.ToString());
                    Debug.Print(errorMessage);
                    DisplayText(errorMessage);
                }
            }
        }

        private void Start()
        {
            //Start the server comm handler
            IServerCommunicationHandler serverCommHandler = ServerCommunicationHandlerFactory.GetServerCommunicationHandler(mInstance.Config.ServerCommType);
            if (serverCommHandler.StartRequestHandling())
            {
                DisplayText("Server comm handler started. Comm type: " + mInstance.Config.ServerCommType);
            }
            else
            {
                DisplayText("Server comm handler failed to start. Comm type: " + mInstance.Config.ServerCommType);
            }


            //Start the sensor data threads
            string[] sensorDataExchangeCommTypes = mConfig.SensorDataExchangeCommTypes.Split(new char[] { DeviceConfig.ListSeparatorChar });
            foreach (string commType in sensorDataExchangeCommTypes)
            {
                ISensorDataExchangeCommunicationHandler sensorCommHandler = ServerCommunicationHandlerFactory.GetSensorDataExchangeCommunicationHandler(commType);
                if (sensorCommHandler.IsHandlingRequests == false)
                {
                    if (sensorCommHandler.StartRequestHandling())
                    {
                        DisplayText("Sensor data exchange comm handler started. Comm type: " + commType);
                    }
                    else
                    {
                        DisplayText("Sensor data exchange comm handler failed to start. Comm type: " + commType);
                    }
                }
            }
        }

        private void Stop()
        {
            mDoRunScanThread = false;
            mSendQueue.Clear();
            mDoRunSendThread = false;

            //Start the server comm handler
            IServerCommunicationHandler serverCommHandler = ServerCommunicationHandlerFactory.GetServerCommunicationHandler(mInstance.Config.ServerCommType);
            serverCommHandler.StopRequestHandling();

            //Stop the sensor data threads
            string[] sensorDataExchangeCommTypes = mConfig.SensorDataExchangeCommTypes.Split(new char[] { DeviceConfig.ListSeparatorChar });
            foreach (string commType in sensorDataExchangeCommTypes)
            {
                ISensorDataExchangeCommunicationHandler sensorCommHandler = ServerCommunicationHandlerFactory.GetSensorDataExchangeCommunicationHandler(commType);
                if (sensorCommHandler.IsHandlingRequests == true)
                    sensorCommHandler.StopRequestHandling();
            }
        }

        #region Config data methods...
        /// <summary>
        /// The method loads device configuration stored on a removable media (SD card).
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        //protected abstract string GetConfigContentFromRemovableMedia(string fileName);

        private void LoadConfigData()
        {
            //Attempt to retrieve the data from the persisent flash store
            //mConfigRef = ExtendedWeakReference.Recover(typeof(DeviceConfig), DeviceConfig.DefaultId);

            if (mConfigRef == null || mConfigRef.Target == null)
            {
                //Check the removable media
                /*
                try
                {
                    
                    var configContent = GetConfigContentFromRemovableMedia("DeviceConfig.json");
                    if (configContent != null)
                    {
                        mConfig = ContentParserFactory.GetParser(CommunicationConstants.ContentTypeApplicationJson).ParseDeviceConfig(configContent);
                    }
                    else
                    {
                        mConfig = null;
                    }
                }
                catch (Exception exc)
                {
                    LogError(exc, Properties.Resources.GetString(Properties.Resources.StringResources.ErrorFailedLoadingConfigFileFromRemovableMedia));
                    mConfig = null;
                } 
                 */


                //Got it loaded?
                if (mConfig == null)
                {
                    //Ok, get the default one
                    mConfig = GetDefaultDeviceConfig();
                }
                mConfigRef = new ExtendedWeakReference(mConfig, typeof(DeviceConfig), DeviceConfig.DefaultId, ExtendedWeakReference.c_SurvivePowerdown);
                mConfigRef.Priority = (Int32)Microsoft.SPOT.ExtendedWeakReference.PriorityLevel.System;
            }
            else
            {
                mConfig = (DeviceConfig)mConfigRef.Target;
                mConfigRef.Target = mConfig;
            }
            InitDevice();
        }

        /*
        private static void PrintVolumeInfo(VolumeInfo volumeInfo)
        {
            Debug.Print(" Name=" + volumeInfo.Name);
            Debug.Print(" Volume ID=" + volumeInfo.VolumeID);
            Debug.Print(" Volume Label=" + volumeInfo.VolumeLabel);
            Debug.Print(" File System=" + volumeInfo.FileSystem);
            Debug.Print(" Root Directory=" + volumeInfo.RootDirectory);
            Debug.Print(" Serial Number=" + volumeInfo.SerialNumber);
            Debug.Print(" Is Formatted=" + volumeInfo.IsFormatted);
            Debug.Print(" Total Size=" + volumeInfo.TotalSize);
            Debug.Print(" Total Free Space=" + volumeInfo.TotalFreeSpace);
            Debug.Print(string.Empty);
        }
         */

        protected void StoreConfigData()
        {
            mConfigRef.Target = mConfig;
        }
        #endregion

        /// <summary>
        /// The method applies the passed device config to the device server instance.
        /// </summary>
        /// <param name="newConfig"></param>
        public void ApplyDeviceConfig(DeviceConfig newConfig)
        {
            try
            {
                //Reset the config
                if (newConfig == null)
                {
                    newConfig = GetDefaultDeviceConfig();
                }

                //Store the new config data
                mConfig.Reload(newConfig);

                //Shutdown
                ShutdownSensors();
                Stop();

                //Init
                Init();

                //Start
                Start();

                //Store the config - seems valid
                mConfigRef.Target = mConfig;
            }
            catch (Exception exc)
            {
                LogError(exc, "Failed applying new config.");
            }
        }

        #region Sensor data scanning and pushing to the server...
        private void ShutdownSensors()
        {
            //Signal the thread to stop
            mDoRunScanThread = false;

            //Abort the current running scanning thread if needed
            if (mScanThread != null && mScanThread.IsAlive)
            {
                try
                {
                    mScanThread.Abort();
                }
                catch { }

                mScanThread = null;
            }


            //Signal the send thread to stop
            //mDoRunSendThread = false;
            mSendQueue.Clear();
            //Sleep to yield for the send thread do finish gracefully
            //Thread.Sleep(100); 
            //mSendThread = null;
        }

        private void InitSensors()
        {
            bool hasConfig = mConfig.SensorConfigs != null && mConfig.SensorConfigs.Count > 0;
            foreach (Sensor nextSensor in Sensors)
            {
                if (hasConfig)
                {
                    //Switched off by default
                    nextSensor.IsActive = false;

                    //Find stored config for the sensors
                    foreach (SensorConfig nextConfig in mConfig.SensorConfigs)
                    {
                        if (nextConfig.Id == nextSensor.Id)
                        {
                            nextSensor.Config = nextConfig;
                            //Has config - switched on
                            nextSensor.IsActive = true;
                            break;
                        }
                    }
                }
                else
                {
                    nextSensor.IsActive = true;
                }

                //Init the sensor with its current config
                if (nextSensor.IsActive)
                {
                    nextSensor.Init();
                }
            }

            //Collect all scanning periods of the sensors
            for (int i = 0; i < Sensors.Count; i++)
            {
                Sensor nextSensor = (Sensor)Sensors[i];
                if (nextSensor.IsActive && nextSensor.Config.ScanFrequencyInMillis > 0)
                {
                    mSensorScanTimes.AddToSorted(new SensorScanTime()
                    {
                        Sensor = (Sensor)Sensors[i],
                        ScanTimeInMillis = nextSensor.Config.ScanFrequencyInMillis
                    },
                    mSensorScanTimeComparer);
                }
            }

            //Stop all sensors
            ShutdownSensors();

            //Start the scan thread, if it makes sense
            if (mSensorScanTimes.Count > 0)
            {
                mScanThread = new Thread(new ThreadStart(ScanSensorValues));
                mDoRunScanThread = true;
                mScanThread.Start();
            }

            //Start the send thread, if it makes sense
            if (mSensors.Count > 0)
            {
                if (mSendThread == null || mSendThread.IsAlive == false)
                {
                    mSendThread = new Thread(new ThreadStart(SendSensorValuesToServer));
                    mDoRunSendThread = true;
                    mSendThread.Start();
                }
            }
        }

        private void InitDevice()
        {
            DeviceServerBase.RunningInstance.UseServerTime = mConfig.UseServerTime;
        }

        protected void SensorRaisedInterrupt(Sensor sourceSensor, SensorData sensorValue)
        {
            if (mConfig.UseServerTime) { sensorValue.GeneratedWhen = GetServerTime(sensorValue.GeneratedWhen); }

            sourceSensor.LastReadValue = sensorValue;
            if (sourceSensor.Config.KeepHistory)
            {
                sourceSensor.StoreCurrentValueToHistory();
            }

            if (sourceSensor.SensorDataRetrievalMode == SensorDataRetrievalMode.PushOnChange)
            {
                mSendQueue.Enqueue(new ScannedSensorData() { Sensor = sourceSensor, Data = sensorValue });
            }
        }

        private void ScanSensorValues()
        {
            try
            {
                DateTime scanStartTime = DateTime.Now;
                //First scan
                bool isCoalesced;
                foreach (Sensor nextSensor in Sensors)
                {
                    if (nextSensor.Config.ScanFrequencyInMillis > 0)
                    {
                        SensorData nextSensorValue = nextSensor.ScanSensorValue(out isCoalesced);
                        //Pass the value to the PUSH sender if needed
                        if (nextSensor.SensorDataRetrievalMode == Base.SensorDataRetrievalMode.Push ||
                            nextSensor.SensorDataRetrievalMode == Base.SensorDataRetrievalMode.Both)
                        {
                            mSendQueue.Enqueue(new ScannedSensorData() { Sensor = nextSensor, Data = nextSensorValue });
                        }
                    }
                }

                //Wait till the next
                Thread.Sleep(((SensorScanTime)mSensorScanTimes[0]).ScanTimeInMillis);

                //Rolling, rolling, rolling...
                while (mDoRunScanThread)
                {
                    SensorScanTime nextSensorTime = (SensorScanTime)mSensorScanTimes[0];
                    SensorData nextSensorValue = nextSensorTime.Sensor.ScanSensorValue(out isCoalesced);
                    if (isCoalesced == false)
                    {
                        //Pass the value to the PUSH sender if needed
                        if (nextSensorTime.Sensor.SensorDataRetrievalMode == Base.SensorDataRetrievalMode.Push ||
                            nextSensorTime.Sensor.SensorDataRetrievalMode == Base.SensorDataRetrievalMode.Both)
                        {
                            mSendQueue.Enqueue(new ScannedSensorData() { Sensor = nextSensorTime.Sensor, Data = nextSensorValue });
                        }
                    }

                    //Move the current sensor to its next position
                    int currentTime = ((SensorScanTime)mSensorScanTimes[0]).ScanTimeInMillis;
                    mSensorScanTimes.RemoveAt(0);
                    nextSensorTime.ScanTimeInMillis += nextSensorTime.Sensor.Config.ScanFrequencyInMillis;
                    mSensorScanTimes.AddToSorted(nextSensorTime, mSensorScanTimeComparer);

                    if (mDoRunScanThread)
                    {
                        //Sleep till the next time
                        int sleepTime = ((SensorScanTime)mSensorScanTimes[0]).ScanTimeInMillis - currentTime;
                        if (sleepTime > 0)
                        {
                            Thread.Sleep(sleepTime);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Debug.Print("ScanSensorValues Thread ABORTED.");
            }
            finally
            {
                Debug.Print("ScanSensorValues Thread EXITED.");
            }
        }

        private void SendSensorValuesToServer()
        {
            try
            {
                while (mDoRunSendThread)
                {
                    //Read the last scanned value
                    ScannedSensorData lastValue = mSendQueue.Dequeue() as ScannedSensorData;

                    //Stop if commanded to do so
                    if (mDoRunSendThread == false) 
                        break;

                    //Last value may be null, if there are several threads accessing the queue. Just a dud.
                    if (lastValue != null)
                    {
                        string sensorDataCommType = CommunicationConstants.GetCommTypeName(lastValue.Sensor.PushModeCommunicationType);
                        ServerCommunicationHandlerFactory.GetServerCommunicationHandler(sensorDataCommType).StoreSensorData(mConfig, Device, lastValue.Sensor, lastValue.Data);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Debug.Print("SendSensorValuesToServer Thread ABORTED.");
            }
            finally
            {
                Debug.Print("SendSensorValuesToServer Thread EXITED.");
            }
        }
        #endregion

        private void RefreshDeviceRegistration()
        {
            ServerCommunicationHandlerFactory.GetServerCommunicationHandler(mConfig.ServerCommType).RegisterDevice(mConfig, Device);
            ServerCommunicationHandlerFactory.GetServerCommunicationHandler(mConfig.ServerCommType).RegisterSensors(mConfig, Device, Sensors);
        }

        #region Private utility methods...

        /// <summary>
        /// The method returns IP address of the device.
        /// </summary>
        /// <returns></returns>
        protected string GetDeviceIpAddress()
        {
            //We assume that a network interface is available
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();

            return nis[0].IPAddress;
        }

        /// <summary>
        /// The method finds a contained sensor by its id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Sensor FindSensor(string id)
        {
            foreach (Sensor sensor in mSensors)
            {
                if (sensor.Id == id)
                {
                    return sensor;
                }
            }

            return null;
        }

        internal protected void LogError(Exception exc, string message, params object[] args)
        {
            string formattedMessage = StringUtility.Format(message, args);

            //To the Debug stream
            if (exc == null)
            {
                Debug.Print(formattedMessage);
            }
            else
            {
                Debug.Print(formattedMessage);
                Debug.Print(exc.ToString());

                DisplayText(formattedMessage + "\n" + exc.ToString());
            }

            //To the stored error log
            mErrorLog.Enqueue(new ErrorLogEntry() { Message = formattedMessage });
        }

        /// <summary>
        /// The method displays the passed text on the device's screen (if the text-displaying application has been set up).
        /// </summary>
        /// <param name="text"></param>
        /// <param name="args"></param>
        internal virtual void DisplayText(string text, params object[] args)
        {
            //Debug output
            string textFormatted = StringUtility.Format(text, args);
            Debug.Print(textFormatted);

            //To the display
            if (mTextDisplayApplication != null)
            {
                mTextDisplayApplication.DisplayText(textFormatted);
            }
        }

        #endregion

        #region Local classes...
        private class SensorScanTime
        {
            public Sensor Sensor { get; set; }
            public Int32 ScanTimeInMillis { get; set; }
        }

        private class ScannedSensorData
        {
            public Sensor Sensor { get; set; }
            public SensorData Data { get; set; }
        }

        /// <summary>
        /// The class implements an <see cref="IComparer"/> for <see cref="SensorScanTime"/> objects.
        /// </summary>
        public class SensorScanTimeComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return ((SensorScanTime)x).ScanTimeInMillis - ((SensorScanTime)y).ScanTimeInMillis;
            }
        }
        #endregion
    }

    /// <summary>
    /// The class encapsulates response data for a REST method call.
    /// </summary>
    [Serializable]
    public class OperationResult
    {
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyNameSuccess = "Success";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyNameTimestamp = "Timestamp";
        /// <summary>
        /// Property name.
        /// </summary>
        public const string PropertyNameErrorMessages = "ErrorMessages";

        /// <summary>
        /// The property contains the local time of the server.
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// The property contains the operation result status flag.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The property contains the operation's error messages, if any.
        /// </summary>
        public string ErrorMessages { get; set; }
    }

    /// <summary>
    /// The class encapsulates data of a single internal error log entry.
    /// </summary>
    [Serializable]
    public class ErrorLogEntry
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ErrorLogEntry()
        {
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Timestamp of the moment the entry has been logged.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        public string Message { get; set; }
    }
}

