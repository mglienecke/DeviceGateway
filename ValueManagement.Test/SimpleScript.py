import clr
import System
clr.AddReference('System.Windows.Forms')
from System.Windows.Forms import *
import System.Diagnostics
import ValueManagement.DynamicCallback
from ValueManagement.DynamicCallback import *
clr.AddReference('GlobalDataContracts')
import GlobalDataContracts
from GlobalDataContracts import *

# callback for detect anomaly
def DetectAnomalyCallback(callbackPassIn):
    callbackResult = CallbackResultData()
    callbackResult.IsValueModified = True
    callbackResult.NewValue = GlobalDataContracts.SensorData(str(int(callbackPassIn.CurrentValue.Value) * 2))
    return(callbackResult);

# callback for adjust sample rate
def AdjustSampleRateCallback(callbackPassIn):
    return(None);

# callback for general check
def GeneralCheckCallback(callbackPassIn):
    callbackResult = CallbackResultData()
    callbackResult.IsValueModified = True
    callbackResult.NewValue = GlobalDataContracts.SensorData(str(int(callbackPassIn.CurrentValue.Value) * 4))
    return(callbackResult);

# callback before storing
def BeforeStoreCallback(callbackPassIn):
    return(None);

# callback after storing
def AfterStoreCallback(callbackPassIn):
    return(None);

# evaluate a virtual value
def VirtualValueCalculationCallback(callbackPassIn):
    callbackResult = CallbackResultData()
    callbackResult.IsValueModified = True

    currentVal = 0
    for x in callbackPassIn.BaseValueDefinitionList:
        if str(x.CurrentValue) != 'None':
			if str(x.CurrentValue.Value) != 'None':
				currentVal += int(x.CurrentValue.Value)
				
    callbackResult.NewValue = GlobalDataContracts.SensorData(str(currentVal * 2))
    return(callbackResult);