using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;
using MongoDB.Driver;
using SolarCoinApi.Core;

namespace SolarCoinApi.Alerts
{
    public class AlertsJob
    {
        private string _componentName;
        private ILog _log;
        private IEmailNotifier _emailNotifier;
        private IMongoCollection<TransactionMongoEntity> _blockchainExplorer;
        private IQueueExt _transitQueue;
        private IQueueExt _cashoutQueue;


        public AlertsJob(string componentName, IMongoCollection<TransactionMongoEntity> blockchainExplorer, IQueueExt transitQueue, IQueueExt cashoutQueue, ILog log, IEmailNotifier emailNotifier)
        {
            _componentName = componentName + ".Job";
            _log = log;
            _emailNotifier = emailNotifier;
            _blockchainExplorer = blockchainExplorer;
            _transitQueue = transitQueue;
            _cashoutQueue = cashoutQueue;
        }

        [TimerTrigger("00:01:00")]
        public async Task Execute()
        {
            await _log.WriteInfoAsync(_componentName, "", "", "Cycle started");


            var filterBuilder = Builders<TransactionMongoEntity>.Filter;
            var txesFilter = filterBuilder.Eq(x => x.WasProcessed, false) | filterBuilder.Exists(x => x.WasProcessed, false);


            var numTxes = await _blockchainExplorer.CountAsync(txesFilter);

            if (numTxes < 100)
            {
                //await _emailNotifier.Notify("a", "bbb");
            }

            if (await _transitQueue.Count() > 100)
            {
                //await _emailNotifier.Notify("", "");
            }

            if (await _cashoutQueue.Count() > 5)
            {
                //await _emailNotifier.Notify("", "");
            }
        }
    }
}
