using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SolarCoinApi.Common.Triggers.Bindings
{
    public interface ITriggerBinding
    {
        void InitBinding(IServiceProvider serviceProvider, MethodInfo callbackMethod);

        Task RunAsync(CancellationToken cancellationToken);
    }
}
