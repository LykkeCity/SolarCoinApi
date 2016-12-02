using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Core.Settings
{
    public class QueueSettings
    {
        public string Name { set; get; }
        public string ConnectionString { set; get; }
    }

    public class TableSettings
    {
        public string Name { set; get; }
        public string ConnectionString { set; get; }
    }

    public class LoggerSettings
    {
        public string ErrorTableName { set; get; }
        public string WarningTableName { set; get; }
        public string InfoTableName { set; get; }
        public string ConnectionString { set; get; }
    }

    public class RpcSettings
    {
        public string Endpoint { set; get; }
        public string Username { set; get; }
        public string Password { set; get; }
    }
}
