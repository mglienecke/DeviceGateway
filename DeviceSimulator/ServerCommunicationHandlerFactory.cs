using System;

using System.Collections;

using Gsiot.Server;
using DeviceServer.Simulator;

namespace DeviceServer.Simulator
{
    /// <summary>
    /// The class implements factory for <see cref="IServerCommunicationHandler"/> implementations.
    /// </summary>
    public sealed class ServerCommunicationHandlerFactory
    {
        private static readonly Hashtable mServerCommunicationHandlers = new Hashtable();
        private static readonly string[][] mServerCommunicationHandlerClassNames = new string[][] { 
            new string[] {CommunicationConstants.ServerCommTypeHttpRest, typeof(HttpRestServerCommunicationHandler).AssemblyQualifiedName},
            new string[] {CommunicationConstants.ServerCommTypeCndep, typeof(CndepServerCommunicationHandler).AssemblyQualifiedName}};

        private ServerCommunicationHandlerFactory()
        {
        }

        /// <summary>
        /// The method returns a content parser instance for the passed content type.
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static IServerCommunicationHandler GetServerCommunicationHandler(DeviceServerSimulator server, string serverCommType)
        {
            IServerCommunicationHandler handler = null;

            //Lazy initialization
            if (mServerCommunicationHandlers.Contains(serverCommType))
            {
                handler = (IServerCommunicationHandler)mServerCommunicationHandlers[serverCommType];
            }
            else
            {
                for (int i = 0; i < mServerCommunicationHandlerClassNames.Length; i++)
                {
                    if (serverCommType == mServerCommunicationHandlerClassNames[i][0])
                    {
                        handler = (IServerCommunicationHandler)Utils.CreateObject(mServerCommunicationHandlerClassNames[i][1]);
                        handler.Server = server;
                        mServerCommunicationHandlers[serverCommType] = handler;
                        break;
                    }
                }

                //Found it?
                if (handler == null)
                {
                    throw new Exception(String.Format(Properties.Resources.ExceptionServerCommTypeNotSupported, serverCommType));
                }
            }

            return handler;
        }
    }
}
