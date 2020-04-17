using System;
using System.Composition;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Serilog;

using FOS.Paymetric.POC.HFSchedulerService.Shared;
using FOS.Paymetric.POC.HFSchedulerService.Shared.Entities;
using FOS.Paymetric.POC.HFSchedulerService.Shared.Interfaces;
using static FOS.Paymetric.POC.HFSchedulerService.Shared.Constants.SchedulerConstants;
using System.Threading;

namespace FIS.Paymetric.POC.EventTypeA.Plugin
{
    /// <summary>
    /// This is a plug-in class and knows how to publish EventTypeA events
    /// </summary>
    [Export(typeof(IJobPlugIn))]
    [ExportMetadata(JobPlugInType.ATTRIBUTE_NAME, @"EventTypeA")]
    public class EventTypeAPublisher : IJobPlugIn
    {
        KafkaServiceConfigBE _kafkaConfig;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public EventTypeAPublisher()
        {
            // load plug-in specific configuration from appsettings.json file copied into the plug-in specific subfolder 
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json", false)
                .Build();

            // read config values (the plug-in would use this information to connect to the correct DB to gather addl data required 
            string dbConnString = configuration.GetConnectionString("DataConnection");
        }

        /// <summary>
        /// Injects the generic configuration known by the hosting process.
        /// </summary>
        /// <param name="kafkaConfig">The configuration information.</param>
        public void InjectConfig(KafkaServiceConfigBE kafkaConfig)
        {
            _kafkaConfig = kafkaConfig;
        }

        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns>StdTaskReturnValueBE.</returns>
        public StdTaskReturnValueBE Execute(string jobId, ILogger logger)
        {
            logger.Information("Hello from Hangfire jobid: {jobId}!", jobId);

            return new StdTaskReturnValueBE() { StepStatus = STD_STEP_STATUS.SUCCESS, ReturnMessage = "Ok" };
        }
    }
}
