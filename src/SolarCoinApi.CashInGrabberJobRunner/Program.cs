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
using CashInGrabberJob = SolarCoinApi.CashInGrabberJob.CashInGrabberJob;
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
            try
            {
                var container = new Container();

#if DEBUG
                var settings = new AppSettings<CashInGrabberSettings>().LoadFile("appsettings.Debug.json");
#elif RELEASE
                var settings = new AppSettings<CashInGrabberSettings>().LoadFile("appsettings.Release.json");
#endif

                IConfigureOptions<LoggerOptions> configureOptions = new ConfigureOptions<LoggerOptions>(x =>
                {
                    x.ConnectionString = settings.Logger.ConnectionString;
                    x.ErrorTableName = settings.Logger.ErrorTableName;
                    x.InfoTableName = settings.Logger.InfoTableName;
                    x.WarningTableName = settings.Logger.WarningTableName;
                });

                var loggerOptions = new OptionsManager<LoggerOptions>(new List<IConfigureOptions<LoggerOptions>> { configureOptions });

                container.Register<ILog>(() => { return new TableLogger(loggerOptions, settings.VerboseLogging); }, Lifestyle.Singleton);


                BsonClassMap.RegisterClassMap<TransactionMongoEntity>();
                var client = new MongoClient($"{settings.Mongo.Host}:{settings.Mongo.Port}");
                IMongoDatabase mongo = client.GetDatabase(settings.Mongo.DbName);
                var collection = mongo.GetCollection<TransactionMongoEntity>(settings.Mongo.CollectionName);

                container.Register<IQueueExt>(() => { return new AzureQueueExt(settings.TransitQueue.ConnectionString, settings.TransitQueue.Name); }, Lifestyle.Transient);


                container.Register<CashInGrabberJob.CashInGrabberJob>(() => new CashInGrabberJob.CashInGrabberJob(
                    "CashInGrabber",
                    settings.Period,
                    container.GetInstance<ILog>(),
                    collection,
                    container.GetInstance<IQueueExt>(),
                    settings.Threshold));

                var job = container.GetInstance<CashInGrabberJob.CashInGrabberJob>();

                job.Start();


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
