using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using FOS.Paymetric.POC.HFSchedulerService.Shared.Entities;

namespace FOS.Paymetric.POC.HFSchedulerService.Hangfire
{
    /// <summary>
    /// This is the generic method executed when a task is picked up out a Hangfire queue for execution
    /// </summary>
    public class StepController
    {
        [DisplayName("Execute Job, Plugin Class {0} => Method {2}]")]
        public static void ExecuteStep(string className, string assyName, string methodName, string parmValue)
        {
            // use reflection to dynamically execute the plugin method
            Type taskType = Type.GetType($"{className}, {assyName}");
            MethodInfo taskMethod = taskType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);

            // NOTE: Normal Reflection Parameter Widening applies (ex: Int32 => Int64), so normal int target method params should be changed to long
            object[] parms = new object[] { parmValue };
            var returnValue = (StdTaskReturnValueBE)taskMethod.Invoke(null, parms);
        }
    }
}
