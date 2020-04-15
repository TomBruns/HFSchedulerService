﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FOS.Paymetric.POC.HFSchedulerService.Shared.Entities
{
    public class PublishMsgResultsBE
    {
        public bool IsSuccess { get; set; }
        public string Msg { get; set; }
        public string Name { get; set; }
        public string TopicName { get; set; }
        public AggregateException Exception { get; set; }
    }
}
