using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

using FOS.Paymetric.POC.HFSchedulerService.Entities;

namespace FOS.Paymetric.POC.HFSchedulerService.Controllers.v1
{
    /// <summary>
    /// This class implements api methods related to recurring jobs
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RecurringJobsController : ControllerBase
    {
        private readonly ILogger<RecurringJobsController> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public RecurringJobsController(ILogger<RecurringJobsController> logger, IBackgroundJobClient backgroundJobClient)
        {
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
        }

        /// <summary>
        /// Schedules the job.
        /// </summary>
        /// <returns>ActionResult.</returns>
        [HttpPost]
        [Route("scheduleJob")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        //public void ScheduledJob(int stdRequestTypeID, string jobIdentifier, string pncUserName, string userComments, Dictionary<string, string> wfDataKeyValuePairs, string schedule, string schedule_time_zone)
        public ActionResult ScheduleJob()
        {
            string jobIdentifier = @"some-id";

            // Background: each time a recurring job starts it needs to go thru the CreateRequest Step, so that is the one we queue

            // 1st remove the Job if it exists
            RecurringJob.RemoveIfExists(jobIdentifier.ToLower());

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;

            //switch (schedule_time_zone.ToUpper())
            //{
            //    case "EST":
            timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            //        break;

            //    default:
            //        timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            //        break;
            //}
            // run the background jon immediately
            //_backgroundJobClient.Enqueue(() => Console.WriteLine("Hello Hangfire job!"));

            var manager = new RecurringJobManager();
            manager.AddOrUpdate(jobIdentifier, Job.FromExpression(() => Console.WriteLine("Hello Hangfire job!")), @"0-59 * * * MON,TUE,WED,THU,FRI", timeZoneInfo);

            return Ok();
        }

        /// <summary>
        /// Gets the scheduled jobs.
        /// </summary>
        /// <returns>ActionResult&lt;List&lt;ExisitingRecurringJobBE&gt;&gt;.</returns>
        [HttpGet]
        [Route("scheduleJobs")]
        [ProducesResponseType(typeof(List<ExisitingRecurringJobBE>), (int)HttpStatusCode.OK)]
        public ActionResult<List<ExisitingRecurringJobBE>> GetScheduledJobs()
        {
            //var monitoringApi = JobStorage.Current.GetMonitoringApi();
            //var scheduledJobs = monitoringApi.ScheduledJobs(0, (int)monitoringApi.ScheduledCount());

            // get the list of recurring jobs
            List<RecurringJobDto> recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();

            // build the result object
            var result = recurringJobs.Select(rj => new ExisitingRecurringJobBE()
            {
                CreatedAt = rj.CreatedAt,
                Id = rj.Id,
                LastExecution = rj.LastExecution,
                LastJobId = rj.LastJobId,
                LastJobState = rj.LastJobState,
                NextExecution = rj.NextExecution,
                Queue = rj.Queue,
                Schedule = rj.Cron,
                TimeZoneId = rj.TimeZoneId
            });

            return Ok(result);
        }
    }
}