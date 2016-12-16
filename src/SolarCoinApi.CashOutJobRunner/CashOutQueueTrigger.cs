using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using SolarCoinApi.AzureStorage;
using SolarCoinApi.AzureStorage.Queue;
using SolarCoinApi.CashOutJobRunner;
using SolarCoinApi.Common.Triggers.Attributes;
using SolarCoinApi.Core;
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
        private INoSQLTableStorage<ExistingCashOutEntity> _existingTxes;
        private ILog _log;
        private ISlackNotifier _slackNotifier;

        public CashOutQueueTrigger(IJsonRpcClient rpcClient, INoSQLTableStorage<ExistingCashOutEntity> existingTxes, ILog log, ISlackNotifier slackNotifier)
        {
            _rpcClient = rpcClient;
            _existingTxes = existingTxes;
            _log = log;
            _slackNotifier = slackNotifier;
        }

        [QueueTrigger("solar-out")]
        public async Task ReceiveMessage(ToSendMessageFromQueue message)
        {
            try
            {
                await _log.WriteInfo("CashOutQueueTrigger", "", message.Id, $"Cash out request grabbed: Address: '{message.Address}', Amount: {message.Amount}");

                if (_existingTxes.Any(x => x.RowKey == message.Id))
                    return;

                await _existingTxes.InsertAsync(new ExistingCashOutEntity { PartitionKey = "part", RowKey = message.Id });

                var resultTxId = await _rpcClient.SendToAddress(message.Address, message.Amount);

                await _log.WriteInfo("CashOutQueueTrigger", "", message.Id, $"Cash out succeded. Resulting transaction Id: '{resultTxId}'");
            }
            catch (Exception e)
            {
                await _slackNotifier.Notify(new SlackMessage { Sender = "CashOutQueueTrigger", Type = "Errors", Message = "Error occured during cashout" });
                await _log.WriteError("CashOutQueueTrigger", "", message.Id, e);
                throw;
            }
        }
    }

    public class ToSendMessageFromQueue
    {
        public string Id { set; get; }
        public string Address { set; get; }
        public decimal Amount { set; get; }
    }

    public class ExistingCashOutEntity : TableEntity
    {
        
    }
 }
