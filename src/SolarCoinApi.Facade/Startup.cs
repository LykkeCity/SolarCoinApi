using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AzureStorage.Queue;
using Common.Log;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolarCoinApi.Common;
using SolarCoinApi.Core;
using SolarCoinApi.Core.Options;
using SolarCoinApi.RpcJson;
using SolarCoinApi.RpcJson.JsonRpc;

namespace SolarCoinApi.Facade
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, ConsoleArgs args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("generalsettings.json", optional: false, reloadOnChange: true);
            
            builder.AddCommandLine(args.Args);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
        

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc();


            services.AddTransient<ILog, TableLogger>();

            ConfigureRpc(services);

            services.AddSwaggerGen(c =>
            {
                c.SingleApiVersion(new Swashbuckle.Swagger.Model.Info
                {
                    Version = $"v1",
                    Title = "SolarCoin Wallets Generator"
                });
            });
        }
        private void ConfigureRpc(IServiceCollection services)
        {


#if DEBUG
            var settings = new AppSettings<FacadeSettings>().LoadFile("appsettings.Debug.json");
#else
            var settings = new AppSettings<FacadeSettings>().LoadFromEnvironment();
#endif


            services.AddTransient<IWalletGenerator, RpcWalletGenerator>();

            services.Configure<RpcWalletGeneratorOptions>(x =>
            {
                x.Endpoint = settings.Rpc.Endpoint;
                x.Username = settings.Rpc.Username;
                x.Password = settings.Rpc.Password;
            });

            services.Configure<LoggerOptions>(x =>
            {
                x.ConnectionString = settings.Logger.ConnectionString;
                x.ErrorTableName = settings.Logger.ErrorTableName;
                x.InfoTableName = settings.Logger.InfoTableName;
                x.WarningTableName = settings.Logger.WarningTableName;
            });

            services.Configure<WalletGeneratorControllerOptions>(x =>
            {
                x.ConnectionString = settings.GeneratedWallets.ConnectionString;
                x.TableName = settings.GeneratedWallets.Name;
            });

            services.AddTransient<ISlackNotifier>(x => new SlackNotifier(new AzureQueueExt(settings.SlackQueue.ConnectionString, settings.SlackQueue.Name)));

            services.AddTransient<IJsonRpcClient, JsonRpcClient>();
            services.AddTransient<IJsonRpcClientRaw, JsonRpcClientRaw>();
            services.AddTransient<IJsonRpcRequestBuilder, JsonRpcRequestBuilder>();
            services.AddTransient<IJsonRpcRawResponseFormatter, JsonRpcRawResponseFormatter>();
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwagger();

            app.UseSwaggerUi();


        }
    }
}
