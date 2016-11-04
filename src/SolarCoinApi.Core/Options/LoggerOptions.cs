using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Core.Options
{
    public class LoggerOptions
    {
        public string ConnectionString { set; get; }
        public string ErrorTableName { set; get; }
        public string WarningTableName { set; get; }
        public string InfoTableName { set; get; }
    }
}
