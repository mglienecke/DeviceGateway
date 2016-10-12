using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using CentralServerService;
using Common.Server;
using GlobalDataContracts;
using System.Net;
using System.IO;
using System.Collections.Specialized;

namespace GatewayServiceContract
{
    /// <summary>
    /// The class contains various methods and constants for the Http Handlers.
    /// </summary>
    internal sealed class CentralServiceUtils
    {
        private CentralServiceUtils()
        {
        }

        public static bool CheckIfDeviceRegistered(string deviceId, HttpContext context)
        {
            //Check if the device is registered
            if (!CentralServerServiceClient.Instance.Service.IsDeviceIdUsed(deviceId))
            {
                context.Response.ContentType = HttpHandlerUtils.GetResponseContentType(context.Request.AcceptTypes);
                context.Response.StatusCode = (Int32)HttpStatusCode.NotFound;

                OperationResult result = new OperationResult() { Success = false };
                result.AddErrorFormat(Properties.Resources.ErrorDeviceNotFound, deviceId);

                HttpHandlerUtils.WriteResponse(context, result);

                return false;
            }

            return true;
        }
    }
}
