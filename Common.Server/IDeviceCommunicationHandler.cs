using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalDataContracts;

namespace Common.Server
{
    /// <summary>
    /// The interface declares properties and methods for components that provide communication services
    /// between the central service server and devices.
    /// </summary>
    public interface IDeviceCommunicationHandler
    {
        /// <summary>
        /// The method retrieves current data of the specified sensor that belongs to the specified device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="sensor"></param>
        /// <returns></returns>
        SensorData GetSensorCurrentData(Device device, Sensor sensor);

        /// <summary>
        /// The method puts the specified data to the specified sensor that belongs to the specified device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="sensor"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        OperationResult PutSensorCurrentData(Device device, Sensor sensor, SensorData data);

        /// <summary>
        /// The property getter returns a flag that defines if the objects of the implementation class are reusable and thread-safe
        /// (or should be instantiated every time before use)
        /// </summary>
        bool IsReusableAndThreadSafe {get;}
    }
}
