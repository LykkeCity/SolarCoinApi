using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Common
{
    public interface IQueueMessage
    {
        object Value(Type type);
        string AsString { get; }
        int DequeueCount { get; }
        DateTimeOffset InsertionTime { get; }
    }

    public interface IQueueReader
    {
        Task<IQueueMessage> GetMessageAsync();
        Task AddMessageAsync(string message);
        Task FinishMessageAsync(IQueueMessage msg);
        Task ReleaseMessageAsync(IQueueMessage msg);
        Task<int> Count();
    }
}
