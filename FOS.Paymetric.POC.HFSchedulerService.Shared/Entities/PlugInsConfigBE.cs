using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FOS.Paymetric.POC.HFSchedulerService.Shared.Entities
{
    public class PlugInsConfigBE
    {
        [JsonPropertyName(@"parentFolder")]
        public string PlugInsParentFolder { get; set; }

        [JsonPropertyName(@"assembliesToSkipLoading")]
        public List<string> AssembliesToSkipLoading { get; set; }
    }
}
