using System.Threading.Tasks;
using MongoDB.Driver;
using SolarCoinApi.Core;
using Newtonsoft.Json;
using System;
using AzureStorage.Queue;
using Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;

namespace SolarCoinApi.CashInGrabberJobRunner
{
    public class CashInGrabberJob
    {
        private IMongoCollection<TransactionMongoEntity> _blockchainExplorer;
        private IQueueExt _transitQueue;
        private int _threshold;
        private string _componentName;
        private ILog _log;

        public CashInGrabberJob(string componentName, ILog log, IMongoCollection<TransactionMongoEntity> blockchainExplorer, IQueueExt transitQueue, int threshold)
        {
            _componentName = componentName + ".Job";
            _blockchainExplorer = blockchainExplorer;
            _transitQueue = transitQueue;
            _threshold = threshold;
            _log = log;
        }

        [TimerTrigger("00:00:10")]
        public async Task Execute()
        {
            await _log.WriteInfoAsync(_componentName, "", "", "Begining to process");

            var filterBuilder = Builders<TransactionMongoEntity>.Filter;
            var txesFilter = filterBuilder.Eq(x => x.WasProcessed, false) | filterBuilder.Exists(x => x.WasProcessed, false);
            
            var newTxes = _blockchainExplorer.Find(txesFilter)
              .Limit(_threshold)
              .ToList();
            

            foreach (var tx in newTxes)
            {
                try
                {
                    await _log.WriteInfoAsync(_componentName, "", tx.TxId, "Preparing to process");

                    await _transitQueue.PutRawMessageAsync(JsonConvert.SerializeObject(tx.ToTransitQueueMessage()));

                    var filter = Builders<TransactionMongoEntity>.Filter.Eq("txid", tx.TxId);

                    var update = Builders<TransactionMongoEntity>.Update.Set("wasprocessed", true);

                    await _blockchainExplorer.UpdateOneAsync(filter, update);

                    await _log.WriteInfoAsync(_componentName, "", tx.TxId, "Successfully processed");
                }
                catch (Exception e)
                {
                    await _log.WriteErrorAsync(_componentName, "", tx.TxId, e);
                }
            }

            await _log.WriteInfoAsync(_componentName, "", "", $"{newTxes.Count} tx-es successfully processed!");
        }
    }
}
