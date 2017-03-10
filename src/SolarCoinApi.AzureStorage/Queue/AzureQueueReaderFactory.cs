using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.AzureStorage.Queue
{
    public class AzureQueueReaderFactory : IQueueReaderFactory
    {
        private readonly string _connectionString;

        public AzureQueueReaderFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IQueueReader Create(string queueName)
        {
            return new AzureQueueReader(new AzureQueueExt(_connectionString, queueName));
        }
    }
}
