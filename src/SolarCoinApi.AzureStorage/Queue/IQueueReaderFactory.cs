using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.AzureStorage.Queue
{
    public interface IQueueReaderFactory
    {
        IQueueReader Create(string queueName);
    }
}
