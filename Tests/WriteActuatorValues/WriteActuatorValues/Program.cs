using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using CentralServerService;
using GlobalDataContracts;
using Common.Server.CNDEP;
using Common.Server;
using Newtonsoft.Json;
using System.Configuration;

namespace WriteActuatorValues
{
    class Program
    {
        private const string MimeTypeJson = "application/json";
        private static int CorrelationId { get; set; }

        private static CommunicationProtocol CndepPrototocol { get; set; }
        private static string CndepContentType { get; set; }
        private static string CndepServerAddress { get; set; }
        private static int CndepRequestTimeout { get; set; }
        private static int CndepRequestRetryCount { get; set; }
        private static int CndepServerPort { get; set; }

        private static bool UseSingleValuePush { get; set; }
        private static int NumberOfIterations { get; set; }
        private static int NumberOfActuatorsToWrite { get; set; }
        private static int DurationBetweenWritesInMsec { get; set; }

        static void Main(string[] args)
        {
            CorrelationId = 1;
            CndepServerAddress = ConfigurationManager.AppSettings["CNDEPServer"];
            CndepServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["CNDEPPort"]);
            CndepPrototocol = (CommunicationProtocol)Enum.Parse(typeof(CommunicationProtocol), ConfigurationManager.AppSettings["CNDEPProtocol"]);
            NumberOfIterations = Convert.ToInt32(ConfigurationManager.AppSettings["NumberOfIterations"]);
            NumberOfActuatorsToWrite = Convert.ToInt32(ConfigurationManager.AppSettings["NumberOfActuatorsToWrite"]);
            DurationBetweenWritesInMsec = Convert.ToInt32(ConfigurationManager.AppSettings["DurationBetweenWritesInMsec"]);
            UseSingleValuePush = Convert.ToBoolean(ConfigurationManager.AppSettings["UseSingleValuePush"]);

            CndepContentType = MimeTypeJson;
            CndepRequestRetryCount = 1;
            CndepRequestTimeout = 5000;

            for (int iteration = 0; iteration < NumberOfIterations; iteration++)
            {
                Console.WriteLine("Iteration {0} of {1}", iteration, NumberOfIterations);

                Random random = new Random();

                //Store
                object responseObject;
                OperationResult resultTemplate = new OperationResult();

                StoreSensorDataRequest request = new StoreSensorDataRequest();


                request.DeviceId = ConfigurationManager.AppSettings["DeviceId"];

                if (UseSingleValuePush)
                {
                    // only one request at a time
                    request.Data = new MultipleSensorData[1];

                    for (int numActuators = 0; numActuators < NumberOfActuatorsToWrite; numActuators++)
                    {
                        request.Data[0] = new MultipleSensorData()
                            {
                                SensorId = ConfigurationManager.AppSettings["ActuatorBaseName"] + numActuators,
                                Measures = new SensorData[1]
                                    {
                                        new SensorData() {GeneratedWhen = DateTime.Now, CorrelationId = CorrelationId++.ToString(), Value = random.Next(1, 100).ToString()}
                                    }
                            };

                        TrackingPoint.TrackingPoint.CreateTrackingPoint
                            (
                                "WriteActuators: " + "SendSingleActuatorToServer_CNDEP",
                                "Before sending " + "Single correlation id: " + CndepPrototocol,
                                NumberOfActuatorsToWrite,
                                string.Format("{0} - {0}", CorrelationId - 1));

                        Debug.Assert(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
                            CndepContentType, resultTemplate, out responseObject), responseObject.ToString());

                        TrackingPoint.TrackingPoint.CreateTrackingPoint
                            (
                                "WriteActuators: " + "SendSingleActuatorToServer_CNDEP",
                                "After sending " + "Single correlation id: " + CndepPrototocol,
                                NumberOfActuatorsToWrite,
                                string.Format("{0} - {0}", CorrelationId - 1));

                        Debug.Assert(responseObject != null);
                        Debug.Assert(responseObject is OperationResult);
                        Debug.Assert(((OperationResult)responseObject).Success, ((OperationResult)responseObject).ErrorMessages);
                    }
                }
                else
                {
                    // Push multiple values at a time
                    request.Data = new MultipleSensorData[NumberOfActuatorsToWrite];

                    for (int numActuators = 0; numActuators < NumberOfActuatorsToWrite; numActuators++)
                    {
                        request.Data[numActuators] = new MultipleSensorData()
                        {
                            SensorId = ConfigurationManager.AppSettings["ActuatorBaseName"] + numActuators,
                            Measures = new SensorData[1]
                            {
                                new SensorData() {GeneratedWhen = DateTime.Now, CorrelationId = CorrelationId++.ToString(), Value = random.Next(1, 100).ToString()}
                            }
                        };
                    }

                    TrackingPoint.TrackingPoint.CreateTrackingPoint
                        (
                            "WriteActuators: " + "SendMultipleActuatorsToServer_CNDEP",
                            "Before sending " + "Multiple correlation ids: " + CndepPrototocol,
                            NumberOfActuatorsToWrite,
                            string.Format("{0} - {1}", CorrelationId - NumberOfActuatorsToWrite, CorrelationId - 1));

                    Debug.Assert(SendCndepRequest(CndepServerAddress, CndepServerPort, CndepCommands.CmdStoreSensorData, 0, ContentParserFactory.GetParser(MimeTypeJson).Encode(request),
                        CndepContentType, resultTemplate, out responseObject), responseObject.ToString());

                    TrackingPoint.TrackingPoint.CreateTrackingPoint
                        (
                            "WriteActuators: " + "SendMultipleActuatorsToServer_CNDEP",
                            "After sending " + "Multiple correlation ids: " + CndepPrototocol,
                            NumberOfActuatorsToWrite,
                            string.Format("{0} - {1}", CorrelationId - NumberOfActuatorsToWrite, CorrelationId - 1));

                    Debug.Assert(responseObject != null);
                    Debug.Assert(responseObject is OperationResult);
                    Debug.Assert(((OperationResult)responseObject).Success, ((OperationResult)responseObject).ErrorMessages);
                }

