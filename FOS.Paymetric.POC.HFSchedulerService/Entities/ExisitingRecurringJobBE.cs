using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FOS.Paymetric.POC.HFSchedulerService.Entities
{
    public class ExisitingRecurringJobBE
    {
        public DateTime? CreatedAt { get; set; }
        public string Schedule { get; set; }
        public string Id { get; set; }
        public DateTime? LastExecution { get; set; }
        public string LastJobId { get; set; }
        public string LastJobState { get; set; }
        public DateTime? NextExecution { get; set; }
        public string Queue { get; set; }
        public string TimeZoneId { get; set; }
    }
}
