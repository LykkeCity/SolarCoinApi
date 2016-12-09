using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Core
{
    public interface ISlackNotifier
    {
        Task Notify(SlackMessage message);
    }

    public class SlackMessage
    {
        public string Sender { set; get; }
        public string Type { set; get; }
        public string Message { set; get; }
    }
}
