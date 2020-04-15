using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using FOS.Paymetric.POC.HFSchedulerService.Shared.Entities;

namespace FOS.Paymetric.POC.HFSchedulerService.Shared.Interfaces
{
    /// <summary>
    /// Interface IMessageSender
    /// </summary>
    /// <remarks>
    /// Define the interface that each plug-in will implement, each will be in a separate independent assy    
    /// </remarks>
    public interface IEventPublisher
    {
        void InjectConfig(KafkaServiceConfigBE kafkaConfig, ILogger logger);

        Task<PublishMsgResultsBE> PublishEvent(int eventId, string message);
    }
}
