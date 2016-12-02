using Microsoft.WindowsAzure.Storage.Queue;
using SolarCoinApi.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SolarCoinApi.AzureStorage.Queue
{
    public class AzureMessage : IQueueMessage
    {
        internal readonly CloudQueueMessage Msg;

        public AzureMessage(CloudQueueMessage msg)
        {
            Msg = msg;
        }

        public object Value(Type type)
        {
            if (Msg == null)
                return null;
            if (type == typeof(string))
                return Msg.AsString;
            if (type.GetTypeInfo().IsValueType)
                return Convert.ChangeType(Msg.AsString, type);

            return Msg.AsString.DeserializeJson(type);
        }

        public string AsString => Msg.AsString;

        public int DequeueCount => Msg.DequeueCount;

        public DateTimeOffset InsertionTime => Msg.InsertionTime ?? DateTimeOffset.UtcNow;
    }
}
