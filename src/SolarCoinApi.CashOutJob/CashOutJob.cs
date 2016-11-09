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
        private AzureQueueExt _queue;
        private IJsonRpcClient _rpcClient;

        public CashOutJob(string componentName, int periodMs, ILog log, AzureQueueExt queue, IJsonRpcClient rpcClient)
            :base(componentName, periodMs, log)
        {
            _queue = queue;
            _rpcClient = rpcClient;
        }

        public override async Task Execute()
        {
            var numInQueue = await _queue.Count();

            if (!numInQueue.HasValue || numInQueue.Value == 0)
                return;

            var toSendRaw = await _queue.PeekRawMessageAsync();
            var toSend = JsonConvert.DeserializeObject<ToSendMessageFromQueue>(toSendRaw.AsString);

            await _log.WriteInfo("CashOutJob", "", "", $"Cash out request grabbed: Address: '{toSend.Address}', Amount: {toSend.Amount}");

            var resultTxId = await _rpcClient.SendToAddress(toSend.Address, toSend.Amount);

            await _log.WriteInfo("CashOutJob", "", "", $"Cash out succeded. Resulting transaction Id: '{resultTxId}'");

            var popped = await _queue.GetRawMessageAsync();
        }
    }

    public class ToSendMessageFromQueue
    {
        public string Address { set; get; }
        public decimal Amount { set; get; }
    }
}
