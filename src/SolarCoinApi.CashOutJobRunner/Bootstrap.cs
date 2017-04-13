using Microsoft.Extensions.Options;
using SolarCoinApi.Common;
using SolarCoinApi.Core;
using SolarCoinApi.Core.Options;
using SolarCoinApi.RpcJson.JsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common;
using Common.Log;
using Lykke.JobTriggers.Abstractions;
using Lykke.JobTriggers.Extenstions;
using Microsoft.Extensions.DependencyInjection;

namespace SolarCoinApi.CashOutJobRunner
{
    class Bootstrap
    {
        public static ContainerBuilder ConfigureBuilder(string componentName, CashOutSettings settings)
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

            builder.Register(ctx => new MonitoringRepository(new AzureTableStorage<MonitoringEntity>(settings.Monitoring.ConnectionString, settings.Monitoring.Name, ctx.Resolve<ILog>()))).As<IMonitoringRepository>().SingleInstance();

            builder.RegisterType<JsonRpcRawResponseFormatter>().As<IJsonRpcRawResponseFormatter>().SingleInstance();

            builder.RegisterType< JsonRpcRequestBuilder>().As<IJsonRpcRequestBuilder>().SingleInstance();

            var rawRpcClientOptions = new ConfigureOptions<RpcWalletGeneratorOptions>(x =>
            {
                x.Endpoint = settings.Rpc.Endpoint;
                x.Password = settings.Rpc.Password;
                x.Username = settings.Rpc.Username;
            });

            builder.Register(ctx => new JsonRpcClientRaw(ctx.Resolve<IJsonRpcRequestBuilder>(), ctx.Resolve<ILog>(), new OptionsManager<RpcWalletGeneratorOptions>(new List<IConfigureOptions<RpcWalletGeneratorOptions>>() { rawRpcClientOptions }))).As <IJsonRpcClientRaw>().SingleInstance();

            builder.Register(ctx => new JsonRpcClient(ctx.Resolve<IJsonRpcClientRaw>(), ctx.Resolve<IJsonRpcRawResponseFormatter>(), ctx.Resolve<ILog>())).As<IJsonRpcClient>().SingleInstance();
            
            builder.Register(ctx => new CashOutQueueTrigger(
                componentName,
                ctx.Resolve<IJsonRpcClient>(),
                new AzureTableStorage<ExistingCashOutEntity>(settings.ExistingTxes.ConnectionString, settings.ExistingTxes.Name, ctx.Resolve<ILog>()),
                ctx.Resolve<ILog>(),
                ctx.Resolve<ISlackNotifier>()
                )).As<CashOutQueueTrigger>().SingleInstance();
            
            builder.Register<CashOutMonitoringJob>(ctx => new CashOutMonitoringJob(
                    componentName,
                    ctx.Resolve<IMonitoringRepository>(),
                    ctx.Resolve<ILog>()
                    )).As<CashOutMonitoringJob>().SingleInstance();
            
            builder.AddTriggers(pool => pool.AddDefaultConnection(settings.CashOutQueue.ConnectionString));
            
            return builder;
        }
    }
}
