using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using fastJSON;
using System.Collections;
using System.Net;
using System.Configuration;
using System.Threading;
using System.Globalization;
using Common.Server;
using GlobalDataContracts;
using System.Runtime.Serialization;
using GatewayServiceContract;
using System.Diagnostics;
using CentralServerService;

namespace GatewayService.LoadTest
{
    class Program
    {
        private const string ContentTypeApplicationJson = "application/json";

        #region Command line parameter names...
        public const string CmdRegisterDevice = "RegisterDevice";
        public const string CmdRegisterSensors = "RegisterSensors";
        public const string CmdPushSensorData = "PushSensorData";
        public const string CmdRetrieveSensorData = "RetrieveSensorData";
        public const string CmdRetrieveOData = "RetrieveOData";

        public const string PrmTemplateFileDevice = "-TemplateFileDevice:";
        public const string PrmTemplateFileSensors = "-TemplateFileSensors:";
        public const string PrmTemplateFileSensorData = "-TemplateFileSensorData:";
        public const string PrmServerUri = "-ServerUri:";
        public const string PrmTimeoutInMillis = "-TimeoutInMillis:";
        public const string PrmContentType = "-ContentType:";
        public const string PrmParamsDataFile = "-ParamsDataFile:";
        public const string PrmCommMode = "-CommMode:";
        public const string PrmShowResponseContent = "-ShowResponseContent:";

        public const string PrmDeviceId = "-DeviceId:";
        public const string PrmSensorId = "-SensorId:";
        public const string PrmCycleCount = "-CycleCount:";
        public const string PrmCyclePeriod = "-CyclePeriod:";
        public const string PrmNumOfParallelRequests = "-NumOfParallelRequests:";
        public const string PrmNumValuesToWrite = "-NumValuesToWrite:";
        public const string PrmIsBulkDataPut = "-IsBulkDataPut:";
        public const string PrmFromDate = "-FromDate:";
        public const string PrmUntilDate = "-UntilDate:";
        public const string PrmMaxValPerSensor = "-MaxValuesPerSensor:";
        public const string PrmODataUri = "-ODataUri:";
        public const string PrmTimingResultFile = "-TimingResultFile:";

        // simple call to just issue a round trip to the server 
        public const string PrmCallServerDummy = "-Calldummy";
        #endregion

        public const string CommModeWcfSoap = "WcfSoap";
        public const string CommModeHttpRest = "HttpRest";
        public const string CommModeRemoting = "Remoting";

        #region Properties...
        static string ServerUri { get; set; }
        static string ContentType { get; set; }
        static int TimeoutInMillis { get; set; }
        static string CommMode { get; set; }
        static GatewayServiceClient Client { get; set; }
        static bool ShowResponseContent { get; set; }

        #endregion

