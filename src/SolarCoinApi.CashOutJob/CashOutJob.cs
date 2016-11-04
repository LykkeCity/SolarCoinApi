using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using SolarCoinApi.AzureStorage;
using SolarCoinApi.AzureStorage.Queue;
using SolarCoinApi.AzureStorage.Tables;
using SolarCoinApi.Core.Log;
using SolarCoinApi.Core.Timers;
using SolarCoinApi.RpcJson.JsonRpc;

namespace SolarCoinApi.CashOutJob
{
    public class CashOutJob : TimerPeriod
    {
        private INoSQLTableStorage<HotWalletUsableTxEntity> _usableTxes;
        private AzureQueueExt _queue;
        private HotWallet _hotWallet;
        private IJsonRpcClient _rpcClient;
        private decimal TX_PRICE = 0.001m;

        public CashOutJob(string componentName, int periodMs, ILog log)
            :base(componentName, periodMs, log)
        {
        }

        public override async Task Execute()
        {
            var numInQueue = await _queue.Count();

            if (!numInQueue.HasValue || numInQueue.Value == 0)
                return;

            var toSendRaw = await _queue.PeekRawMessageAsync();
            var toSend = JsonConvert.DeserializeObject<ToSendMessageFromQueue>(toSendRaw.AsString);

            var usableTx = await _usableTxes.GetTopRecordAsync("part");
            while (usableTx.Used)
            {
                await Task.Delay(500);
                usableTx = await _usableTxes.GetTopRecordAsync("part");
            }


            var recip = new Dictionary<string, decimal>
            {
                {_hotWallet.Address, usableTx.Available - toSend.Amount - TX_PRICE},
                {toSend.Address, toSend.Amount}
            };

            var rawTransaction = await _rpcClient.CreateRawTransaction(new object[] { new { txid = usableTx.Output, vout = 0 } }, recip);

            var signedTransaction = await _rpcClient.SignRawTransaction(rawTransaction, _hotWallet.PrivKey);

            var sentTxHash = await _rpcClient.SendRawTransaction(signedTransaction.Hex);

            usableTx.Used = true;

            await _usableTxes.InsertAsync(new HotWalletUsableTxEntity
            {
                PartitionKey = "part",
                RowKey = IdGenerator.GenerateDateTimeIdNewFirst(DateTime.Now),
                Available = usableTx.Available - toSend.Amount - TX_PRICE,
                Output = 0,
                TxId = sentTxHash,
                Used = false
            });
        }
    }

    public class HotWalletUsableTxEntity : TableEntity
    {
        public string TxId { set; get; }
        public int Output { set; get; }
        public decimal Available { set; get; }
        public bool Used { set; get; }
    }

    public class ToSendMessageFromQueue
    {
        public string Address { set; get; }
        public decimal Amount { set; get; }
    }

    public class HotWallet
    {
        public string Address { set; get; }
        public string PrivKey { set; get; }
    }

    public static class IdGenerator
    {
        public static string GenerateDateTimeId(DateTime creationDateTime)
        {
            return $"{creationDateTime.Ticks:d19}_{Guid.NewGuid():N}";
        }

        public static string GenerateDateTimeIdNewFirst(DateTime creationDateTime)
        {
            return $"{(DateTime.MaxValue.Ticks - creationDateTime.Ticks):d19}_{Guid.NewGuid():N}";
        }
    }
}
