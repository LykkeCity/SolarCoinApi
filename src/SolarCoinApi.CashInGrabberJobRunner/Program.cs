using SimpleInjector;
using SolarCoinApi.AzureStorage;
using SolarCoinApi.AzureStorage.Queue;
using SolarCoinApi.AzureStorage.Tables;
using SolarCoinApi.Common;
using SolarCoinApi.Common.Triggers;
using SolarCoinApi.Common.Triggers.Attributes;
using SolarCoinApi.Common.Triggers.Bindings;
using SolarCoinApi.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SolarCoinApi.CashInGrabberJob;
using SolarCoinApi.Core.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
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
                var settings = new AppSettings<CashInGrabberSettings>().LoadFile("appsettings.Release.json");
#endif
                Bootrsrap.Start(container, settings);

                BsonClassMap.RegisterClassMap<TransactionMongoEntity>();

                var job = container.GetInstance<CashInGrabberJob.CashInGrabberJob>();
                job.Start();

                monitoringJob = container.GetInstance<MonitoringJob>();
                monitoringJob.Start();

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

#if DEBUG
                Console.ReadKey();
#endif
            }
        }
    }
}
