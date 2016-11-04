using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SolarCoinApi.Core.ContractModels;
using SolarCoinApi.RpcJson.JsonRpc;

namespace SolarCoinApi.RpcJson
{
    public class RpcCashOut
    {
        private readonly IJsonRpcClient _client;

        public RpcCashOut(IJsonRpcClient client)
        {
            _client = client;
        }

        public async Task<string> CreateRawTransaction()
        {
            try
            {
                var dic = new Dictionary<string, decimal>();
                dic.Add("asdf", 2m);

                //var r = await _client.CreateRawTransaction("id", 2, dic);

                    //return new WalletGeneratorContractModel { PrivateKey = r.PrivateKey, PublicKey = r.PublicKey, Address = r.Address };
            }
            catch (Exception exception)
            {
                var a = 23;
                throw;
            }
            return null;
        }
    }
}
