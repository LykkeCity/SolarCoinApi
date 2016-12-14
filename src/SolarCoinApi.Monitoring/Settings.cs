using SolarCoinApi.Common;
using SolarCoinApi.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Monitoring
{
    public class MonitoringSettings : IValidatable
    {
        public LoggerSettings Logger { set; get; }
        public MongoSettings Mongo { set; get; }
        public RpcSettings Rpc { set; get; }

        public void Validate()
        {
            //throw new NotImplementedException();
        }
    }
}
