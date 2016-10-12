using System;
using System.Text;

#if DEVICE_IMPLEMENTATION
using Ws.ServiceModel;
#else
using System.ServiceModel;
using System.Runtime.Serialization;
#endif


namespace GlobalDataContracts
{
    [Serializable]
    [DataContract(Name = "SensorDataRetrievalMode", Namespace = "http://GatewaysService")]
	public enum SensorDataRetrievalMode
	{
        [EnumMember]
        None = 0,
        [EnumMember]
		Pull = 1,
        [EnumMember]
		Push = 2,
        [EnumMember]
		Both = 4,
        [EnumMember]
        PushOnChange = 8
	}

    [Serializable]
    [DataContract(Name = "SensorValueDataType", Namespace = "http://GatewaysService")]
	public enum SensorValueDataType
	{
        [EnumMember]
		Bit = TypeCode.Boolean,
        [EnumMember]
		Int = TypeCode.Int32,
        [EnumMember]
		Long = TypeCode.Int64,
        [EnumMember]
		Decimal = TypeCode.Decimal,
        [EnumMember]
		String = TypeCode.String
	}

    [Serializable]
    [DataContract(Name = "PullModeCommunicationType", Namespace = "http://GatewaysService")]
	public enum PullModeCommunicationType
	{
        [EnumMember]
		Undefined = 0,
        [EnumMember]
		REST = 1,
        [EnumMember]
		SOAP = 2,
        [EnumMember]
		DotNetObject = 3,
        [EnumMember]
        CNDEP = 4,
        [EnumMember]
        MSMQ = 5
	}

    [Serializable]
    [DataContract(Name = "VirtualSensorCalculationType", Namespace = "http://GatewaysService")]
	public enum VirtualSensorCalculationType
	{
        [EnumMember]
		Undefined = 0,
        [EnumMember]
		Cyclic = 1,
        [EnumMember]
		OnChange = 2,
        [EnumMember]
		OnRequest = 3
	}

    [Serializable]
    [DataContract(Name = "VirtualSensorDefinitionType", Namespace = "http://GatewaysService")]
	public enum VirtualSensorDefinitionType
	{
        [EnumMember]
		Undefined = 0,
        [EnumMember]
		Formula = 1,
        [EnumMember]
		FSharpExpression = 2,
        [EnumMember]
		CSharpExpression = 3,
        [EnumMember]
		IronPhyton = 4,
        [EnumMember]
		IronRuby = 5,
        [EnumMember]
		DotNetObject = 6,
        [EnumMember]
		SQL = 7,
        [EnumMember]
        WWF = 8
	}

    [Serializable]
	[DataContract(Namespace = "http://GatewaysService")]
	public class VirtualSensorDefinition
	{
		[DataMember]
        public VirtualSensorCalculationType VirtualSensorCalculationType { get; set; }

		[DataMember]
        public VirtualSensorDefinitionType VirtualSensorDefinitionType { get; set; }

		[DataMember]
		public string Definition { get; set; }
	}

    [Serializable]
	[DataContract(Namespace = "http://GatewaysService")]
	public class Sensor
	{
		[DataMember]
		public string DeviceId { get; set; }

		[DataMember]
		public string Id { get; set; }

		/// <summary>
		/// Normally not used by clients
		/// </summary>
		[DataMember]
		public int InternalSensorId { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public string Category { get; set; }

        [DataMember]
        public string DefaultValue { get; set; }

		[DataMember]
		public bool IsVirtualSensor { get; set; }

		[DataMember]
		public bool ShallSensorDataBePersisted { get; set; }

        [DataMember]
        public bool PersistDirectlyAfterChange { get; set; }

		[DataMember]
		public string UnitSymbol { get; set; }

		[DataMember]
        public SensorDataRetrievalMode SensorDataRetrievalMode { get; set; }

		[DataMember]
        public SensorValueDataType SensorValueDataType { get; set; }

		[DataMember]
        public PullModeCommunicationType PullModeCommunicationType { get; set; }

        [DataMember]
        public PullModeCommunicationType PushModeCommunicationType { get; set; }

		[DataMember]
		public int PullFrequencyInSeconds { get; set; }

		[DataMember]
		public string PullModeDotNetObjectType { get; set; }

		[DataMember]
		public VirtualSensorDefinition VirtualSensorDefinition { get; set; }

        [DataMember]
        public bool IsSynchronousPushToActuator { get; set; }

        [DataMember]
        public bool IsActuator { get; set; }
	}
}
