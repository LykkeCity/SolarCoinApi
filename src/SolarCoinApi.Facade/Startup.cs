using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
#if DEBUG
                .AddJsonFile("appsettings.Debug.json", optional: false, reloadOnChange: true);
#elif RELEASE
                .AddJsonFile("appsettings.Release.json", optional: false, reloadOnChange: true);
#endif
            //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);


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

            services.AddTransient<IWalletGenerator, RpcWalletGenerator>();

            services.Configure<RpcWalletGeneratorOptions>(x =>
            {
                x.Endpoint = Configuration.GetSection("RpcServer:Endpoint").Value;
                x.Username = Configuration.GetSection("RpcServer:Username").Value;
                x.Password = Configuration.GetSection("RpcServer:Password").Value;
            });

            services.Configure<LoggerOptions>(x =>
            {
                x.ConnectionString = Configuration.GetSection("logging:connectionString").Value;
                x.ErrorTableName = Configuration.GetSection("logging:errorTableName").Value;
                x.InfoTableName = Configuration.GetSection("logging:infoTableName").Value;
                x.WarningTableName = Configuration.GetSection("logging:warningTableName").Value;
            });

            services.Configure<WalletGeneratorControllerOptions>(x =>
            {
                x.ConnectionString = Configuration.GetSection("generatedWallets:connectionString").Value;
                x.TableName = Configuration.GetSection("generatedWallets:tableName").Value;
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
