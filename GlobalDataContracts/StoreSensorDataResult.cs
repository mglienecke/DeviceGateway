using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace GlobalDataContracts
{
    [Serializable]
	[DataContract(Namespace = "http://GatewaysService")]
	public class StoreSensorDataResult : OperationResult
	{
		[DataMember]
		public List<SensorScanPeriod> AdjustedScanPeriods { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StoreSensorDataResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		/// <param name="errorMessages">The error messages.</param>
		/// <param name="data">The data.</param>
		public StoreSensorDataResult(bool success, List<string> errorMessages, List<SensorScanPeriod> data)
			: base(success, errorMessages)
		{
			AdjustedScanPeriods = data;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GetMultipleSensorDataResult"/> class.
		/// </summary>
		public StoreSensorDataResult()
			: base(false, null)
		{
			AdjustedScanPeriods = null;
		}

        /// <summary>
        /// Add a new adjusted sampling rate for a single sensor.
        /// </summary>
        /// <param name="sensorId"></param>
        /// <param name="samplingRateInMillis"></param>
        public void AddAdjustedSamplingRate(string sensorId, Int32 samplingRateInMillis)
        {
            if (AdjustedScanPeriods == null)
            {
                AdjustedScanPeriods = new List<SensorScanPeriod>();
            }

            AdjustedScanPeriods.Add(new SensorScanPeriod() { SensorId = sensorId, ScanPeriodInMillis = samplingRateInMillis });
        }
	}

    /// <summary>
    /// The class encapsulates scan period data of a single sensor.
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://GatewaysService")]
    public class SensorScanPeriod
    {
        /// <summary>
        /// returns the sensor id.
        /// </summary>
        public string SensorId { get; set; }

        /// <summary>
        /// returns the internal sensor id.
        /// </summary>
        public Int32 InternalSensorId { get; set; }

        /// <summary>
        /// returns the scan period in milliseconds
        /// </summary>
        public Int32 ScanPeriodInMillis
        {
            get;
            set;
        }
    }
}
