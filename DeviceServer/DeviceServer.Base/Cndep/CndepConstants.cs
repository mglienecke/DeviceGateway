using System;
using System.Text;

namespace DeviceServer.Base.Cndep
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
