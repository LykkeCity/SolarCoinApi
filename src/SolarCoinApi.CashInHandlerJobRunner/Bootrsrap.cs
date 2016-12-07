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

            container.Register<ILog>(() => { return new TableLogger(loggerOptions, settings.VerboseLogging); }, Lifestyle.Singleton);

            container.Register<IQueueExt>(() => { return new AzureQueueExt(settings.CashOutQueue.ConnectionString, settings.CashOutQueue.Name); }, Lifestyle.Singleton);

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

            container.Register<IQueueReaderFactory>(() => new AzureQueueReaderFactory(settings.TransitQueue.ConnectionString));

            container.Register<CashInHandlerQueueTrigger>(
                () => new CashInHandlerQueueTrigger(
                    new AzureTableStorage<WalletStorageEntity>(
                        settings.GeneratedWallets.ConnectionString, settings.GeneratedWallets.Name, container.GetInstance<ILog>()),
                        container.GetInstance<ILog>(),
                        container.GetInstance<IQueueExt>(),
                        container.GetInstance<IJsonRpcClient>(),
                        settings.HotWalletAddress,
                        settings.TxFee,
                        settings.MinTxAmount), Lifestyle.Singleton);

            container.Register<QueueTriggerBinding>(Lifestyle.Singleton);

            //container.Register<TransitQueueMessage>(Lifestyle.Transient);

            container.Verify();
        }
    }
}
