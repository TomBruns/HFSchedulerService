using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Serilog;

using FOS.Paymetric.POC.HFSchedulerService.Shared.Entities;

namespace FOS.Paymetric.POC.HFSchedulerService.Shared.Interfaces
{
    /// <summary>
    /// Interface IJobPlugIn
    /// </summary>
    /// <remarks>
    /// Define the interface that each plug-in will implement, each will be implemented in a separate, independent assy    
    /// </remarks>
    public interface IJobPlugIn
    {
        void InjectConfig(KafkaServiceConfigBE kafkaConfig);

        StdTaskReturnValueBE Execute(string jobId, ILogger logger);
    }
}
