using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Hangfire;

namespace FOS.Paymetric.POC.HFSchedulerService.Hangfire
{
    public static class RequestController
    {
        /// <summary>
        /// Enqueue a request with the information necessary to dynamically load the necessary assy on the other side of the hangfire queue
        /// </summary>
        [DisplayName("Enqueue Job, PlugIn Token: {0}")]
        public static void EnqueueRequest(string plugInToken, string className, string assyName, string methodName, string parmValue)
        {
            // the process submitting creates new fire & forget jobs
            // they can be processed in parallel on any availabel thread on any server running hangfire
            BackgroundJob.Enqueue(() => StepController.ExecuteStep(className, assyName, methodName, parmValue));
        }
    }
}
