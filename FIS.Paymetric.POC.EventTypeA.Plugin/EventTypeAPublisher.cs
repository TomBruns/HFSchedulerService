using System;

using FOS.Paymetric.POC.HFSchedulerService.Shared.Entities;
using static FOS.Paymetric.POC.HFSchedulerService.Shared.Constants.SchedulerConstants;

namespace FIS.Paymetric.POC.EventTypeA.Plugin
{
    public class EventTypeAPublisher
    {
        public static StdTaskReturnValueBE Execute(string paramValue)
        {
            Console.WriteLine($"Hello Hangfire job using paramValue: [{paramValue}]!");

            return new StdTaskReturnValueBE() { StepStatus = STD_STEP_STATUS.SUCCESS, ReturnMessage = "Ok" };
        }
    }
}
