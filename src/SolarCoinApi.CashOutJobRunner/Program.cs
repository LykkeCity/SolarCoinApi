using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SolarCoinApi.AzureStorage.Queue;
using SolarCoinApi.AzureStorage.Tables;
using SolarCoinApi.CashOutJobRunner;
using SolarCoinApi.Common;
using SolarCoinApi.Core;
using SolarCoinApi.Core.Options;
using SolarCoinApi.RpcJson.JsonRpc;
using SimpleInjector;
using SolarCoinApi.Core.Log;
using SolarCoinApi.Common.Triggers;

namespace SolarCoinApi.CashOutJobRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MonitoringJob monitoringJob = null;

            try
            {
                Console.Title = "SolarCoin CashOut job";

#if DEBUG
                var settings = new AppSettings<CashOutSettings>().LoadFile("appsettings.Debug.json");
#elif RELEASE
                var settings = new AppSettings<CashOutSettings>().LoadFile("appsettings.Release.json");
#endif

                var container = new Container();

                Bootstrap.Start(container, settings);

                var rpcClient = container.GetInstance<IJsonRpcClient>();
                
                Console.WriteLine("Importing private key to the local node - this may take up to several minutes...");

                rpcClient.ImportPrivateKey(settings.HotWalletPrivKey).Wait();

                Console.WriteLine("The key was imported!");
                
                monitoringJob = container.GetInstance<MonitoringJob>();

                monitoringJob.Start();


                var triggerHost = new TriggerHost(container);
                triggerHost.StartAndBlock();
                
                Console.WriteLine("The job has started! Enter 'q' to quit...");

                while (Console.ReadLine() != "q")
                    continue;

            }
            catch (Exception e)
            {
                monitoringJob?.Stop();

                var err = e;

                while (err != null)
                {
                    Console.WriteLine(err.Message);
                    Console.WriteLine();
                    Console.WriteLine("Stack trace:");
                    Console.WriteLine(err.StackTrace);

                    err = err.InnerException;
                }

                Console.ReadKey();
            }


        }
    }
}
