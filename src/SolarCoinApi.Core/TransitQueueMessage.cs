using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Core
{
    public class TransitQueueMessage
    {
        public TransitQueueMessage()
        {
            Vouts = new List<Vout>();
            Vins = new List<Vin>();
        }
        public string TxId { set; get; }
        public List<Vout> Vouts { set; get; }
        public List<Vin> Vins { set; get; }
    }

    public class Vout
    {
        public long Amount { set; get; }
        public string Address { set; get; }
    }

    public class Vin
    {
        public long Amount { set; get; }
        public string Address { set; get; }
    }
}
