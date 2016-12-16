using System;
using SimpleInjector;
using SolarCoinApi.Common;
using SolarCoinApi.Common.Triggers;
using System.Runtime.Loader;
using System.Threading;

namespace SolarCoinApi.CashInHandlerJobRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MonitoringJob monitoringJob = null;

            try
            {
                Console.Title = "SolarCoin CashIn Handler job";

#if DEBUG
                var settings = new AppSettings<CashInHandlerSettings>().LoadFile("appsettings.Debug.json");
#elif RELEASE
                var settings = new AppSettings<CashInHandlerSettings>().LoadFromEnvironment();
#endif
                
                var container = new Container();

                Bootrsrap.Start(container, settings);

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

                triggerHost.StartAndBlock();

                end.Set();

            }
            catch (Exception e)
            {
                monitoringJob?.Stop();

                e.PrintToConsole();

                Console.ReadKey();
            }

        }
    }
}
