//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.18444
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DeviceServer.Base.Properties
{
    
    internal partial class Resources
    {
        private static System.Resources.ResourceManager manager;
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if ((Resources.manager == null))
                {
                    Resources.manager = new System.Resources.ResourceManager("DeviceServer.Base.Properties.Resources", typeof(Resources).Assembly);
                }
                return Resources.manager;
            }
        }
        internal static Microsoft.SPOT.Font GetFont(Resources.FontResources id)
        {
            return ((Microsoft.SPOT.Font)(Microsoft.SPOT.ResourceUtility.GetObject(ResourceManager, id)));
        }
        internal static string GetString(Resources.StringResources id)
        {
            return ((string)(Microsoft.SPOT.ResourceUtility.GetObject(ResourceManager, id)));
        }
        [System.SerializableAttribute()]
        internal enum StringResources : short
        {
            ExceptionMissingJSONSensorConfigProperty = -31542,
            CndepResponseId = -29176,
            HttpResponseStatusWithMessage = -25974,
            ErrorSettingSensorValueNotSupported = -25447,
            ErrorMissingRequestContent = -24946,
            CndepResponseStatusWithMessage = -21146,
            ErrorFailedSendingSensorDataOverCndep = -19081,
            ErrorNoSensorConfigurationFound = -18482,
            ErrorValueNotProvidedInRequest = -15186,
            ErrorFailedSendingSensorDataOverHttp = -13718,
            ErrorFailedLoadingConfigFileFromRemovableMedia = -9522,
            ExceptionContentParserTypeNotFound = -9228,
            ExceptionTypeInstantiationFailed = -5865,
            ExceptionTypeNotFound = -5355,
            CfgGatewayServiceUrl = -4983,
            SocketStatusWithMessage = -1481,
            ErrorUnknownCndepCommand = 641,
            ExceptionServerCommTypeNotSupported = 1582,
            ErrorPullModeNotSupported = 1883,
            ErrorFailedRefsreshingDeviceSensorsRegistration = 5484,
            ErrorUnsupportedResponseContentType = 8038,
            NullResponse = 8218,
            ErrorNoSensorIdInRequest = 12904,
            ExceptionContentTypeNotSupported = 13749,
            ErrorUnableRefreshDeviceRegistrationNoServerUrl = 15196,
            ExceptionMissingJSONDeviceConfigProperty = 17174,
            TextStart = 20017,
            ExceptionFailedSettingUpSensorPorts = 21321,
            TextCndepRequestArrived = 23818,
            ErrorFailedRegisteringDeviceOverCndep = 23885,
            ErrorSensorNotFound = 24312,
            ErrorFailedRegisteringSensorsOverCndep = 24880,
            HttpResponseStatus = 25715,
            ExceptionFailedParsingJsonOperationResult = 26825,
            ErrorUnknownCndepCommandFunction = 28615,
            ErrorFailedRefsreshingDeviceRegistration = 32312,
        }
        [System.SerializableAttribute()]
        internal enum FontResources : short
        {
            small = 13070,
        }
    }
}