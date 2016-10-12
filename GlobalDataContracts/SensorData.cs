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
    [DataContract(Namespace = "http://GatewaysService")]
    public class SensorData
    {
        ///// <summary>
        ///// Initializes a new instance of the <see cref="SensorData"/> class.
        ///// </summary>
        ///// <param name="generatedWhen">The generated when.</param>
        ///// <param name="value">The value.</param>
        //public SensorData(DateTime generatedWhen, string value)
        //{
        //    GeneratedWhen = generatedWhen;
        //    Value = value;
        //}

        ///// <summary>
        ///// Initializes a new instance of the <see cref="SensorData"/> class.
        ///// </summary>
        ///// <param name="generatedWhenTicks">The generated when.</param>
        ///// <param name="value">The value.</param>
        //public SensorData(long generatedWhenTicks, string value)
        //    : this(new DateTime(generatedWhenTicks), value)
        //{
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorData"/> class.
        /// </summary>
        /// <param name="generatedWhen">The generated when.</param>
        /// <param name="value">The value.</param>
        /// <param name="correlationId"></param>
        public SensorData(DateTime generatedWhen, string value, string correlationId)
        {
            GeneratedWhen = generatedWhen;
            Value = value;
            CorrelationId = correlationId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorData"/> class.
        /// </summary>
        /// <param name="generatedWhenTicks">The generated when.</param>
        /// <param name="value">The value.</param>
        /// <param name="correlationId"></param>
        public SensorData(long generatedWhenTicks, string value, string correlationId)
            : this(new DateTime(generatedWhenTicks), value, correlationId)
        {
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the created timestamp
        /// </summary>
        /// <value>The created.</value>
        [DataMember]
        public DateTime GeneratedWhen { get; set; }

        /// <summary>
        /// Gets or sets the correlation id.
        /// </summary>
        /// <value>The correlation id.</value>
        [DataMember]
        public string CorrelationId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorData"/> class.
        /// </summary>
        public SensorData()
        {
            GeneratedWhen = DateTime.Now;
            Value = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorData"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public SensorData(string value)
        {
            Value = value;
            GeneratedWhen = DateTime.Now;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SensorData"/> class.
        /// </summary>
        /// <param name="value">The value may not be null.</param>
        public SensorData(SensorData value)
        {
            Value = value.Value;
            GeneratedWhen = value.GeneratedWhen;
            CorrelationId = value.CorrelationId;
        }

        #region Conversion operators (implicit conversion)

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueManagement.SensorData"/> to <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="val">The value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator int(SensorData val)
        {
            return (Convert.ToInt32(val != null ? val.Value : null));
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueManagement.SensorData"/> to <see cref="System.Int64"/>.
        /// </summary>
        /// <param name="val">The val.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator long(SensorData val)
        {
            return (Convert.ToInt64(val != null ? val.Value : null));
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueManagement.SensorData"/> to <see cref="System.Int16"/>.
        /// </summary>
        /// <param name="val">The val.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator short(SensorData val)
        {
            return (Convert.ToInt16(val != null ? val.Value : null));
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueManagement.SensorData"/> to <see cref="System.Boolean"/>.
        /// </summary>
        /// <param name="val">The val.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator bool(SensorData val)
        {
            return (Convert.ToBoolean(val != null ? val.Value : null));
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueManagement.SensorData"/> to <see cref="System.Double"/>.
        /// </summary>
        /// <param name="val">The val.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator double(SensorData val)
        {
            return (Convert.ToDouble(val != null ? val.Value : null));
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueManagement.SensorData"/> to <see cref="System.Decimal"/>.
        /// </summary>
        /// <param name="val">The val.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator decimal(SensorData val)
        {
            return (Convert.ToDecimal(val != null ? val.Value : null));
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueManagement.SensorData"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="val">The val.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator string(SensorData val)
        {
            return (val != null ? val.Value : string.Empty);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="ValueManagement.SensorData"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator SensorData(string value)
        {
            return (new SensorData(value));
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="ValueManagement.SensorData"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator SensorData(int value)
        {
            return (new SensorData(value.ToString()));
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int64"/> to <see cref="ValueManagement.SensorData"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator SensorData(long value)
        {
            return (new SensorData(value.ToString()));
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int16"/> to <see cref="ValueManagement.SensorData"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator SensorData(short value)
        {
            return (new SensorData(value.ToString()));
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Boolean"/> to <see cref="ValueManagement.SensorData"/>.
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator SensorData(bool value)
        {
            return (new SensorData(value.ToString()));
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Decimal"/> to <see cref="ValueManagement.SensorData"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator SensorData(decimal value)
        {
            return (new SensorData(value.ToString()));
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Double"/> to <see cref="ValueManagement.SensorData"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator SensorData(double value)
        {
            return (new SensorData(value.ToString()));
        }

        #endregion

        /// <summary>
        /// The method copies all data from the source object to this one.
        /// </summary>
        public void CopyData(SensorData source)
        {
            Value = source.Value;
            GeneratedWhen = source.GeneratedWhen;
            CorrelationId = source.CorrelationId;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance (which is the value)
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return (Value);
        }

        /// <summary>
        /// Overridden base method.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return GeneratedWhen.GetHashCode();
        }

        /// <summary>
        /// Overridden base method. The equality is checked only for the <see cref="GeneratedWhen"/> values.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            SensorData other = obj as SensorData;
            if (other != null)
            {
                return other.GeneratedWhen == this.GeneratedWhen;
            }
            else
            {
                return true;
            }
        }
    }



    [DataContract(Namespace = "http://GatewaysService")]
    public class SensorDataForDevice : SensorData
    {
        [DataMember]
        public int SensorId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorDataForDevice"/> class.
        /// </summary>
        /// <param name="sensorId">The sensor id.</param>
        /// <param name="generatedWhen">The generated when.</param>
        /// <param name="value">The value.</param>
        /// <param name="correlationId"></param>
        public SensorDataForDevice(int sensorId, DateTime generatedWhen, string value, string correlationId)
            : base(generatedWhen, value, correlationId)
        {
            SensorId = sensorId;
        }

    }
}
