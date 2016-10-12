using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using GlobalDataContracts;
using System.IO;
using System.Collections.Specialized;
using CentralServerService;
using System.Net;
using Common.Server;

namespace GatewayServiceContract
{
    /// <summary>
    /// The class handler requests for sensor registration data and state.
    /// 1) Check if a sensor is registered for a device
    ///     GET /Devices/{deviceId}/Sensors/{sensorId}/isRegistered
    /// 2) Update sensor registration data
    ///    PUT, POST /Devices/{deviceId}/Sensors/{sensorId}   
    /// </summary>
    public class SensorHttpHandler : IHttpHandler
    {
        internal const string HttpRequestIsRegistered = "/isRegistered";

        #region IHttpHandler members...

        public bool IsReusable { get { return true; } }


        public void ProcessRequest(HttpContext context)
        {
            //Get device and sensor id
            //"/Devices/{deviceId}/Sensors/{sensorId}"
            string[] pathParts = context.Request.AppRelativeCurrentExecutionFilePath.Split(HttpHandlerUtils.PathDelimiter);
            string deviceId = pathParts[2];
            string sensorId = pathParts[4];

            try
            {
                //Check if the device is registered
                if (CentralServiceUtils.CheckIfDeviceRegistered(deviceId, context) == false)
                {
                    context.Response.StatusCode = (Int32)HttpStatusCode.NotFound;
                    return;
                }

                if (context.Request.AppRelativeCurrentExecutionFilePath.EndsWith(HttpRequestIsRegistered))
                {
                    //Check if registered?
                    CheckIfSensorRegistered(context, deviceId, sensorId);
                }
                else
                {
                    //Process reg data request
                    ProcessSensorRegistrationDataRequest(context, deviceId, sensorId);
                }
            }
            catch (System.Exception ex)
            {
                HttpHandlerUtils.WriteResponse(context, new OperationResult(false, HttpHandlerUtils.GetErrorDescription("SensorHttpHandler", ex)));
            }
            finally
            {
                context.Response.Output.Close();
            }
        }

        public static void CheckIfSensorRegistered(HttpContext context, string deviceId, string sensorId)
        {
            //Check if the sensor is registered
            bool isRegistered = CentralServerServiceClient.Instance.Service.IsSensorIdRegisteredForDevice(deviceId, sensorId);
            IsSensorIdRegisteredForDeviceResult result = new IsSensorIdRegisteredForDeviceResult(true, null, isRegistered);

            HttpHandlerUtils.WriteResponse(context, result);
        }


        private void ProcessSensorRegistrationDataRequest(HttpContext context, string deviceId, string sensorId)
        {
            switch (context.Request.HttpMethod)
            {
                case HttpHandlerUtils.HttpMethodPut:
                case HttpHandlerUtils.HttpMethodPost:
                    //Collect the passed data
                    Sensor sensor;
                    using (StreamReader reader = new StreamReader(context.Request.InputStream))
                    {
                        sensor = (Sensor)HttpHandlerUtils.DecodeDataObject(context, reader, typeof(Sensor));
                    }

                    //Just to make sure it's the same
                    sensor.DeviceId = deviceId;
                    sensor.Id = sensorId;

                    //Store the data
                    OperationResult result;
                    if (CentralServerServiceClient.Instance.Service.IsSensorIdRegisteredForDevice(deviceId, sensor.Id))
                    {
                        result = CentralServerServiceClient.Instance.Service.UpdateSensor(sensor);
                    }
                    else
                    {
                        result = CentralServerServiceClient.Instance.Service.RegisterSensors(new Sensor[]{ sensor });
                    }

                    //Write the response
                    HttpHandlerUtils.WriteResponse(context, result);
                    break;

                case HttpHandlerUtils.HttpMethodGet:
                    GetSensorResult resultGetSensor = CentralServerServiceClient.Instance.Service.GetSensor(deviceId, sensorId);
                    //Write the response
                    HttpHandlerUtils.WriteResponse(context, resultGetSensor);
                    break;

                default:
                    //Method not supported
                    HttpHandlerUtils.WriteResponseOperationFailed(context, HttpStatusCode.MethodNotAllowed, String.Empty);
                    break;
            }
        }

        #endregion
    }
}
