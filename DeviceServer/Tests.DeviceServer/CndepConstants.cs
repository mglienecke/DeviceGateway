using System;
using System.Text;

namespace Tests.DeviceServer
{
    /// <summary>
    /// The class contains constants for the CNDEP Device Protocol implementation.
    /// </summary>
    public class CndepConstants
    {
        /// <summary>
        /// Command
        /// </summary>
        public const byte CmdGetSensorData = 1;
        /// <summary>
        /// Command
        /// </summary>
        public const byte CmdStoreSensorData = 2;
        /// <summary>
        /// Command
        /// </summary>
        public const byte CmdRegisterDevice = 3;
        /// <summary>
        /// Command
        /// </summary>
        public const byte CmdRegisterSensors = 4;
        /// <summary>
        /// Command
        /// </summary>
        public const byte CmdGetSensors = 5;
        /// <summary>
        /// Command
        /// </summary>
        public const byte CmdGetSensorsValues = 6;
        /// <summary>
        /// Command
        /// </summary>
        public const byte CmdGetErrorLog = 7;
        /// <summary>
        /// Command
        /// </summary>
        public const byte CmdPutSensorValue = 8;
        /// <summary>
        /// Command
        /// </summary>
        public const byte CmdPutDeviceConfig = 9;
        /// <summary>
        /// Command
        /// </summary>
        public const byte CmdGetDeviceConfig = 10;
        /// <summary>
        /// Command function
        /// </summary>
        public const byte FncGetSensorDataCurrentValue = 1;
        /// <summary>
        /// COmmand funcation
        /// </summary>
        public const byte FncGetSensorDataAllValues = 2;
        /// <summary>
        /// Response
        /// </summary>
        public const byte RspOK = 0;
        /// <summary>
        /// Response
        /// </summary>
        public const byte RspError = 1;
    }
}
