using SolarCoinApi.Common;
using SolarCoinApi.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Facade
{
    public class FacadeSettings : IValidatable
    {
        public RpcSettings Rpc { set; get; }
        public LoggerSettings Logger { set; get; }
        public QueueSettings SlackQueue { set; get; }
        public TableSettings GeneratedWallets { set; get; }

        public void Validate()
        {

            if (GeneratedWallets == null)
                throw new Exception("Generated Wallets section should be present");
            if (Logger == null)
                throw new Exception("Logger section should be present");
            if (SlackQueue == null)
                throw new Exception("Slack Queue section should be present");
            if (Rpc == null)
                throw new Exception("Rpc section should be present");



            if (string.IsNullOrWhiteSpace(Logger.ConnectionString))
                throw new Exception("Logger Connection String should be present");
            if (string.IsNullOrWhiteSpace(Logger.ErrorTableName))
                throw new Exception("Logger Error Table Name should be present");
            if (string.IsNullOrWhiteSpace(Logger.InfoTableName))
                throw new Exception("Logger Info Table Name should be present");
            if (string.IsNullOrWhiteSpace(Logger.WarningTableName))
                throw new Exception("Logger Warning Table Name should be present");

            if (string.IsNullOrWhiteSpace(SlackQueue.ConnectionString))
                throw new Exception("Slack Queue Connection String should be present");
            if (string.IsNullOrWhiteSpace(SlackQueue.Name))
                throw new Exception("Slack Queue should be present");

            if (string.IsNullOrWhiteSpace(Rpc.Endpoint))
                throw new Exception("RPC Endpoint should be present");
            if (string.IsNullOrWhiteSpace(Rpc.Username))
                throw new Exception("RPC Username should be present");
            if (string.IsNullOrWhiteSpace(Rpc.Password))
                throw new Exception("RPC Password should be present");

            if (string.IsNullOrWhiteSpace(GeneratedWallets.ConnectionString))
                throw new Exception("Generated Wallets Connection String should be present");
            if (string.IsNullOrWhiteSpace(GeneratedWallets.Name))
                throw new Exception("Generated Wallets Name should be present");

        }
    }
}
