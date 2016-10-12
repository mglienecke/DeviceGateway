using System;
using System.Collections;
//using System.Net;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Reflection;

using System.IO;
using Common.Server;
using fastJSON;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Configuration;

using DeviceServer.Simulator;

namespace DeviceServer.Simulator
{
    /// <summary>
    /// Class simulates a device server deployed on a read NET MF device..
    /// </summary>
    public class DeviceServerSimulator
    {
        internal static readonly DateTime TimeZero = new DateTime(1700, 1, 1);

        private static DeviceConfig mConfig;
        private static DeviceServerSimulator mInstance;
        private readonly LimitedQueue mErrorLog = new LimitedQueue();
        private static Thread mServerThread;

        private readonly ArrayList mSensors = new ArrayList();
        private readonly SensorScanTimeComparer mSensorScanTimeComparer = new SensorScanTimeComparer();
        private readonly ArrayList mSensorScanTimes = new ArrayList();

        private readonly BlockingQueue mSendQueue = new BlockingQueue(50);
        private bool mStopSendingData;

        private Thread mScanThread;
        private Thread mSendThread;

        #region Constants...
        /// <summary>
        /// Shall the configuration be used for a property
        /// </summary>
        public const string UseConfigurationString = "UseConfiguration";

        /// <summary>
        /// the default IP address config string
        /// </summary>
        public const string DefaultDeviceIpAddressConfig = "DefaultDeviceIpAddress";

        /// <summary>
        /// the default device id config string
        /// </summary>
        public const string DefaultDeviceIdConfig = "DefaultDeviceId";

        /// <summary>
        /// The macro which will replace localhost
        /// </summary>
        public const string LocalHostMacro = "@localhost";

        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string DeviceJsonFilePathConfig = "DeviceJsonFilePath";
        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string SensorsJsonFilePathConfig = "SensorsJsonFilePath";
        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string DeviceConfigJsonFilePathConfig = "DeviceConfigJsonFilePath";

        /// <summary>
        /// Default property value.
        /// </summary>
        public const string DefaultDeviceJsonFilePath = "Device.json";
        /// <summary>
        /// Default property value.
        /// </summary>
        public const string DefaultSensorsJsonFilePath = "Sensors.json";
        /// <summary>
        /// Default property value.
        /// </summary>
        public const string DefaultDeviceConfigJsonFilePath = "ConfigData.json";

