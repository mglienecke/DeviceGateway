using System;

using System.Collections;

namespace DeviceServer.Simulator
{
    /// <summary>
    /// The interface declares methods for device-side server communication handlers.
    /// </summary>
    public interface IServerCommunicationHandler
    {
         /// <summary>
        /// The propery contains the port number to be used by the device for accepting incoming requests.
        /// </summary>
        UInt16 LocalPort { get; set; }

        /// <summary>
        /// The property sets the master server instance.
        /// </summary>
        DeviceServerSimulator Server { set; }

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

        /// <summary>
        /// The method initializes the device server for handling incoming requests.
        /// </summary>
        /// <returns></returns>
        bool Init();

        /// <summary>
        /// The method starts handling incoming requests.
        /// </summary>
        /// <returns><c>True</c> if the start has been successful.</returns>
        bool StartRequestHandling();

        /// <summary>
        /// The method stops handling incoming requests.
        /// </summary>
        /// <returns><c>True</c> if the stop has been successful.</returns>
        bool StopRequestHandling();
    }
}
