using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SolarCoinApi.Core.ContractModels;

namespace SolarCoinApi.Core
{
    public interface IWalletGenerator
    {
        Task<WalletGeneratorContractModel> GenerateWalletAsync();
    }
}
