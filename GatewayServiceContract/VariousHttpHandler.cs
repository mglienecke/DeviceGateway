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
    /// The class handles various request (for not further qualified items)
    /// </summary>
    public class VariousHttpHandler : IHttpHandler
    {

        #region IHttpHandler members...

        public bool IsReusable { get { return true; } }


        public void ProcessRequest(HttpContext context)
        {
            string[] pathParts = context.Request.AppRelativeCurrentExecutionFilePath.Split(HttpHandlerUtils.PathDelimiter);

            try
            {
                switch (context.Request.HttpMethod)
                {
                    case HttpHandlerUtils.HttpMethodGet:
                        GetCorrelationIdResult result = CentralServerServiceClient.Instance.Service.GetNextCorrelationId();
                        HttpHandlerUtils.WriteResponse(context, result);
                        break;

                    case HttpHandlerUtils.HttpMethodPost:
                    case HttpHandlerUtils.HttpMethodPut:
                        HttpHandlerUtils.WriteResponseOperationFailed(context, HttpStatusCode.MethodNotAllowed, String.Empty);
                        break;

                    default:
                        HttpHandlerUtils.WriteResponseOperationFailed(context, HttpStatusCode.MethodNotAllowed, String.Empty);
                        break;
                }
            }
            catch (System.Exception ex)
            {
                HttpHandlerUtils.WriteResponse(context, new OperationResult(false, HttpHandlerUtils.GetErrorDescription("VariousHttpHandler", ex)));
            }
            finally
            {
                context.Response.Output.Close();
            }
        }
        #endregion

        private void ProcessDeviceRegistrationDataRequest(HttpContext context, string deviceId)
        {
            
        }

        private void CheckIfDeviceUsed(HttpContext context, string deviceId)
        {
            bool isUsed = CentralServerServiceClient.Instance.Service.IsDeviceIdUsed(deviceId);
            IsDeviceIdUsedResult result = new IsDeviceIdUsedResult(true, null, isUsed);

            HttpHandlerUtils.WriteResponse(context, result);
        }
    }
}
