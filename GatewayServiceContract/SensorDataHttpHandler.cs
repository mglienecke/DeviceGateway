using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using CentralServerService;
using fastJSON;
using GlobalDataContracts;
using System.IO;
using System.Net;
using System.Globalization;
using Common.Server;

namespace GatewayServiceContract
{
    /// <summary>
    /// The class handles the sensor data operations.
    /// 1) Store data of a single device sensor
    ///     PUT, POST /Devices/{deviceId}/Sensors/{sensorId}/data
    /// 2) Store data of a set of sensors.
    ///     PUT, POST /Devices/{deviceId}/Sensors/SensorData
    /// 3) Get stored sensor data 
    ///     GET /Devices/{deviceId}/Sensors/SensorData?maxValuesPerSensor={Int32}&generatedBefore={DateTime}&generatedAfter={DateTime}
    /// 4) Get the latest stored sensor data
    ///     GET /Devices/{deviceId}/Sensors/SensorData/latest?maxValuesPerSensor={Int32}
    /// 
    /// </summary>
    public class SensorDataHttpHandler : IHttpHandler
    {
        internal const string HttpRequestStoreSingleData = "/data";
        internal const string HttpRequestStoreMultipleData = "/SensorData";
        internal const string HttpRequestStoreMultipleDataLatest = "/SensorData/latest";

        #region IHttpHandler members...

        public bool IsReusable { get { return true; } }


        public void ProcessRequest(HttpContext context)
        {
            //Get device and sensor id
            //"/Devices/{deviceId}/Sensors/{sensorId}/data"
            string[] pathParts = context.Request.AppRelativeCurrentExecutionFilePath.Split(HttpHandlerUtils.PathDelimiter);
            string deviceId = pathParts[2];

            try
            {
                //Check if the device is registered
                if (CentralServiceUtils.CheckIfDeviceRegistered(deviceId, context) == false)
                {
                    context.Response.StatusCode = (Int32)HttpStatusCode.NotFound;
                    return;
                }


                if (context.Request.AppRelativeCurrentExecutionFilePath.EndsWith(HttpRequestStoreSingleData))
                {
                    string sensorId = pathParts[4];
                    StoreSingleSensorData(context, deviceId, sensorId);
                }
                else
                    if (context.Request.AppRelativeCurrentExecutionFilePath.Contains(HttpRequestStoreMultipleData))
                    {
                        ProcessMultipleSensorDataRequest(context, deviceId);
                    }
                    else
                    {
                        //Bad URL
                        HttpHandlerUtils.WriteResponseOperationFailed(context, HttpStatusCode.BadRequest, String.Empty);
                        return;
                    }
            }
            catch (System.Exception ex)
            {
                HttpHandlerUtils.WriteResponse(context, new OperationResult(false, HttpHandlerUtils.GetErrorDescription("SensorDataHttpHandler", ex)));
            }
            finally
            {
                context.Response.Output.Close();
            }
        }

