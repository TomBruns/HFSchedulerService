using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Hangfire.Server;
using Serilog.Events;

namespace FOS.Paymetric.POC.HFSchedulerService.Logging
{
    internal class PerformContextValue : LogEventPropertyValue
    {
        // The context attached to this property value
        public PerformContext PerformContext { get; set; }

        /// <inheritdoc />
        public override void Render(TextWriter output, string format = null, IFormatProvider formatProvider = null)
        {
            // How the value will be rendered in Json output, etc.
            // Not important for the function of this code..
            output.Write(PerformContext.BackgroundJob.Id);
        }
    }
}
