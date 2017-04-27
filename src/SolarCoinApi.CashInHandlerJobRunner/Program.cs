using System;
using SolarCoinApi.Common;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.JobTriggers.Triggers;
using Microsoft.Extensions.DependencyInjection;
using SolarCoinApi.CashInHandlerJobRunner;

namespace SolarCoinApi.CashInHandlerJobRunner
{
    public class Program
    {
        private static AutofacServiceProvider ServiceProvider { get; set; }

        private static TriggerHost TriggerHost { set; get; }

        private static string ComponentName = "SolarCoinApi.CashInHandler";

        public static void Main(string[] args)
        {
            try
            {
                Console.Title = "SolarCoin CashIn Handler job";

#if DEBUG
                var settings = new AppSettings<CashInHandlerSettings>().LoadFile("appsettings.Debug.json");
#elif RELEASE
                var settings = new AppSettings<CashInHandlerSettings>().LoadFromWeb(Environment.GetEnvironmentVariable("SlrSettingsUrl")).Result;
#endif

                ServiceProvider = new AutofacServiceProvider(Bootrsrap.ConfigureBuilder(ComponentName, settings).Build());
                
                TriggerHost = new TriggerHost(ServiceProvider);

                AssemblyLoadContext.Default.Unloading += ctx =>
                {
                    Console.WriteLine("SIGTERM recieved");

                    TriggerHost.Cancel();
                };

                TriggerHost.Start().GetAwaiter().GetResult();

            }
            catch (Exception e)
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
