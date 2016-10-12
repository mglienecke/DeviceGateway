using System;
using System.Text;

namespace GlobalDataContracts
{
	/// <summary>
	/// a class to transport data to and from a service
	/// </summary>
	[Serializable]
	public class BinaryTransport
	{
		private byte command;
		private object data;

		/// <summary>
		/// Gets or sets the command.
		/// </summary>
		/// <value>
		/// The command.
		/// </value>
		public byte Command
		{
			get { return (command); }
			set { command = value;  }
		}

		/// <summary>
		/// Gets or sets the data.
		/// </summary>
		/// <value>
		/// The data.
		/// </value>
		public object Data
		{
			get { return (data); }
			set { data = value;  }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BinaryTransport"/> class.
		/// </summary>
		/// <param name="command">The command.</param>
		/// <param name="data">The data.</param>
		public BinaryTransport(byte command, object data)
		{
			Command = command;
			Data = data;
		}
	}
}
