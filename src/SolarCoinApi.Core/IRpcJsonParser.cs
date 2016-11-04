using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using SolarCoinApi.Core.ContractModels;

namespace SolarCoinApi.Core
{
    public interface IRpcJsonParser
    {
        WalletGeneratorContractModel ParseNewWalletResponse(string json);
    }
}
