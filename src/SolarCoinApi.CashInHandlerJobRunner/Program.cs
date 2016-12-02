using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleInjector;
using SolarCoinApi.Common;
using SolarCoinApi.CashInHandlerJobRunner;
using SolarCoinApi.Core.Log;
using SolarCoinApi.Common.Triggers;
using SolarCoinApi.RpcJson.JsonRpc;

namespace SolarCoinApi.CashInHandlerJobRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.Title = "SolarCoin CashIn Handler job";

#if DEBUG
                var settings = new AppSettings<CashInHandlerSettings>().LoadFile("appsettings.Debug.json");
#elif RELEASE
                var settings = new AppSettings<CashInHandlerSettings>().LoadFile("appsettings.Release.json");
#endif

                var container = new Container();

                Bootrsrap.Start(container, settings);

                var triggerHost = new TriggerHost(container);
                triggerHost.StartAndBlock();

                Console.WriteLine("The job has started! Enter 'q' to quit...");

                while (Console.ReadLine() != "q")
                    continue;

            }
            catch (Exception e)
            {
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
