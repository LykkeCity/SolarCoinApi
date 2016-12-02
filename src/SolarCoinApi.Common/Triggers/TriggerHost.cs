using SolarCoinApi.Common.Triggers.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SolarCoinApi.Common.Triggers
{
    public class TriggerHost
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly List<ITriggerBinding> _bindings = new List<ITriggerBinding>();

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public TriggerHost(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void StartAndBlock()
        {
            _bindings.AddRange(new TriggerBindingCollector<TimerTriggerBinding>().CollectFromEntryAssembly(_serviceProvider));
            _bindings.AddRange(new TriggerBindingCollector<QueueTriggerBinding>().CollectFromEntryAssembly(_serviceProvider));

            var tasks = _bindings.Select(o => o.RunAsync(_cancellationTokenSource.Token)).ToArray();
            Task.WaitAll(tasks);
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }


    }
}
