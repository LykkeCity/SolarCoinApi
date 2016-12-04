using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SolarCoinApi.Common;
using SolarCoinApi.Core.Settings;

namespace SolarCoinApi.CashOutJobRunner
{

    public class CashOutSettings : IValidatable
    {

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(HotWalletPrivKey))
                throw new Exception("Hot Wallet Private Key should be present");

            if (string.IsNullOrWhiteSpace(Logger.ConnectionString))
                throw new Exception("Logger Connection String should be present");
            if (string.IsNullOrWhiteSpace(Logger.ErrorTableName))
                throw new Exception("Logger Error Table Name should be present");
            if (string.IsNullOrWhiteSpace(Logger.InfoTableName))
                throw new Exception("Logger Info Table Name should be present");
            if (string.IsNullOrWhiteSpace(Logger.WarningTableName))
                throw new Exception("Logger Warning Table Name should be present");

            if (string.IsNullOrWhiteSpace(ExistingTxes.ConnectionString))
                throw new Exception("Existing Transaction Connection String should be present");
            if (string.IsNullOrWhiteSpace(ExistingTxes.Name))
                throw new Exception("Existing Transaction Table Name should be present");

            if (string.IsNullOrWhiteSpace(Queue.ConnectionString))
                throw new Exception("Queue Connection String should be present");
            if (string.IsNullOrWhiteSpace(Queue.Name))
                throw new Exception("Queue Name should be present");

            if (string.IsNullOrWhiteSpace(Rpc.Endpoint))
                throw new Exception("RPC Endpoint should be present");
            if (string.IsNullOrWhiteSpace(Rpc.Username))
                throw new Exception("RPC Username should be present");
            if (string.IsNullOrWhiteSpace(Rpc.Password))
                throw new Exception("RPC Password should be present");
        }
        
        public int PeriodMs { set; get; }
        public bool VerboseLogging { set; get; }
        public string HotWalletPrivKey { set; get; }
        public QueueSettings Queue { set; get; }
        public TableSettings ExistingTxes { set; get; }
        public LoggerSettings Logger { set; get; }
        public RpcSettings Rpc { set; get; }
    }
}
