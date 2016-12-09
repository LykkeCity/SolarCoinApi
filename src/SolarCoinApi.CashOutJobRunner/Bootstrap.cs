using Microsoft.Extensions.Options;
using SimpleInjector;
using SolarCoinApi.AzureStorage.Queue;
using SolarCoinApi.AzureStorage.Tables;
using SolarCoinApi.Common;
using SolarCoinApi.Common.Triggers.Bindings;
using SolarCoinApi.Core;
using SolarCoinApi.Core.Log;
using SolarCoinApi.Core.Options;
using SolarCoinApi.RpcJson.JsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.CashOutJobRunner
{
    class Bootstrap
    {
        public static void Start(Container container, CashOutSettings settings)
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

            //container.Register<IQueueExt>(() => { return new AzureQueueExt(settings.Queue.ConnectionString, settings.Queue.Name); }, Lifestyle.Transient);

            container.Register<IMonitoringRepository>(() => new MonitoringRepository(new AzureTableStorage<MonitoringEntity>(settings.Monitoring.ConnectionString, settings.Monitoring.Name, container.GetInstance<ILog>())), Lifestyle.Singleton);

            container.Register<IJsonRpcRawResponseFormatter, JsonRpcRawResponseFormatter>(Lifestyle.Singleton);

            container.Register<IJsonRpcRequestBuilder, JsonRpcRequestBuilder>(Lifestyle.Singleton);

            var rawRpcClientOptions = new ConfigureOptions<RpcWalletGeneratorOptions>(x =>
            {
                x.Endpoint = settings.Rpc.Endpoint;
                x.Password = settings.Rpc.Password;
                x.Username = settings.Rpc.Username;
            });

            container.Register<IJsonRpcClientRaw>(() => { return new JsonRpcClientRaw(container.GetInstance<IJsonRpcRequestBuilder>(), container.GetInstance<ILog>(), new OptionsManager<RpcWalletGeneratorOptions>(new List<IConfigureOptions<RpcWalletGeneratorOptions>>() { rawRpcClientOptions })); }, Lifestyle.Singleton);

            container.Register<IJsonRpcClient>(() => new JsonRpcClient(container.GetInstance<IJsonRpcClientRaw>(), container.GetInstance<IJsonRpcRawResponseFormatter>(), container.GetInstance<ILog>()), Lifestyle.Singleton);

            container.Register<IQueueReaderFactory>(() => new AzureQueueReaderFactory(settings.Queue.ConnectionString), Lifestyle.Singleton);

            container.Register<ISlackNotifier>(() => new SlackNotifier(new AzureQueueExt(settings.SlackQueue.ConnectionString, settings.SlackQueue.Name)), Lifestyle.Singleton);

            container.Register<CashOutQueueTrigger>(() => new CashOutQueueTrigger(
                container.GetInstance<IJsonRpcClient>(),
                new AzureTableStorage<ExistingCashOutEntity>(settings.ExistingTxes.ConnectionString, settings.ExistingTxes.Name, container.GetInstance<ILog>()),
                container.GetInstance<ILog>(),
                container.GetInstance<ISlackNotifier>()
                ), Lifestyle.Singleton);


            container.Register<MonitoringJob>(() => new MonitoringJob(
                    "SolarCoinApi.CashOutJob",
                    container.GetInstance<IMonitoringRepository>(),
                    container.GetInstance<ILog>()
                    ), Lifestyle.Singleton);

            container.Register<QueueTriggerBinding>(Lifestyle.Singleton);

            //container.Register<ToSendMessageFromQueue>(Lifestyle.Transient);

            container.Verify();
        }
    }
}
