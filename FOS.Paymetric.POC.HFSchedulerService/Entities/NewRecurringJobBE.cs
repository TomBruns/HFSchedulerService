using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace FOS.Paymetric.POC.HFSchedulerService.Entities
{
    /// <summary>
    /// This class defines a recurring job that should be scheduled
    /// </summary>
    public class NewRecurringJobBE
    {
        /// <summary>
        /// Gets or sets the job identifier.
        /// </summary>
        /// <value>The job identifier.</value>
        [JsonPropertyName(@"job_id")]
        public string JobId { get; set; }

        /// <summary>
        /// Gets or sets the job description.
        /// </summary>
        /// <value>The job description.</value>
        [JsonPropertyName(@"job_description")]
        public string JobDescription { get; set; }

        /// <summary>
        /// Gets or sets the type of the job plug in.
        /// </summary>
        /// <value>The type of the job plug in.</value>
        [JsonPropertyName(@"job_plugin_type")]
        public string JobPlugInType { get; set; }

        //[JsonPropertyName(@"parameters")]
        //public object[] parameters { get; set; }

        /// <summary>
        /// Gets or sets the cron schedule.
        /// </summary>
        /// <value>The cron schedule.</value>
        [JsonPropertyName(@"cron_schedule")]
        public string CronSchedule { get; set; }

        /// <summary>
        /// Gets or sets the schedule time zone.
        /// </summary>
        /// <value>The schedule time zone.</value>
        [JsonPropertyName(@"schedule_time_zone")]
        public string ScheduleTimeZone { get; set; }
    }

}
