using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SolarCoinApi.RpcJson.JsonRpc;
using System.Net.Http;
using SolarCoinApi.Core.Log;

namespace SolarCoinApi.Monitoring.Controllers
{
    [Route("api/[controller]")]
    public class RpcController : Controller
    {
        private IJsonRpcClient _client;
        private ILog _logger;

        public RpcController(IJsonRpcClient client, ILog logger)
        {
            _client = client;
            _logger = logger;
        }

        /// <summary>
        /// Get SolarCoin node's general info
        /// </summary>
        /// <returns>General info</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var resp = new RpcResponseModel();

                var count = await _client.GetBlockCount();

                resp.RpcBlockCount = count;
                resp.RpcIsAlive = true;

                return Json(resp);
            }
            catch (HttpRequestException e)
            {
                return Json(new RpcResponseModel { RpcIsAlive = false, RpcBlockCount = -1 });
            }
            catch (Exception e)
            {
                await _logger.WriteError("SolarCoinApi.Monitoring.RpcController", "", "", e);
                return StatusCode(500);
            }
        }
    }
}
