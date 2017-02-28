using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Queue;

namespace SolarCoinApi.Common
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
