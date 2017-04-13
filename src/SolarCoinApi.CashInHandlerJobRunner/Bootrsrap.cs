using Microsoft.Extensions.Options;
using SolarCoinApi.Core;
using SolarCoinApi.Core.Options;
using SolarCoinApi.RpcJson.JsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Lykke.JobTriggers.Extenstions;
using SolarCoinApi.Common;

namespace SolarCoinApi.CashInHandlerJobRunner
{
    public static class Bootrsrap
    {
        public static ContainerBuilder ConfigureBuilder(string componentName, CashInHandlerSettings settings)
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

            builder.Register(ctx => new AzureQueueExt(settings.CashInQueue.ConnectionString, settings.CashInQueue.Name)).As<IQueueExt>().SingleInstance();

            builder.Register(ctx => new MonitoringRepository(new AzureTableStorage<MonitoringEntity>(settings.Monitoring.ConnectionString, settings.Monitoring.Name, ctx.Resolve<ILog>()))).As<IMonitoringRepository>().SingleInstance();

            builder.RegisterType<JsonRpcRawResponseFormatter>().As<IJsonRpcRawResponseFormatter>().SingleInstance();

            builder.RegisterType<JsonRpcRequestBuilder>().As<IJsonRpcRequestBuilder>().SingleInstance();

            var rawRpcClientOptions = new ConfigureOptions<RpcWalletGeneratorOptions>(x =>
            {
                x.Endpoint = settings.Rpc.Endpoint;
                x.Password = settings.Rpc.Password;
                x.Username = settings.Rpc.Username;
            });

            builder.Register(ctx => new JsonRpcClientRaw(ctx.Resolve<IJsonRpcRequestBuilder>(), ctx.Resolve<ILog>(), new OptionsManager<RpcWalletGeneratorOptions>(new List<IConfigureOptions<RpcWalletGeneratorOptions>>() { rawRpcClientOptions }))).As<IJsonRpcClientRaw>().SingleInstance();

            builder.Register(ctx => new JsonRpcClient(ctx.Resolve<IJsonRpcClientRaw>(), ctx.Resolve<IJsonRpcRawResponseFormatter>(), ctx.Resolve<ILog>())).As<IJsonRpcClient>().SingleInstance();

            builder.Register(
                ctx => new CashInHandlerQueueTrigger(
                    componentName,
                    new AzureTableStorage<WalletStorageEntity>(
                        settings.GeneratedWallets.ConnectionString, settings.GeneratedWallets.Name, ctx.Resolve<ILog>()),
                        ctx.Resolve<ILog>(),
                        ctx.Resolve<IQueueExt>(),
                        ctx.Resolve<IJsonRpcClient>(),
                        ctx.Resolve<ISlackNotifier>(),
                        settings.HotWalletAddress,
                        settings.CashInTxFee,
                        settings.CashInMinTxAmount)).As<CashInHandlerQueueTrigger>().SingleInstance();

            builder.Register(ctx => new CashInHandlerMonitoringJob(
                    componentName,
                    ctx.Resolve<IMonitoringRepository>(),
                    ctx.Resolve<ILog>())).As<CashInHandlerMonitoringJob>().SingleInstance();

            builder.AddTriggers(pool => pool.AddDefaultConnection(settings.CashInQueue.ConnectionString));

            return builder;
        }
    }
}
