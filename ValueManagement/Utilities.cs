using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;

namespace ValueManagement
{
	public static class Utilities
	{
		/// <summary>
		/// Logs the exception.
		/// </summary>
		/// <param name="methodName">Name of the method.</param>
		/// <param name="message">The message.</param>
		/// <param name="exc">The exc.</param>
		public static void LogException(this ILog log, string methodName, string message, Exception exc)
		{
			// log the information
			log.ErrorFormat(Properties.Resources.ErrorExceptionOccured, methodName, message, exc.Message, exc.StackTrace);
		}
	}
}
