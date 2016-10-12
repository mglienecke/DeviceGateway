using System;
using Microsoft.SPOT;
using NetMf.CommonExtensions;

namespace DeviceServer.Base
{
    /// <summary>
    /// The class implements various utility methods.
    /// </summary>
    internal sealed class Utils
    {
        private Utils()
        {
        }

        /// <summary>
        /// The method creates an object instance by its class name.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        internal static object CreateObject(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                throw new Exception(StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ExceptionTypeNotFound), typeName));
            }

            try
            {
                return type.GetConstructor(new Type[0]).Invoke(new object[0]);
            }
            catch (Exception exc)
            {
                throw new Exception(StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ExceptionTypeInstantiationFailed), exc.Message));
            }
        }

        /// <summary>
        /// The method creates an object instance by its class name.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        internal static object CreateObject(string typeName, Type[] paramTypes, object[] parameters)
        {
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                throw new Exception(StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ExceptionTypeNotFound), typeName));
            }

            try
            {
                return type.GetConstructor(paramTypes).Invoke(parameters);
            }
            catch (Exception exc)
            {
                throw new Exception(StringUtility.Format(Properties.Resources.GetString(Properties.Resources.StringResources.ExceptionTypeInstantiationFailed), exc.Message));
            }
        }
    }
}
