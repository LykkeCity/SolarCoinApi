using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SolarCoinApi.Core.Options;
using SolarCoinApi.RpcJson;
using SolarCoinApi.RpcJson.JsonRpc;

namespace ConsoleApp1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IConfigureOptions<RpcWalletGeneratorOptions> o = new ConfigureOptions<RpcWalletGeneratorOptions>(x =>
            {
                x.Endpoint = "http://localhost:18181";
                x.Password = "password";
                x.Username = "solarcoinrpc";
            });
            var a = new OptionsManager<RpcWalletGeneratorOptions>(new IConfigureOptions<RpcWalletGeneratorOptions>[] {o});
            
            var rpc = new RpcCashOut(new JsonRpcClient(new JsonRpcClientRaw(new JsonRpcRequestBuilder(), null, a), new JsonRpcRawResponseFormatter(), null));

            var b = rpc.CreateRawTransaction();

            var c = b;
            Console.ReadKey();
        }
    }
}
