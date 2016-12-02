using SolarCoinApi.Common.Triggers.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SolarCoinApi.Common.Triggers.Bindings
{
    public class TriggerBindingCollector<T> where T : ITriggerBinding
    {
        public List<T> CollectFromEntryAssembly(IServiceProvider serviceProvider)
        {
            var defineAttribute = typeof(T).GetTypeInfo().GetCustomAttribute<TriggerDefineAttribute>();
            if (defineAttribute == null)
                throw new Exception("Type T must have TriggerDefineAttribute");

            return Assembly.GetEntryAssembly().GetTypes().SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(
                m => m.GetCustomAttribute(defineAttribute.Type, false) != null))
                .Select(m =>
                {
                    var binding = serviceProvider.GetService<T>();
                    binding.InitBinding(serviceProvider, m);
                    return binding;
                }).ToList();
        }

    }
}
