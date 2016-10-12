import clr
import ValueManagement.DynamicCallback
from ValueManagement.DynamicCallback import *
clr.AddReference('GlobalDataContracts')
import GlobalDataContracts
from GlobalDataContracts import *

# callback for general check
def GeneralCheckCallback(callbackPassIn):
    callbackResult = CallbackResultData()
    callbackResult.IsValueModified = True
    callbackResult.NewValue = GlobalDataContracts.SensorData(str(int(callbackPassIn.CurrentValue.Value) * 3))
    return(callbackResult);
