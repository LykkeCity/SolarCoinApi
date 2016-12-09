using SolarCoinApi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.CashInGrabberJobRunner
{
    public static class Helper
    {
        public static TransitQueueMessage ToTransitQueueMessage(this TransactionMongoEntity entity)
        {
            var result = new TransitQueueMessage { TxId = entity.TxId };

            foreach (var vin in entity.Vins)
                result.Vins.Add(new Vin { Address = vin.Addresses, Amount = vin.Amount });

            foreach (var vout in entity.Vouts)
                result.Vouts.Add(new Vout { Address = vout.Addresses, Amount = vout.Amount });

            return result;
        }
    }
}
