using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Common
{
    public interface IQueueReaderFactory
    {
        IQueueReader Create(string queueName);
    }
}
