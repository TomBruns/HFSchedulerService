using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Hangfire.Console;
using Serilog.Core;
using Serilog.Events;

namespace FOS.Paymetric.POC.HFSchedulerService.Logging
{
    public class HangfireConsoleSink : ILogEventSink
    {
        /// <inheritdoc />
        public void Emit(LogEvent logEvent)
        {
            // Get property
            if (logEvent.Properties.TryGetValue("PerformContext", out var logEventPerformContext))
            {
                // Get the object reference from our custom property
                var performContext = (logEventPerformContext as PerformContextValue)?.PerformContext;

                // And write the line on it
                performContext?.WriteLine(GetColor(logEvent.Level), logEvent.RenderMessage());
            }

            // Some nice coloring for log levels
            ConsoleTextColor GetColor(LogEventLevel level)
            {
                switch (level)
                {
                    case LogEventLevel.Fatal:
                    case LogEventLevel.Error:
                        return ConsoleTextColor.Red;
                    case LogEventLevel.Warning:
                        return ConsoleTextColor.Yellow;
                    case LogEventLevel.Information:
                        return ConsoleTextColor.White;
                    case LogEventLevel.Verbose:
                    case LogEventLevel.Debug:
                        return ConsoleTextColor.Gray;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
