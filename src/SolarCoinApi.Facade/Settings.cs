using SolarCoinApi.Common;
using SolarCoinApi.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Facade
{
    public class FacadeSettings : IValidatable
    {
        public RpcSettings Rpc { set; get; }
        public LoggerSettings Logger { set; get; }
        public TableSettings GeneratedWallets { set; get; }

        public void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
