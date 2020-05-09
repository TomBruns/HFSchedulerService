using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Hangfire.Storage;

using FOS.Paymetric.POC.HFSchedulerService.Shared;
using FOS.Paymetric.POC.HFSchedulerService.Shared.Entities;
using FOS.Paymetric.POC.HFSchedulerService.Shared.Interfaces;
using FOS.Paymetric.POC.HFSchedulerService.Logging;
using System.Runtime.Loader;

namespace FOS.Paymetric.POC.HFSchedulerService.Managers
{
    /// <summary>
    /// This class holds the methods used to Enqueue and Execute Jobs in Hangfire
    /// </summary>
    public class HangfireRequestManager
    {
        private KafkaServiceConfigBE _kafkaConfig;
        //private ILogger _logger;
        //private IEnumerable<Lazy<IJobPlugIn, JobPlugInType>> _jobPlugIns;
        private PlugInsManager _plugInsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireRequestManager"/> class.
        /// </summary>
        /// <param name="plugInsManager">The PlugIn Manager.</param>
        /// <param name="kafkaConfig">The kafka configuration.</param>
        public HangfireRequestManager(PlugInsManager plugInsManager, KafkaServiceConfigBE kafkaConfig)
        {
            _plugInsManager = plugInsManager;
            _kafkaConfig = kafkaConfig;
        }

        /// <summary>
        /// Enqueue a request with the information necessary to dynamically load the necessary assy on the other side of the hangfire queue
        /// </summary>
        [DisplayName("Enqueue Job Id: {0}, Token: {1}.v{2}")]
        public static void EnqueueRequest(string jobId, string plugInToken, decimal plugInVersion)
        {
            // the process submitting creates new fire & forget jobs
            // they can be processed in parallel on any available thread on any server running hangfire
            //BackgroundJob.Enqueue(() => RequestController.ExecuteRequest(className, assyName, methodName, parmValue));
            BackgroundJob.Enqueue<HangfireRequestManager>(rc => rc.ExecuteRequest(jobId, plugInToken, plugInVersion, null));
        }

        #region Load via Reflection Approach
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
        #endregion

        /// <summary>
        /// Executes the request using an implementation in a plug-in assy
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="pluginToken">The plugin token.</param>
        /// <param name="plugInVersion">The plugin version.</param>
        /// <param name="context">The context.</param>
        [DisplayName("Execute Job Id: {0}, Token: {1}")]
        public void ExecuteRequest(string jobId, string pluginToken, decimal plugInVersion, PerformContext context)
        {
            // create a logger
            var logger = context.CreateLoggerForPerformContext<HangfireRequestManager>();

            // This is a 1st pass at preventing duplicate recurring job when the previous execution is still running
            var job = JobStorage.Current.GetConnection().GetRecurringJobs().Where(j => j.Id == jobId).FirstOrDefault();
            if (job != null)
            {
                if (job.LastJobState == "Enqueued" || job.LastJobState == "Processing")
                {
                    //logger.Information("This goes to the job console automatically");

                    logger.Warning("Skipping execution of JobId: {jobId}, it is still running from a previous execution.", jobId);
                    return;
                }
            }

            // use the string value of the EventType property to dynamically select the correct plug-in assy to use to process the event
            IJobPlugIn jobPlugIn = GetJobPlugIn(pluginToken, plugInVersion);

            var xx = AssemblyLoadContext.All;

            var plugInLoadContextName = AssemblyLoadContext.GetLoadContext(jobPlugIn.GetType().Assembly).Name;
            logger.Information("Running plugin {pluginToken}.{plugInVersion} from ALC: {plugInLoadContextName}.", pluginToken, plugInVersion, plugInLoadContextName);

            // call the method on the dynamically selected assy
            jobPlugIn.Execute(context.BackgroundJob.Id, logger);

            //context.WriteLine();
        }

        /// <summary>
        /// Gets the job plug in.
        /// </summary>
        /// <param name="jobPlugInType">Type of the job plugIn.</param>
        /// <param name="jobPlugInVersion">Version of the job plugIn.</param>
        /// <returns>IScheduledTask.</returns>
        /// <exception cref="ApplicationException">No plug-in found for Event Type: [{jobPlugInType}]</exception>
        /// <exception cref="ApplicationException">Multiple plug-ins [{plugIn.Count()}] found for Event Type: [{scheduledTaskType}]</exception>
        private IJobPlugIn GetJobPlugIn(string jobPlugInType, decimal jobPlugInVersion)
        {
            var plugIn = _plugInsManager.PlugIns
              .Where(ms => ms.Metadata.Name.Equals(jobPlugInType) && ms.Metadata.Version.Equals((double)jobPlugInVersion))
              .Select(ms => ms.Value);

            if (plugIn == null || plugIn.Count() == 0)
            {
                throw new ApplicationException($"No plug-in found for Job Type: [{jobPlugInType}], Version: [{jobPlugInVersion}]");
            }
            else if (plugIn.Count() != 1)
            {
                throw new ApplicationException($"Multiple plug-ins [{plugIn.Count()}] found for Job Type: [{jobPlugInType}], Version: [{jobPlugInVersion}]");
            }
            else
            {
                return plugIn.FirstOrDefault();
            }
        }
    }
}
