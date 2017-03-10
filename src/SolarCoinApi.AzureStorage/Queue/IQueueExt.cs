﻿using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;

namespace SolarCoinApi.AzureStorage.Queue
{
    public class QueueData
    {
        public object Token { get; set; }
        public object Data { get; set; }
    }

    public class QueueType
    {
        public string Id { get; set; }
        public Type Type { get; set; }

        public static QueueType Create(string id, Type type)
        {
            return new QueueType
            {
                Id = id,
                Type = type
            };
        }
    }

    public interface IQueueExt
    {
        Task PutRawMessageAsync(string msg);
        Task<string> PutMessageAsync(object itm);

        Task<QueueData> GetMessageAsync();
        Task FinishMessageAsync(QueueData token);

        Task<object[]> GetMessagesAsync(int maxCount);

        Task ClearAsync();

        void RegisterTypes(params QueueType[] type);
        Task<CloudQueueMessage> GetRawMessageAsync(int visibilityTimeoutSeconds = 30);
        Task FinishRawMessageAsync(CloudQueueMessage msg);
        Task ReleaseRawMessageAsync(CloudQueueMessage msg);
    }
}
