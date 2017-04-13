using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using SolarCoinApi.CashOutJobRunner;
using SolarCoinApi.Core;
using SolarCoinApi.RpcJson.JsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;

namespace SolarCoinApi.CashOutJobRunner
{
    public class CashOutQueueTrigger
    {
        private IJsonRpcClient _rpcClient;
        private INoSQLTableStorage<ExistingCashOutEntity> _existingTxes;
        private ILog _log;
        private ISlackNotifier _slackNotifier;
        private string _component;

        public CashOutQueueTrigger(string component, IJsonRpcClient rpcClient, INoSQLTableStorage<ExistingCashOutEntity> existingTxes, ILog log, ISlackNotifier slackNotifier)
        {
            _component = component + ".QueueTrigger";
            _rpcClient = rpcClient;
            _existingTxes = existingTxes;
            _log = log;
            _slackNotifier = slackNotifier;
        }
        
        [QueueTrigger("solar-out")]
        public async Task Process(ToSendMessageFromQueue message)
        {
            try
            {
                await _log.WriteInfoAsync(_component, "", message.Id, $"Cash out request grabbed: Address: '{message.Address}', Amount: {message.Amount}");

                if (_existingTxes.Any(x => x.RowKey == message.Id))
                    return;
                
                var resultTxId = await _rpcClient.SendToAddress(message.Address, message.Amount);

                await _existingTxes.InsertAsync(new ExistingCashOutEntity { PartitionKey = "part", RowKey = message.Id });

                await _log.WriteInfoAsync(_component, "", message.Id, $"Cash out succeded. Resulting transaction Id: '{resultTxId}'");
            }
            catch (Exception e)
            {
                //await _slackNotifier.Notify(new SlackMessage { Sender = _component, Type = "Errors", Message = "Error occured during cashout" });
                await _log.WriteErrorAsync(_component, "", message.Id, e);
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