        /// <summary>
        /// Command line application start parameter.
        /// </summary>
        public const string CmdParamDeviceId = "-DeviceId:";
        /// <summary>
        /// Command line application start parameter.
        /// </summary>
        public const string CmdParamDeviceIpEndpoint = "-DeviceIpEndpoint:";
        /// <summary>
        /// Command line application start parameter.
        /// </summary>
        public const string CmdParamDeviceJsonFile = "-DeviceJsonFile:";
        /// <summary>
        /// Command line application start parameter.
        /// </summary>
        public const string CmdParamSensorsJsonFile = "-SensorsJsonFile:";
        /// <summary>
        /// Command line application start parameter.
        /// </summary>
        public const string CmdParamConfigJsonFile = "-ConfigJsonFile:";
        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DeviceServerSimulator()
        {
            DeviceJsonFilePath = DefaultDeviceJsonFilePath;
            SensorsJsonFilePath = DefaultSensorsJsonFilePath;
            DeviceConfigJsonFilePath = DefaultDeviceConfigJsonFilePath;

            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[DeviceJsonFilePath]) == false)
                DeviceJsonFilePath = ConfigurationManager.AppSettings[DeviceJsonFilePath];

            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[SensorsJsonFilePath]) == false)
                SensorsJsonFilePath = ConfigurationManager.AppSettings[SensorsJsonFilePath];

            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[DeviceConfigJsonFilePath]) == false)
                DeviceConfigJsonFilePath = ConfigurationManager.AppSettings[DeviceConfigJsonFilePath];
        }

        /// <summary>
        /// The start program method.
        /// </summary>
        public static void Main(string[] args)
        {
            try
            {
                var deviceId = args.FirstOrDefault(x => x.StartsWith(CmdParamDeviceId));
                if (deviceId != null)
                    deviceId = deviceId.Substring(CmdParamDeviceId.Length);
                var deviceIpEndpoint = args.FirstOrDefault(x => x.StartsWith(CmdParamDeviceIpEndpoint));
                if (deviceIpEndpoint != null)
                    deviceIpEndpoint = deviceIpEndpoint.Substring(CmdParamDeviceIpEndpoint.Length);
                var configJsonFile = args.FirstOrDefault(x => x.StartsWith(CmdParamConfigJsonFile));
                if (configJsonFile != null)
                    configJsonFile = configJsonFile.Substring(CmdParamConfigJsonFile.Length);
                else
                {
                    configJsonFile = "ConfigData.json";
                }
                var deviceJsonFile = args.FirstOrDefault(x => x.StartsWith(CmdParamDeviceJsonFile));
                if (deviceJsonFile != null)
                    deviceJsonFile = deviceJsonFile.Substring(CmdParamDeviceJsonFile.Length);
                else
                {
                    deviceJsonFile = "Device.json";
                }
                var sensorsJsonFile = args.FirstOrDefault(x => x.StartsWith(CmdParamSensorsJsonFile));
                if (sensorsJsonFile != null)
                    sensorsJsonFile = sensorsJsonFile.Substring(CmdParamSensorsJsonFile.Length);
                else
                {
                    sensorsJsonFile = "Sensors.json";
                }
                var runThread = new Thread(() =>
                    {
                        DeviceServerSimulator.Run(new DeviceServerSimulator()
                            {
                                DeviceId = deviceId,
                                DeviceIpEndpoint = deviceIpEndpoint,
                                DeviceConfigJsonFilePath = configJsonFile,
                                SensorsJsonFilePath = sensorsJsonFile,
                                DeviceJsonFilePath = deviceJsonFile
                            });
                    });
                runThread.IsBackground = true;
                runThread.Start();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
            Environment.Exit(0);
        }

        #region Properties...
        /// <summary>
        /// The property contains id for the device emulated by this DeviceServerSimulator instance. This value (if present) overrides the values 
        /// from the application config file or the Device JSON file.
        /// </summary>
        private string DeviceId { get; set; }

        /// <summary>
        /// The property contains IP endpoint (an IP address plus a port number) for the device emulated by this DeviceServerSimulator instance. This value (if present) overrides the values 
        /// from the application config file or the Device JSON file.
        /// </summary>
        private string DeviceIpEndpoint { get; set; }

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
                return Properties.Resources.CfgGatewayServiceUrl;
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

        /// <summary>
        /// The property contains path to the JSON file that contains the device description to be loaded.
        /// </summary>
        public string DeviceJsonFilePath { get; set; }

        /// <summary>
        /// The property contains path to the JSON file that contains the sensors description to be loaded.
        /// </summary>
        public string SensorsJsonFilePath { get; set; }

        /// <summary>
        /// The property contains path to the JSON file that contains the device configuration to be loaded.
        /// </summary>
        public string DeviceConfigJsonFilePath { get; set; }

        /// <summary>
        /// The property contains the JSON representation of the loaded device. The value can be loaded automatically from the <see cref="DeviceJsonFilePath"/>
        /// or set programmatically.
        /// </summary>
        public string JsonDevice { get; set; }

        /// <summary>
        /// The property contains the JSON representation of the loaded sensors. The value can be loaded automatically from the <see cref="SensorsJsonFilePath"/>
        /// or set programmatically.
        /// </summary>
        public string JsonSensors { get; set; }

        /// <summary>
        /// The property contains the JSON representation of the loaded device configuration. The value can be loaded automatically from the <see cref="DeviceConfigJsonFilePath"/>
        /// or set programmatically.
        /// </summary>
        public string JsonDeviceConfig { get; set; }
        #endregion

        #region Methods for specific subclass implementations...

        /// <summary>
        /// The method initializes sensors for a specific implementation.
        /// </summary>
        protected void LoadSensors()
        {
            //The sensors data are not set, load from the file?
            if (String.IsNullOrEmpty(JsonSensors))
            {
                if (File.Exists(SensorsJsonFilePath))
                {
                    using (FileStream stream = new FileStream(SensorsJsonFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (TextReader reader = new StreamReader(stream))
                        {
                            JsonSensors = reader.ReadToEnd();
                        }
                    }
                }
                else
                {
                    throw new Exception(String.Format("Path is not found: \"{0}\"", SensorsJsonFilePath));
                }

                if (JsonSensors == null)
                {
                    throw new Exception(String.Format("The file \"{0}\" has no contents", SensorsJsonFilePath));
                }
            }

            Sensor[] loadedSensors = (Sensor[]) JSON.Instance.ToObject(JsonSensors, typeof (Sensor[]));
            if (loadedSensors == null)
            {
                throw new Exception("The property JsonSensors contains no JSON-parseable contents");
            }

            foreach (Sensor nextSensor in loadedSensors)
            {
                if (nextSensor.HardwareSensorSimulatorTypeName == null)
                    throw new Exception("Sensor.HardwareSensorSimulatorTypeName is not set.");

                if (nextSensor.DeviceId == UseConfigurationString)
                {
                    nextSensor.DeviceId = Device.Id;
                }

                Type hssType = Type.GetType(nextSensor.HardwareSensorSimulatorTypeName);
                if (hssType == null)
                    throw new Exception("Type for Sensor.HardwareSensorSimulatorTypeName is not found. Type name: " +
                                        nextSensor.HardwareSensorSimulatorTypeName);

                IHardwareSensorSimulator hsSensor = (IHardwareSensorSimulator) Activator.CreateInstance(hssType);
                hsSensor.DeviceId = Device.Id;
                hsSensor.SensorId = nextSensor.Id;
                if (hsSensor.GetValueEnabled)
                    nextSensor.ValueGetter = hsSensor.GetValue;
                if (hsSensor.SetValueEnabled)
                    nextSensor.ValueSetter = hsSensor.SetValue;

                Sensors.Add(nextSensor);
            }
        }

        /// <summary>
        /// The method initializes device for a specific implementation.
        /// </summary>
        protected void LoadDevice()
        {
            //The device data are not set, load from the file?
            if (String.IsNullOrEmpty(JsonDevice))
            {
                if (File.Exists(DeviceJsonFilePath))
                {
                    using (FileStream stream = new FileStream(DeviceJsonFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (TextReader reader = new StreamReader(stream))
                        {
                            JsonDevice = reader.ReadToEnd();
                        }
                    }
                }
                else
                {
                    throw new Exception(String.Format("Path is not found: \"{0}\"", DeviceJsonFilePath));
                }

                if (JsonDevice == null)
                {
                    throw new Exception(String.Format("The file \"{0}\" has no contents", DeviceJsonFilePath));
                }
            }

            Device = (Device) JSON.Instance.ToObject(JsonDevice, typeof (Device));
            if (Device == null)
            {
                throw new Exception("The property JsonDevice contains no JSON-parseable contents");
            }

            //Device id?
            if (String.IsNullOrEmpty(DeviceId) == false)
            {
                Device.Id = DeviceId;
            }
            else if (Device.Id == UseConfigurationString)
            {
                Device.Id = ConfigurationManager.AppSettings[DefaultDeviceIdConfig];
            }

            //Device IP endpoint?
            if (String.IsNullOrEmpty(DeviceIpEndpoint) == false)
            {
                Device.DeviceIpEndPoint = DeviceIpEndpoint;
            }
            else if (Device.DeviceIpEndPoint == UseConfigurationString)
            {
                string defaultDeviceIp = ConfigurationManager.AppSettings[DefaultDeviceIpAddressConfig];
                if (defaultDeviceIp.Contains(LocalHostMacro))
                {
                    // find the real local IP address of the machine
                    string localIp = NetworkUtilities.GetIpV4AddressForDns(Dns.GetHostName()).ToString();

                    // replace the macro with the real IP address (as this has to be called from the server) and append the port
                    defaultDeviceIp = defaultDeviceIp.IndexOf(':') >= 0
                                          ? localIp + ":" + defaultDeviceIp.Split(':')[1]
                                          : localIp;
                }

                Device.DeviceIpEndPoint = defaultDeviceIp;
            }
        }

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
        /// THe property contains reference to the actual implementation instance that has been launched the .
        /// </summary>
        internal protected static DeviceServerSimulator RunningInstance
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
            return localTime.AddTicks(DeviceServerSimulator.RunningInstance.ServerTimeOffset);
        }

        /// <summary>
        /// The method initializes and starts the passed device server instance.
        /// </summary>
        /// <param name="server"></param>
        protected static void Run(DeviceServerSimulator server)
        {
            mInstance = server;
            mInstance.Init();

            mServerThread = new Thread(mInstance.Start);
            mServerThread.Start();
        }

        /// <summary>
        /// The method starts the text-displaying WPF application.
        /// </summary>
        /// <param name="startText"></param>
        protected static void StartTextDisplayApplication(string startText)
        {
            //mTextDisplayApplication = new TextDisplayApplication(startText);
            //mTextDisplayApplication.StartApplication();
        }

        private void Init()
        {
            //Load device
            LoadDevice();

            //Load sensor data
            LoadSensors();

            //Load config data
            LoadConfigData();

            //Refresh device registration if needed
            if (mConfig.IsRefreshDeviceRegOnStartup)
            {
                RefreshDeviceRegistration();
            }

            //Init the sensor data
            InitSensors();

            if (ServerCommunicationHandlerFactory.GetServerCommunicationHandler(this, mConfig.ServerCommType).Init())
            {
                DisplayText("Device server initialized.");
            }
            else
            {
                DisplayText("Device server initialized failed.");
            }
        }

        private void Start()
        {
            if (ServerCommunicationHandlerFactory.GetServerCommunicationHandler(this, mConfig.ServerCommType).StartRequestHandling())
            {
                DisplayText("Device server started.");
            }
            else
            {
                DisplayText("Device server failed to start.");
            }
        }

        private void Stop()
        {
            mStopSendingData = true;
        }

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

                //Init
                InitDevice();
                InitSensors();

                //Store the config - seems valid
                StoreConfigData();
            }
            catch (Exception exc)
            {
                LogError(exc, "Failed applying new config.");
            }
        }

        #region Config data methods...
        private void LoadConfigData()
        {
            Dictionary<string, object> mGlobalTypes = new Dictionary<string, object>();

            //The config data are not set, load from the file?
            if (String.IsNullOrEmpty(JsonDeviceConfig))
            {
                if (File.Exists(DeviceConfigJsonFilePath))
                {
                    using (FileStream stream = new FileStream(DeviceConfigJsonFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (TextReader reader = new StreamReader(stream))
                        {
                            JsonDeviceConfig = reader.ReadToEnd();
                        }
                    }
                }

                if (JsonDeviceConfig == null)
                {
                    mConfig = GetDefaultDeviceConfig();
                }
            }

            if (mConfig == null)
            {
                Dictionary<string, object> globalTypes = new Dictionary<string, object>();
                globalTypes["SensorConfig"] = typeof (SensorConfig).AssemblyQualifiedName;
                globalTypes["DeviceConfig"] = typeof(DeviceConfig).AssemblyQualifiedName;
                globalTypes["2"] = typeof(SensorConfig).AssemblyQualifiedName;
                globalTypes["1"] = typeof(DeviceConfig).AssemblyQualifiedName;
                mConfig = (DeviceConfig) JSON.Instance.ToObject(JsonDeviceConfig, globalTypes, typeof (DeviceConfig));
            }

            InitDevice();

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
                        if (nextConfig.ServerUrl.Contains(LocalHostMacro))
                        {
                            // find the real local IP address of the machine
                            string localIp = NetworkUtilities.GetIpV4AddressForDns(Dns.GetHostName()).ToString();

                            // replace the macro with the real IP address (as this has to be called from the server) and append the port
                            nextConfig.ServerUrl = nextConfig.ServerUrl.Replace(LocalHostMacro, localIp);

                        }
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
        }

        protected void StoreConfigData()
        {
            //Create directory if needed
            //string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.PathSeparator + "DeviceServerSimulator";
            string path = DeviceConfigJsonFilePath;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            //Create/open the config file and write there
            path += Path.PathSeparator + DeviceConfigJsonFilePath;
            
            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
            {
                using (TextWriter writer = new StreamWriter(stream))
                {
                    writer.Write(JSON.Instance.ToJSON(mConfig));
                }
            }
        }
        #endregion

        #region Sensor data scanning and pushing to the server...
        private void ShutdownSensors()
        {
            //Sort them
            mSensorScanTimes.Sort(mSensorScanTimeComparer);

            //Abort the current running scanning thread if needed
            if (mScanThread != null && mScanThread.IsAlive)
            {
                try
                {
                    mScanThread.Abort();
                }
                catch { }
            }

            //Abort the currently running send thread
            if (mSendThread != null && mSendThread.IsAlive)
            {
                try
                {
                    mStopSendingData = true;
                    mSendThread.Abort();
                }
                catch { }
            }
        }

        private void InitSensors()
        {
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
                mScanThread.Start();
            }

            //Start the send thread, if it makes sense
            if (mSensors.Count > 0)
            {
                mSendThread = new Thread(new ThreadStart(SendSensorValuesToServer));
                mStopSendingData = false;
                mSendThread.Start();
            }
        }

        private void InitDevice()
        {
            DeviceServerSimulator.RunningInstance.UseServerTime = mConfig.UseServerTime;
            //Pass the port number to the communication handler
            if (String.IsNullOrEmpty(DeviceIpEndpoint) == false)
            {
                var pos = DeviceIpEndpoint.IndexOf(':');
                if (pos > 0 && pos < DeviceIpEndpoint.Length - 1)
                    ServerCommunicationHandlerFactory.GetServerCommunicationHandler(this, mConfig.ServerCommType).LocalPort =
                        ushort.Parse(DeviceIpEndpoint.Substring(pos + 1));
            }
        }

        protected void SensorRaisedInterrupt(Sensor sourceSensor, SensorData sensorValue)
        {
            if (mConfig.UseServerTime) { sensorValue.GeneratedWhen = GetServerTime(sensorValue.GeneratedWhen); }

            sourceSensor.LastReadValue = sensorValue;
            if (sourceSensor.Config.KeepHistory)
            {
                sourceSensor.StoreCurrentValueToHistory();
            }

            if (sourceSensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.PushOnChange))
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
                    if (nextSensor.IsActive && nextSensor.Config.ScanFrequencyInMillis > 0)
                    {
                        SensorData nextSensorValue = nextSensor.ScanSensorValue(out isCoalesced);
                        //Pass the value to the PUSH sender if needed
                        if (nextSensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.Push) ||
                            nextSensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.Both))
                        {
                            mSendQueue.Enqueue(new ScannedSensorData() { Sensor = nextSensor, Data = nextSensorValue });
                        }
                    }
                }

                //Wait till the next
                Thread.Sleep(((SensorScanTime)mSensorScanTimes[0]).ScanTimeInMillis);

                //Rolling, rolling, rolling...
                while (true)
                {
                    SensorScanTime nextSensorTime = (SensorScanTime)mSensorScanTimes[0];
                    SensorData nextSensorValue = nextSensorTime.Sensor.ScanSensorValue(out isCoalesced);
                    if (isCoalesced == false)
                    {
                        //Pass the value to the PUSH sender if needed
                        if (nextSensorTime.Sensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.Push) ||
                            nextSensorTime.Sensor.SensorDataRetrievalMode.HasFlag(SensorDataRetrievalMode.Both))
                        {
                            mSendQueue.Enqueue(new ScannedSensorData() { Sensor = nextSensorTime.Sensor, Data = nextSensorValue });
                        }
                    }

                    //Move the current sensor to its next position
                    int currentTime = ((SensorScanTime)mSensorScanTimes[0]).ScanTimeInMillis;
                    mSensorScanTimes.RemoveAt(0);
                    nextSensorTime.ScanTimeInMillis += nextSensorTime.Sensor.Config.ScanFrequencyInMillis;
                    mSensorScanTimes.AddToSorted(nextSensorTime, mSensorScanTimeComparer);


                    //Sleep till the next time
                    int sleepTime = ((SensorScanTime)mSensorScanTimes[0]).ScanTimeInMillis - currentTime;
                    if (sleepTime > 0)
                    {
                        Thread.Sleep(sleepTime);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                //Ignore
            }
        }

        private void SendSensorValuesToServer()
        {
            try
            {
                while (true)
                {
                    //Read the last scanned value
                    ScannedSensorData lastValue = mSendQueue.Dequeue() as ScannedSensorData;

                    //Stop if commanded to do so
                    if (mStopSendingData == true) return;

                    //Last value may be null, if there are several threads accessing the queue. Just a dud.
                    if (lastValue != null)
                    {
                        ServerCommunicationHandlerFactory.GetServerCommunicationHandler(this, mConfig.ServerCommType).StoreSensorData(mConfig, Device, lastValue.Sensor, lastValue.Data);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                //Ignore
            }
        }
        #endregion

        private void RefreshDeviceRegistration()
        {
            if (mConfig.ServerUrl != null && mConfig.ServerUrl.Length > 0)
            {
                //Update the device's IP with the current device port number
                Device.DeviceIpEndPoint = ComplementDeviceIpPointWithPortNumber(Device.DeviceIpEndPoint);
                ServerCommunicationHandlerFactory.GetServerCommunicationHandler(this, mConfig.ServerCommType).RegisterDevice(mConfig, Device);
                ServerCommunicationHandlerFactory.GetServerCommunicationHandler(this, mConfig.ServerCommType).RegisterSensors(mConfig, Device, Sensors);
            }
            else
            {
                LogError(null, Properties.Resources.ErrorUnableRefreshDeviceRegistrationNoServerUrl);
            }
        }

        private string ComplementDeviceIpPointWithPortNumber(string ipAddress)
        {
            if (ipAddress.IndexOf(':') >= 0)
                //Already there
                return ipAddress;

            return ipAddress + ':' + ServerCommunicationHandlerFactory.GetServerCommunicationHandler(this, mConfig.ServerCommType).LocalPort;
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

            return nis[0].GetIPProperties().UnicastAddresses[0].Address.ToString();
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
            string formattedMessage = String.Format(message, args);

            //To the Debug stream
            if (exc == null)
            {
                Console.WriteLine(formattedMessage);
            }
            else
            {
                Console.WriteLine(formattedMessage);
                Console.WriteLine(exc.ToString());

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
            //if (mTextDisplayApplication != null)
            //{
            //    mTextDisplayApplication.DisplayText(text, args);
            //}
            Console.WriteLine(text, args);
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

/// <summary>
    /// Extension methods for the ArrayList class.
    /// </summary>
    public static class ArrayListExtensions
    {

        /// <summary>
        /// The method inserts the passed object to the array, which is presumed to be sorted in the ascending order, preserving the sorting order.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <param name="comparer"></param>
        public static void AddToSorted(this ArrayList array, object value, IComparer comparer)
        {
            int count = array.Count;
            for (int i = 0; i < count; i++)
            {
                if (comparer.Compare(array[i], value) > 0)
                {
                    array.Insert(i, value);
                    return;
                }
            }

            //So the new value is the biggest one
            array.Add(value);
        }
    }

    /// <summary>
    /// The class implements an <see cref="IComparer"/> for Int32 objects.
    /// </summary>
    public class Int32Comparer : IComparer
    {
        public int Compare(object x, object y)
        {
            return (Int32)x - (Int32)y;
        }
    }
}

