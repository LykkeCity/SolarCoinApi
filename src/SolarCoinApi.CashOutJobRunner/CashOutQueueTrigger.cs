﻿using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using SolarCoinApi.AzureStorage;
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
        private INoSQLTableStorage<ExistingCashOutEntity> _existingTxes;
        private ILog _log;

        public CashOutQueueTrigger(IJsonRpcClient rpcClient, INoSQLTableStorage<ExistingCashOutEntity> existingTxes, ILog log)
        {
            _rpcClient = rpcClient;
            _existingTxes = existingTxes;
            _log = log;
        }

        [QueueTrigger("solar-out")]
        public async Task ReceiveMessage(ToSendMessageFromQueue message)
        {
            await _log.WriteInfo("", "", "", $"Cash out request grabbed: Address: '{message.Address}', Amount: {message.Amount}");

            if (_existingTxes.Any(x => x.RowKey == message.Id))
                return;

            var resultTxId = await _rpcClient.SendToAddress(message.Address, message.Amount);

            await _existingTxes.InsertAsync(new ExistingCashOutEntity { PartitionKey = "part", RowKey = message.Id });

            await _log.WriteInfo("", "", "", $"Cash out succeded. Resulting transaction Id: '{resultTxId}'");
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
