using System;
using Microsoft.SPOT;
using System.Collections;

namespace DeviceServer.Base
{
    /// <summary>
    /// The interface declares methods for device-side sensor data exchange communication handlers.
    /// </summary>
    public interface ISensorDataExchangeCommunicationHandler
    {
        /// <summary>
        /// The property contains a flag showing if the handler is initialized.
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// The method calls the server to store data for the sensor with the passed id.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="device"></param>
        /// <param name="sensor"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        bool StoreSensorData(DeviceConfig config, Device device, Sensor sensor, SensorData data);

        /// <summary>
        /// The method initializes the device server for handling incoming requests.
        /// </summary>
        /// <returns></returns>
        bool Init();

        /// <summary>
        /// The method starts handling incoming requests.
        /// </summary>
        bool StartRequestHandling();

        /// <summary>
        /// The method stops handling incoming requests.
        /// </summary>
        void StopRequestHandling();

        /// <summary>
        /// The property returns if the comm handler is in the request-handling state.
        /// </summary>
        bool IsHandlingRequests { get; }
    }

    /// <summary>
    /// The interface declares methods for device-side server communication handlers.
    /// </summary>
    public interface IServerCommunicationHandler : ISensorDataExchangeCommunicationHandler
    {
        /// <summary>
        /// The method calls the server to register the passed device.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        bool RegisterDevice(DeviceConfig config, Device device);

        /// <summary>
        /// The method calls the server to register the passed sensors.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="device"></param>
        /// <param name="sensors"></param>
        /// <returns></returns>
        bool RegisterSensors(DeviceConfig config, Device device, ArrayList sensors);
    }
}
