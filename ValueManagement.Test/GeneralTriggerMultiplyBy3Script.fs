module public Program

open ValueManagement.DynamicCallback
open GlobalDataContracts
open System

type public FSharpInteractiveClass() =
  static member public DetectAnomalyCallback(data: CallbackPassInData) =
    let result = new CallbackResultData()
    result.IsValueModified <- true
    result.NewValue <- new SensorData()
    result.NewValue.Value <- Convert.ToString(Convert.ToInt32(data.CurrentValue.Value) * 2)
    result
