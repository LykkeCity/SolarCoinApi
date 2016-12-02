using System;
using System.Threading.Tasks;
using SolarCoinApi.Core;
using SolarCoinApi.Core.Timers.Interfaces;

namespace SolarCoinApi.AzureStorage.Queue
{

    public interface IQueueReader
    {
        Task<IQueueMessage> GetMessageAsync();
        Task AddMessageAsync(string message);
        Task FinishMessageAsync(IQueueMessage msg);
        Task ReleaseMessageAsync(IQueueMessage msg);
    }
}
