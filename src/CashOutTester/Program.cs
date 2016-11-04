using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SolarCoinApi.Core.Options;
using SolarCoinApi.RpcJson.JsonRpc;

namespace CashOutTester
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    Console.WriteLine(args[1]);
                }

                M();
                Console.ReadKey();

                //client.CreateRawTransaction(new {txid = "989f7ac8317c47d65299f76dc8408fa667227b27c6833c40eae0f8f9b3547a65", vout = 0}, dic).Wait();
            }
            catch (Exception e)
            {
                var a = 234;
            }
        }

        public static async void M()
        {
            var o = new ConfigureOptions<RpcWalletGeneratorOptions>(x =>
            {
                x.Endpoint = "http://localhost:18181";
                x.Password = "password";
                x.Username = "solarcoinrpc";
            });
            var client =
                new JsonRpcClient(
                    new JsonRpcClientRaw(new JsonRpcRequestBuilder(), null,
                        new OptionsManager<RpcWalletGeneratorOptions>(
                            new List<IConfigureOptions<RpcWalletGeneratorOptions>>() { o })),
                    new JsonRpcRawResponseFormatter(), null);

            var dic = new Dictionary<string, decimal>();
            dic.Add("8Q7aVvbVkZviZw2oKnEeaNQoJtn1dEWSnz", 36m);
            dic.Add("8PNfyewj7UkGR1NJehnXt7qZLp5a38aunL", 1.1m);

            string raw =
                await
                    client.CreateRawTransaction(
                        new object[] { new { txid = "8d0f1a1d596893de3131070680b0ee89ba2573eb7b29b46d13520386ff668520", vout = 0 } }, dic);

            var signed = await client.SignRawTransaction(raw, "Ngz8HT1h9dzYQ4hfFwcqSHZ7fNmMTPQQGjkm9UVZkhZeXrdfNt7T");

            var a = signed.Hex;

            var c = await client.SendRawTransaction(signed.Hex);

            var b = signed.Hex;
        }
    }
}
