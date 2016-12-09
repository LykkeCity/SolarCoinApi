using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Monitoring
{

    public class RpcResponseModel
    {
        public int RpcBlockCount { set; get; }
        public bool RpcIsAlive { set; get; }
    }

    public class MongoResponseModel
    {
        public bool MongoIsAlive { set; get; }
        public int MongoTxesCount { set; get; }
    }
}
