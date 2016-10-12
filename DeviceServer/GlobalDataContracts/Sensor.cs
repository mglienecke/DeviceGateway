using System;
using Microsoft.SPOT;

namespace GlobalDataContracts
{
    [Serializable]
    public enum SensorDataRetrievalMode
    {
        Pull = 0,

        Push = 1,

        Both = 2,

        PushOnChange = 4
    }

    [Serializable]
    public enum SensorValueDataType
    {
        Bit = TypeCode.Boolean,

        Int = TypeCode.Int32,

        Long = TypeCode.Int64,

        Decimal = TypeCode.Decimal,

        String = TypeCode.String
    }

    [Serializable]
    public enum PullModeCommunicationType
    {
        Undefined = 0,

        REST = 1,

        SOAP = 2,

        DotNetObject = 3
    }

    [Serializable]
    public class SensorConfig
    {
        public string Id { get; set; }

        public SensorDataRetrievalMode SensorDataRetrievalMode { get; set; }

        public string PushUrl { get; set; }

        public bool IsCoalesce { get; set; }

        public bool IsLocalStore { get; set; }

        public int IsLocalStoreMaxRecords {get;set;}

        public int ScanFrequencyInSeconds { get; set; }
    }

    [Serializable]
    public class Sensor
    {
        public string DeviceId { get; set; }

        public string Id { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        public string UnitSymbol { get; set; }

        public SensorDataRetrievalMode AvailableRetrievalMode { get; set; }

        public SensorValueDataType SensorValueDataType { get; set; }

        public PullModeCommunicationType PullModeCommunicationType { get; set; }

        public SensorConfig Config { get; set; }
    }
}
