using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SolarCoinApi.RpcJson.JsonRpc;
using System.Net.Http;
using Common.Log;

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
                await _logger.WriteErrorAsync("SolarCoinApi.Monitoring.RpcController", "", "", e);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Lists transactions sent/received by the node's default account
        /// </summary>
        /// <param name="count">Amount of transactions</param>
        /// <returns></returns>
        [HttpGet]
        [Route("listrawtransactions/{count}")]
        public async Task<IActionResult> ListTransactions(int count)
        {
            try
            {
               return Json(await _client.ListTransactions(count));
            }
            catch (HttpRequestException e)
            {
                return Json(new RpcResponseModel { RpcIsAlive = false, RpcBlockCount = -1 });
            }
            catch (Exception e)
            {
                await _logger.WriteErrorAsync("SolarCoinApi.Monitoring.RpcController", "ListTransactions", "", e);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Gets a hex of a given transaction
        /// </summary>
        /// <param name="txId">Transaction hash</param>
        /// <returns></returns>
        [HttpGet]
        [Route("getrawtransaction/{txId}")]
        public async Task<IActionResult> GetRawTransaction(string txId)
        {
            try
            {
                return Json(await _client.GetRawTransaction(txId));
            }
            catch (HttpRequestException e)
            {
                return Json(new RpcResponseModel { RpcIsAlive = false, RpcBlockCount = -1 });
            }
            catch (Exception e)
            {
                await _logger.WriteErrorAsync("SolarCoinApi.Monitoring.RpcController", "GetRawTransaction", "", e);
                    return StatusCode(500);
            }
        }
    }
}
