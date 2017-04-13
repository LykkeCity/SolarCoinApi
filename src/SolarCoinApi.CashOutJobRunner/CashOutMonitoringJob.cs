using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;
using SolarCoinApi.Common;
using SolarCoinApi.Core;

namespace SolarCoinApi.CashOutJobRunner
{
    public class CashOutMonitoringJob : MonitoringJob
    {
        public CashOutMonitoringJob(string component, IMonitoringRepository repository, ILog logger) : base(component, repository, logger)
        {
        }

        [TimerTrigger("00:00:10")]
        public override Task Execute()
        {
            return base.Execute();
        }
    }
}
