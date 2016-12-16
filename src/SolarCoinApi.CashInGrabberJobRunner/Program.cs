using SimpleInjector;
using SolarCoinApi.Common;
using System;
using MongoDB.Bson.Serialization;
using SolarCoinApi.Core;

namespace SolarCoinApi.CashInGrabberJobRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MonitoringJob monitoringJob = null;

            try
            {
                var container = new Container();

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
                
                e.PrintToConsole();
#if DEBUG
                Console.ReadKey();
#endif
            }
        }
    }
}
