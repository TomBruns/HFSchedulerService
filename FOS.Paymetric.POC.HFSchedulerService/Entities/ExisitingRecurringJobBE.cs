using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FOS.Paymetric.POC.HFSchedulerService.Entities
{
    /// <summary>
    /// This class represents a Existing Recurring Job
    /// </summary>
    public class ExisitingRecurringJobBE
    {
        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        /// <value>The created at.</value>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>The schedule.</value>
        public string Schedule { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the last execution.
        /// </summary>
        /// <value>The last execution.</value>
        public DateTime? LastExecution { get; set; }

        /// <summary>
        /// Gets or sets the last job identifier.
        /// </summary>
        /// <value>The last job identifier.</value>
        public string LastJobId { get; set; }

        /// <summary>
        /// Gets or sets the last state of the job.
        /// </summary>
        /// <value>The last state of the job.</value>
        public string LastJobState { get; set; }

        /// <summary>
        /// Gets or sets the next execution.
        /// </summary>
        /// <value>The next execution.</value>
        public DateTime? NextExecution { get; set; }

        /// <summary>
        /// Gets or sets the queue.
        /// </summary>
        /// <value>The queue.</value>
        public string Queue { get; set; }

        /// <summary>
        /// Gets or sets the time zone identifier.
        /// </summary>
        /// <value>The time zone identifier.</value>
        public string TimeZoneId { get; set; }
    }
}
