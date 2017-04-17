using SolarCoinApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;

namespace SolarCoinApi.Common
{
    public class MonitoringJob
    {
        private string _compoenentForLogging;
        private string _component;
        private readonly ILog _log;
        private readonly IMonitoringRepository _repository;

        public MonitoringJob(string component, IMonitoringRepository repository, ILog logger)
        {
            _log = logger;
            _component = component;
            _compoenentForLogging = component + ".Monitoring";
            _repository = repository;
        }


        public virtual async Task Execute()
        {
            await _log.WriteInfoAsync(_compoenentForLogging, "Monitoring", "", "preparing to write to table");
            await _repository.SaveAsync(new Monitoring { DateTime = DateTime.UtcNow, ServiceName = _component });
            await _log.WriteInfoAsync(_compoenentForLogging, "Monitoring", "", "done writing to table");
        }
    }
}
