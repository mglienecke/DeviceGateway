using System;
using ValueManagement.DynamicCallback;

public class CSharpInteractiveClass
{
    static public CallbackResultData AdjustSampleRateCallback(CallbackPassInData passInData)
    {
        CallbackResultData result = new CallbackResultData();
        result.IsValueModified = true;
        result.NewValue = passInData.CurrentValue * 5;
        return (result);
    }
}
