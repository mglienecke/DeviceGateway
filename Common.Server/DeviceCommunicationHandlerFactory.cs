using System;
using System.Collections.Generic;

namespace Common.Server
{
    /// <summary>
    /// The class implements a factory for <see cref="IDeviceCommunicationHandler"/> instances.
    /// </summary>
    public class DeviceCommunicationHandlerFactory
    {
        #region Configuration properties...
        /// <summary>
        /// Configuration property name.
        /// </summary>
        public const string CfgDeviceCommunicationHandlerTypeNamePrefix = "DeviceCommunicationHandlerTypeName.";
        /// <summary>
        /// Default configuration property value.
        /// </summary>
        public static readonly string DefaultDeviceCommunicationHandlerTypeName = typeof(HttpRestDeviceCommunicationHandler).AssemblyQualifiedName;
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly static Dictionary<string, IDeviceCommunicationHandler> DeviceCommHandlers = new Dictionary<string, IDeviceCommunicationHandler>();


        private DeviceCommunicationHandlerFactory(){}

        /// <summary>
        /// The method return a device communication handler instance corresponding to the passed communication type name (<see cref="GlobalDataContracts.PullModeCommunicationType"/>)
        /// </summary>
        /// <param name="communicationType"></param>
        /// <returns></returns>
        public static IDeviceCommunicationHandler GetDeviceCommunicationHandler(string communicationType)
        {
            IDeviceCommunicationHandler result;

            //Already instantiated?
            if (DeviceCommHandlers.TryGetValue(communicationType, out result))
            {
                return result;
            }

            //Instantiate
            try
            {
                string appSettingsKeyName = CfgDeviceCommunicationHandlerTypeNamePrefix + communicationType;
                result = Utilities.CreateObjectByAppSettingsKeyName<IDeviceCommunicationHandler>(appSettingsKeyName);
                //Store if reusable
                if (result.IsReusableAndThreadSafe)
                {
                    lock (DeviceCommHandlers)
                    {
                        DeviceCommHandlers[communicationType] = result;
                    }
                }
                return result;
            }
            catch (Exception exc)
            {
                throw new Exception(String.Format(Properties.Resources.ExceptionFailedCreatingIDeviceCommunicationHandlerInstance, communicationType, exc.Message), exc);
            }
        }
    }
}
