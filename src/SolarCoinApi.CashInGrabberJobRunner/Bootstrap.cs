using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SolarCoinApi.Core;
using SolarCoinApi.Core.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using SolarCoinApi.Common;

namespace SolarCoinApi.CashInGrabberJobRunner
{
    public static class Bootrsrap
    {
        public static ContainerBuilder ConfigureBuilder(string componentName, CashInGrabberSettings settings)
        {
            var builder = new ContainerBuilder();

            IConfigureOptions<LoggerOptions> configureOptions = new ConfigureOptions<LoggerOptions>(x =>
            {
                x.ConnectionString = settings.Logger.ConnectionString;
                x.ErrorTableName = settings.Logger.ErrorTableName;
                x.InfoTableName = settings.Logger.InfoTableName;
                x.WarningTableName = settings.Logger.WarningTableName;
            });

            var loggerOptions = new OptionsManager<LoggerOptions>(new List<IConfigureOptions<LoggerOptions>> { configureOptions });

            builder.Register(ctx => new SlackNotifier(new AzureQueueExt(settings.SlackQueue.ConnectionString, settings.SlackQueue.Name))).As<ISlackNotifier>().SingleInstance();

            builder.Register(ctx => new TableLogger(ctx.Resolve<ISlackNotifier>(), loggerOptions, settings.VerboseLogging)).As<ILog>().SingleInstance();
            
            var client = new MongoClient($"{settings.Mongo.Host}:{settings.Mongo.Port}");
            IMongoDatabase mongo = client.GetDatabase(settings.Mongo.DbName);
            var collection = mongo.GetCollection<TransactionMongoEntity>(settings.Mongo.CollectionName);

            builder.Register(ctx => new AzureQueueExt(settings.TransitQueue.ConnectionString, settings.TransitQueue.Name)).As<IQueueExt>().SingleInstance();

            builder.Register(ctx => new MonitoringRepository(new AzureTableStorage<MonitoringEntity>(settings.Monitoring.ConnectionString, settings.Monitoring.Name, ctx.Resolve<ILog>()))).As<IMonitoringRepository>().SingleInstance();

            builder.Register(ctx => new CashInGrabberJob(
                componentName,
                settings.CashInPeriod,
                ctx.Resolve<ILog>(),
                collection,
                ctx.Resolve<IQueueExt>(),
                ctx.Resolve<ISlackNotifier>(),
                settings.CashInThreshold)).As<CashInGrabberJob>().SingleInstance();

            builder.Register(ctx => new CashInGrabberMonitoringJob(
                componentName,
                ctx.Resolve<IMonitoringRepository>(),
                ctx.Resolve<ILog>()
                )).As<CashInGrabberMonitoringJob>().SingleInstance();

            return builder;
        }
    }
}
