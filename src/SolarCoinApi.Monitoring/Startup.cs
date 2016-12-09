using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolarCoinApi.RpcJson.JsonRpc;
using SolarCoinApi.Core.Options;
using SolarCoinApi.Common;
using SolarCoinApi.Core.Log;
using MongoDB.Bson.Serialization;
using SimpleInjector;
using SimpleInjector.Integration.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using SimpleInjector.Integration.AspNetCore;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using SolarCoinApi.Core;
using Microsoft.Extensions.PlatformAbstractions;

namespace SolarCoinApi.Monitoring
{
    public class Startup
    {
        private Container container = new Container();

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
#if DEBUG
                .AddJsonFile("appsettings.Debug.json", optional: false, reloadOnChange: true);
#elif RELEASE
                .AddJsonFile("appsettings.Release.json", optional: false, reloadOnChange: true);
#endif

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
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
            container.RegisterMvcControllers(app);

            container.RegisterMvcViewComponents(app);
            
            container.RegisterSingleton<LoggerOptions>(() => new LoggerOptions
            {
                ConnectionString = Configuration.GetSection("logging:connectionString").Value,
                ErrorTableName = Configuration.GetSection("logging:errorTableName").Value,
                InfoTableName = Configuration.GetSection("logging:infoTableName").Value,
                WarningTableName = Configuration.GetSection("logging:warningTableName").Value
            });

            container.RegisterSingleton<ILog>(() => new TableLogger1(container.GetInstance<LoggerOptions>(), Convert.ToBoolean(Configuration.GetSection("verboseLogging").Value)));

            container.RegisterSingleton<RpcWalletGeneratorOptions>(() => new RpcWalletGeneratorOptions
            {
                Endpoint = Configuration.GetSection("rpc:Endpoint").Value,
                Username = Configuration.GetSection("rpc:Username").Value,
                Password = Configuration.GetSection("rpc:Password").Value,
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

            container.RegisterSingleton<IMongoClient>(() => new MongoClient($"{Configuration.GetSection("mongo:host").Value}:{Configuration.GetSection("mongo:port").Value}"));
            container.RegisterSingleton<IMongoDatabase>(() => container.GetInstance<IMongoClient>().GetDatabase(Configuration.GetSection("mongo:dbName").Value));
            container.RegisterSingleton<IMongoCollection<TransactionMongoEntity>>(() => container.GetInstance<IMongoDatabase>().GetCollection<TransactionMongoEntity>(Configuration.GetSection("mongo:collectionName").Value));

        }
    }
}
