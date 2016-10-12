using System;



namespace DeviceServer.Simulator
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
                throw new Exception(String.Format(Properties.Resources.ExceptionTypeNotFound, typeName));
            }

            try
            {
                return type.GetConstructor(new Type[0]).Invoke(new object[0]);
            }
            catch (Exception exc)
            {
                throw new Exception(String.Format(Properties.Resources.ExceptionTypeInstantiationFailed, exc.Message));
            }
        }
    }
}
