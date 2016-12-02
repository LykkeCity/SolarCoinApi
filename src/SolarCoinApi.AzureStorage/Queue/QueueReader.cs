using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SolarCoinApi.Core.Log;
using SolarCoinApi.Core.Timers;

namespace SolarCoinApi.AzureStorage.Queue
{
    public class AzureQueueReader : IQueueReader
    {
        private readonly int _visibilityTimeoutSeconds = (int)TimeSpan.FromMinutes(10).TotalSeconds;

        private readonly IQueueExt _queue;

        public AzureQueueReader(IQueueExt queue)
        {
            _queue = queue;
        }

        public async Task<IQueueMessage> GetMessageAsync()
        {
            var msg = await _queue.GetRawMessageAsync(_visibilityTimeoutSeconds);
            if (msg != null)
                return new AzureMessage(msg);
            return null;
        }

        public Task AddMessageAsync(string message)
        {
            return _queue.PutRawMessageAsync(message);
        }

        public Task FinishMessageAsync(IQueueMessage msg)
        {
            var internalMsg = (msg as AzureMessage)?.Msg;
            if (internalMsg != null)
                return _queue.FinishRawMessageAsync(internalMsg);
            return Task.CompletedTask;
        }

        public Task ReleaseMessageAsync(IQueueMessage msg)
        {
            var internalMsg = (msg as AzureMessage)?.Msg;
            if (internalMsg != null)
                return _queue.ReleaseRawMessageAsync(internalMsg);
            return Task.CompletedTask;
        }
    }
}
