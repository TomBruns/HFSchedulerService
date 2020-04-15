using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Hangfire;

using FOS.Paymetric.POC.HFSchedulerService.Shared;
using FOS.Paymetric.POC.HFSchedulerService.Shared.Entities;
using FOS.Paymetric.POC.HFSchedulerService.Shared.Interfaces;


namespace FOS.Paymetric.POC.HFSchedulerService.Hangfire
{
    public class RequestController
    {
        private KafkaServiceConfigBE _kafkaConfig;
        private ILogger _logger;
        private IEnumerable<Lazy<IEventPublisher, MessageSenderType>> _messageSenders;

        /// <summary>
        /// Injects the configuration.
        /// </summary>
        /// <param name="messageSenders">The message senders.</param>
        /// <param name="kafkaConfig">The kafka configuration.</param>
        /// <param name="logger">The logger.</param>
        public void InjectConfig(IEnumerable<Lazy<IEventPublisher, MessageSenderType>> messageSenders, KafkaServiceConfigBE kafkaConfig, ILogger logger)
        {
            _messageSenders = messageSenders;
            _kafkaConfig = kafkaConfig;
            _logger = logger;

        }

        /// <summary>
        /// Enqueue a request with the information necessary to dynamically load the necessary assy on the other side of the hangfire queue
        /// </summary>
        [DisplayName("Enqueue Job, PlugIn Token: {0}")]
        public static void EnqueueRequest(string plugInToken, string className, string assyName, string methodName, string parmValue)
        {
            // the process submitting creates new fire & forget jobs
            // they can be processed in parallel on any availabel thread on any server running hangfire
            //BackgroundJob.Enqueue(() => RequestController.ExecuteRequest(className, assyName, methodName, parmValue));
            BackgroundJob.Enqueue<RequestController>(rc => rc.ExecuteRequest(className, assyName, methodName, parmValue));
        }

        /// <summary>
        /// Dynamically load the correct assy and invoke the target method
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
    }
}
