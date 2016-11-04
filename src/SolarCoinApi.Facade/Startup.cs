using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolarCoinApi.AzureStorage;
using SolarCoinApi.Common;
using SolarCoinApi.Core;
using SolarCoinApi.Core.Log;
using SolarCoinApi.Core.Options;
using SolarCoinApi.RpcJson;
using SolarCoinApi.RpcJson.JsonRpc;

namespace SolarCoinApi.Facade
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsEnvironment("Development"))
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc();


            services.AddTransient<ILog, TableLogger>();

            ConfigureRpc(services);
        }
        private void ConfigureRpc(IServiceCollection services)
        {

            services.AddTransient<IWalletGenerator, RpcWalletGenerator>();

            services.Configure<RpcWalletGeneratorOptions>(x =>
            {
                x.Endpoint = Configuration.GetSection("RpcServer:Endpoint").Value;
                x.Username = Configuration.GetSection("RpcServer:Username").Value;
                x.Password = Configuration.GetSection("RpcServer:Password").Value;
            });

            services.Configure<LoggerOptions>(x =>
            {
                x.ConnectionString = Configuration.GetSection("ConnectionStrings:Default").Value;
                x.ErrorTableName = Configuration.GetSection("DbTables1:ErrorTableName").Value;
                x.InfoTableName = Configuration.GetSection("DbTables1:InfoTableName").Value;
                x.WarningTableName = Configuration.GetSection("DbTables1:WarningTableName").Value;
            });

            services.Configure<WalletGeneratorControllerOptions>(x =>
            {
                x.ConnectionString = Configuration.GetSection("ConnectionStrings:Default").Value;
                x.TableName = Configuration.GetSection("DbTables1:WalletsTableName").Value;
            });

            services.AddTransient<IJsonRpcClient, JsonRpcClient>();
            services.AddTransient<IJsonRpcClientRaw, JsonRpcClientRaw>();
            services.AddTransient<IJsonRpcRequestBuilder, JsonRpcRequestBuilder>();
            services.AddTransient<IJsonRpcRawResponseFormatter, JsonRpcRawResponseFormatter>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseMvc();
        }
    }
}
