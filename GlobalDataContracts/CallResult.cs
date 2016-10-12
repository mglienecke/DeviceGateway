using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalDataContracts
{
	/// <summary>
	/// the base class for any result which is used 
	/// </summary>
	[Serializable]
	public class CallResult
	{
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="CallResult"/> is success.
		/// </summary>
		/// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
		public bool Success { get; set; }

		/// <summary>
		/// Gets or sets the error messages.
		/// </summary>
		/// <value>The error messages.</value>
		public List<string> ErrorMessages { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CallResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		/// <param name="errorMessages">The error messages.</param>
		public CallResult(bool success, List<string> errorMessages) : this(success)
		{
			ErrorMessages = errorMessages;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CallResult"/> class with just one error
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		/// <param name="errorMessage">The error message.</param>
		public CallResult(string errorMessage) : this(false)
		{
			ErrorMessages = new List<string>();
			ErrorMessages.Add(errorMessage);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CallResult"/> class.
		/// </summary>
		/// <param name="success">if set to <c>true</c> [success].</param>
		public CallResult(bool success)
		{
			Success = success;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CallResult"/> class where everything is fine
		/// </summary>
		public CallResult()
		{
			Success = true;
			ErrorMessages = null;
		}

		/// <summary>
		/// Adds the error message to the list and sets the status to false
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns>the original message</returns>
		public string AddError(string message)
		{
			Success = false;
			if (ErrorMessages == null)
			{
				ErrorMessages = new List<string>();
			}

			ErrorMessages.Add(message);
			return (message);
		}

		/// <summary>
		/// Adds an error with formatting option
		/// </summary>
		/// <param name="formatString">The format string.</param>
		/// <param name="data">The data.</param>
		/// <returns>the formatted string</returns>
		public string AddErrorFormat(string formatString, params object [] data)
		{
			return (AddError(string.Format(formatString, data)));
		}
	}
}
