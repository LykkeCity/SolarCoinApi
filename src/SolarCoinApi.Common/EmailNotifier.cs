using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Lykke.EmailProvider.Models;
using Newtonsoft.Json;
using SolarCoinApi.Core;

namespace SolarCoinApi.Common
{
    public class EmailNotifier : IEmailNotifier
    {
        private IQueueExt _emailQueue;
        private List<string> _to;

        public EmailNotifier(List<string> to, IQueueExt emailQueue)
        {
            _to = to;
            _emailQueue = emailQueue;
        }

        public async Task Notify(string subject, string content)
        {
            await _emailQueue.PutRawMessageAsync(JsonConvert.SerializeObject(new EmailMessage {To = _to, Subject = subject, Body = content}));
        }
    }
}
