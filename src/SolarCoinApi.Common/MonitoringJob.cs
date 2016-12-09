using SolarCoinApi.Core;
using SolarCoinApi.Core.Log;
using SolarCoinApi.Core.Timers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Common
{
    public class MonitoringJob : TimerPeriod
    {
        private const int TimerPeriodSeconds = 30;

        private string _component;

        private readonly IMonitoringRepository _repository;

        public MonitoringJob(string component, IMonitoringRepository repository, ILog logger)
            : this($"{component}-MonitoringJob", TimerPeriodSeconds * 1000, logger)
        {
            _component = component;
            _repository = repository;
        }

        private MonitoringJob(string componentName, int periodMs, ILog log) : base(componentName, periodMs, log)
        {
        }

        public override async Task Execute()
        {
            await _repository.SaveAsync(new Monitoring { DateTime = DateTime.UtcNow, ServiceName = _component });
        }
    }
}
