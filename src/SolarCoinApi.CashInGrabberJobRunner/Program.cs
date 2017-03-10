using SimpleInjector;
using System;
using Common.Log;
using MongoDB.Bson.Serialization;
using SolarCoinApi.Core;
using SolarCoinApi.Common;

namespace SolarCoinApi.CashInGrabberJobRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MonitoringJob monitoringJob = null;

            Container container = null;

            try
            {
                container = new Container();

#if DEBUG
                var settings = new AppSettings<CashInGrabberSettings>().LoadFile("appsettings.Debug.json");
#elif RELEASE
                var settings = new AppSettings<CashInGrabberSettings>().LoadFromEnvironment();
#endif
                BsonClassMap.RegisterClassMap<TransactionMongoEntity>();

                Bootrsrap.Start(container, settings);
                
                var job = container.GetInstance<CashInGrabberJob>();

                monitoringJob = container.GetInstance<MonitoringJob>();

                var jobsRunner = new JobsRunner();

                jobsRunner.AddJob(job);
                jobsRunner.AddJob(monitoringJob);

                jobsRunner.StartAndWatch().GetAwaiter().GetResult();
                

            }
            catch (Exception e)
            {
                monitoringJob?.Stop();

                container?.GetInstance<ILog>().WriteErrorAsync("CashInGrabberJobRunner", "", "", e);
                
                e.PrintToConsole();
#if DEBUG
                Console.ReadKey();
#endif
            }
        }
    }
}
