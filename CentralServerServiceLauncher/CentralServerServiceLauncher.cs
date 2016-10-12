using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.Configuration;
using System.Globalization;
using System.Diagnostics;
using System.Threading;
using System.Runtime.Remoting;

using log4net;
using System.IO;

namespace CentralServerServiceLauncher
{
	public class CentralServerServiceStart : ServiceBase
	{
		// Create a logger for use in this class
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly object mExitLockMonitor = new object();
		private static Thread mConsoleServerManagementThread;
		
		private static CentralServerServiceStart mServer;

		private System.Threading.Timer timerGC;
		private bool mIsService = true;
		private bool shutdownPerformed;


		/// <summary>
		/// Configuration property name. Value type: boolean.
		/// </summary>
		internal const string CFG_RUN_AS_PROCESS = "Server.run.as.process";
		internal const string CFG_RUN_MULTIPLE_INSTANCES = "Server.run.multiple.instances";
		internal const string PROCESS_NAME = "CentralServerService";

		internal const string EXIT_COMMAND = "EXIT";

		/// <summary>
		/// The main entry point of the Server application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			log4net.Config.XmlConfigurator.Configure();

			mServer = new CentralServerServiceStart();
			//mServer.timerGC = new System.Threading.Timer((state) => {Console.WriteLine("Total memory: " + System.GC.GetTotalMemory(false));} , null, new TimeSpan(0, 1, 0), new TimeSpan(0, 1, 0));

            try
            {
                //IsService?
                mServer.mIsService = !Convert.ToBoolean(ConfigurationManager.AppSettings[CFG_RUN_AS_PROCESS], CultureInfo.InvariantCulture);

                //Service or process?
                if (mServer.mIsService)
                {
                    //Log
                    log.Info(Properties.Resources.InfoStartingCentralService);
                    //Start the service
                    ServiceBase.Run(mServer);
                    //Log
                    log.Info(Properties.Resources.InfoCentralServiceStarted);
                }
                else
                {
                    bool useMultipleInstances = ConfigurationManager.AppSettings[CFG_RUN_MULTIPLE_INSTANCES] == null ? false : Convert.ToBoolean(ConfigurationManager.AppSettings[CFG_RUN_MULTIPLE_INSTANCES], CultureInfo.InvariantCulture);

                    //Run as process
                    Process[] currentProcesses = Process.GetProcessesByName(PROCESS_NAME);
                    if ((currentProcesses.Length == 0) || useMultipleInstances)
                    {
                        currentProcesses = null;
                        // only one process
                        try
                        {
                            //Log
                            log.Info(Properties.Resources.InfoStartingCentralService);
                            //Start
                            mServer.OnStart(null);
                            //Log
                            log.Info(Properties.Resources.InfoCentralServiceStarted);

                            Process.GetCurrentProcess().EnableRaisingEvents = true;
                            Process.GetCurrentProcess().Exited +=
                                (sender, evArgs) =>
                                {
                                    if (!mServer.shutdownPerformed)
                                    {
                                        mServer.OnStop();
                                    }
                                };

                            lock (mExitLockMonitor)
                            {
                                //Console management
                                mConsoleServerManagementThread = new Thread(() =>
                                    {
                                        //Wait until exit command is entered.
                                        System.Console.WriteLine(Properties.Resources.ExitInvitation);

                                        string readLine;
                                        while (true)
                                        {
                                            if (((readLine = System.Console.ReadLine()) == null) || (readLine.ToUpper() == EXIT_COMMAND))
                                            {
                                                lock (mExitLockMonitor)
                                                {
                                                    Monitor.PulseAll(mExitLockMonitor);
                                                }
                                            }
                                        }
                                    });

                                mConsoleServerManagementThread.Start();

                                // now we are going to wait until the user presses EXIT
                                Monitor.Wait(mExitLockMonitor);
                            }
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            try
                            {
                                mServer.OnStop();
                            }
                            catch (Exception exc)
                            {
                                log.Error(Properties.Resources.ErrorFailedStoppingServer, exc);
                                System.Console.WriteLine(Properties.Resources.ErrorFailedStoppingServer);
                                System.Console.WriteLine(exc.ToString());
                                Console.ReadLine();
                                Environment.Exit(-1);
                            }
                        }
                    }
                    else
                    {
                        System.Console.WriteLine(Properties.Resources.ErrorProcessInstanceRunning);
                        Console.ReadLine();
                        Environment.Exit(1);
                    }
                }

            }
            catch (Exception exc)
            {
                log.Error(Properties.Resources.ErrorFailedStartingServer, exc);
                System.Console.WriteLine(Properties.Resources.ErrorFailedStartingServer);
                System.Console.WriteLine(exc.ToString());
                Console.ReadLine();
                return;
            }
            finally
            {
                Environment.Exit(0);
            }
		}


		#region Service control methods...

		/// <summary>
		/// Called on the Windows service start
		/// </summary>
		/// <param name="args">arguments</param>
		protected override void OnStart(string[] args)
		{
			RemotingConfiguration.Configure(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, false);

            //Initiate the instance 
            //CentralServerService.CentralServerServiceImpl.GetInstance().GetDevices();
            RemotingServices.Marshal((MarshalByRefObject)CentralServerService.CentralServerServiceImpl.GetInstance(),
                            "RemoteCentralServerService", typeof(CentralServerService.ICentralServerService));
		}

		/// <summary>
		/// Called on stop. If there is an instance of the service running bring it down gracefully
		/// </summary>
		protected override void OnStop()
		{
            //Log
            log.Info(Properties.Resources.InfoStoppingCentralService);

            //Stop if the ref is still there
			if (CentralServerService.CentralServerServiceImpl.Instance != null)
			{
				CentralServerService.CentralServerServiceImpl.Instance.CancelService();
			}

            //Log
            log.Info(Properties.Resources.InfoCentralServiceStopped);
		}

		/// <summary>
		/// Called on system shutdown
		/// </summary>
		protected override void OnShutdown()
		{
			OnStop();
			shutdownPerformed = true;
		}


		#endregion
	}
}
