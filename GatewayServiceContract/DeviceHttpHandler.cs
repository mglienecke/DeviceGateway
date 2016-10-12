using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using CentralServerService;
using GlobalDataContracts;
using System.IO;
using System.Net;
using Common.Server;

namespace GatewayServiceContract
{
    /// <summary>
    /// The class handles creation and update requests for Devices.
    /// 1) Create/update device
    ///     PUT/POST /Devices/{deviceId}
    /// 2) Check if a device is used
    ///     GET /Devices/{deviceId}/isUsed
    /// </summary>
    public class DeviceHttpHandler : IHttpHandler
    {
        internal const string HttpRequestIsUsed = "/isUsed";

        #region IHttpHandler members...

        public bool IsReusable { get { return true; } }


        public void ProcessRequest(HttpContext context)
        {
            //Get device id
            //"/Devices/{deviceId}"
            string[] pathParts = context.Request.AppRelativeCurrentExecutionFilePath.Split(HttpHandlerUtils.PathDelimiter);
            string deviceId = pathParts[2];

            try{
                if (context.Request.AppRelativeCurrentExecutionFilePath.EndsWith(HttpRequestIsUsed))
                {
                    //Check if used
                    CheckIfDeviceUsed(context, deviceId);
                }
                else
                {
                    //Process device data request
                    ProcessDeviceRegistrationDataRequest(context, deviceId);
                }
            }
            catch (System.Exception ex)
            {
                HttpHandlerUtils.WriteResponse(context, new OperationResult(false, HttpHandlerUtils.GetErrorDescription("DeviceHttpHandler", ex)));
            }
            finally
            {
                context.Response.Output.Close();
            }
        }
        #endregion

        private void ProcessDeviceRegistrationDataRequest(HttpContext context, string deviceId){
            switch (context.Request.HttpMethod)
            {
                case HttpHandlerUtils.HttpMethodGet:
                    GetDevicesResult resultDevices = CentralServerServiceClient.Instance.Service.GetDevices(new string[]{deviceId}); ;
                    HttpHandlerUtils.WriteResponse(context, resultDevices);
                    break;
                case HttpHandlerUtils.HttpMethodPost:
                case HttpHandlerUtils.HttpMethodPut:
                    OperationResult result;
                    //Check if the device is already registered
                    bool isUsed = CentralServerServiceClient.Instance.Service.IsDeviceIdUsed(deviceId);

                    //Collect the device data
                    Device device;
                    using (StreamReader reader = new StreamReader(context.Request.InputStream))
                    {
                        device = (Device)HttpHandlerUtils.DecodeDataObject(context, reader, typeof(Device));
                    }
                    //To make sure it's the same
                    device.Id = deviceId;

                    //Create or update
                    if (isUsed)
                    {
                        result = CentralServerServiceClient.Instance.Service.UpdateDevice(device);
                    }
                    else
                    {
                        result = CentralServerServiceClient.Instance.Service.RegisterDevice(device);
                    }

                    //Write the response
                    HttpHandlerUtils.WriteResponse(context, result);
                    break;

                default:
                    HttpHandlerUtils.WriteResponseOperationFailed(context, HttpStatusCode.MethodNotAllowed, String.Empty);
                    break;
            }
        }

        private void CheckIfDeviceUsed(HttpContext context, string deviceId)
        {
            bool isUsed = CentralServerServiceClient.Instance.Service.IsDeviceIdUsed(deviceId);
            IsDeviceIdUsedResult result = new IsDeviceIdUsedResult(true, null, isUsed);

            HttpHandlerUtils.WriteResponse(context, result);
        }
    }
}
