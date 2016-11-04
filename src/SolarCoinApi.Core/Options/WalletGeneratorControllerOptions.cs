using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Core.Options
{
    public class WalletGeneratorControllerOptions
    {
        public string ConnectionString { set; get; }
        public string TableName { set; get; }
    }
}
