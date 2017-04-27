using System;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.JobTriggers.Triggers;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using SolarCoinApi.Core;
using SolarCoinApi.Common;

namespace SolarCoinApi.CashInGrabberJobRunner
{
    public class Program
    {
        private static AutofacServiceProvider ServiceProvider { get; set; }
        private static TriggerHost TriggerHost { set; get; }

        private static string ComponentName = "SolarCoinApi.CashInGrabber";

        public static void Main(string[] args)
        {
            try
            {
#if DEBUG
                var settings = new AppSettings<CashInGrabberSettings>().LoadFile("appsettings.Debug.json");
#elif RELEASE
                var settings =  new AppSettings<CashInGrabberSettings>().LoadFromWeb(Environment.GetEnvironmentVariable("SlrSettingsUrl")).Result;
#endif
                BsonClassMap.RegisterClassMap<TransactionMongoEntity>();

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

                ServiceProvider?.GetService<ILog>()?.WriteErrorAsync(ComponentName, "", "", e).GetAwaiter().GetResult();
                
                e.PrintToConsole();
#if DEBUG
                Console.ReadKey();
#endif
            }
        }
    }
}
