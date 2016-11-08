﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SolarCoinApi.AzureStorage.Queue;
using SolarCoinApi.AzureStorage.Tables;
using SolarCoinApi.CashInJobRunner;
using SolarCoinApi.Common;
using SolarCoinApi.Core;
using SolarCoinApi.Core.Options;
using SolarCoinApi.RpcJson.JsonRpc;

namespace SolarCoinApi.CashOutJobRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.Title = "SolarCoin CashIn job";

                var settings = AppSettings.FromFile("appsettings.json");

                IConfigureOptions<LoggerOptions> configureOptions = new ConfigureOptions<LoggerOptions>(x =>
                {
                    x.ConnectionString = settings.Logger.ConnectionString;
                    x.ErrorTableName = settings.Logger.ErrorTableName;
                    x.InfoTableName = settings.Logger.InfoTableName;
                    x.WarningTableName = settings.Logger.WarningTableName;
                });

                var logger = new TableLogger(new OptionsManager<LoggerOptions>(new List<IConfigureOptions<LoggerOptions>> { configureOptions }));

                var queue = new AzureQueueExt(settings.Queue.ConnectionString, settings.Queue.Name);
                

                var rawRpcClientOptions = new ConfigureOptions<RpcWalletGeneratorOptions>(x =>
                {
                    x.Endpoint = settings.Rpc.Endpoint;
                    x.Password = settings.Rpc.Password;
                    x.Username = settings.Rpc.Username;
                });

                var rpcClient = new JsonRpcClient(new JsonRpcClientRaw(new JsonRpcRequestBuilder(), logger, new OptionsManager<RpcWalletGeneratorOptions>(new List<IConfigureOptions<RpcWalletGeneratorOptions>>() { rawRpcClientOptions })), new JsonRpcRawResponseFormatter(), logger);

                var timer = new CashOutJob.CashOutJob("CashInJob", 60 * 1000, logger, queue, rpcClient);

                timer.Start();

                Console.WriteLine("The job has started! Enter 'q' to quit...");

                while (Console.ReadLine() != "q") continue;

                timer.Stop();

            }
            catch (Exception e)
            {
                var err = e;
                while (err != null)
                {
                    Console.WriteLine(err.Message);
                    Console.WriteLine();
                    Console.WriteLine("Stack trace:");
                    Console.WriteLine(err.StackTrace);

                    err = err.InnerException;
                }

                Console.ReadKey();
            }
        }
    }
}