                // Console.WriteLine("{0} data entries have been written for the actuators of device {1}", request.Data.Length, request.DeviceId);

                Thread.Sleep(DurationBetweenWritesInMsec);

            }
        }


        private static CndepClient GetCndepClient(int serverPort, IPAddress serverAddress, CommunicationProtocol protocol)
        {
            CndepClient client;
            switch (protocol)
            {
                case CommunicationProtocol.UDP:
                    client = new CndepUdpClient(serverPort, serverAddress);
                    break;
                case CommunicationProtocol.TCP:
                    client = new CndepTcpClient(serverPort, serverAddress);
                    break;
                default:
                    throw new Exception(String.Format("Unhandled CommunicationProtocol value: {0}.", protocol));
            }

            return client;
        }

        private static bool SendCndepRequest(string serverAddress, int serverPort, byte command, byte function, string requestData, string contentType, object resultObjectTemplate, out object result, params object[] parameters)
        {
            try
            {
                using (CndepClient cndepClient = GetCndepClient(serverPort, NetworkUtilities.GetIpV4AddressForDns(serverAddress), CndepPrototocol))
                {
                    byte[] data = UTF8Encoding.UTF8.GetBytes(requestData);

                    //Create message
                    CndepMessageRequest request = new CndepMessageRequest(GetSessionId(), command, function, data);

                    //Connect and send in the sync mode
                    cndepClient.Open();

                    CndepMessageResponse response = null;
                    int retryCount = 0;

                    do
                    {
                        response = cndepClient.Send(request, CndepRequestTimeout);

                        string responseDataStr;
                        if (response != null)
                        {
                            switch (response.ResponseId)
                            {
                                case CndepCommands.RspOK:
                                case CndepCommands.RspError:
                                    //Get data
                                    responseDataStr = UTF8Encoding.UTF8.GetString(response.Data, 0, response.Data.Length);

                                    //Check data
                                    if (responseDataStr == null || responseDataStr.Length == 0)
                                    {
                                        result = String.Format("Server replied with an empty string.");
                                        return false;
                                    }

                                    JsonSerializer serializer = new JsonSerializer();
                                    using (StringReader reader = new StringReader(responseDataStr))
                                    {
                                        serializer.Populate(reader, resultObjectTemplate);
                                    }

                                    result = resultObjectTemplate;
                                    return true;
                                default:
                                    result = String.Format("Unknown CNDEP response code received. Response code: {0}",
                                        response.ResponseId);
                                    return false;
                            }
                        }
                    }
                    while (response == null && retryCount++ < CndepRequestRetryCount);

                    result = "Response timeout";
                    return false;
                }

            }
            catch (Exception exc)
            {
                result = String.Format("CNDEP request failed. Error: {0}", exc.Message);
                return false;
            }
        }

        private static byte mSessionId;
        private static byte GetSessionId()
        {
            //lock (this)
            //{
            return mSessionId++;
            //}
        }
    }
}
