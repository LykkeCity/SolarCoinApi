using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.JobTriggers.Triggers;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using SolarCoinApi.Common;
using SolarCoinApi.Core;

namespace SolarCoinApi.Alerts
{
    public class Program
    {
        private static AutofacServiceProvider ServiceProvider { get; set; }
        private static TriggerHost TriggerHost { set; get; }

        private static string ComponentName = "SolarCoinApi.Alerts";

        public static void Main(string[] args)
        {
            try
            {
#if DEBUG
                var settings = new AppSettings<AlertsSettings>().LoadFile("appsettings.Debug.json");
#elif RELEASE
                var settings = new AppSettings<AlertsSettings>().LoadFromEnvironment();
#endif
                BsonClassMap.RegisterClassMap<TransactionMongoEntity>();
                
                ServiceProvider = new AutofacServiceProvider(Bootstrap.ConfigureBuilder(ComponentName, settings).Build());

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
