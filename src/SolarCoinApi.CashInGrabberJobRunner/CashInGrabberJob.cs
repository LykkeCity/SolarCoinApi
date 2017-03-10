using SolarCoinApi.Core.Timers;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Linq;
using SolarCoinApi.Core;
using Newtonsoft.Json;
using System;
using AzureStorage.Queue;
using Common.Log;

namespace SolarCoinApi.CashInGrabberJobRunner
{
    public class CashInGrabberJob : TimerPeriodEx
    {
        private IMongoCollection<TransactionMongoEntity> _blockchainExplorer;
        private IQueueExt _transitQueue;
        private int _threshold;
        private int _normalPeriodMs;
        private ISlackNotifier _slackNotifier;

        public CashInGrabberJob(string componentName, int periodMs, ILog log, IMongoCollection<TransactionMongoEntity> blockchainExplorer, IQueueExt transitQueue, ISlackNotifier slackNotifier, int threshold) : base(componentName, periodMs, log)
        {
            _blockchainExplorer = blockchainExplorer;
            _transitQueue = transitQueue;
            _threshold = threshold;
            _normalPeriodMs = periodMs;
            _slackNotifier = slackNotifier;
        }
        
        public override async Task Execute()
        {
            try
            {
                var newTxes = _blockchainExplorer.Find(Builders<TransactionMongoEntity>.Filter.Exists(d => d.WasProcessed, false))
                  .Limit(_threshold)
                  .ToList();

                if (newTxes.Count() == 0)
                {
                    await _log.WriteInfoAsync(GetComponentName(), "", "", "No unprocessed tx-es found. Reseting period to normal");
                    UpdatePeriod(_normalPeriodMs);
                    return;
                }

                if (newTxes.Count() == _threshold)
                {
                    await _log.WriteInfoAsync(GetComponentName(), "", "", "Threshold reached. Minifying period.");
                    UpdatePeriod(0);
                }

                if (newTxes.Count() < _threshold)
                {
                    await _log.WriteInfoAsync(GetComponentName(), "", "", $"{newTxes.Count()} unprocessed tx-es found. Reseting period to normal. Processing...");
                    UpdatePeriod(_normalPeriodMs);
                }

                foreach (var tx in newTxes)
                {
                    await _log.WriteInfoAsync(GetComponentName(), "", tx.TxId, "Preparing to process");

                    await _transitQueue.PutRawMessageAsync(JsonConvert.SerializeObject(tx.ToTransitQueueMessage()));

                    var filter = Builders<TransactionMongoEntity>.Filter.Eq("txid", tx.TxId);

                    var update = Builders<TransactionMongoEntity>.Update.Set("wasprocessed", true);

                    await _blockchainExplorer.UpdateOneAsync(filter, update);
                }

                await _log.WriteInfoAsync(GetComponentName(), "", "", $"{newTxes.Count()} tx-es successfully processed!");
            }
            catch (Exception e)
            {
                await _slackNotifier.Notify(new SlackMessage { Sender = "CashInGrabberJob", Type = "Errors", Message = "Error occured during transfer from mongo" });
                throw;
            }
        }
    }
}