        static void Main(string[] args)
        {
            // make sure no old results are lingering around
            ExecutionDurationDumper.Clear();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            try
            {
                string command = args[0];

                //Load comm mode
                CommMode = GetParameterValue(args, PrmCommMode);
                ShowResponseContent = Convert.ToBoolean(GetParameterValue(args, PrmShowResponseContent, "false"));

                switch (CommMode)
                {
                    case CommModeHttpRest:
                        //Load server URI
                        ServerUri = GetParameterValue(args, PrmServerUri);

                        //Load timeout
                        TimeoutInMillis = Convert.ToInt32(GetParameterValue(args, PrmTimeoutInMillis));

                        //Load content type
                        ContentType = GetParameterValue(args, PrmContentType);
                        break;
                    case CommModeWcfSoap:
                        //WCF client
                        Client = new GatewayServiceClient("GatewayServiceWsHttpBinding");
                        break;
                    case CommModeRemoting:
                        break;
                    default:
                        Console.WriteLine("ERROR: Unknown -CommMode:<HttpRest|WcfSoap|Remoting> parameter value. Value: {0}", CommMode);
                        return;
                }

                switch (command)
                {
                    case CmdRegisterDevice:
                        RegisterDevice(args);
                        break;
                    case CmdRegisterSensors:
                        RegisterSensors(args);
                        break;
                    case CmdPushSensorData:
                        PushSensorData(args);
                        break;
                    case CmdRetrieveSensorData:
                        RetrieveSensorData(args);
                        break;
                    case CmdRetrieveOData:
                        RetrieveOData(args);
                        break;
                    default:
                        Console.WriteLine("Unknown command: {0}", command);
                        break;
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }
            watch.Stop();
            Console.WriteLine("Elasped msec = {0}", watch.ElapsedMilliseconds);


            string timingResultFile = GetParameterValue(args, PrmTimingResultFile, null);
            StreamWriter resultFile = null;

            if (timingResultFile != null)
            {
                resultFile = new StreamWriter(timingResultFile);
            }

            try
            {
                foreach (string section in ExecutionDurationDumper.Sections)
                {
                    string data = string.Format("Timing data for section: {0}", section);
                    if (resultFile != null)
                    {
                        resultFile.WriteLine(data);
                    }
                    else
                    {
                        Console.WriteLine(data);
                    }
                    foreach (long timing in ExecutionDurationDumper.GetExecutionDurationsForSection(section))
                    {
                        if (resultFile != null)
                        {
                            resultFile.WriteLine(timing.ToString());
                        }
                        else
                        {
                            Console.WriteLine(timing);
                        }
                    }

                    string sectionSep = "------------------";

                    if (resultFile != null)
                    {
                        resultFile.WriteLine(sectionSep);
                    }
                    else
                    {
                        Console.WriteLine(sectionSep);
                    }
                }
            }
            finally
            {
                if (resultFile != null)
                {
                    resultFile.Close();
                }
            }

            if (resultFile == null)
            {
                Console.WriteLine("Press RETURN");
                Console.ReadLine();
            }
        }

        private static void RegisterDevice(string[] args)
        {
            //Load device
            string devicePath = GetParameterValue(args, PrmTemplateFileDevice);

            //Register devices
            foreach (string next in LoadSourceList(devicePath, args))
            {
                try
                {
                    Device device = (Device)ToObject(next, ContentTypeApplicationJson, typeof(Device));

                    switch (CommMode)
                    {
                        case CommModeHttpRest:
                            HttpStatusCode statusCode;
                            string content, responseContentType;
                            //Refresh the device
                            //PUT /Devices/X
                            string request = String.Format(ServerUri + String.Format(ServerHttpRestUrls.SingleDeviceWithId, device.Id));
                            Console.WriteLine("REQUEST: {0}", request);

                            HttpClient.SendPutRequest(request, ContentParserFactory.GetParser(ContentType).Encode(device),
                                ContentType, TimeoutInMillis, out statusCode, out responseContentType, out content);

                            if (statusCode == HttpStatusCode.OK)
                                Console.WriteLine("RESPONSE: status: {0}; content:\n{1}", statusCode, content);
                            else
                                Console.WriteLine("!!! ----> RESPONSE ERROR: status: {0}", statusCode);
                            break;
                        case CommModeWcfSoap:
                            IsDeviceIdUsedResult resultIsUsed = Client.IsDeviceIdUsed(device.Id);
                            OperationResult result;
                            if (resultIsUsed.Success)
                            {
                                if (resultIsUsed.IsUsed)
                                {
                                    Console.WriteLine("REQUEST: UpdateDevice(DeviceId:{0})", device.Id);
                                    result = Client.UpdateDevice(device);
                                }
                                else
                                {
                                    Console.WriteLine("REQUEST: RegisterDevice(DeviceId:{0})", device.Id);
                                    result = Client.RegisterDevice(device);
                                }

                                if (result.Success)
                                    Console.WriteLine("    RESPONSE: SUCCESS");
                                else
                                    Console.WriteLine("!!! ----> RESPONSE ERROR: {0}", result.ErrorMessages);
                            }
                            else
                            {
                                Console.WriteLine("!!! ----> RESPONSE ERROR: {0}", resultIsUsed.ErrorMessages);
                            }
                            break;
                    }
                }
                catch (WebException exc)
                {
                    Console.WriteLine("!!! ---->  ERROR: Failed registering device.\n" + exc.ToString());
                }
            }
        }

        private static void RegisterSensors(string[] args)
        {
            //Device id
            string deviceId = GetParameterValue(args, PrmDeviceId);
            //Sensor template
            string sensorsPath = GetParameterValue(args, PrmTemplateFileSensors);

            //Register sensors
            List<Sensor> sensorList = new List<Sensor>();
            foreach (string next in LoadSourceList(sensorsPath, args))
            {
                sensorList.Add((Sensor)ToObject(next, ContentTypeApplicationJson, typeof(Sensor)));
            }
            try
            {
                HttpStatusCode statusCode;
                string content, responseContentType;

                Sensor[] sensors = sensorList.ToArray();

                switch (CommMode)
                {
                    case CommModeHttpRest:
                        //Refresh the sensors
                        //PUT /Devices/X/Sensors/Y
                        string request = String.Format(ServerUri + ServerHttpRestUrls.DeviceWithIdMultipleSensors, deviceId);
                        Console.WriteLine("REQUEST: {0}", request);

                        HttpClient.SendPutRequest(request, ContentParserFactory.GetParser(ContentType).Encode(sensors),
                            ContentType, TimeoutInMillis, out statusCode, out responseContentType, out content);

                        if (statusCode == HttpStatusCode.OK)
                            Console.WriteLine("RESPONSE: status: {0}; content:\n{1}", statusCode, content);
                        else
                            Console.WriteLine("!!! ----> RESPONSE ERROR: status: {0}", statusCode);
                        break;
                    case CommModeWcfSoap:
                        //Set the device id
                        sensorList.ForEach(x => x.DeviceId = deviceId);
                        List<Sensor> sensorListRegistered = new List<Sensor>();
                        List<Sensor> sensorListUnRegistered = new List<Sensor>();
                        //Get all that are registered
                        foreach (Sensor sensor in sensorList)
                        {
                            IsSensorIdRegisteredForDeviceResult resultIsRegistered = Client.IsSensorIdRegisteredForDevice(deviceId, sensor.Id);
                            if (resultIsRegistered.Success)
                            {
                                if (resultIsRegistered.IsRegistered)
                                    sensorListRegistered.Add(sensor);
                                else
                                    sensorListUnRegistered.Add(sensor);
                            }
                            else
                            {
                                Console.WriteLine("!!! ----> RESPONSE ERROR: {0}", resultIsRegistered.ErrorMessages);
                                return;
                            }
                        }

                        //Register sensors
                        OperationResult result;
                        if (sensorListUnRegistered.Count > 0)
                        {
                            Console.WriteLine("RegisterSensors(DeviceId:{0}, Sensors:(...))", deviceId);

                            result = Client.RegisterSensors(sensorListUnRegistered.ToArray());

                            if (result.Success)
                                Console.WriteLine("    RESPONSE: SUCCESS");
                            else
                                Console.WriteLine("!!! ----> RESPONSE ERROR: {0}", result.ErrorMessages);
                        }

                        //Update sensors
                        if (sensorListRegistered.Count > 0)
                        {
                            foreach (Sensor sensor in sensorListRegistered)
                            {
                                Console.WriteLine("UpdateSensor(DeviceId:{0}, SensorId:{1})", deviceId, sensor.Id);

                                result = Client.UpdateSensor(sensor);

                                if (result.Success)
                                    Console.WriteLine("    RESPONSE: SUCCESS");
                                else
                                    Console.WriteLine("!!! ----> RESPONSE ERROR: {0}", result.ErrorMessages);
                            }
                        }

                        break;
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("!!! ---->  ERROR: Failed registering sensors.\n" + exc.ToString());
            }
        }

        private static void PushSensorData(string[] args)
        {
            //Sensor data tenplate
            string sensorDataPath = GetParameterValue(args, PrmTemplateFileSensorData);

            //Device id
            string deviceId = GetParameterValue(args, PrmDeviceId);

            //Repetionions
            int cycleCount = Convert.ToInt32(GetParameterValue(args, PrmCycleCount));

            //Period
            int period = Convert.ToInt32(GetParameterValue(args, PrmCyclePeriod));

            //Bulk put?
            bool isBulkDataPut = Convert.ToBoolean(GetParameterValue(args, PrmIsBulkDataPut));

            //Num of parallel requests
            int numOfParallelRequests = Convert.ToInt32(GetParameterValue(args, PrmNumOfParallelRequests, "1"));

            // how many values to write in one go
            int numValuesToWrite = Convert.ToInt32(GetParameterValue(args, PrmNumValuesToWrite, "1"));

            List<string> sourceList = LoadSourceList(sensorDataPath, args);
            List<SensorDataWithId> dataList = new List<SensorDataWithId>();
            foreach (string next in sourceList)
            {
                dataList.Add((SensorDataWithId)ToObject(next, ContentTypeApplicationJson, typeof(SensorDataWithId)));
            }

            // Create the barrier to coordinate the threads
            Barrier threadBarrier = new Barrier(numOfParallelRequests);

           
            // Will be used several times...
            MultipleSensorData[] dataToSend = null;

            //Create multiple threads
            Action[] runThreads = new Action[numOfParallelRequests];

            for (int threadIndex = 0; threadIndex < numOfParallelRequests; threadIndex++)
                runThreads[threadIndex] = new Action(() =>
                {
                    // This will measure out activity
                    Stopwatch requestCost = new Stopwatch();

                    if (isBulkDataPut)
                    {
                        //Group by sensors
                        Dictionary<string, List<SensorDataWithId>> groupedBySensors = new Dictionary<string, List<SensorDataWithId>>();
                        foreach (SensorDataWithId next in dataList)
                        {
                            List<SensorDataWithId> list;
                            if (groupedBySensors.TryGetValue(next.SensorId, out list))
                                list.Add(next);
                            else
                            {
                                list = new List<SensorDataWithId>();
                                list.Add(next);
                                groupedBySensors[next.SensorId] = list;
                            }
                        }

                        //Create data to send
                        MultipleSensorData[] data = new MultipleSensorData[groupedBySensors.Count];
                        int index = 0;
                        foreach (string nextSensorId in groupedBySensors.Keys)
                        {
                            data[index] = new MultipleSensorData();
                            data[index].SensorId = nextSensorId;
                            data[index].Measures = groupedBySensors[nextSensorId].ToArray();
                            index++;
                        }

                        for (int i = 0; i < cycleCount; i++)
                        {
                            //Set date
                            foreach (MultipleSensorData next in data)
                                foreach (SensorData nextData in next.Measures)
                                    nextData.GeneratedWhen = DateTime.Now;

                            try
                            {
                                // Now we want to measure
                                requestCost.Restart();

                                switch (CommMode)
                                {
                                    case CommModeHttpRest:
                                        //Send to the sever
                                        string request = ServerUri +
                                                         String.Format(
                                                             ServerHttpRestUrls.DeviceWithIdSensorsSensorData, deviceId);
                                        Console.WriteLine("REQUEST: {0}", request);
                                        string content, responseContentType;
                                        HttpStatusCode statusCode;

                                        //PUT Devices/X/Sensors/SensorData
                                        HttpClient.SendPutRequest(request,
                                                                  ContentParserFactory.GetParser(ContentType)
                                                                                      .Encode(data),
                                                                  ContentType, TimeoutInMillis, out statusCode,
                                                                  out responseContentType, out content);

                                        if (statusCode == HttpStatusCode.OK)
                                            Console.WriteLine("RESPONSE: status: {0}; content:\n{1}", statusCode,
                                                              content);
                                        else
                                            Console.WriteLine("!!! ----> RESPONSE ERROR: status: {0}", statusCode);
                                        break;
                                    case CommModeWcfSoap:
                                        Console.WriteLine("REQUEST: StoreSensorData(DeviceId:{0}, Data:(...))", deviceId);

                                        OperationResult result = Client.StoreSensorData(deviceId, data);

                                        if (result.Success)
                                            Console.WriteLine("    RESPONSE: SUCCESS");
                                        else
                                            Console.WriteLine("!!! ----> RESPONSE ERROR: {0}", result.ErrorMessages);
                                        break;
                                    case CommModeRemoting:
                                         Console.WriteLine("REQUEST: StoreSensorData(DeviceId:{0}, Data:(...))", deviceId);

                                         StoreSensorDataResult remotingResult = CentralServerServiceClient.Instance.Service.StoreSensorData(deviceId, data.ToList());

                                        if (remotingResult.Success)
                                            Console.WriteLine("    RESPONSE: SUCCESS");
                                        else
                                            Console.WriteLine("!!! ----> RESPONSE ERROR: {0}", remotingResult.ErrorMessages);
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Exception during BULK Upload: ", e);
                            }
                            finally
                            {
                                requestCost.Stop();

                                Console.WriteLine("REQUEST duration = {0} microsec", requestCost.ElapsedMicroSeconds());
                                requestCost.AddExecutionDurationToList("PushSensorDataBulk_" + CommMode.ToString());
                            }
                            
                            // Wait a period
                            // Thread.Sleep(period);
                        }
                    }
                    else
                    {

                        for (int i = 0; i < cycleCount; i++)
                        {
                            //Send data
                            foreach (SensorDataWithId next in dataList)
                            {
                                //Set current date
                                next.GeneratedWhen = DateTime.Now;

                                //Prepare send thread
                                try
                                {

                                    // Now we want to measure
                                    requestCost.Restart();

                                     // Create the data to send - here we can use our internal classes
                                    dataToSend = new MultipleSensorData[]
                                        {
                                            new MultipleSensorData()
                                                {
                                                    SensorId = next.SensorId,
                                                    Measures = new SensorData[numValuesToWrite] 
                                                }
                                        };

                                    for (int valueIndex = 0; valueIndex < numValuesToWrite; valueIndex++)
                                    {
                                        dataToSend[0].Measures[valueIndex] = new SensorData(DateTime.Now,
                                                                                            valueIndex.ToString(), null);
                                    }

                                    switch (CommMode)
                                    {
                                        case CommModeHttpRest:
                                            HttpStatusCode statusCode;
                                            String content, responseContentType;
                                            string request = ServerUri +
                                                             String.Format(
                                                                 ServerHttpRestUrls.DeviceWithIdSensorWithIdSensorData,
                                                                 deviceId, next.SensorId);
                                            Console.WriteLine("REQUEST: {0}", request);
                                            //PUT Devices/X/Sensors/Y/data
                                            HttpClient.SendPutRequest(request,
                                                                      ContentParserFactory.GetParser(ContentType)
                                                                                          .Encode(dataToSend[0].Measures),
                                                                      ContentType, TimeoutInMillis, out statusCode,
                                                                      out responseContentType, out content);


                                            //Check the response
                                            if (statusCode == HttpStatusCode.OK)
                                                Console.WriteLine("RESPONSE: status: {0}; content:\n{1}", statusCode,
                                                                  content);
                                            else
                                                Console.WriteLine("!!! ----> RESPONSE ERROR: status: {0}", statusCode);
                                            break;

                                        case CommModeWcfSoap:
                                           
                                            Console.WriteLine(
                                                "REQUEST: StoreSensorData(DeviceId:{0}, SensorId:{1}, Data:(...))",
                                                deviceId, next.SensorId);

                                            OperationResult result = Client.StoreSensorData(deviceId, dataToSend);

                                            if (result.Success)
                                                Console.WriteLine("    RESPONSE: SUCCESS");
                                            else
                                                Console.WriteLine("!!! ----> RESPONSE ERROR: {0}", result.ErrorMessages);
                                            break;

                                        case CommModeRemoting:
                                            Console.WriteLine(
                                                "Remoting-REQUEST: StoreSensorData(DeviceId:{0}, SensorId:{1}, Data:(...))",
                                                deviceId, next.SensorId);

                                            StoreSensorDataResult remotingResult = CentralServerServiceClient.Instance.Service.StoreSensorData(deviceId, dataToSend[0]);
                                            if (remotingResult.Success)
                                            {
                                                Console.WriteLine("    RESPONSE: SUCCESS");
                                            }
                                            else
                                            {
                                                Console.WriteLine("!!! ----> RESPONSE ERROR: {0}", remotingResult.ErrorMessages);                                                
                                            }
                                            break;
                                    }
                                }
                                catch (Exception exc)
                                {
                                    Console.WriteLine("!!! ---->  ERROR: Failed storing sensor data.\n" + exc.ToString());
                                }
                                finally
                                {
                                    requestCost.Stop();

                                    Console.WriteLine("REQUEST duration = {0} microsec", requestCost.ElapsedMicroSeconds());
                                    requestCost.AddExecutionDurationToList("PushSensorDataSingle_" + CommMode.ToString());
                                }
                            }

                            // Wait a period
                            // Thread.Sleep(period);
                        }
                    }
                    threadBarrier.SignalAndWait();
                });


            Parallel.Invoke(runThreads);


            //Make sure that all threads had a chance to finish
            //if (isBulkDataPut == false)
            //    Thread.Sleep(TimeoutInMillis * 2);
        }

        private static void RetrieveOData(string[] args)
        {
            //OData URI
            string oDataUri = GetParameterValue(args, PrmODataUri);

            //Repetitions
            int count = Convert.ToInt32(GetParameterValue(args, PrmCycleCount));

            //Period
            int period = Convert.ToInt32(GetParameterValue(args, PrmCyclePeriod));

            //NUm of parallel requests
            int numOfParallelRequests = Convert.ToInt32(GetParameterValue(args, PrmNumOfParallelRequests, "1"));

            for (int i = 0; i < count; i++)
            {
                if (numOfParallelRequests == 1)
                {
                    RetrieveOData(oDataUri);
                }
                else
                {
                    //Create multiple threads
                    Thread[] runThreads = new Thread[numOfParallelRequests];
                    for (int j = 0; j < numOfParallelRequests; j++)
                        runThreads[j] = new Thread(new ThreadStart(() =>
                        {
                            RetrieveOData(oDataUri);
                        }));

                    //Start
                    for (int j = 0; j < numOfParallelRequests; j++)
                        runThreads[j].Start();
                }
            }
        }

        private static void RetrieveOData(string oDataUri)
        {
            try
            {
                string request = oDataUri;
                Console.WriteLine("REQUEST: {0}", request);

                HttpStatusCode statusCode;
                string responseContentType, content;
                Common.Server.HttpClient.SendGetRequest(request, ContentType, TimeoutInMillis, out statusCode, out responseContentType, out content);

                if (statusCode == HttpStatusCode.OK)
                    if (ShowResponseContent)
                        Console.WriteLine("RESPONSE: status: {0}.\nContent: {1}", statusCode, content);
                    else
                        Console.WriteLine("RESPONSE: status: {0}; Content length: {1}", statusCode, content.Length);
                else
                    Console.WriteLine("!!! ----> RESPONSE ERROR: status: {0}", statusCode);
            }
            catch (Exception exc)
            {
                Console.WriteLine("!!! ---->  ERROR: Failed retrieving OData data.\n" + exc.ToString());
            }
        }

        private static void RetrieveSensorData(string[] args)
        {
            //Device id
            string deviceId = GetParameterValue(args, PrmDeviceId);

            //Sensor id
            string sensorId = GetParameterValue(args, PrmSensorId);

            //Repetitions
            int count = Convert.ToInt32(GetParameterValue(args, PrmCycleCount));

            //Period
            int period = Convert.ToInt32(GetParameterValue(args, PrmCyclePeriod));

            //NUm of parallel requests
            int numOfParallelRequests = Convert.ToInt32(GetParameterValue(args, PrmNumOfParallelRequests, "1"));

            //From date
            //Until date
            string dateFromString = GetParameterValue(args, PrmFromDate, null);
            string dateUntilString = GetParameterValue(args, PrmUntilDate, null);
            bool isGetLatest = dateFromString == null || dateUntilString == null;
            DateTime fromDate = DateTime.Now;
            DateTime untilDate = DateTime.Now;

            //Max count of entries per sensor
            int maxCount = Convert.ToInt32(GetParameterValue(args, PrmMaxValPerSensor));

            Barrier threadBarrier = new Barrier(numOfParallelRequests,
                (b) => { Console.WriteLine("Phase: {0}", b.CurrentPhaseNumber); });

            for (int i = 0; i < count; i++)
            {

                if (isGetLatest == false)
                {
                    fromDate = Convert.ToDateTime(dateFromString);
                    untilDate = Convert.ToDateTime(GetParameterValue(args, PrmUntilDate));
                }


                if (numOfParallelRequests == 1)
                {
                    if (isGetLatest)
                    {
                        RetrieveLatestSensorData(deviceId, sensorId, maxCount);
                    }
                    else
                    {
                        RetrieveSensorData(deviceId, sensorId, fromDate, untilDate, maxCount);
                    }
                }
                else
                {
                    //Create multiple threads
                    Action[] runThreads = new Action[numOfParallelRequests];

                    for (int j = 0; j < numOfParallelRequests; j++)
                        runThreads[j] = new Action(() =>
                        {
                            if (isGetLatest)
                            {

                                RetrieveLatestSensorData(deviceId, sensorId, maxCount);
                            }
                            else
                            {
                                RetrieveSensorData(deviceId, sensorId, fromDate, untilDate, maxCount);
                            }
                            threadBarrier.SignalAndWait();
                        });



                    Parallel.Invoke(runThreads);
                }
            }
        }

        private static void RetrieveLatestSensorData(string deviceId, string sensorId, int maxCount)
        {
            Stopwatch requestCost = new Stopwatch();
            requestCost.Start();

            try
            {
                switch (CommMode)
                {
                    case CommModeHttpRest:
                        string request = ServerUri +
                                         String.Format(
                                             Common.Server.ServerHttpRestUrls.DeviceWithIdSensorsSensorDataLatest +
                                             "?id0={1}&maxValuesPerSensor={2}", deviceId, sensorId, maxCount);
                        Console.WriteLine("REQUEST: {0}", request);

                        HttpStatusCode statusCode;
                        string responseContentType, content;
                        Common.Server.HttpClient.SendGetRequest(request, ContentType, TimeoutInMillis, out statusCode,
                                                                out responseContentType, out content);

                        if (statusCode == HttpStatusCode.OK)
                        {
                            if (ShowResponseContent)
                            {
                                Console.WriteLine("RESPONSE: status: {0}.\nContent: {1}", statusCode, content);
                            }
                            else
                            {
                                Console.WriteLine("RESPONSE: status: {0}; Content length: {1}", statusCode,
                                                  content.Length);
                            }
                        }
                        else
                        {
                            Console.WriteLine("!!! ----> RESPONSE ERROR: status: {0}", statusCode);
                        }
                        break;
                    case CommModeWcfSoap:
                        Console.WriteLine("REQUEST: GetSensorDataLatest(DeviceId:{0}, SensorId:{1}, MaxCount: {2})",
                                          deviceId, sensorId, maxCount);

                        GetMultipleSensorDataResult result = Client.GetSensorDataLatest(deviceId,
                                                                                        new string[] { sensorId },
                                                                                        maxCount);

                        if (result.Success)
                            if (ShowResponseContent)
                            {
                                Console.WriteLine("    RESPONSE: SUCCESS. Retrieved {1} records for {0} sensor(s).",
                                                  result.SensorDataList.Count,
                                                  result.SensorDataList.Sum(x => x.Measures.Length));
                            }
                            else
                            {
                                Console.WriteLine("    RESPONSE: SUCCESS. Retrieved data for {0} sensors",
                                                  result.SensorDataList.Count);
                            }
                        else
                        {
                            Console.WriteLine("!!! ----> RESPONSE ERROR: {0}", result.ErrorMessages);
                        }

                        break;
                    case CommModeRemoting:
                        List<string> sensorIdList = new List<string>();
                        sensorIdList.Add(sensorId);
                        GetMultipleSensorDataResult remotingResult = CentralServerServiceClient.Instance.Service.GetSensorDataLatest(deviceId, sensorIdList, maxCount);
                        if (remotingResult.Success)
                        {
                            if (ShowResponseContent)
                            {
                                Console.WriteLine("    RESPONSE: SUCCESS. Retrieved {1} records for {0} sensor(s).",
                                                  remotingResult.SensorDataList.Count,
                                                  remotingResult.SensorDataList.Sum(x => x.Measures.Length));
                            }
                            else
                            {
                                Console.WriteLine("    RESPONSE: SUCCESS. Retrieved data for {0} sensors",
                                                  remotingResult.SensorDataList.Count);
                            }
                        }
                        else
                        {
                            Console.WriteLine("!!! ----> RESPONSE ERROR: {0}", remotingResult.ErrorMessages);
                        }

                        break;
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("!!! ---->  ERROR: Failed retrieving sensor data.\n" + exc.ToString());
            }
            finally
            {
                requestCost.Stop();

                Console.WriteLine("REQUEST duration = {0} microsec", requestCost.ElapsedMicroSeconds());
                requestCost.AddExecutionDurationToList("RetrieveLatestSensorData");
            }
            Console.WriteLine();
        }

        private static void RetrieveSensorData(string deviceId, string sensorId, DateTime fromDate, DateTime untilDate, int maxCount)
        {
            try
            {
                switch (CommMode)
                {
                    case CommModeHttpRest:
                        string request = ServerUri + String.Format(Common.Server.ServerHttpRestUrls.DeviceWithIdSensorsSensorData + "?id0={1}&generatedAfter={2}&generatedBefore={3}&maxValuesPerSensor={4}",
                            deviceId, sensorId, fromDate.ToString(CultureInfo.InvariantCulture), untilDate.ToString(CultureInfo.InvariantCulture), maxCount);
                        Console.WriteLine("REQUEST: {0}", request);
                        HttpStatusCode statusCode;
                        string responseContentType, content;
                        Common.Server.HttpClient.SendGetRequest(request, ContentType, TimeoutInMillis, out statusCode, out responseContentType, out content);

                        if (statusCode == HttpStatusCode.OK)
                            if (ShowResponseContent)
                                Console.WriteLine("RESPONSE: status: {0}.\nContent: {1}", statusCode, content);
                            else
                                Console.WriteLine("RESPONSE: status: {0}; Content length: {1}", statusCode, content.Length);
                        else
                            Console.WriteLine("!!! ----> RESPONSE ERROR: status: {0}", statusCode);
                        break;
                    case CommModeWcfSoap:
                        Console.WriteLine("REQUEST: GetSensorDataLatest(DeviceId:{0}, SensorId:{1}, FromDate:{2}, UntilDate:{3}, MaxCount: {4})", deviceId, sensorId, fromDate, untilDate, maxCount);

                        GetMultipleSensorDataResult result = Client.GetSensorData(deviceId, new string[] { sensorId }, fromDate, untilDate, maxCount);

                        if (result.Success)
                            Console.WriteLine("    RESPONSE: SUCCESS. Retrieved {1} records for {0} sensor(s).", result.SensorDataList.Count, result.SensorDataList.Sum(x => x.Measures.Length));
                        else
                            Console.WriteLine("!!! ----> RESPONSE ERROR: {0}", result.ErrorMessages);
                        break;
                }

                Console.WriteLine();

            }
            catch (Exception exc)
            {
                Console.WriteLine("!!! ---->  ERROR: Failed retrieving sensor data.\n" + exc.ToString());
            }
        }

        #region Utility methods...
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
            }
        }

        private static string GetParameterValue(string[] args, string paramName, bool deliverNullIfNotPresent = false)
        {
            string paramValue = args.FirstOrDefault(x => x.StartsWith(paramName));
            if (paramValue == null)
            {
                //Try app.config
                paramValue = ConfigurationManager.AppSettings[paramName];
                if (paramValue == null)
                {
                    if (deliverNullIfNotPresent == false)
                    {
                        throw new ArgumentNullException(String.Format("ERROR: No \"{0}\" parameter specified.", paramName));
                    }
                    else
                    {
                        return (null);
                    }
                }

                return paramValue;
            }
            else
            {
                return paramValue.Substring(paramName.Length);
            }
        }

        private static string GetParameterValue(string[] args, string paramName, string defaultValue)
        {
            string result = GetParameterValue(args, paramName, true);
            result = result ?? defaultValue;
            return (result);
        }

        private static List<string> LoadSourceList(string templatePath, string[] args)
        {
            //Source data
            string source = LoadTextFileContents(templatePath);

            //Params file name
            string paramsFileName = GetParameterValue(args, PrmParamsDataFile, null);

            //Multiple objects or just one?
            List<string> sourceData = new List<string>();
            if (paramsFileName == null)
            {
                sourceData.Add(source);
            }
            else
            {
                List<string[]> paramsData = LoadDataArray(paramsFileName);
                if (paramsData.Count == 0)
                    sourceData.Add(source);
                else
                    foreach (object[] next in paramsData)
                        sourceData.Add(String.Format(source, next));
            }

            return sourceData;
        }

        private static string LoadTextFileContents(string path)
        {
            string source = null;
            if (File.Exists(path))
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (TextReader reader = new StreamReader(stream))
                    {
                        source = reader.ReadToEnd();
                    }
                }
            }
            else
            {
                throw new Exception(String.Format("Path is not found: \"{0}\"", path));
            }

            if (source == null)
            {
                throw new Exception(String.Format("The file \"{0}\" has no contents", path));
            }
            return source;
        }

