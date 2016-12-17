using SolarCoinApi.Common;
using SolarCoinApi.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.CashInHandlerJobRunner
{
    public class CashInHandlerSettings : IValidatable
    {
        public TableSettings GeneratedWallets { set; get; }
        public LoggerSettings Logger { set; get; }
        public QueueSettings CashInQueue { set; get; }
        public TableSettings Monitoring { set; get; }
        public QueueSettings TransitQueue { set; get; }
        public QueueSettings SlackQueue { set; get; }
        public RpcSettings Rpc { set; get; }
        public string HotWalletAddress { set; get; }
        public decimal CashInTxFee { set; get; }
        public decimal CashInMinTxAmount { set; get; }
        public bool VerboseLogging { set; get; }

        public void Validate()
        {
            if (GeneratedWallets == null)
                throw new Exception("Generated Wallets section should be present");
            if (Logger == null)
                throw new Exception("Logger section should be present");
            if (CashInQueue == null)
                throw new Exception("CashIn Queue section should be present");
            if (Monitoring == null)
                throw new Exception("Monitoring section should be present");
            if (TransitQueue == null)
                throw new Exception("Transit Queue section should be present");
            if (SlackQueue == null)
                throw new Exception("Slack Queue section should be present");
            if (Rpc == null)
                throw new Exception("Rpc section should be present");


            if (string.IsNullOrWhiteSpace(HotWalletAddress))
                throw new Exception("Hot Wallet Address Key should be present");

            if (string.IsNullOrWhiteSpace(Logger.ConnectionString))
                throw new Exception("Logger Connection String should be present");
            if (string.IsNullOrWhiteSpace(Logger.ErrorTableName))
                throw new Exception("Logger Error Table Name should be present");
            if (string.IsNullOrWhiteSpace(Logger.InfoTableName))
                throw new Exception("Logger Info Table Name should be present");
            if (string.IsNullOrWhiteSpace(Logger.WarningTableName))
                throw new Exception("Logger Warning Table Name should be present");

            if (string.IsNullOrWhiteSpace(CashInQueue.ConnectionString))
                throw new Exception("Cash In Queue Connection String should be present");
            if (string.IsNullOrWhiteSpace(CashInQueue.Name))
                throw new Exception("Cash In Queue should be present");

            if (string.IsNullOrWhiteSpace(SlackQueue.ConnectionString))
                throw new Exception("Slack Queue Connection String should be present");
            if (string.IsNullOrWhiteSpace(SlackQueue.Name))
                throw new Exception("Slack Queue Name should be present");

            if (string.IsNullOrWhiteSpace(Monitoring.Name))
                throw new Exception("Monitoring Connection Name should be present");
            if (string.IsNullOrWhiteSpace(Monitoring.ConnectionString))
                throw new Exception("Monitoring Name should be present");

            if (string.IsNullOrWhiteSpace(GeneratedWallets.ConnectionString))
                throw new Exception("Generated Wallets Connection String should be present");
            if (string.IsNullOrWhiteSpace(GeneratedWallets.Name))
                throw new Exception("Generated Wallets Name should be present");


            if (string.IsNullOrWhiteSpace(TransitQueue.ConnectionString))
                throw new Exception("Transit Queue Connection String should be present");
            if (string.IsNullOrWhiteSpace(TransitQueue.Name))
                throw new Exception("Transit Queue should be present");

            if (string.IsNullOrWhiteSpace(Rpc.Endpoint))
                throw new Exception("RPC Endpoint should be present");
            if (string.IsNullOrWhiteSpace(Rpc.Username))
                throw new Exception("RPC Username should be present");
            if (string.IsNullOrWhiteSpace(Rpc.Password))
                throw new Exception("RPC Password should be present");
        }
    }
}
