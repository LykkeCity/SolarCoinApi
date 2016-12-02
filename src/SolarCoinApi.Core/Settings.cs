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

    public class MongoSettings
    {
        public string Host { set; get; }
        public string Port { set; get; }
        public string DbName { set; get; }
        public string CollectionName { set; get; }
    }
}
