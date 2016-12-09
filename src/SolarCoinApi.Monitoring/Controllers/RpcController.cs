using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SolarCoinApi.RpcJson.JsonRpc;
using System.Net.Http;

namespace SolarCoinApi.Monitoring.Controllers
{
    [Route("api/[controller]")]
    public class RpcController : Controller
    {
        private IJsonRpcClient _client;


        public RpcController(IJsonRpcClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Get SolarCoin node's general info
        /// </summary>
        /// <returns>General info</returns>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var resp = new RpcResponseModel();

                var count = _client.GetBlockCount().Result;

                resp.RpcBlockCount = count;
                resp.RpcIsAlive = true;

                return Json(resp);
            }
            catch (AggregateException e)
            {
                if (e.InnerExceptions.Any(x => x is HttpRequestException))
                    return Json(new RpcResponseModel { RpcIsAlive = false, RpcBlockCount = -1 });
            }
            catch (Exception e) { }
            return null;
        }
    }
}
