using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SolarCoinApi.Core;
using SolarCoinApi.Core.ContractModels;
using System.Text;
using SolarCoinApi.Core.Options;
using Microsoft.Extensions.Options;
using SolarCoinApi.RpcJson.JsonRpc;

namespace SolarCoinApi.RpcJson
{
    public class RpcWalletGenerator : IWalletGenerator
    {
        private readonly IJsonRpcClient _client;

        public RpcWalletGenerator(IJsonRpcClient client)
        {
            _client = client;
        }

        public async Task<WalletGeneratorContractModel> GenerateWalletAsync()
        {
                var r = await _client.MakeKeyPair();

                return new WalletGeneratorContractModel { PrivateKey = r.PrivKey, PublicKey = r.PublicKey, Address = r.Address };
        }

    }
}
