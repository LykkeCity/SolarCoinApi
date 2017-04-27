using System;
using SolarCoinApi.Common;
using SolarCoinApi.RpcJson.JsonRpc;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.JobTriggers.Triggers;
using Microsoft.Extensions.DependencyInjection;

namespace SolarCoinApi.CashOutJobRunner
{
    public class Program
    {
        private static AutofacServiceProvider ServiceProvider { get; set; }
        private static TriggerHost TriggerHost { set; get; }

        private static string ComponentName = "SolarCoinApi.CashOut";

        public static void Main(string[] args)
        {
            try
            {
                Console.Title = "SolarCoin CashOut job";

#if DEBUG
                var settings = new AppSettings<CashOutSettings>().LoadFile("appsettings.Debug.json");
#elif RELEASE
                var settings = new AppSettings<CashOutSettings>().LoadFromWeb(Environment.GetEnvironmentVariable("SlrSettingsUrl")).Result;
#endif

                ServiceProvider = new AutofacServiceProvider(Bootstrap.ConfigureBuilder(ComponentName, settings).Build());
                
                var rpcClient = ServiceProvider.GetService<IJsonRpcClient>();
                
                Console.WriteLine("Importing private key to the local node - this may take up to several minutes...");

                rpcClient.ImportPrivateKey(settings.HotWalletPrivKey).GetAwaiter().GetResult();

                Console.WriteLine("The key was imported!");

                TriggerHost = new TriggerHost(ServiceProvider);
                
                AssemblyLoadContext.Default.Unloading += ctx =>
                {
                    Console.WriteLine("SIGTERM recieved");

                    TriggerHost.Cancel();
                };

                TriggerHost.Start().GetAwaiter().GetResult();

            }
            catch(Exception e)
            {
                TriggerHost?.Cancel();

                Task.Delay(1000).GetAwaiter().GetResult();

                ServiceProvider?.GetService<ILog>()?.WriteErrorAsync(ComponentName, "", "", e).GetAwaiter().GetResult();
                
                e.PrintToConsole();
#if DEBUG
                Console.ReadKey();
#endif
            }

        }
    }
}
