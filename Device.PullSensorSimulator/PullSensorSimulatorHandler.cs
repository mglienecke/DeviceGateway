using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Net;
using Common.Server;
using GlobalDataContracts;

namespace Device.Simulator.HttpRest
{
    /// <summary>
    /// HTTP handler to simulate a device sensor with the HTTP REST interface.
    /// </summary>
    public class PullSensorSimulatorHandler : IHttpHandler
    {
        internal const string HttpRequestCurrentValue = "/currentValue";
        internal const char PathDelimiter = '/';

        #region IHttpHandler members...

        public bool IsReusable { get { return true; } }


        public void ProcessRequest(HttpContext context)
        {
            //Get sensor id
            //"/Sensors/{sensorId}/currentValue"
            string[] pathParts = context.Request.AppRelativeCurrentExecutionFilePath.Split(PathDelimiter);
            string sensorId = pathParts[2];

            try
            {
                if (context.Request.AppRelativeCurrentExecutionFilePath.EndsWith(HttpRequestCurrentValue))
                {
                    //Check if used
                    GetCurrentSensorValue(context, sensorId);
                }
            }
            catch (System.Exception ex)
            {
                HttpHandlerUtils.WriteResponse(context, new OperationResult(false, HttpHandlerUtils.GetErrorDescription("PullSensorSimulatorHandler", ex)));
            }
            finally
            {
                context.Response.Output.Close();
            }
        }
        #endregion

        private void GetCurrentSensorValue(HttpContext context, string sensorId)
        {
            try
            {
                HttpHandlerUtils.WriteResponse(context, new SensorData(DateTime.Now, DateTime.Now.Millisecond.ToString(), DateTime.Now.Ticks.ToString()));
            }
            catch (System.Exception ex)
            {
                HttpHandlerUtils.WriteResponse(context, new OperationResult(false, HttpHandlerUtils.GetErrorDescription("PullSensorSimulatorHandler", ex)));
            }
            finally
            {
                context.Response.Output.Close();
            }
        }
    }
}
