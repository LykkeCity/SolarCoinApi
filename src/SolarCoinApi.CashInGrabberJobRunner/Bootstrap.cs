using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SimpleInjector;
using SolarCoinApi.Core;
using SolarCoinApi.Core.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using SolarCoinApi.Common;

namespace SolarCoinApi.CashInGrabberJobRunner
{
    public static class Bootrsrap
    {
        public static void Start(Container container, CashInGrabberSettings settings)
        {
            IConfigureOptions<LoggerOptions> configureOptions = new ConfigureOptions<LoggerOptions>(x =>
            {
                x.ConnectionString = settings.Logger.ConnectionString;
                x.ErrorTableName = settings.Logger.ErrorTableName;
                x.InfoTableName = settings.Logger.InfoTableName;
                x.WarningTableName = settings.Logger.WarningTableName;
            });

            var loggerOptions = new OptionsManager<LoggerOptions>(new List<IConfigureOptions<LoggerOptions>> { configureOptions });

            container.Register<ILog>(() => { return new TableLogger(loggerOptions, settings.VerboseLogging); }, Lifestyle.Singleton);
            
            var client = new MongoClient($"{settings.Mongo.Host}:{settings.Mongo.Port}");
            IMongoDatabase mongo = client.GetDatabase(settings.Mongo.DbName);
            var collection = mongo.GetCollection<TransactionMongoEntity>(settings.Mongo.CollectionName);

            container.Register<IQueueExt>(() => { return new AzureQueueExt(settings.TransitQueue.ConnectionString, settings.TransitQueue.Name); }, Lifestyle.Singleton);

            container.Register<IMonitoringRepository>(() => new MonitoringRepository(new AzureTableStorage<MonitoringEntity>(settings.Monitoring.ConnectionString, settings.Monitoring.Name, container.GetInstance<ILog>())), Lifestyle.Singleton);

            container.Register<ISlackNotifier>(() => new SlackNotifier(new AzureQueueExt(settings.SlackQueue.ConnectionString, settings.SlackQueue.Name)), Lifestyle.Singleton);

            container.Register<CashInGrabberJob>(() => new CashInGrabberJob(
                "CashInGrabber",
                settings.CashInPeriod,
                container.GetInstance<ILog>(),
                collection,
                container.GetInstance<IQueueExt>(),
                container.GetInstance<ISlackNotifier>(),
                settings.CashInThreshold), Lifestyle.Singleton);

            container.Register<MonitoringJob>(() => new MonitoringJob(
                "SolarCoinApi.CashInGrabber",
                container.GetInstance<IMonitoringRepository>(),
                container.GetInstance<ILog>()
                ), Lifestyle.Singleton);

        }
    }
}
