using SolarCoinApi.AzureStorage.Queue;
using SolarCoinApi.Common;
using SolarCoinApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace SolarCoinApi.Common
{
    public class SlackNotifier : ISlackNotifier
    {
        private IQueueExt _slackQueue;
        public SlackNotifier(IQueueExt slackQueue)
        {
            _slackQueue = slackQueue;
        }
        public async Task Notify(SlackMessage message)
        {
            try
            {
                await _slackQueue.PutRawMessageAsync(JsonConvert.SerializeObject(message));
            }
            catch (Exception e)
            {

            }
        }
    }
}
