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
        private string _hotWalletAddress;
        private decimal _minTxAmount;

        public CashInJob(string componentName, int periodMs, ILog log,
            INoSQLTableStorage<WalletStorageEntity> generatedWallets, INoSQLTableStorage<ExistingTxEntity> existingTxes,
            IMongoDatabase blockchainExplorer, AzureQueueExt txesQueue, IJsonRpcClient rpcClient, string hotWalletAddress, decimal minTxAmount)
            : base(componentName, periodMs, log)
        {
            _generatedWallets = generatedWallets;
            _exisingTxes = existingTxes;
            _blockchainExplorer = blockchainExplorer;
            _txesQueue = txesQueue;
            _rpcClient = rpcClient;
            _hotWalletAddress = hotWalletAddress;
            _minTxAmount = minTxAmount;
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

                        await _exisingTxes.InsertAsync(new ExistingTxEntity
                        {
                            PartitionKey = "part",
                            RowKey = tx.TxId,
                            TxId = tx.TxId
                        });
                        await _log.WriteInfo(GetComponentName(), "", "", $"{tx.TxId} added to the list of existing txes");

                        if (changeInSlr < _minTxAmount)
                        {
                            await _log.WriteWarning(GetComponentName(), "", "", $"The amount for a transaction '{tx.TxId}' was {changeInSlr}, should be more than {_minTxAmount}");
                            continue;
                        }

                        var outs = new List<object>();
                        for (int i = 0; i < tx.Vouts.Count; i++)
                        {
                            if (tx.Vouts[i].Addresses == wallet.Address)
                                outs.Add(new { txid = tx.TxId, vout = i });
                        }
                        
                        var dest = new Dictionary<string, decimal> { { _hotWalletAddress, changeInSlr - 0.1m } };

                        var rawTx = await _rpcClient.CreateRawTransaction(outs.ToArray(), dest);
                        var signedTx = await _rpcClient.SignRawTransaction(rawTx, wallet.PrivateKey);
                        var sentTx = await _rpcClient.SendRawTransaction(signedTx.Hex);
                        await _log.WriteInfo(GetComponentName(), "", "", $"'{tx.TxId}' transferred to hot wallet resulting in tx with id '{sentTx}'");

                        await _txesQueue.PutRawMessageAsync(JsonConvert.SerializeObject(new { Address = wallet.Address, Amount = changeInSlr }));
                        await _log.WriteInfo(GetComponentName(), "", "", $"{tx.TxId} added to queue");
                        
                    }
                }
            });

            await _log.WriteInfo(GetComponentName(), "", "", "Cycle ended");
            

        }
    }
}
