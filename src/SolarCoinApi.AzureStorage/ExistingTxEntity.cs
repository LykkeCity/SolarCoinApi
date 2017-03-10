using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace SolarCoinApi.AzureStorage
{
    public class ExistingTxEntity : TableEntity
    {
        public string TxId { set; get; }
    }
}
