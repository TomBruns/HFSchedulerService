using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Hangfire.Server;
using Serilog.Core;
using Serilog.Events;

namespace FOS.Paymetric.POC.HFSchedulerService.Logging
{

    // https://www.dennis-s.dk/2019-07/hangfire-console-serilog-sink
    internal class HangfireConsoleSerilogEnricher : ILogEventEnricher
    {
        // The context used to enrich log events
        public PerformContext PerformContext { get; set; }

        /// <inheritdoc />
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            // Create property value with PerformContext and put as "PerformContext"
            var prop = new LogEventProperty(
                "PerformContext", new PerformContextValue { PerformContext = PerformContext }
            );
            logEvent.AddOrUpdateProperty(prop);
        }
    }
}
