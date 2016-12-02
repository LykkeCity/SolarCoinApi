using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Common.Triggers.Attributes
{
    public class QueueTriggerAttribute : Attribute
    {
        public string Queue { get; }

        public QueueTriggerAttribute(string queue)
        {
            Queue = queue;
        }
    }
}
