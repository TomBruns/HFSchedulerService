using FOS.Paymetric.POC.HFSchedulerService.Entities;
using FOS.Paymetric.POC.HFSchedulerService.Hangfire;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringJobsController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="backgroundJobClient">The background job client.</param>
        public RecurringJobsController(ILogger<RecurringJobsController> logger, IBackgroundJobClient backgroundJobClient)
        {
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
        }

        /// <summary>
        /// Schedules a recurring ob.
        /// </summary>
        /// <returns>ActionResult.</returns>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        //public void ScheduledJob(int stdRequestTypeID, string jobIdentifier, string pncUserName, string userComments, Dictionary<string, string> wfDataKeyValuePairs, string schedule, string schedule_time_zone)
        public ActionResult ScheduleRecurringJob()
        {
            string recurringJobId = @"some-id".ToLower(); ;

            // 1st remove the Job if it exists
            RecurringJob.RemoveIfExists(recurringJobId);

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

            // run the background job immediately
            //_backgroundJobClient.Enqueue(() => Console.WriteLine("Hello Hangfire job!"));

            // setup a recurring job

            #region Using Expression
            //var manager = new RecurringJobManager();
            //manager.AddOrUpdate(recurringJobId, Job.FromExpression(() => Console.WriteLine("Hello Hangfire job!")), @"0-59 * * * MON,TUE,WED,THU,FRI", timeZoneInfo);
            #endregion

            #region Using Reflection
            //RecurringJob.AddOrUpdate(recurringJobId, () => RequestController.EnqueueRequest(@"EventTypeA",
            //                                                                                @"FIS.Paymetric.POC.EventTypeA.Plugin.EventTypeAPublisher", 
            //                                                                                @"FIS.Paymetric.POC.EventTypeA.Plugin", 
            //                                                                                @"Execute", 
            //                                                                                @"xtobr"),
            //                                                                               @"0-59 * * * MON,TUE,WED,THU,FRI", 
            //                                                                               timeZoneInfo);
            #endregion

            RecurringJob.AddOrUpdate(recurringJobId, () => RequestController.EnqueueRequest(@"EventTypeA"),
                                                                               @"0-59 * * * MON,TUE,WED,THU,FRI",
                                                                               timeZoneInfo);

            return Ok($"Recurring job: [{recurringJobId}] created.");
        }

        /// <summary>
        /// Deletes a recurring job.
        /// </summary>
        /// <param name="recurringJobId">The recurring job identifier.</param>
        /// <returns>ActionResult&lt;ExisitingRecurringJobBE&gt;.</returns>
        [HttpDelete]
        [Route("{recurringJobId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public ActionResult<ExisitingRecurringJobBE> DeleteRecurringJob(string recurringJobId)
        {
            List<RecurringJobDto> recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();

            recurringJobId = recurringJobId.ToLower();

            var existingRecurringJob = recurringJobs.Where(rj => rj.Id == recurringJobId).FirstOrDefault();

            if(existingRecurringJob != null)
            {
                RecurringJob.RemoveIfExists(recurringJobId);

                return Ok($"Recurring job: [{recurringJobId}] deleted.");
            }
            else
            {
                return NotFound($"No recurring job found matching: [{recurringJobId}].");
            }
        }

        /// <summary>
        /// Gets all of the recurring jobs.
        /// </summary>
        /// <returns>ActionResult&lt;List&lt;ExisitingRecurringJobBE&gt;&gt;.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ExisitingRecurringJobBE>), (int)HttpStatusCode.OK)]
        public ActionResult<List<ExisitingRecurringJobBE>> GetRecurringJobs()
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