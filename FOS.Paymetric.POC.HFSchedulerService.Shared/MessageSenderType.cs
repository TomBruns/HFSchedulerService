using System;
using System.Collections.Generic;
using System.Text;

namespace FOS.Paymetric.POC.HFSchedulerService.Shared
{
    /// <summary>
    /// This class will hold the value of ExportMetadata attribute at runtime when the assy is loaded
    /// </summary>
    public class MessageSenderType
    {
        public const string ATTRIBUTE_NAME = "Name";

        public string Name { get; set; }
    }
}
