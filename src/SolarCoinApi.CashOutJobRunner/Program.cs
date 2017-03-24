using System;
using SolarCoinApi.Common;
using SolarCoinApi.RpcJson.JsonRpc;
using SimpleInjector;
using System.Runtime.Loader;
using System.Threading;
using Common.Log;
using Lykke.JobTriggers.Triggers;

namespace SolarCoinApi.CashOutJobRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MonitoringJob monitoringJob = null;

            Container container = null;

            try
            {
                Console.Title = "SolarCoin CashOut job";

#if DEBUG
                var settings = new AppSettings<CashOutSettings>().LoadFile("appsettings.Debug.json");
#elif RELEASE
                var settings = new AppSettings<CashOutSettings>().LoadFromEnvironment();
#endif

                container = new Container();

                Bootstrap.Start(container, settings);

                var rpcClient = container.GetInstance<IJsonRpcClient>();
                
                Console.WriteLine("Importing private key to the local node - this may take up to several minutes...");

                rpcClient.ImportPrivateKey(settings.HotWalletPrivKey).Wait();

                Console.WriteLine("The key was imported!");
                
                monitoringJob = container.GetInstance<MonitoringJob>();
                monitoringJob.Start();
                
                var triggerHost = new TriggerHost(container);

                var end = new ManualResetEvent(false);

                AssemblyLoadContext.Default.Unloading += ctx =>
                {
                    Console.WriteLine("SIGTERM recieved");
                    triggerHost.Cancel();

                    end.WaitOne();
                };

                triggerHost.Start().GetAwaiter().GetResult();

                end.Set();
            }
            catch(Exception e)
            {
                monitoringJob?.Stop();

                container?.GetInstance<ILog>().WriteErrorAsync("CashOutJobRunner", "", "", e);
                
                e.PrintToConsole();
#if DEBUG
                Console.ReadKey();
#endif
            }

        }
    }
}