        private static object ToObject(string source, string contentType, Type objectType)
        {
            switch (contentType)
            {
                case ContentTypeApplicationJson:
                    object result = JSON.Instance.ToObject(source, objectType);
                    if (result == null)
                    {
                        throw new Exception("The passed string is not JSON-parseable.");
                    }
                    return result;
                default:
                    throw new Exception(String.Format("Unknown content type: {0}", contentType));
            }
        }

        private static List<string[]> LoadDataArray(string path)
        {
            List<string[]> result = new List<string[]>();
            if (File.Exists(path))
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (TextReader reader = new StreamReader(stream))
                    {
                        string nextLine;
                        while ((nextLine = reader.ReadLine()) != null)
                        {
                            nextLine = nextLine.Trim();
                            if (nextLine.Length == 0 || nextLine.StartsWith("#"))
                                continue;

                            result.Add(nextLine.Split(','));
                        }
                    }
                }
            }
            else
            {
                throw new Exception(String.Format("Path is not found: \"{0}\"", path));
            }

            return result;
        }
        #endregion

    }

    /// <summary>
    /// Class for coupling sensor data with its sensor local id.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://GatewaysService", Name = "SensorData")]
    public class SensorDataWithId : SensorData
    {
        /// <summary>
        /// Local device-related sensor id
        /// </summary>
        public string SensorId { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SensorDataWithId()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorDataForDevice"/> class.
        /// </summary>
        /// <param name="sensorId">The sensor id.</param>
        /// <param name="generatedWhen">The generated when.</param>
        /// <param name="value">The value.</param>
        public SensorDataWithId(string sensorId, DateTime generatedWhen, string value)
            : base(generatedWhen, value, null)
        {
            SensorId = sensorId;
        }
    }
}
