using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Hangfire;
using Hangfire.Server;
using Hangfire.Console;

using FOS.Paymetric.POC.HFSchedulerService.Shared;
using FOS.Paymetric.POC.HFSchedulerService.Shared.Entities;
using FOS.Paymetric.POC.HFSchedulerService.Shared.Interfaces;
using FOS.Paymetric.POC.HFSchedulerService.Logging;

namespace FOS.Paymetric.POC.HFSchedulerService.Hangfire
{
    public class RequestController
    {
        private KafkaServiceConfigBE _kafkaConfig;
        //private ILogger _logger;
        private IEnumerable<Lazy<IJobPlugIn, JobPlugInType>> _jobPlugIns;

        public RequestController(IEnumerable<Lazy<IJobPlugIn, JobPlugInType>> messageSenders, KafkaServiceConfigBE kafkaConfig)
        {
            _jobPlugIns = messageSenders;
            _kafkaConfig = kafkaConfig;
        }

        /// <summary>
        /// Injects the configuration.
        /// </summary>
        /// <param name="messageSenders">The message senders.</param>
        /// <param name="kafkaConfig">The kafka configuration.</param>
        /// <param name="logger">The logger.</param>
        public void InjectConfig(IEnumerable<Lazy<IJobPlugIn, JobPlugInType>> messageSenders, KafkaServiceConfigBE kafkaConfig, ILogger logger)
        {
            _jobPlugIns = messageSenders;
            _kafkaConfig = kafkaConfig;
        }

        /// <summary>
        /// Enqueue a request with the information necessary to dynamically load the necessary assy on the other side of the hangfire queue
        /// </summary>
        [DisplayName("Enqueue Job, PlugIn Token: {0}")]
        public static void EnqueueRequest(string plugInToken)
        {
            // the process submitting creates new fire & forget jobs
            // they can be processed in parallel on any available thread on any server running hangfire
            //BackgroundJob.Enqueue(() => RequestController.ExecuteRequest(className, assyName, methodName, parmValue));
            BackgroundJob.Enqueue<RequestController>(rc => rc.ExecuteRequest(plugInToken, null));
        }

        /// <summary>
        /// Dynamically load the correct assy and invoke the target method using reflection
        /// </summary>
        [DisplayName("Execute Job, Plugin Class {0} => Method {2}]")]
        public void ExecuteRequest(string className, string assyName, string methodName, string parmValue)
        {
            // use reflection to dynamically execute the plugin method
            Type taskType = Type.GetType($"{className}, {assyName}");
            MethodInfo taskMethod = taskType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);

            // NOTE: Normal Reflection Parameter Widening applies (ex: Int32 => Int64), so normal int target method params should be changed to long
            object[] parms = new object[] { parmValue };
            var returnValue = (StdTaskReturnValueBE)taskMethod.Invoke(null, parms);
        }

        /// <summary>
        /// Executes the request using an implementation in a plug-in assy
        /// </summary>
        /// <param name="pluginToken">The plugin token.</param>
        /// <param name="context">The context.</param>
        [DisplayName("Execute Job: Plugin [{0}]")]
        public void ExecuteRequest(string pluginToken, PerformContext context)
        {
            // use the string value of the EventType property to dynamically select the correct plug-in assy to use to process the event
            IJobPlugIn jobPlugIn = GetJobPlugIn(pluginToken);

            var logger = context.CreateLoggerForPerformContext<RequestController>();

            //context.WriteLine("Did this work?");
            //logger.Information("This goes to the job console automatically");

            // call the method on the dynamically selected assy
            jobPlugIn.Execute(context.BackgroundJob.Id, logger);

            context.WriteLine();
        }

        /// <summary>
        /// Gets the job plug in.
        /// </summary>
        /// <param name="jobPlugInType">Type of the job plugIn.</param>
        /// <returns>IScheduledTask.</returns>
        /// <exception cref="ApplicationException">No plug-in found for Event Type: [{jobPlugInType}]</exception>
        /// <exception cref="ApplicationException">Multiple plug-ins [{plugIn.Count()}] found for Event Type: [{scheduledTaskType}]</exception>
        private IJobPlugIn GetJobPlugIn(string jobPlugInType)
        {
            var plugIn = _jobPlugIns
              .Where(ms => ms.Metadata.Name.Equals(jobPlugInType))
              .Select(ms => ms.Value);

            if (plugIn == null || plugIn.Count() == 0)
            {
                throw new ApplicationException($"No plug-in found for Job Type: [{jobPlugInType}]");
            }
            else if (plugIn.Count() != 1)
            {
                throw new ApplicationException($"Multiple plug-ins [{plugIn.Count()}] found for Job Type: [{jobPlugInType}]");
            }
            else
            {
                return plugIn.FirstOrDefault();
            }
        }
    }
}
