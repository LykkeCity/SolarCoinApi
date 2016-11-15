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

        public static AppSettings FromFile(string filePath)
        {
            var txt = File.ReadAllText(filePath);
            var fileNameWithExtension = Path.GetFileName(filePath);

            var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<AppSettings>(txt);

            Validate(settings);

            return settings;
        }

        public static void Validate(AppSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.HotWalletAddress))
                throw new Exception("Hot Wallet Address should be present");

            if (string.IsNullOrWhiteSpace(settings.Logger.ConnectionString))
                throw new Exception("Logger Connection String should be present");
            if (string.IsNullOrWhiteSpace(settings.Logger.ErrorTableName))
                throw new Exception("Logger Error Table Name should be present");
            if (string.IsNullOrWhiteSpace(settings.Logger.InfoTableName))
                throw new Exception("Logger Info Table Name should be present");
            if (string.IsNullOrWhiteSpace(settings.Logger.WarningTableName))
                throw new Exception("Logger Warning Table Name should be present");

            if (string.IsNullOrWhiteSpace(settings.Queue.ConnectionString))
                throw new Exception("Queue Connection String should be present");
            if (string.IsNullOrWhiteSpace(settings.Queue.Name))
                throw new Exception("Queue Name should be present");

            if (string.IsNullOrWhiteSpace(settings.Rpc.Endpoint))
                throw new Exception("RPC Endpoint should be present");
            if (string.IsNullOrWhiteSpace(settings.Rpc.Username))
                throw new Exception("RPC Username should be present");
            if (string.IsNullOrWhiteSpace(settings.Rpc.Password))
                throw new Exception("RPC Password should be present");

            if (string.IsNullOrWhiteSpace(settings.Explorer.DbName))
                throw new Exception("Explorer's Database Name should be present");
            if (string.IsNullOrWhiteSpace(settings.Explorer.Host))
                throw new Exception("Explorer's Host Address should be present");
            if (string.IsNullOrWhiteSpace(settings.Explorer.Port))
                throw new Exception("Explorer's Database's Port should be present");

            if (string.IsNullOrWhiteSpace(settings.ExistingTxesStorage.Name))
                throw new Exception("Existing Transactions' Storage's Name should be prestent");
            if (string.IsNullOrWhiteSpace(settings.ExistingTxesStorage.ConnectionString))
                throw new Exception("Existing Transactions' Storage's Connection String should be prestent");

            if (string.IsNullOrWhiteSpace(settings.GeneratedWalletsStorage.Name))
                throw new Exception("Generated Wallet's Storage's Name should be present");
            if (string.IsNullOrWhiteSpace(settings.GeneratedWalletsStorage.ConnectionString))
                throw new Exception("Generated Wallet's Storage's Connection String should be present");
        }

        public int PeriodMs { set; get; }
        public string HotWalletAddress { get; set; }
        public decimal MinimumTxAmount { get; set; }
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
