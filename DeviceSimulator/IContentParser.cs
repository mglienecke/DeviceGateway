using System;

using System.Collections;

namespace DeviceServer.Simulator
{
    /// <summary>
    /// The interface declares methods for creating response content strings content or parsing request content strings. The string may contain, for example, XML or JSON data.
    /// </summary>
    public interface IContentParser
    {
        /// <summary>
        /// The method parses a response content containing a serialized <see cref="OperationResult"/> object.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        OperationResult ParseOperationResult(string content);

        /// <summary>
        /// The method parses a <see cref="SensorData"/> object out of the passed content string.
        /// </summary>
        /// <param name="content"></param>
        /// <returns><c>null</c> if no SensorData object can be parsed.</returns>
        SensorData ParseSensorData(string content);

        /// <summary>
        /// The method parses a <see cref="PutActuatorDataRequest"/> object out of the passed content string.
        /// </summary>
        /// <param name="content"></param>
        /// <returns><c>null</c> if no PutActuatorDataRequest object can be parsed.</returns>
        PutActuatorDataRequest ParsePutActuatorDataRequest(string content);

        /// <summary>
        /// The method creates response content for the sensors registration data request. 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="sensors"></param>
        /// <returns></returns>
        string CreateGetSensorsResponseContent(string deviceId, ArrayList sensors);

        /// <summary>
        /// The method creates response content for the error log request.
        /// </summary>
        /// <param name="errorLog"></param>
        /// <returns></returns>
        string CreateGetErrorLogResponseContent(ICollection errorLog);

        /// <summary>
        /// The method creates response content for teh cases when a request fails, and an error message has to be sent back. 
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        string CreateErrorResponseContent(string errorMessage);

        /// <summary>
        /// The method creates response content for the current sensor data request.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        string CreateGetCurrentSensorDataResponseContent(SensorData data, SensorValueDataType dataType);

        /// <summary>
        /// The method creates response content for the last sensor data list request.
        /// </summary>
        /// <param name="sensor"></param>
        /// <returns></returns>
        string CreateGetLastSensorValuesResponse(Sensor sensor);

        /// <summary>
        /// The method creates response content for the all device's sensors last data list request
        /// </summary>
        /// <param name="sensors"></param>
        /// <returns></returns>
        string CreateGetLastSensorsValuesResponse(ArrayList sensors);

        /// <summary>
        /// The method creates request content for the sensor registration renewal request.
        /// </summary>
        /// <param name="sensors"></param>
        /// <returns></returns>
        string CreatePutSensorsRequestContent(ArrayList sensors);

        /// <summary>
        /// The method creates request content for the device registration renewal request.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        string CreatePutDeviceRequestContent(Device device);

        /// <summary>
        /// The method creates response content for getting the device's config data.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        string CreateGetDeviceConfigResponseContent(DeviceConfig config);

        /// <summary>
        /// The method creates request content for putting sensor data to the server.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        string PutSensorDataRequest(SensorData[] data, SensorValueDataType dataType);

        /// <summary>
        /// The method creates request content for putting multiple sensor data to the server.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        string PutSensorDataRequest(MultipleSensorData[] data);

        /// <summary>
        /// The method parses the request content for the encoded <see cref="DeviceConfig"/> object.
        /// </summary>
        /// <param name="requestContent"></param>
        /// <returns></returns>
        DeviceConfig ParseDeviceConfig(string requestContent);

        /// <summary>
        /// The method creates response content with encoded <see cref="OperationResult"/> object.
        /// </summary>
        /// <param name="success"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        string CreateOperationResultResponse(bool success, string message);

        /// <summary>
        /// The property getter returns the MIME content type supported by specific parser implementation.
        /// </summary>
        string MimeContentType { get; }
    }
}
