using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FOS.Paymetric.POC.HFSchedulerService.Entities
{
    public class NewRecurringJobBE
    {
        public int std_request_type_id { get; set; }
        public string job_identifier { get; set; }
        public string pnc_username { get; set; }
        public string user_comments { get; set; }
        public object[] parameters { get; set; }
        public string schedule { get; set; }
        public string schedule_time_zone { get; set; }
    }

}
