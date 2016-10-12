using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValueManagement
{
	/// <summary>
	/// represents an internal data value in the system
	/// </summary>
	public class ValueData
	{
		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		public string Value { get; set; }

		/// <summary>
		/// Gets or sets the created timestamp
		/// </summary>
		/// <value>The created.</value>
		public DateTime Created { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueData"/> class.
		/// </summary>
		public ValueData()
		{
			Created = DateTime.Now;
			Value = string.Empty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueData"/> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public ValueData(string value)
		{
			Value = value;
			Created = DateTime.Now;
		}

		#region Conversion operators (implicit conversion)
		
		/// <summary>
		/// Performs an implicit conversion from <see cref="ValueManagement.ValueData"/> to <see cref="System.Int32"/>.
		/// </summary>
		/// <param name="val">The value to convert.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator int(ValueData val)
		{
			return (Convert.ToInt32(val != null ? val.Value : null));
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="ValueManagement.ValueData"/> to <see cref="System.Int64"/>.
		/// </summary>
		/// <param name="val">The val.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator long(ValueData val)
		{
			return (Convert.ToInt64(val != null ? val.Value : null));
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="ValueManagement.ValueData"/> to <see cref="System.Int16"/>.
		/// </summary>
		/// <param name="val">The val.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator short(ValueData val)
		{
			return (Convert.ToInt16(val != null ? val.Value : null));
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="ValueManagement.ValueData"/> to <see cref="System.Boolean"/>.
		/// </summary>
		/// <param name="val">The val.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator bool(ValueData val)
		{
			return (Convert.ToBoolean(val != null ? val.Value : null));
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="ValueManagement.ValueData"/> to <see cref="System.Double"/>.
		/// </summary>
		/// <param name="val">The val.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator double(ValueData val)
		{
			return (Convert.ToDouble(val != null ? val.Value : null));
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="ValueManagement.ValueData"/> to <see cref="System.Decimal"/>.
		/// </summary>
		/// <param name="val">The val.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator decimal(ValueData val)
		{
			return (Convert.ToDecimal(val != null ? val.Value : null));
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="ValueManagement.ValueData"/> to <see cref="System.String"/>.
		/// </summary>
		/// <param name="val">The val.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator string(ValueData val)
		{
			return (val != null ? val.Value : string.Empty);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="System.String"/> to <see cref="ValueManagement.ValueData"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator ValueData(string value)
		{
			return (new ValueData(value));
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="ValueManagement.ValueData"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator ValueData(int value)
		{
			return (new ValueData(value.ToString()));
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="System.Int64"/> to <see cref="ValueManagement.ValueData"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator ValueData(long value)
		{
			return (new ValueData(value.ToString()));
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="System.Int16"/> to <see cref="ValueManagement.ValueData"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator ValueData(short value)
		{
			return (new ValueData(value.ToString()));
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="System.Boolean"/> to <see cref="ValueManagement.ValueData"/>.
		/// </summary>
		/// <param name="value">if set to <c>true</c> [value].</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator ValueData(bool value)
		{
			return (new ValueData(value.ToString()));
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="System.Decimal"/> to <see cref="ValueManagement.ValueData"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator ValueData(decimal value)
		{
			return (new ValueData(value.ToString()));
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="System.Double"/> to <see cref="ValueManagement.ValueData"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>The result of the conversion.</returns>
		public static implicit operator ValueData(double value)
		{
			return (new ValueData(value.ToString()));
		}

		#endregion

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
	}
}
