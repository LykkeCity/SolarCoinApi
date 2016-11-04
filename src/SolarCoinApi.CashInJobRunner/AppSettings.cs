using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.CashInJobRunner
{
    public class AppSettings
    {
        private AppSettings() { }

        public static AppSettings FromFile(string fileName)
        {
            var txt = File.ReadAllText(fileName);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AppSettings>(txt);
        }
        public double PeriodMs { set; get; }
        public ExplorerSettings Explorer { set; get; }
        public ExistingTxesStorageSettings ExistingTxesStorage { set; get; }
        public WalletsSettings GeneratedWalletsStorage { set; get; }
        public QueueSettings Queue { set; get; }
        public LoggerSettings Logger { set; get; }
        public RpcSettings Rpc { set; get; }
    }

    public class ExplorerSettings
    {
        public string Host { set; get; }
        public string Port { set; get; }
        public string DbName { set; get; }
    }

    public class ExistingTxesStorageSettings
    {
        public string Name { set; get; }
        public string ConnectionString { set; get; }
    }

    public class WalletsSettings
    {
        public string Name { set; get; }
        public string ConnectionString { set; get; }
    }

    public class QueueSettings
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
