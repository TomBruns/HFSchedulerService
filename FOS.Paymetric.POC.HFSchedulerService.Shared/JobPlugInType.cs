using System;
using System.Collections.Generic;
using System.Text;

namespace FOS.Paymetric.POC.HFSchedulerService.Shared
{
    /// <summary>
    /// This class will hold the value of ExportMetadata attribute at runtime when the assy is loaded
    /// </summary>
    public class JobPlugInType
    {
        public const string NAME_ATTRIBUTE = "Name";
        public const string VERSION_ATTRIBUTE = "Version";

        public string Name { get; set; }

        // parameter widening
        public double Version { get; set; }
    }
}
