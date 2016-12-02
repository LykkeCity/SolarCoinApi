using SimpleInjector;
using SolarCoinApi.AzureStorage;
using SolarCoinApi.AzureStorage.Queue;
using SolarCoinApi.AzureStorage.Tables;
using SolarCoinApi.Common;
using SolarCoinApi.Common.Triggers;
using SolarCoinApi.Common.Triggers.Attributes;
using SolarCoinApi.Common.Triggers.Bindings;
using SolarCoinApi.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.CashInGrabberJobRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var container = new Container();
            

            while (true) Console.ReadKey();

        }
    }
}
