using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalDataContracts;
using System.IO;
using System.Net;
using System.Configuration;

namespace Common.Server
{
    /// <summary>
    /// The class implements the HTTP REST version of the <see cref="IDeviceCommunicationHandler"/>.
    /// </summary>
    public class HttpRestDeviceCommunicationHandler: IDeviceCommunicationHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region IDeviceCommunicationHandler members...
        SensorData IDeviceCommunicationHandler.GetSensorCurrentData(Device device, Sensor sensor)
        {
            try
            {
                String uriString = String.Format("http://{0}/Sensors/{1}/currentValue", device.DeviceIpEndPoint, sensor.Id);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uriString);
                request.Method = "GET";
                //request.Accept = JsonContentParser.ContentType;
                request.KeepAlive = false;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                SensorData sensorData = null;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        sensorData = (SensorData)ContentParserFactory.GetParser(response.ContentType).Decode(reader, typeof(SensorData));
                    }
                }
                else
                {
                    log.Error(String.Format(Properties.Resources.ExceptionFailedObtainingSensorCurrentDataUsingHttpRestDetails,
                        device.Id, sensor.Id, response.StatusCode, response.StatusDescription));

                    throw new Exception(String.Format(Properties.Resources.ExceptionFailedObtainingSensorCurrentDataUsingHttpRestDetails,
                        device.Id, sensor.Id, response.StatusCode, response.StatusDescription));
                }

                return sensorData;
            }
            catch (Exception exc)
            {
                log.Error(String.Format(Properties.Resources.ExceptionFailedObtainingSensorCurrentDataUsingHttpRest, device.Id, sensor.Id), exc);
                throw;
            }
        }

        OperationResult IDeviceCommunicationHandler.PutSensorCurrentData(Device device, Sensor sensor, SensorData data)
        {
            try
            {
                String uriString = String.Format("http://{0}/Sensors/{1}/currentValue", device.DeviceIpEndPoint, sensor.Id);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uriString);
                request.Method = "PUT";

                var contentType = JsonContentParser.ContentType;
                byte[] buffer = Encoding.UTF8.GetBytes(ContentParserFactory.GetParser(contentType).Encode(data));

                // request headers
                request.ContentLength = buffer.Length;
                request.ContentType = contentType;
                request.KeepAlive = false;

                // request body
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Close();
                }

                //request.Accept = JsonContentParser.ContentType;
                request.KeepAlive = false;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                OperationResult result = null;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        return (OperationResult)ContentParserFactory.GetParser(response.ContentType).Decode(reader, typeof(OperationResult));
                    }
                }
                else
                {
                    log.Error(String.Format(Properties.Resources.ExceptionFailedSettingSensorCurrentDataUsingHttpRestDetails,
                        device.Id, sensor.Id, data.Value, response.StatusCode, response.StatusDescription));

                    throw new Exception(String.Format(Properties.Resources.ExceptionFailedSettingSensorCurrentDataUsingHttpRestDetails,
                        device.Id, sensor.Id, data.Value, response.StatusCode, response.StatusDescription));
                }
            }
            catch (Exception exc)
            {
                log.Error(String.Format(Properties.Resources.ExceptionFailedSettingSensorCurrentDataUsingHttpRest, device.Id, sensor.Id, data.Value), exc);
                throw;
            }
        }

        bool IDeviceCommunicationHandler.IsReusableAndThreadSafe { get { return true; } }
        #endregion
    }
}
