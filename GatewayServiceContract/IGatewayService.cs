using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using GlobalDataContracts;

namespace GatewayServiceContract
{
    [ServiceContract(Namespace = "http://GatewaysService")]
    public interface IGatewayService
    {
        [System.ServiceModel.OperationContractAttribute(Action = "urn:#RegisterDevice")]
        //[System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "Result")]
        OperationResult RegisterDevice(Device Device);

        [System.ServiceModel.OperationContractAttribute(Action = "urn:#GetDevices")]
        //[System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "Result")]
        [WebGet(UriTemplate = "Devices")]
        GetDevicesResult GetDevices();

        [System.ServiceModel.OperationContractAttribute(Action = "urn:#GetDevice")]
        //[System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "Result")]
        [WebGet(UriTemplate = "Devices/{deviceId}")]
        GetDevicesResult GetDevice(string deviceId);

        [System.ServiceModel.OperationContractAttribute(Action = "urn:#UpdateDevice")]
        //[System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "Result")]
        OperationResult UpdateDevice(Device Device);

        [System.ServiceModel.OperationContractAttribute(Action = "urn:#RegisterSensors")]
        //[System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "Result")]
        OperationResult RegisterSensors(Sensor[] Sensors);

        [System.ServiceModel.OperationContractAttribute(Action = "urn:#UpdateSensor")]
        //[System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "Result")]
        OperationResult UpdateSensor(Sensor Sensor);

        [System.ServiceModel.OperationContractAttribute(Action = "urn:#GetSensorsForDevice")]
        //[System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "Result")]
        [WebGet(UriTemplate = "Devices/{deviceId}/Sensors")]
        GetSensorsForDeviceResult GetSensorsForDevice(string deviceId);

        [System.ServiceModel.OperationContractAttribute(Action = "urn:#IsDeviceIdUsed")]
        //[System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "Result")]
        [WebGet(UriTemplate = "Devices/{deviceId}/isUsed")]
        IsDeviceIdUsedResult IsDeviceIdUsed(string deviceId);

        [System.ServiceModel.OperationContractAttribute(Action = "urn:#IsSensorIdRegisteredForDevice")]
        //[System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "Result")]
        [WebGet(UriTemplate = "Devices/{deviceId}/Sensors/{sensorId}/isRegistered")]
        IsSensorIdRegisteredForDeviceResult IsSensorIdRegisteredForDevice(string deviceId, string sensorId);

        [System.ServiceModel.OperationContractAttribute(Action = "urn:#GetSensor")]
        //[System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "Result")]
        [WebGet(UriTemplate = "Devices/{deviceId}/Sensors/{sensorId}")]
        GetSensorResult GetSensor(string deviceId, string sensorId);


        [System.ServiceModel.OperationContractAttribute(Action = "urn:#StoreSensorData")]
        //[System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "Result")]
        [WebInvoke(Method = "POST", UriTemplate = "Devices/{deviceId}")]
        StoreSensorDataResult StoreSensorData(string deviceId, MultipleSensorData[] data);

        [System.ServiceModel.OperationContractAttribute(Action = "urn:#GetSensorData")]
        //[System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "Result")]
        [WebInvoke(Method="POST", UriTemplate = "Devices/{deviceId}/Sensors/SensorData?generatedBefore={takenFrom}&generatedAfter={takenUntil}&maxValuesPerSensors={maxValues}")]
        GetMultipleSensorDataResult GetSensorData(string deviceId, string[] sensors, System.DateTime takenFrom, System.DateTime takenUntil, int maxValues);

        [System.ServiceModel.OperationContractAttribute(Action = "urn:#GetSensorDataLatest")]
        //[System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, Use = System.ServiceModel.OperationFormatUse.Encoded)]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "Result")]
        [WebInvoke(Method = "POST", UriTemplate = "Devices/{deviceId}/Sensors/SensorData/latest?maxValuesPerSensors={maxValues}")]
        GetMultipleSensorDataResult GetSensorDataLatest(string deviceId, string[] sensors, int maxValues);

        [System.ServiceModel.OperationContractAttribute(Action = "urn:#GetNextCorrelationId")]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "Result")]
        [WebGet(UriTemplate = "GetNextCorrelationId")]
        GetCorrelationIdResult GetNextCorrelationId();

    }


    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IGatewayServiceChannel : IGatewayService, System.ServiceModel.IClientChannel
    {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GatewayServiceClient : System.ServiceModel.ClientBase<IGatewayService>, IGatewayService
    {
        public GatewayServiceClient()
        {
        }

        public GatewayServiceClient(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        public GatewayServiceClient(string endpointConfigurationName, string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public GatewayServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public GatewayServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        public GetDevicesResult GetDevices()
        {
            return base.Channel.GetDevices();
        }

        public GetDevicesResult GetDevice(string Id)
        {
            return base.Channel.GetDevice(Id);
        }

        public OperationResult RegisterDevice(Device Device)
        {
            return base.Channel.RegisterDevice(Device);
        }

        public OperationResult UpdateDevice(Device Device)
        {
            return base.Channel.UpdateDevice(Device);
        }

        public OperationResult RegisterSensors(Sensor[] Sensors)
        {
            return base.Channel.RegisterSensors(Sensors);
        }

        public OperationResult UpdateSensor(Sensor Sensor)
        {
            return base.Channel.UpdateSensor(Sensor);
        }

        public GetSensorsForDeviceResult GetSensorsForDevice(string DeviceId)
        {
            return base.Channel.GetSensorsForDevice(DeviceId);
        }

        public IsDeviceIdUsedResult IsDeviceIdUsed(string DeviceId)
        {
            return base.Channel.IsDeviceIdUsed(DeviceId);
        }

        public IsSensorIdRegisteredForDeviceResult IsSensorIdRegisteredForDevice(string DeviceId, string SensorId)
        {
            return base.Channel.IsSensorIdRegisteredForDevice(DeviceId, SensorId);
        }

        public GetSensorResult GetSensor(string DeviceId, string SensorId)
        {
            return base.Channel.GetSensor(DeviceId, SensorId);
        }

        public StoreSensorDataResult StoreSensorData(string DeviceId, MultipleSensorData[] Data)
        {
            return base.Channel.StoreSensorData(DeviceId, Data);
        }

        public GetMultipleSensorDataResult GetSensorData(string DeviceId, string[] Sensors, System.DateTime TakenFrom, System.DateTime TakenUntil, int MaxValues)
        {
            return base.Channel.GetSensorData(DeviceId, Sensors, TakenFrom, TakenUntil, MaxValues);
        }

        public GetMultipleSensorDataResult GetSensorDataLatest(string DeviceId, string[] Sensors, int MaxValues)
        {
            return base.Channel.GetSensorDataLatest(DeviceId, Sensors, MaxValues);
        }

        public GetCorrelationIdResult GetNextCorrelationId()
        {
            return base.Channel.GetNextCorrelationId();
        }
    }
}
