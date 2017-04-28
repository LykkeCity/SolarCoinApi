using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Core
{
    public interface IMonitoring
    {
        DateTime DateTime { get; set; }
        string ServiceName { get; set; }
        string Version { set; get; }
    }

    public class Monitoring : IMonitoring
    {
        public DateTime DateTime { get; set; }
        public string ServiceName { get; set; }
        public string Version { set; get; }
    }

    public interface IMonitoringRepository
    {
        Task SaveAsync(IMonitoring redirect);
    }
}
