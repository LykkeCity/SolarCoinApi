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
        public QueueSettings CashOutQueue { set; get; }
        public QueueSettings TransitQueue { set; get; }
        public RpcSettings Rpc { set; get; }
        public string HotWalletAddress { set; get; }
        public decimal TxFee { set; get; }
        public decimal MinTxAmount { set; get; }
        public bool VerboseLogging { set; get; }

        public void Validate()
        {
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

            if (string.IsNullOrWhiteSpace(CashOutQueue.ConnectionString))
                throw new Exception("Cash Out Queue Connection String should be present");
            if (string.IsNullOrWhiteSpace(CashOutQueue.Name))
                throw new Exception("Cash Out Queue should be present");

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
