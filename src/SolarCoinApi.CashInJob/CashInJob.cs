using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json;
using SolarCoinApi.AzureStorage;
using SolarCoinApi.AzureStorage.Queue;
using SolarCoinApi.Core;
using SolarCoinApi.Core.Log;
using SolarCoinApi.Core.Timers;
using SolarCoinApi.RpcJson.JsonRpc;

namespace SolarCoinApi.CashInJob
{
    public class CashInJob : TimerPeriod
    {
        private INoSQLTableStorage<WalletStorageEntity> _generatedWallets;
        private INoSQLTableStorage<ExistingTxEntity> _exisingTxes;
        private IMongoDatabase _blockchainExplorer;
        private AzureQueueExt _txesQueue;
        private IJsonRpcClient _rpcClient;

        public CashInJob(string componentName, int periodMs, ILog log,
            INoSQLTableStorage<WalletStorageEntity> generatedWallets, INoSQLTableStorage<ExistingTxEntity> existingTxes,
            IMongoDatabase blockchainExplorer, AzureQueueExt txesQueue, IJsonRpcClient rpcClient)
            : base(componentName, periodMs, log)
        {
            _generatedWallets = generatedWallets;
            _exisingTxes = existingTxes;
            _blockchainExplorer = blockchainExplorer;
            _txesQueue = txesQueue;
            _rpcClient = rpcClient;
        }

        public override async Task Execute()
        {
            Console.WriteLine("");
            await _log.WriteInfo(GetComponentName(), "", "", "Starting cycle");

            var collection = _blockchainExplorer.GetCollection<TransactionMongoEntity>("txes");

            await _generatedWallets.ScanDataAsync("part", async delegate (IEnumerable<WalletStorageEntity> entities)
            {
                foreach (var wallet in entities)
                {
                    await _log.WriteInfo(GetComponentName(), "", "", $"Beginning scanning address: {wallet.Address}");
                    var allTxesPerWallet = collection.AsQueryable()
                        .Where(e => e.Vouts.Any(y => y.Addresses == wallet.Address))
                        .Select(e => e).ToList();

                    var newTxesPerWallet = allTxesPerWallet.Where(x => _exisingTxes.All(y => y.RowKey != x.TxId));

                    var newTransactionMongoEntities = newTxesPerWallet as TransactionMongoEntity[] ?? newTxesPerWallet.ToArray();

                    await _log.WriteInfo(GetComponentName(), "", "", $"{newTransactionMongoEntities.Length} new transactions found");

                    foreach (var tx in newTransactionMongoEntities)
                    {
                        await _log.WriteInfo(GetComponentName(), "", "", $"  TxId: {tx.TxId}");
                        var ourVouts = tx.Vouts.Where(x => x.Addresses == wallet.Address);

                        decimal changeInSatoshis = ourVouts.Sum(x => x.Amount);
                        decimal changeInSlr = changeInSatoshis / 100000000m;

                        var outs = new List<object>();
                        for (int i = 0; i < tx.Vouts.Count; i++)
                        {
                            if (tx.Vouts[i].Addresses == wallet.Address)
                                outs.Add(new { txid = tx.TxId, vout = i });
                        }
                        
                        var dest = new Dictionary<string, decimal> { { "8Q7aVvbVkZviZw2oKnEeaNQoJtn1dEWSnz", changeInSlr - 0.1m } };

                        var rawTx = await _rpcClient.CreateRawTransaction(outs.ToArray(), dest);
                        var signedTx = await _rpcClient.SignRawTransaction(rawTx, wallet.PrivateKey);
                        var sentTx = await _rpcClient.SendRawTransaction(signedTx.Hex);
                        await _log.WriteInfo(GetComponentName(), "", "", $"{tx.TxId} ransferred to hot wallet");

                        await _txesQueue.PutRawMessageAsync(JsonConvert.SerializeObject(new { Address = wallet.Address, Amount = changeInSlr }));
                        await _log.WriteInfo(GetComponentName(), "", "", $"{tx.TxId} added to queue");

                        await _exisingTxes.InsertAsync(new ExistingTxEntity
                        {
                            PartitionKey = "part",
                            RowKey = tx.TxId,
                            TxId = tx.TxId
                        });
                        await _log.WriteInfo(GetComponentName(), "", "", $"{tx.TxId} added to the list of existing txes");
                    }
                }
            });

            await _log.WriteInfo(GetComponentName(), "", "", "Cycle ended");
            /*
            foreach (var wallet in _generatedWallets)
            {
                //var allTxesPerWallet = collection.FindSync(x => x.Vouts.Any(y => y.Addresses == wallet.Address)).ToList();

                var allTxesPerWallet = collection.AsQueryable()
                    .Where(e => e.Vouts.Any(y => y.Addresses == wallet.Address))
                    .Select(e => e);

                var newTxesPerWallet = allTxesPerWallet.Where(x => _exisingTxes.All(y => y.TxId != x.TxId));

                foreach (var tx in newTxesPerWallet)
                {
                    var ourVouts = tx.Vouts.Where(x => x.Addresses == wallet.Address);

                    decimal changeInSatoshis = ourVouts.Sum(x => x.Amount);

                    var outs = new List<object>();
                    for (int i = 0; i < tx.Vouts.Count; i++)
                    {
                        if(tx.Vouts[i].Addresses == wallet.Address)
                            outs.Add(new {txid = tx.TxId, vout = i });
                    }

                    var dest = new Dictionary<string, decimal> {{"hotwalletaddress", changeInSatoshis - 0.1m}};

                    var rawTx = await _rpcClient.CreateRawTransaction(outs.ToArray(), dest);
                    var signedTx = await _rpcClient.SignRawTransaction(rawTx, wallet.PrivateKey);
                    var sentTx = await _rpcClient.SendRawTransaction(signedTx.Hex);

                    await _txesQueue.PutRawMessageAsync(JsonConvert.SerializeObject(new { Address = wallet.Address, Amount = changeInSatoshis / 100000000m }));
                    await _exisingTxes.InsertAsync(new ExistingTxEntity
                    {
                        PartitionKey = "part",
                        RowKey = tx.TxId,
                        TxId = tx.TxId
                    }); 
                }
                */

        }
    }
}
