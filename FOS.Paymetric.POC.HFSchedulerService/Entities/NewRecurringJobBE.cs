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
        [JsonPropertyName(@"job_id")]
        public string JobId { get; set; }

        [JsonPropertyName(@"job_plugin_type")]
        public string JobPlugInType { get; set; }

        //[JsonPropertyName(@"parameters")]
        //public object[] parameters { get; set; }

        [JsonPropertyName(@"cron_schedule")]
        public string CronSchedule { get; set; }

        [JsonPropertyName(@"schedule_time_zone")]
        public string ScheduleTimeZone { get; set; }
    }

}
