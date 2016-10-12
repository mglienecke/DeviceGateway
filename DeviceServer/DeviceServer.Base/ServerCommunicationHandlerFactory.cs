using System;
using Microsoft.SPOT;
using System.Collections;
using NetMf.CommonExtensions;
using Gsiot.Server;
using DeviceServer.Base.Cndep;

namespace DeviceServer.Base
{
    /// <summary>
    /// The class implements factory for <see cref="IServerCommunicationHandler"/> implementations.
    /// </summary>
    internal sealed class ServerCommunicationHandlerFactory
    {
        private static readonly Hashtable mServerCommunicationHandlers = new Hashtable();
        private static readonly string[][] mServerCommunicationHandlerClassNames = new string[][] { 
            new string[] {CommunicationConstants.ServerCommTypeHttpRest, typeof(HttpRestServerCommunicationHandler).AssemblyQualifiedName},
            new string[] {CommunicationConstants.ServerCommTypeUdpCndep, typeof(CndepServerCommunicationHandler).AssemblyQualifiedName}};

        private static readonly string[][] mSensorDataExchangeCommunicationHandlerClassNames = new string[][] { 
            new string[] {CommunicationConstants.ServerCommTypeHttpRest, typeof(HttpRestServerCommunicationHandler).AssemblyQualifiedName},
            new string[] {CommunicationConstants.ServerCommTypeUdpCndep, typeof(CndepServerCommunicationHandler).AssemblyQualifiedName}};

        private ServerCommunicationHandlerFactory()
        {
        }

        /// <summary>
        /// The method returns a server communication handler instance for the passed content type.
        /// </summary>
        /// <param name="serverCommType"></param>
        /// <returns></returns>
        public static IServerCommunicationHandler GetServerCommunicationHandler(string serverCommType)
        {
            IServerCommunicationHandler handler = null;

            //Lazy initialization
            if (mServerCommunicationHandlers.Contains(serverCommType))
            {
                handler = mServerCommunicationHandlers[serverCommType] as IServerCommunicationHandler;
            }
            else
            {
                for (int i = 0; i < mServerCommunicationHandlerClassNames.Length; i++)
                {
                    if (serverCommType == mServerCommunicationHandlerClassNames[i][0])
                    {
                        handler = (IServerCommunicationHandler)Utils.CreateObject(mServerCommunicationHandlerClassNames[i][1], new Type[]{typeof(DeviceServerBase)}, new object[]{DeviceServerBase.RunningInstance});
                        handler.Init();
                        mServerCommunicationHandlers[serverCommType] = handler;
                        break;
                    }
                }
            }

            //Found it?
            if (handler == null)
            {
                throw new Exception(StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ExceptionServerCommTypeNotSupported), serverCommType));
            }

            return handler;
        }

        /// <summary>
        /// The method returns a sensor data exchange communication handler instance for the passed content type.
        /// </summary>
        /// <param name="serverCommType"></param>
        /// <returns></returns>
        public static ISensorDataExchangeCommunicationHandler GetSensorDataExchangeCommunicationHandler(string serverCommType)
        {
            ISensorDataExchangeCommunicationHandler handler = null;

            //Lazy initialization
            if (mServerCommunicationHandlers.Contains(serverCommType))
            {
                handler = mServerCommunicationHandlers[serverCommType] as ISensorDataExchangeCommunicationHandler;
            }
            else
            {
                for (int i = 0; i < mSensorDataExchangeCommunicationHandlerClassNames.Length; i++)
                {
                    if (serverCommType == mSensorDataExchangeCommunicationHandlerClassNames[i][0])
                    {
                        handler = (ISensorDataExchangeCommunicationHandler)Utils.CreateObject(mSensorDataExchangeCommunicationHandlerClassNames[i][1], new Type[] { typeof(DeviceServerBase) }, new object[] { DeviceServerBase.RunningInstance });
                        handler.Init();
                        mServerCommunicationHandlers[serverCommType] = handler;
                        break;
                    }
                }
            }

            //Found it?
            if (handler == null)
            {
                throw new Exception(StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ExceptionServerCommTypeNotSupported), serverCommType));
            }

            return handler;
        }

        /// <summary>
        /// The method permanently cleans up all currenlty contained handlers.
        /// </summary>
        public static void Shutdown()
        {
            mServerCommunicationHandlers.Clear();
        }
    }
}
