using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Common.Triggers.Attributes
{
    public class TriggerDefineAttribute : Attribute
    {
        public Type Type { get; }

        public TriggerDefineAttribute(Type type)
        {
            Type = type;
        }
    }
}
