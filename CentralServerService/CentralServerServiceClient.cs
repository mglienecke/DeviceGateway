using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;

using GlobalDataContracts;

namespace CentralServerService
{
    /// <summary>
    /// The class implements a client-side singleton for accessing the Central Service instace.
    /// </summary>
	public class CentralServerServiceClient
	{
		private static CentralServerServiceClient instance;
		private static ICentralServerService service;
		private static bool useRemoting = true;

		private static object lockObject = new object();

		/// <summary>
		/// Gets or sets a value indicating whether remoting shall be used
		/// </summary>
		/// <value><c>true</c> if [use remoting]; otherwise, <c>false</c>.</value>
		public static bool UseRemoting
		{
			get { return (useRemoting); }
			set { useRemoting = value; }
		}

		/// <summary>
		/// Gets the current instance or creates a new instance
		/// </summary>
		/// <value>The current.</value>
		public static CentralServerServiceClient Instance
		{
			get
			{
				lock (lockObject)
				{
					if (instance == null)
					{
						instance = new CentralServerServiceClient();
					}

					return (instance);
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CentralServerServiceClient"/> class. This could be either a remote or a local implementation depending on the flag 
		/// </summary>
		public CentralServerServiceClient()
		{
			if (UseRemoting)
			{
                if (RemotingConfiguration.ApplicationName == null)
                {
                    RemotingConfiguration.Configure(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, false);
                }
			}
			service = new CentralServerServiceImpl();			
		}

		/// <summary>
		/// Gets the service.
		/// </summary>
		/// <value>The service.</value>
		public ICentralServerService Service
		{
			get { return (service); }
		}
	}
}
