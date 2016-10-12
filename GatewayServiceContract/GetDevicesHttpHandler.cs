using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using GlobalDataContracts;
using CentralServerService;
using Common.Server;

namespace GatewayServiceContract
{
    /// <summary>
    /// The class implements a HTTP handler for servicing the GET request for Device data.
    /// </summary>
    public class GetDevicesHttpHandler : IHttpHandler
    {
        #region IHttpHandler members...

        public bool IsReusable { get { return true; } }


        public void ProcessRequest(HttpContext context)
        {
            try
            {
                //Collect the passed data
                string[] deviceIds = null;
                if (context.Request.QueryString.Count > 0)
                {
                    //Get sensor ids
                    deviceIds = HttpHandlerUtils.GetIds(context.Request.QueryString);
                }

                GetDevicesResult result;
                //Get all or just some
                if (deviceIds == null)
                {
                    result = CentralServerServiceClient.Instance.Service.GetDevices();
                }
                else
                {
                    result = CentralServerServiceClient.Instance.Service.GetDevices(deviceIds); ;
                }

                HttpHandlerUtils.WriteResponse(context, result);
            }
            catch (System.Exception ex)
            {
                HttpHandlerUtils.WriteResponse(context, new GetDevicesResult(false, HttpHandlerUtils.GetErrorDescription("GetDevicesHttpHandler", ex), null));
            }
            finally
            {
                context.Response.Output.Close();
            }
        }
        #endregion
    }
}
