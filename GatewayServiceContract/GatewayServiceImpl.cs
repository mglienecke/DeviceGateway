using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using GlobalDataContracts;
using CentralServerService;

namespace GatewayServiceContract
{
	public class GatewayServiceImpl : IGatewayService 
	{
		public GatewayServiceImpl()
		{
			// we want debugging
			//CentralServerServiceClient.UseRemoting = false;
		}


        public StoreSensorDataResult StoreSingleSensorData(string deviceId, string sensorId, SensorData data)
		{
            //Wrap it in a list
            List<MultipleSensorData> listData = new List<MultipleSensorData>(1);
            MultipleSensorData element = new MultipleSensorData();
            element.SensorId = sensorId;
            element.Measures = new SensorData[] { data };
            listData.Add(element);

            return StoreSensorData(deviceId, listData.ToArray());
		}

        public StoreSensorDataResult StoreSensorData(string deviceId, MultipleSensorData[] data)
		{
			try
			{
                return CentralServerServiceClient.Instance.Service.StoreSensorData(deviceId, new List<MultipleSensorData>(data)); 
				//OperationResult result = CentralServerServiceClient.Instance.Service.StoreSensorData(deviceId, data);
				//return (new OperationResult(result.Success, result.ErrorMessages));
			}
			catch (System.Exception ex)
			{
                return (new StoreSensorDataResult(false, GetErrorDescription("StoreSensorData", ex), null));
			}
		}

		public OperationResult RegisterDevice(Device device)
		{
			try
			{
                return CentralServerServiceClient.Instance.Service.RegisterDevice(device);
				//CallResult result = CentralServerServiceClient.Instance.Service.RegisterDevice(device);
				//return (new OperationResult(result.Success, result.ErrorMessages));
			}
			catch (System.Exception ex)
			{
				return (new OperationResult(false, GetErrorDescription("RegisterDevice", ex)));
			}
		}

		public OperationResult UpdateDevice(Device device)
		{
			try
			{
                return CentralServerServiceClient.Instance.Service.UpdateDevice(device); 
				//CallResult result = CentralServerServiceClient.Instance.Service.UpdateDevice(device);
				//return (new OperationResult(result.Success, result.ErrorMessages));
			}
			catch (System.Exception ex)
			{
				return (new OperationResult(false, GetErrorDescription("UpdateDevice", ex)));
			}
		}

		public OperationResult RegisterSensors(Sensor[] sensorList)
		{
			try
			{
                return CentralServerServiceClient.Instance.Service.RegisterSensors(new List<Sensor>(sensorList));
				//CallResult result = CentralServerServiceClient.Instance.Service.RegisterSensors(sensorList);
				//return (new OperationResult(result.Success, result.ErrorMessages));
			}
			catch (System.Exception ex)
			{
				return (new OperationResult(false, GetErrorDescription("RegisterSensor", ex)));
			}
		}

		public OperationResult UpdateSensor(Sensor sensor)
		{
			try
			{
                return CentralServerServiceClient.Instance.Service.UpdateSensor(sensor);
				//CallResult result = CentralServerServiceClient.Instance.Service.UpdateSensor(sensor);
				//return (new OperationResult(result.Success, result.ErrorMessages));
			}
			catch (System.Exception ex)
			{
				return (new OperationResult(false, GetErrorDescription("UpdateSensor", ex)));
			}
		}

		public GetDevicesResult GetDevices()
		{
			try
			{
				return (CentralServerServiceClient.Instance.Service.GetDevices());
			}
			catch (System.Exception ex)
			{
				return (new GetDevicesResult(false, GetErrorDescription("GetDevices", ex), null));
			}
		}

        public GetDevicesResult GetDevice(string deviceId)
        {
            try
            {
                return (CentralServerServiceClient.Instance.Service.GetDevices(new List<string>(new string[]{deviceId})));
            }
            catch (System.Exception ex)
            {
                return (new GetDevicesResult(false, GetErrorDescription("GetDevice", ex), null));
            }
        }

		public GetSensorsForDeviceResult GetSensorsForDevice(string deviceId)
		{
			try
			{
				return (CentralServerServiceClient.Instance.Service.GetSensorsForDevice(deviceId));
			}
			catch (System.Exception ex)
			{
				return (new GetSensorsForDeviceResult(false, GetErrorDescription("GetSensorsForDevice", ex), null));
			}
		}

		public IsDeviceIdUsedResult IsDeviceIdUsed(string deviceId)
		{
			try
			{
				return (new IsDeviceIdUsedResult(true, null, CentralServerServiceClient.Instance.Service.IsDeviceIdUsed(deviceId)));
			}
			catch (System.Exception ex)
			{
				return (new IsDeviceIdUsedResult(false, GetErrorDescription("IsDeviceIdUsed", ex), false));
			}
		}

		public IsSensorIdRegisteredForDeviceResult IsSensorIdRegisteredForDevice(string deviceId, string sensorId)
		{
			try
			{
				return (new IsSensorIdRegisteredForDeviceResult(true, null, CentralServerServiceClient.Instance.Service.IsSensorIdRegisteredForDevice(deviceId, sensorId)));
			}
			catch (System.Exception ex)
			{
				return (new IsSensorIdRegisteredForDeviceResult(false, GetErrorDescription("IsSensorIdRegisteredForDevice", ex), false));
			}
		}

		public GetSensorResult GetSensor(string deviceId, string sensorId)
		{
			try
			{
				return (CentralServerServiceClient.Instance.Service.GetSensor(deviceId, sensorId));
			}
			catch (System.Exception ex)
			{
				return (new GetSensorResult(false, GetErrorDescription("GetSensor", ex), null));
			}
		}

		public GetMultipleSensorDataResult GetSensorData(string deviceId, string[] sensors, DateTime generatedAfter, DateTime generatedBefore, int maxValuesToDeliverPerSensor)
		{
			try
			{
				return(CentralServerServiceClient.Instance.Service.GetSensorData(deviceId, new List<string>(sensors), generatedAfter, generatedBefore, maxValuesToDeliverPerSensor));
			}
			catch (System.Exception ex)
			{
				return (new GetMultipleSensorDataResult(false, GetErrorDescription("GetSensorData", ex), null));
			}
		}

        public GetMultipleSensorDataResult GetSensorDataLatest(string deviceId, string[] sensors, int maxValuesToDeliverPerSensor)
        {
            try
            {
                return (CentralServerServiceClient.Instance.Service.GetSensorDataLatest(deviceId, new List<string>(sensors), maxValuesToDeliverPerSensor));
            }
            catch (System.Exception ex)
            {
                return (new GetMultipleSensorDataResult(false, GetErrorDescription("GetSensorDataLatest", ex), null));
            }
        }


        public GetCorrelationIdResult GetNextCorrelationId()
        {
            try
            {
                return (CentralServerServiceClient.Instance.Service.GetNextCorrelationId());
            }
            catch (System.Exception ex)
            {
                return (new GetCorrelationIdResult(false, GetErrorDescription("GetNextCorrelationId", ex), null));
            }
        }


		/// <summary>
		/// Gets the error description.
		/// </summary>
		/// <param name="method">The method.</param>
		/// <param name="x">The x.</param>
		/// <returns></returns>
		protected List<string> GetErrorDescription(string method, Exception x)
		{
			List<string> result = new List<string>();
			result.Add(string.Format(Properties.Resources.ExceptionOccured, method, x.Message, x.StackTrace));
			return (result);
		}

	}
}