        private void ProcessMultipleSensorDataRequest(HttpContext context, string deviceId)
        {
            switch (context.Request.HttpMethod)
            {
                case HttpHandlerUtils.HttpMethodPut:
                case HttpHandlerUtils.HttpMethodPost:
                    //Collect the passed data
                    MultipleSensorData[] listData;
                    using (StreamReader reader = new StreamReader(context.Request.InputStream))
                    {
                        listData = (MultipleSensorData[])HttpHandlerUtils.DecodeDataObject(context, reader, typeof(MultipleSensorData[]));
                    }


                    Stopwatch watch = new Stopwatch();
                    TrackingPoint.TrackingPoint.CreateTrackingPoint
                        (
                         "Server: HTTP receive multiple",
                            "before processing: ",
                            listData.Length,
                            string.Format
                                ("{0} - {1}", listData[0].Measures[0].CorrelationId, listData[listData.Length - 1].Measures[0].CorrelationId));
                    watch.Start();

                    //Store the data
                    OperationResult result = CentralServerServiceClient.Instance.Service.StoreSensorData(deviceId, new List<MultipleSensorData>(listData));

                    watch.Stop();
                    TrackingPoint.TrackingPoint.CreateTrackingPoint
                        (
                         "Server: HTTP receive multiple",
                            "after processing: ",
                            watch.ElapsedMilliseconds,
                            string.Format
                                ("{0} - {1}", listData[0].Measures[0].CorrelationId, listData[listData.Length - 1].Measures[0].CorrelationId));

                    //Write the response
                    HttpHandlerUtils.WriteResponse(context, result);
                    break;
                case HttpHandlerUtils.HttpMethodGet:
                    //MaxValuesPerSensor
                    NameValueCollection queryString = context.Request.QueryString;

                    //Sensor ids
                    string[] sensorIds = HttpHandlerUtils.GetIds(queryString);

                    int maxValuesPerSensor = GetMaxValuesPerSensor(queryString);

                    
                    Stopwatch getWatch = new Stopwatch();
                    TrackingPoint.TrackingPoint.CreateTrackingPoint
                        ("Server: HTTP request latest", "before processing: ", maxValuesPerSensor, string.Format("{0} - {1}", sensorIds[0], sensorIds[sensorIds.Length - 1]));
                    getWatch.Start();

                    //Get the sensor data
                    GetMultipleSensorDataResult resultGet;
                    if (context.Request.Path.EndsWith(HttpRequestStoreMultipleDataLatest))
                    {
                        resultGet = CentralServerServiceClient.Instance.Service.GetSensorDataLatest(deviceId, new List<string>(sensorIds), maxValuesPerSensor);
                    }
                    else
                    {
                        //Generated before
                        DateTime generatedBefore = GetGeneratedBefore(queryString);

                        //Generated after
                        DateTime generatedAfter = GetGeneratedAfter(queryString);

                        //Get the data
                        resultGet = CentralServerServiceClient.Instance.Service.GetSensorData(deviceId, new List<string>(sensorIds), generatedAfter, generatedBefore, maxValuesPerSensor);
                    }

                    getWatch.Stop();
                    TrackingPoint.TrackingPoint.CreateTrackingPoint
                        ("Server: HTTP request latest", "after processing: ", getWatch.ElapsedMilliseconds, string.Format("{0} - {1}", sensorIds[0], sensorIds[sensorIds.Length - 1]));


                    //Write the response
                    HttpHandlerUtils.WriteResponse(context, resultGet);
                    break;

                default:
                    //Method not supported
                    HttpHandlerUtils.WriteResponseOperationFailed(context, HttpStatusCode.MethodNotAllowed, String.Empty);
                    break;
            }
        }

        private void StoreSingleSensorData(HttpContext context, string deviceId, string sensorId)
        {
            switch (context.Request.HttpMethod)
            {
                case HttpHandlerUtils.HttpMethodPut:
                case HttpHandlerUtils.HttpMethodPost:
                    //Collect the passed data
                    SensorData[] sensorData;
                    using (StreamReader reader = new StreamReader(context.Request.InputStream))
                    {
                        sensorData = (SensorData[])HttpHandlerUtils.DecodeDataObject(context, reader, typeof(SensorData[]));
                    }

                    //Wrap it in a list
                    MultipleSensorData data = new MultipleSensorData();
                    data.SensorId = sensorId;
                    data.Measures = sensorData;

                    //Store the data
                    OperationResult result = CentralServerServiceClient.Instance.Service.StoreSensorData(deviceId, data);

                    //Write the response
                    HttpHandlerUtils.WriteResponse(context, result);
                    break;
                default:
                    HttpHandlerUtils.WriteResponseOperationFailed(context, HttpStatusCode.MethodNotAllowed, String.Empty);
                    break;
            }
        }

        private int GetMaxValuesPerSensor(NameValueCollection queryString)
        {
            //MaxValuesPerSensor
            string tmpVal = queryString.Get(HttpHandlerUtils.QueryStringParamMaxValuesPerSensor);
            int maxValuesPerSensor;
            if (tmpVal == null || tmpVal.Length == 0)
            {
                maxValuesPerSensor = 0;
            }
            else
            {
                maxValuesPerSensor = Int32.Parse(tmpVal);
            }

            return maxValuesPerSensor;
        }

        private DateTime GetGeneratedAfter(NameValueCollection queryString)
        {
            string tmpVal = queryString.Get(HttpHandlerUtils.QueryStringParamGeneratedAfter);
            DateTime generatedAfter;
            if (tmpVal == null || tmpVal.Length == 0)
            {
                //100 years before
                generatedAfter = DateTime.Now.AddYears(-100);
            }
            else
            {
                generatedAfter = DateTime.Parse(tmpVal, CultureInfo.InvariantCulture);
            }

            return generatedAfter;
        }

        private DateTime GetGeneratedBefore(NameValueCollection queryString)
        {
            string tmpVal = queryString.Get(HttpHandlerUtils.QueryStringParamGeneratedBefore);
            DateTime generatedBefore;
            if (tmpVal == null || tmpVal.Length == 0)
            {
                //100 years after
                generatedBefore = DateTime.Now.AddYears(100);
            }
            else
            {
                generatedBefore = DateTime.Parse(tmpVal, CultureInfo.InvariantCulture);
            }

            return generatedBefore;
        }

        #endregion
    }
}
