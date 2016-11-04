using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Core.ContractModels
{
    public class WalletGeneratorContractModel
    {
        public string PublicKey { set; get; }
        public string PrivateKey { set; get; }
        public string Address { set; get; }
    }
}
