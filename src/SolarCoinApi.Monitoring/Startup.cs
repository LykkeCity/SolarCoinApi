using Common.Log;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SimpleInjector;
using SimpleInjector.Integration.AspNetCore;
using SimpleInjector.Integration.AspNetCore.Mvc;
using SolarCoinApi.Common;
using SolarCoinApi.Core;
using SolarCoinApi.Core.Options;
using SolarCoinApi.RpcJson.JsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Monitoring
{
    public class Startup
    {
        private Container container = new Container();

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("generalsettings.json", optional: false, reloadOnChange: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var pathToXmlCommentsDoc = GetXmlCommentsPath();

            services.AddSingleton<IControllerActivator>(
            new SimpleInjectorControllerActivator(container));
            services.AddSingleton<IViewComponentActivator>(
                new SimpleInjectorViewComponentActivator(container));



            BsonClassMap.RegisterClassMap<TransactionMongoEntity>();

            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SingleApiVersion(new Swashbuckle.Swagger.Model.Info
                {
                    Version = $"v1",
                    Title = "SolarCoin Monitoring"
                });
                c.IncludeXmlComments(pathToXmlCommentsDoc);
                c.DescribeAllEnumsAsStrings();
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseSimpleInjectorAspNetRequestScoping(container);

            container.Options.DefaultScopedLifestyle = new AspNetRequestLifestyle();

            InitializeContainer(app);

            container.Verify();

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

        private string GetXmlCommentsPath()
        {
            var app = PlatformServices.Default.Application;
            return System.IO.Path.Combine(app.ApplicationBasePath, "SolarCoinApi.Monitoring.xml");
        }

        private void InitializeContainer(IApplicationBuilder app)
        {

#if DEBUG
            //var settings = new AppSettings<MonitoringSettings>().LoadFromEnvironment();
            var settings = new AppSettings<MonitoringSettings>().LoadFile("appsettings.Debug.json");
#else
            var settings = new AppSettings<MonitoringSettings>().LoadFromEnvironment();
#endif


            container.RegisterMvcControllers(app);

            container.RegisterMvcViewComponents(app);

            container.RegisterSingleton<LoggerOptions>(() => new LoggerOptions
            {
                ConnectionString = settings.Logger.ConnectionString,
                ErrorTableName = settings.Logger.ErrorTableName,
                InfoTableName = settings.Logger.InfoTableName,
                WarningTableName = settings.Logger.WarningTableName
            });

            container.RegisterSingleton<ILog>(() => new TableLogger1(container.GetInstance<LoggerOptions>(), Convert.ToBoolean(Configuration.GetSection("verboseLogging").Value)));

            container.RegisterSingleton<RpcWalletGeneratorOptions>(() => new RpcWalletGeneratorOptions
            {
                Endpoint = settings.Rpc.Endpoint,
                Username = settings.Rpc.Username,
                Password = settings.Rpc.Password,
            });

            IConfigureOptions<RpcWalletGeneratorOptions> configureOptions = new ConfigureOptions<RpcWalletGeneratorOptions>(x =>
            {
                x.Endpoint = container.GetInstance<RpcWalletGeneratorOptions>().Endpoint;
                x.Password = container.GetInstance<RpcWalletGeneratorOptions>().Password;
                x.Username = container.GetInstance<RpcWalletGeneratorOptions>().Username;
            });

            container.RegisterSingleton<IOptions<RpcWalletGeneratorOptions>>(() => new OptionsManager<RpcWalletGeneratorOptions>(new List<IConfigureOptions<RpcWalletGeneratorOptions>> { configureOptions }));

            container.RegisterSingleton<IJsonRpcRawResponseFormatter, JsonRpcRawResponseFormatter>();
            container.RegisterSingleton<IJsonRpcClient, JsonRpcClient>();
            container.RegisterSingleton<IJsonRpcClientRaw, JsonRpcClientRaw>();
            container.RegisterSingleton<IJsonRpcRequestBuilder, JsonRpcRequestBuilder>();

            container.RegisterSingleton<IMongoClient>(() => new MongoClient($"{settings.Mongo.Host}:{settings.Mongo.Port}"));
            container.RegisterSingleton<IMongoDatabase>(() => container.GetInstance<IMongoClient>().GetDatabase(settings.Mongo.DbName));
            container.RegisterSingleton<IMongoCollection<TransactionMongoEntity>>(() => container.GetInstance<IMongoDatabase>().GetCollection<TransactionMongoEntity>(settings.Mongo.CollectionName));

        }
    }
}
