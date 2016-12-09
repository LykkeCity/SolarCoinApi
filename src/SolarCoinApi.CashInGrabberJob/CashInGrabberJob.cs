using SolarCoinApi.Core.Timers;
using System.Threading.Tasks;
using SolarCoinApi.Core.Log;
using MongoDB.Driver;
using SolarCoinApi.AzureStorage.Queue;
using MongoDB.Driver.Linq;
using System.Linq;
using SolarCoinApi.Core;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace SolarCoinApi.CashInGrabberJob
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
                    await _log.WriteInfo(GetComponentName(), "", "", "No unprocessed tx-es found. Reseting period to normal");
                    UpdatePeriod(_normalPeriodMs);
                    return;
                }

                if (newTxes.Count() == _threshold)
                {
                    await _log.WriteInfo(GetComponentName(), "", "", "Threshold reached. Minifying period.");
                    UpdatePeriod(0);
                }

                if (newTxes.Count() < _threshold)
                {
                    await _log.WriteInfo(GetComponentName(), "", "", $"{newTxes.Count()} unprocessed tx-es found. Reseting period to normal. Processing...");
                    UpdatePeriod(_normalPeriodMs);
                }

                foreach (var tx in newTxes)
                {
                    await _transitQueue.PutRawMessageAsync(JsonConvert.SerializeObject(tx.ToTransitQueueMessage()));

                    var filter = Builders<TransactionMongoEntity>.Filter.Eq("txid", tx.TxId);

                    var update = Builders<TransactionMongoEntity>.Update.Set("wasprocessed", true);

                    await _blockchainExplorer.UpdateOneAsync(filter, update);
                }

                await _log.WriteInfo(GetComponentName(), "", "", $"{newTxes.Count()} tx-es successfully processed!");
            }
            catch (Exception e)
            {
                await _slackNotifier.Notify(new SlackMessage { Sender = "CashInGrabberJob", Type = "Error", Message = "Error occured during transfer from mongo" });
                throw;
            }
        }
    }

    public static class Helper
    {
        public static TransitQueueMessage ToTransitQueueMessage(this TransactionMongoEntity entity)
        {
            var result = new TransitQueueMessage { TxId = entity.TxId };

            foreach (var vin in entity.Vins)
                result.Vins.Add(new Vin { Address = vin.Addresses, Amount = vin.Amount });

            foreach (var vout in entity.Vouts)
                result.Vouts.Add(new Vout { Address = vout.Addresses, Amount = vout.Amount });

            return result;
        }
    }
}
