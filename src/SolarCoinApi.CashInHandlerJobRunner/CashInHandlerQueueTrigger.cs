﻿using Newtonsoft.Json;
using SolarCoinApi.AzureStorage;
using SolarCoinApi.AzureStorage.Queue;
using SolarCoinApi.Common.Triggers.Attributes;
using SolarCoinApi.Core;
using SolarCoinApi.Core.Log;
using SolarCoinApi.Core.Settings;
using SolarCoinApi.RpcJson.JsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.CashInHandlerJobRunner
{
    public class CashInHandlerQueueTrigger
    {
        private INoSQLTableStorage<WalletStorageEntity> _generatedWallets;
        private ILog _log;
        private IQueueExt _txesQueue;
        private IJsonRpcClient _rpcClient;
        private string _hotWalletAddress;
        private decimal _txFee;
        private decimal _minTxAmount;

        public CashInHandlerQueueTrigger(INoSQLTableStorage<WalletStorageEntity> generatedWallets, ILog log, IQueueExt txesQueue,
            IJsonRpcClient rpcClient, string hotWalletAddress, decimal txFee, decimal minTxAmount)
        {
            _generatedWallets = generatedWallets;
            _log = log;
            _txesQueue = txesQueue;
            _rpcClient = rpcClient;
            _hotWalletAddress = hotWalletAddress;
            _txFee = txFee;
            _minTxAmount = minTxAmount;
        }

        [QueueTrigger("solar-transit")]
        public async Task ReceiveMessage(TransitQueueMessage message)
        {
            try
            {
                var ourVouts = new List<VoutEx>();

                // get outputs that where dedicated to our users
                for (int i = 0; i < message.Vouts.Count; i++)
                {
                    var vout = message.Vouts[i];

                    if (_generatedWallets.Any(x => x.Address == vout.Address))
                    {
                        ourVouts.Add(new VoutEx { Address = vout.Address, Amount = vout.Amount, voutId = i });
                    }
                }

                // if none of the outputs where dedicated to our users, return;
                if (ourVouts.Count == 0)
                {
                    await _log.WriteWarning("", "", "", "didn't contain relevant addresses");
                    return;
                }


                foreach (var addr in ourVouts.Select(x => x.Address).Distinct())
                {
                    var changePerAddress = ourVouts.Where(x => x.Address == addr).Sum(x => x.Amount);
                    var changePerAddressInSlr = changePerAddress / 100000000m;

                    if (changePerAddressInSlr < _minTxAmount)
                        continue;

                    var dest = new Dictionary<string, decimal> { { _hotWalletAddress, changePerAddressInSlr - _txFee } };

                    var userWallet = _generatedWallets.FirstOrDefault(x => x.Address == addr);

                    var rawTx = await _rpcClient.CreateRawTransaction(ourVouts.Where(x => x.Address == addr).Select(x => new { txid = message.TxId, vout = x.voutId }).ToArray(), dest);
                    var signedTx = await _rpcClient.SignRawTransaction(rawTx, userWallet.PrivateKey);
                    var sentTx = await _rpcClient.SendRawTransaction(signedTx.Hex);

                    await _txesQueue.PutRawMessageAsync(JsonConvert.SerializeObject(new QueueModel { Address = addr, Amount = changePerAddressInSlr, TxId = message.TxId }));
                }

            }
            catch (Exception e)
            {
                await _log.WriteError("CashInHandlerQueueTrigger", "", "", e);
            }
        }
    }
    public class VoutEx
    {
        public string Address { set; get; }
        public decimal Amount { set; get; }
        public int voutId { set; get; }
    }

    public class QueueModel
    {
        public string Address { set; get; }
        public decimal Amount { set; get; }
        public string TxId { set; get; }

        public bool Equals(QueueModel anotherModel)
        {
            return Address == anotherModel.Address && Amount == anotherModel.Amount && TxId == anotherModel.TxId;
        }
    }

}
