using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using GlobalDataContracts;
using CentralServerService;
using System.IO;
using System.Net;
using Common.Server;

namespace GatewayServiceContract
{
    /// <summary>
    /// The class handles creation and data retrieval requests for sensors of a single device.
    /// 1)Get sensors registered for a device
    ///     GET /Devices/{deviceId}/Sensors
    /// 2) Register sensors for a device
    ///     PUT /Devices/{deviceId}/Sensors
    /// </summary>
    public class DeviceSensorsHttpHandler : IHttpHandler
    {
        #region IHttpHandler members...

        public bool IsReusable { get { return true; } }


        public void ProcessRequest(HttpContext context)
        {
            // Get device id
            //"/Devices/{deviceId}/Sensors"
            string[] pathParts = context.Request.AppRelativeCurrentExecutionFilePath.Split(HttpHandlerUtils.PathDelimiter);
            string deviceId = pathParts[2];

            try
            {
                // Check if the device is registered - if not deliver NotFound
                if (CentralServiceUtils.CheckIfDeviceRegistered(deviceId, context) == false)
                {
                    context.Response.StatusCode = (Int32) HttpStatusCode.NotFound;
                    return;
                }
                
                //Handle
                switch (context.Request.HttpMethod)
                {

                    case HttpHandlerUtils.HttpMethodPost:
                    case HttpHandlerUtils.HttpMethodPut:
                         //Collect the sensor data
                        Sensor[] sensors;
                        using (StreamReader reader = new StreamReader(context.Request.InputStream))
                        {
                            sensors = (Sensor[])HttpHandlerUtils.DecodeDataObject(context, reader, typeof(Sensor[]));
                        }
                        List<Sensor> sensorsToRegister = new List<Sensor>(sensors);
                        foreach (Sensor sensor in sensors)
                        {
                            //Ensure the device id
                            sensor.DeviceId = deviceId;

                            //Update the existing
                            if (CentralServerServiceClient.Instance.Service.IsSensorIdRegisteredForDevice(sensor.DeviceId, sensor.Id))
                            {
                                sensorsToRegister.Remove(sensor);
                                CentralServerServiceClient.Instance.Service.UpdateSensor(sensor);
                            }
                        }

                        //Register the sensors
                        OperationResult resultPut = CentralServerServiceClient.Instance.Service.RegisterSensors(sensorsToRegister);

                        //Write the response
                        HttpHandlerUtils.WriteResponse(context, resultPut);
                        break;

                    case HttpHandlerUtils.HttpMethodGet:
                        //Get the sensors
                        GetSensorsForDeviceResult resultGet = CentralServerServiceClient.Instance.Service.GetSensorsForDevice(deviceId);

                        //Write the response
                        HttpHandlerUtils.WriteResponse(context, resultGet);
                        break;

                    default:
                        HttpHandlerUtils.WriteResponseOperationFailed(context, HttpStatusCode.MethodNotAllowed, String.Empty);
                        break;
                }
            }
            catch (System.Exception ex)
            {
                HttpHandlerUtils.WriteResponse(context, new OperationResult(false, HttpHandlerUtils.GetErrorDescription("DeviceSensorsHttpHandler", ex)));
            }
            finally
            {
                context.Response.Output.Close();
            }
        }
        #endregion
    }
}
