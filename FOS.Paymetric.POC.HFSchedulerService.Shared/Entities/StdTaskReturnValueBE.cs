using System;
using System.Collections.Generic;
using System.Text;

using static FOS.Paymetric.POC.HFSchedulerService.Shared.Constants.SchedulerConstants;

namespace FOS.Paymetric.POC.HFSchedulerService.Shared.Entities
{
    public class StdTaskReturnValueBE
    {
        public STD_STEP_STATUS StepStatus { get; set; }

        public string ReturnMessage { get; set; }
    }
}
