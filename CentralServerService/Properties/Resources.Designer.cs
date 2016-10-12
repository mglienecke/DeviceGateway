﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CentralServerService.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CentralServerService.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cancel is not allowed for objects which are not cancellable.
        /// </summary>
        internal static string CancelNotAllowed {
            get {
                return ResourceManager.GetString("CancelNotAllowed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Changing the value is not allowed for objects which are not modifiable.
        /// </summary>
        internal static string ChangeValueNotAllowed {
            get {
                return ResourceManager.GetString("ChangeValueNotAllowed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed when activating a sensor data sink. Internal sensor id: {0}; Data sink type: {1}..
        /// </summary>
        internal static string ErrorActivatingDataSink {
            get {
                return ResourceManager.GetString("ErrorActivatingDataSink", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The device {0} has been registered already.
        /// </summary>
        internal static string ErrorDeviceAlreadyRegistered {
            get {
                return ResourceManager.GetString("ErrorDeviceAlreadyRegistered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The device id may not been null.
        /// </summary>
        internal static string ErrorDeviceIdIsNull {
            get {
                return ResourceManager.GetString("ErrorDeviceIdIsNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The device may not been null.
        /// </summary>
        internal static string ErrorDeviceIsNull {
            get {
                return ResourceManager.GetString("ErrorDeviceIsNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The device {0} is not registered.
        /// </summary>
        internal static string ErrorDeviceNotRegistered {
            get {
                return ResourceManager.GetString("ErrorDeviceNotRegistered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to In method {0} an exception occured: {1}
        ///{2}
        ///{3}.
        /// </summary>
        internal static string ErrorExceptionOccured {
            get {
                return ResourceManager.GetString("ErrorExceptionOccured", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed adding sensor dependency. Base sensor internal id: {0}; Dependent sensor internal id: {1}..
        /// </summary>
        internal static string ErrorFailedAddingSensorDependency {
            get {
                return ResourceManager.GetString("ErrorFailedAddingSensorDependency", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed getting communication handler for sensor. Device id: {0}; Sensor id: {1}..
        /// </summary>
        internal static string ErrorFailedGettingCommunicationHandlerForSensor {
            get {
                return ResourceManager.GetString("ErrorFailedGettingCommunicationHandlerForSensor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed scanning sensor. Device id: {0}; Sensor id: {1}; Error: {2}.
        /// </summary>
        internal static string ErrorFailedScanningSensor {
            get {
                return ResourceManager.GetString("ErrorFailedScanningSensor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed scanning sensor data. Device id: {0}; Sensor id: {1}..
        /// </summary>
        internal static string ErrorFailedScanningSensorData {
            get {
                return ResourceManager.GetString("ErrorFailedScanningSensorData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed sending sensor data to sensor. Device: {0}; Sensor: {1}..
        /// </summary>
        internal static string ErrorFailedSettingSensorData {
            get {
                return ResourceManager.GetString("ErrorFailedSettingSensorData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed sending sensor data to sensor. Device: {0}; Sensor: {1}; Data: {2}; Error: {3}..
        /// </summary>
        internal static string ErrorFailedSettingSensorDataWithDetails {
            get {
                return ResourceManager.GetString("ErrorFailedSettingSensorDataWithDetails", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sensor Data for Device {0} (Sensor list: {1}) from {2} until {3} with maximum results {4} could not be retrieved. Error: {5}.
        /// </summary>
        internal static string ErrorGetSensorData {
            get {
                return ResourceManager.GetString("ErrorGetSensorData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sensor Data for Device {0} (Sensor list: {1}) with maximum results {2} could not be retrieved. Error: {3}.
        /// </summary>
        internal static string ErrorGetSensorDataLatest {
            get {
                return ResourceManager.GetString("ErrorGetSensorDataLatest", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occured while determining which value shall be written: {0}.
        /// </summary>
        internal static string ErrorGettingValuesToWrite {
            get {
                return ResourceManager.GetString("ErrorGettingValuesToWrite", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The identifier {0} is not valid.
        /// </summary>
        internal static string ErrorIdentifierIsInvalid {
            get {
                return ResourceManager.GetString("ErrorIdentifierIsInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occured while initializing the central service.
        /// </summary>
        internal static string ErrorInitializingCentralService {
            get {
                return ResourceManager.GetString("ErrorInitializingCentralService", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The data item {0} for sensor {1} for device {2} has an invalid format.
        /// </summary>
        internal static string ErrorInvalidDataFormatForData {
            get {
                return ResourceManager.GetString("ErrorInvalidDataFormatForData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No ValueDefinition object has been found for the passed sensor. Device id: {0}; sensor id: {1}..
        /// </summary>
        internal static string ErrorNoValueDefinitionForSensor {
            get {
                return ResourceManager.GetString("ErrorNoValueDefinitionForSensor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Null sensor data passed in the request. Device: {0}; Sensor: {1}..
        /// </summary>
        internal static string ErrorNullSensorDataPassedInRequest {
            get {
                return ResourceManager.GetString("ErrorNullSensorDataPassedInRequest", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Device {0} could not be registered. Error: {1}..
        /// </summary>
        internal static string ErrorRegisteringDevice {
            get {
                return ResourceManager.GetString("ErrorRegisteringDevice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sensor {0} could not be registered for Device {1}. Error: {2}..
        /// </summary>
        internal static string ErrorRegisteringSensor {
            get {
                return ResourceManager.GetString("ErrorRegisteringSensor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The sensor {0} has been already registered for device {1}..
        /// </summary>
        internal static string ErrorSensorAlreadyRegistered {
            get {
                return ResourceManager.GetString("ErrorSensorAlreadyRegistered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The passed sensor data is null..
        /// </summary>
        internal static string ErrorSensorDataIsNull {
            get {
                return ResourceManager.GetString("ErrorSensorDataIsNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sensor {0} for Device {1} is not registered..
        /// </summary>
        internal static string ErrorSensorIsNotRegistered {
            get {
                return ResourceManager.GetString("ErrorSensorIsNotRegistered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The sensor may not been null..
        /// </summary>
        internal static string ErrorSensorIsNull {
            get {
                return ResourceManager.GetString("ErrorSensorIsNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The passed sensor list contains a null element..
        /// </summary>
        internal static string ErrorSensorListContainsNullElement {
            get {
                return ResourceManager.GetString("ErrorSensorListContainsNullElement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The passed sensor list is null..
        /// </summary>
        internal static string ErrorSensorListIsNull {
            get {
                return ResourceManager.GetString("ErrorSensorListIsNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sensor is not registered for the device. Sensor id: {0}; Device Id: {1}..
        /// </summary>
        internal static string ErrorSensorNotRegisteredForDevice {
            get {
                return ResourceManager.GetString("ErrorSensorNotRegisteredForDevice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed setting up device for scanning. Device id: {0}..
        /// </summary>
        internal static string ErrorSettingUpDeviceScanning {
            get {
                return ResourceManager.GetString("ErrorSettingUpDeviceScanning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An exception occured while storing value {0} for sensor {1} for device {2}.
        ///
        ///{3}.
        /// </summary>
        internal static string ErrorStoringData {
            get {
                return ResourceManager.GetString("ErrorStoringData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Data for sensor {0} could not be stored as the sensor is not registered for device {1}..
        /// </summary>
        internal static string ErrorStoringDataForSensor {
            get {
                return ResourceManager.GetString("ErrorStoringDataForSensor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sensor data could not be written..
        /// </summary>
        internal static string ErrorStoringSensorData {
            get {
                return ResourceManager.GetString("ErrorStoringSensorData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Device {0} could not be updated. Error: {1}..
        /// </summary>
        internal static string ErrorUpdatingDevice {
            get {
                return ResourceManager.GetString("ErrorUpdatingDevice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sensor {0} could not be updated for Device {1}. Error: {2}..
        /// </summary>
        internal static string ErrorUpdatingSensor {
            get {
                return ResourceManager.GetString("ErrorUpdatingSensor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Data {0}  in format {1} for sensor {2} (device {3}) could not be stored: 
        ///{4}..
        /// </summary>
        internal static string ErrorWhileStoringSensorData {
            get {
                return ResourceManager.GetString("ErrorWhileStoringSensorData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occured while writing the values: {0}..
        /// </summary>
        internal static string ErrorWritingValues {
            get {
                return ResourceManager.GetString("ErrorWritingValues", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Device is not in use. Id: {0}.
        /// </summary>
        internal static string ExceptionDeviceIsNotInUse {
            get {
                return ResourceManager.GetString("ExceptionDeviceIsNotInUse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed adding sensor dependency. Base sensor id: {0}; dependent sensor id: {1}..
        /// </summary>
        internal static string ExceptionFailedAddingSensorDependency {
            get {
                return ResourceManager.GetString("ExceptionFailedAddingSensorDependency", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed creating IDeviceCommunicationHandler instance. Error: {0}.
        /// </summary>
        internal static string ExceptionFailedCreatingIDeviceCommunicationHandlerInstance {
            get {
                return ResourceManager.GetString("ExceptionFailedCreatingIDeviceCommunicationHandlerInstance", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed getting sensors for device. Error message: {0}.
        /// </summary>
        internal static string ExceptionFailedGettingSensorsForDevice {
            get {
                return ResourceManager.GetString("ExceptionFailedGettingSensorsForDevice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed removing sensor dependency. Base sensor internal id: {0}; dependent sensor internal id: {1}..
        /// </summary>
        internal static string ExceptionFailedRemovingSensorDependency {
            get {
                return ResourceManager.GetString("ExceptionFailedRemovingSensorDependency", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid configuration property value. Property: {0}; Value: {1}..
        /// </summary>
        internal static string ExceptionInvalidConfigurationPropertyValue {
            get {
                return ResourceManager.GetString("ExceptionInvalidConfigurationPropertyValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sensor is not virtual. Internal id: {0}..
        /// </summary>
        internal static string ExceptionSensorIsNotVirtual {
            get {
                return ResourceManager.GetString("ExceptionSensorIsNotVirtual", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sensor is not found. Internal id: {0}..
        /// </summary>
        internal static string ExceptionSensorNotFound {
            get {
                return ResourceManager.GetString("ExceptionSensorNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown VirtualSensorDefinitionType. Value: {0}.
        /// </summary>
        internal static string ExceptionUnknownVirtualSensorDefinitionType {
            get {
                return ResourceManager.GetString("ExceptionUnknownVirtualSensorDefinitionType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Starting scanning device sensors. Device  id: {0}..
        /// </summary>
        internal static string InfoStartingScanningDeviceSensors {
            get {
                return ResourceManager.GetString("InfoStartingScanningDeviceSensors", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Starting scanning sensors. Device id: {0}; Sensor id: {1}..
        /// </summary>
        internal static string InfoStartingScanningSensor {
            get {
                return ResourceManager.GetString("InfoStartingScanningSensor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stopping scanning device sensors. Device  id: {0}..
        /// </summary>
        internal static string InfoStoppingScanningDeviceSensors {
            get {
                return ResourceManager.GetString("InfoStoppingScanningDeviceSensors", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stopping scanning sensors. Device id: {0}; Sensor id: {1}..
        /// </summary>
        internal static string InfoStoppingScanningSensor {
            get {
                return ResourceManager.GetString("InfoStoppingScanningSensor", resourceCulture);
            }
        }
    }
}
