using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FOS.Paymetric.POC.HFSchedulerService.Entities
{
    /// <summary>
    /// This class represents the information used to configure Hangfire
    /// </summary>
    public class HangfireServiceConfigBE
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is use SSL.
        /// </summary>
        /// <value><c>true</c> if this instance is use SSL; otherwise, <c>false</c>.</value>
        [JsonPropertyName(@"isUseSSL")]
        public bool IsUseSSL { get; set; }

        /// <summary>
        /// Gets or sets the dashboard port number.
        /// </summary>
        /// <value>The dashboard port number.</value>
        [JsonPropertyName(@"dashboardPortNumber")]
        public int DashboardPortNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is dashboard remote access enabled.
        /// </summary>
        /// <value><c>true</c> if this instance is dashboard remote access enabled; otherwise, <c>false</c>.</value>
        [JsonPropertyName(@"isDashboardRemoteAccessEnabled")]
        public bool IsDashboardRemoteAccessEnabled { get; set; }

        /// <summary>
        /// Gets or sets the worker count.
        /// </summary>
        /// <value>The worker count.</value>
        [JsonPropertyName(@"workerCount")]
        public int WorkerCount { get; set; }

        /// <summary>
        /// Gets or sets the poll interval in secs.
        /// </summary>
        /// <value>The poll interval in secs.</value>
        [JsonPropertyName(@"pollIntervalInSecs")]
        public int PollIntervalInSecs { get; set; }
    }
}
