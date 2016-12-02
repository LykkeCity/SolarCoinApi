using SolarCoinApi.Core.Timers;
using System.Threading.Tasks;
using SolarCoinApi.Core.Log;
using MongoDB.Driver;
using SolarCoinApi.AzureStorage.Queue;
using MongoDB.Driver.Linq;
using System.Linq;
using SolarCoinApi.Core;

namespace SolarCoinApi.CashInGrabberJob
{
    public class CashInGrabberJob : TimerPeriodEx
    {
        private IMongoCollection<TransactionMongoEntity> _blockchainExplorer;
        private IQueueExt _transitQueue;
        private int _threshold;
        private int _normalPeriodMs;

        public CashInGrabberJob(string componentName, int periodMs, ILog log, IMongoCollection<TransactionMongoEntity> blockchainExplorer, IQueueExt transitQueue, int threshold) : base(componentName, periodMs, log)
        {
            _blockchainExplorer = blockchainExplorer;
            _transitQueue = transitQueue;
            _threshold = threshold;
            _normalPeriodMs = periodMs;
        }

        private TransitQueueMessage ProduceTransitQueueMessage(TransactionMongoEntity entity)
        {
            var result = new TransitQueueMessage { TxId = entity.TxId };

            foreach(var vin in entity.Vins)
                result.Vins.Add(new Core.Vin { Address = vin.Addresses, Amount = vin.Amount });

            foreach (var vout in entity.Vouts)
                result.Vouts.Add(new Core.Vout { Address = vout.Addresses, Amount = vout.Amount });

            return result;
        }

        public override async Task Execute()
        {
            var newTxes = _blockchainExplorer.AsQueryable().Where(e => e.WasProcessed == false).Take(1000).ToList();

            if (newTxes.Count() == 0)
            {
                UpdatePeriod(_normalPeriodMs);
                return;
            }

            if(newTxes.Count() == _threshold)
            {
                UpdatePeriod(0);
            }

            if(newTxes.Count() < _threshold)
            {
                UpdatePeriod(_normalPeriodMs);
            }

            foreach(var tx in newTxes)
            {
                await _transitQueue.PutMessageAsync(ProduceTransitQueueMessage(tx));

                var filter = Builders<TransactionMongoEntity>.Filter.Eq("txid", tx.TxId);

                var update = Builders<TransactionMongoEntity>.Update.Set("wasprocessed", true);

                await _blockchainExplorer.UpdateOneAsync(filter, update);
            }
        }
    }
}
