using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Activities;
using System.Activities.Statements;
using System.Reflection;
using GlobalDataContracts;
using ValueManagement;
using ValueManagement.DynamicCallback;
using ValueManagement.Test;

namespace WorkflowConsoleTest
{

    class Program
    {
        static void Main(string[] args)
        {

            // the dictionary for the pass in data
            Dictionary<string, object> arguments = new Dictionary<string, object>();

            // the basic data to be used
            SensorData sensor = new SensorData(DateTime.Now, "150", string.Empty);
            CallbackPassInData data = new CallbackPassInData(CallbackType.VirtualValueCalculation, new ValueDefinition(1, "AA"), sensor);
            arguments.Add(SoftwareAgentCallback.SoftwareAgentPassInDataName, data);

            // now invoke the graphic activity
            WorkflowInvoker invoke = new WorkflowInvoker(new MultiplyBy3GraphicActivity());
            IDictionary<string, object> result = invoke.Invoke(arguments);

            // and display results
            CallbackResultData resultData = result[SoftwareAgentCallback.SoftwareAgentResultName] as CallbackResultData;
            Console.WriteLine("Result-Data for graphic workflow is {0} present", resultData == null ? "not" : string.Empty);
            if (resultData != null)
            {
                Console.WriteLine("Value of the result = {0}", resultData.NewValue);
            }

            // now the same with the dynamic calculation
            var action = new MultiplyBy3CodeActivity();
            PropertyInfo[] properties = action.GetType().GetProperties();
            PropertyInfo inProp = properties.FirstOrDefault(p => p.Name == SoftwareAgentCallback.SoftwareAgentPassInDataName);
            if (inProp == null)
            {
                Console.WriteLine("Type {0} has no Property of name {1}", action.GetType().Name, SoftwareAgentCallback.SoftwareAgentPassInDataName);
            }
            else if (inProp.PropertyType != typeof(InArgument<CallbackPassInData>))
            {
                Console.WriteLine("Type {0} has a property {1}, but it's of the wrong type {2} instead of {3}", action.GetType().Name, SoftwareAgentCallback.SoftwareAgentPassInDataName, inProp.PropertyType.Name, typeof(InArgument<CallbackPassInData>).Name);
            }
            else
            {
                WorkflowInvoker invoker2 = new WorkflowInvoker(action);
                result = invoker2.Invoke(arguments);

                // and results
                resultData = result[SoftwareAgentCallback.SoftwareAgentResultName] as CallbackResultData;
                Console.WriteLine("Result-Data for code activity workflow is {0} present", resultData == null ? "not" : string.Empty);
                if (resultData != null)
                {
                    Console.WriteLine("Value of the result = {0}", resultData.NewValue);
                }
            }

            Console.WriteLine("Press ENTER to continue");
            Console.ReadLine();
        }
    }
}
