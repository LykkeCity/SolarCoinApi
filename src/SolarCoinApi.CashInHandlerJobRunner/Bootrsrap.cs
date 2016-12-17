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

namespace SolarCoinApi.CashInHandlerJobRunner
{
    public static class Bootrsrap
    {
        public static void Start(Container container, CashInHandlerSettings settings)
        {
            IConfigureOptions<LoggerOptions> configureOptions = new ConfigureOptions<LoggerOptions>(x =>
            {
                x.ConnectionString = settings.Logger.ConnectionString;
                x.ErrorTableName = settings.Logger.ErrorTableName;
                x.InfoTableName = settings.Logger.InfoTableName;
                x.WarningTableName = settings.Logger.WarningTableName;
            });

            var loggerOptions = new OptionsManager<LoggerOptions>(new List<IConfigureOptions<LoggerOptions>> { configureOptions });

            container.RegisterSingleton<ILog>(() => { return new TableLogger(loggerOptions, settings.VerboseLogging); });

            container.RegisterSingleton<IQueueExt>(() => { return new AzureQueueExt(settings.CashInQueue.ConnectionString, settings.CashInQueue.Name); });

            container.RegisterSingleton<IMonitoringRepository>(() => new MonitoringRepository(new AzureTableStorage<MonitoringEntity>(settings.Monitoring.ConnectionString, settings.Monitoring.Name, container.GetInstance<ILog>())));

            container.RegisterSingleton<IJsonRpcRawResponseFormatter, JsonRpcRawResponseFormatter>();

            container.RegisterSingleton<IJsonRpcRequestBuilder, JsonRpcRequestBuilder>();

            var rawRpcClientOptions = new ConfigureOptions<RpcWalletGeneratorOptions>(x =>
            {
                x.Endpoint = settings.Rpc.Endpoint;
                x.Password = settings.Rpc.Password;
                x.Username = settings.Rpc.Username;
            });

            container.RegisterSingleton<IJsonRpcClientRaw>(() => { return new JsonRpcClientRaw(container.GetInstance<IJsonRpcRequestBuilder>(), container.GetInstance<ILog>(), new OptionsManager<RpcWalletGeneratorOptions>(new List<IConfigureOptions<RpcWalletGeneratorOptions>>() { rawRpcClientOptions })); });

            container.RegisterSingleton<IJsonRpcClient>(() => new JsonRpcClient(container.GetInstance<IJsonRpcClientRaw>(), container.GetInstance<IJsonRpcRawResponseFormatter>(), container.GetInstance<ILog>()));

            container.RegisterSingleton<IQueueReaderFactory>(() => new AzureQueueReaderFactory(settings.TransitQueue.ConnectionString));

            container.RegisterSingleton<ISlackNotifier>(() => new SlackNotifier(new AzureQueueExt(settings.SlackQueue.ConnectionString, settings.SlackQueue.Name)));

            container.RegisterSingleton<CashInHandlerQueueTrigger>(
                () => new CashInHandlerQueueTrigger(
                    new AzureTableStorage<WalletStorageEntity>(
                        settings.GeneratedWallets.ConnectionString, settings.GeneratedWallets.Name, container.GetInstance<ILog>()),
                        container.GetInstance<ILog>(),
                        container.GetInstance<IQueueExt>(),
                        container.GetInstance<IJsonRpcClient>(),
                        container.GetInstance<ISlackNotifier>(),
                        settings.HotWalletAddress,
                        settings.CashInTxFee,
                        settings.CashInMinTxAmount));

            container.RegisterSingleton<MonitoringJob>(() => new MonitoringJob(
                    "SolarCoinApi.CashInHandler",
                    container.GetInstance<IMonitoringRepository>(),
                    container.GetInstance<ILog>()));

            container.RegisterSingleton<QueueTriggerBinding>();
            
            container.Verify();
        }
    }
}
