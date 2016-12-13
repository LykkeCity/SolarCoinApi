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
        public int PeriodMs { set; get; }
        public bool VerboseLogging { set; get; }
        public string HotWalletPrivKey { set; get; }
        public QueueSettings CashOutQueue { set; get; }
        public QueueSettings SlackQueue { set; get; }
        public TableSettings Monitoring { set; get; }
        public TableSettings ExistingTxes { set; get; }
        public LoggerSettings Logger { set; get; }
        public RpcSettings Rpc { set; get; }

        public void Validate()
        {
            if (Logger == null)
                throw new Exception("Logger section should be present");
            if (CashOutQueue == null)
                throw new Exception("CashOut Queue section should be present");
            if (Monitoring == null)
                throw new Exception("Monitoring section should be present");
            if (SlackQueue == null)
                throw new Exception("Slack Queue section should be present");
            if (ExistingTxes == null)
                throw new Exception("Existing Txes section should be present");
            if (Rpc == null)
                throw new Exception("Rpc section should be present");


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

            if (string.IsNullOrWhiteSpace(Monitoring.Name))
                throw new Exception("Monitoring Connection Name should be present");
            if (string.IsNullOrWhiteSpace(Monitoring.ConnectionString))
                throw new Exception("Monitoring Name should be present");

            if (string.IsNullOrWhiteSpace(SlackQueue.ConnectionString))
                throw new Exception("Slack Queue Connection String should be present");
            if (string.IsNullOrWhiteSpace(SlackQueue.Name))
                throw new Exception("Slack Queue Name should be present");

            if (string.IsNullOrWhiteSpace(CashOutQueue.ConnectionString))
                throw new Exception("Queue Connection String should be present");
            if (string.IsNullOrWhiteSpace(CashOutQueue.Name))
                throw new Exception("Queue Name should be present");

            if (string.IsNullOrWhiteSpace(Rpc.Endpoint))
                throw new Exception("RPC Endpoint should be present");
            if (string.IsNullOrWhiteSpace(Rpc.Username))
                throw new Exception("RPC Username should be present");
            if (string.IsNullOrWhiteSpace(Rpc.Password))
                throw new Exception("RPC Password should be present");
        }
        
    }
}
