using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using SolarCoinApi.Core.Log;
using SolarCoinApi.Core;

namespace SolarCoinApi.Monitoring.Controllers
{
    [Route("api/[controller]")]
    public class MongoController : Controller
    {
        private IMongoDatabase _mongoDb;
        private IMongoCollection<TransactionMongoEntity> _collection;
        private ILog _logger;

        public MongoController(IMongoDatabase mongoDb, IMongoCollection<TransactionMongoEntity> collection, ILog logger)
        {
            _mongoDb = mongoDb;
            _collection = collection;
            _logger = logger;
        }
        
        /// <summary>
        /// Get database's general info
        /// </summary>
        /// <returns>General info</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var resp = new MongoResponseModel();

            try
            {

                await _mongoDb.RunCommandAsync((Command<BsonDocument>)"{ping:1}");

                resp.MongoTxesCount = (int)await _collection.CountAsync(x => true);
                resp.MongoIsAlive = true;

                return Json(resp);
            }
            catch (Exception e) when (e is MongoConnectionException || e is TimeoutException)
            {
                resp.MongoTxesCount = -1;
                resp.MongoIsAlive = false;

                return Json(resp);
            }
            catch (Exception e)
            {
                await _logger.WriteError("SolarCoinApi.Monitoring.MongoController", "", "", e);
                return StatusCode(500);
            }
        }
        
        /// <summary>
        /// Get info about specific transaction
        /// </summary>
        /// <param name="txId">Transaction ID</param>
        /// <returns>Transaction if it exists, 'null' otherwise</returns>
        [HttpGet("{txId}")]
        public async Task<IActionResult> Get(string txId)
        {
            try
            {
                return Json(_collection.Find(x => x.TxId == txId).FirstOrDefault());
            }
            catch(Exception e)
            {
                await _logger.WriteError("SolarCoinApi.Monitoring.MongoController", "", "", e);
                return StatusCode(500);
            }
        }
    }
}
