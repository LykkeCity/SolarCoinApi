using Newtonsoft.Json;
using SolarCoinApi.AzureStorage.Queue;
using SolarCoinApi.CashOutJobRunner;
using SolarCoinApi.Common.Triggers.Attributes;
using SolarCoinApi.Core.Log;
using SolarCoinApi.RpcJson.JsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.CashOutJobRunner
{
    public class CashOutQueueTrigger
    {
        private IJsonRpcClient _rpcClient;
        private ILog _log;

        public CashOutQueueTrigger(IJsonRpcClient rpcClient, ILog log)
        {
            _rpcClient = rpcClient;
            _log = log;
        }

        [QueueTrigger("solar-out")]
        public async Task ReceiveMessage(ToSendMessageFromQueue message)
        {
            await _log.WriteInfo("", "", "", $"Cash out request grabbed: Address: '{message.Address}', Amount: {message.Amount}");

            var resultTxId = await _rpcClient.SendToAddress(message.Address, message.Amount);

            await _log.WriteInfo("", "", "", $"Cash out succeded. Resulting transaction Id: '{resultTxId}'");
        }
    }

    public class ToSendMessageFromQueue
    {
        public string Address { set; get; }
        public decimal Amount { set; get; }
    }
 }
