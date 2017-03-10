using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.AzureStorage.Queue
{
    public interface IQueueMessage
    {
        object Value(Type type);
        string AsString { get; }
        int DequeueCount { get; }
        DateTimeOffset InsertionTime { get; }
    }
}
