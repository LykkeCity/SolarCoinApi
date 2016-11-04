using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace SolarCoinApi.Core
{
    public class WalletStorageEntity : TableEntity
    {
        public string Address { set; get; }
        public string PublicKey { set; get; }
        public string PrivateKey { set; get; }
    }
}
