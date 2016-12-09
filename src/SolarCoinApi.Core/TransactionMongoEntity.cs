using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Core
{
    public class MongoEntity
    {
        [BsonId]
        public object Id { set; get; }
    }

    [BsonIgnoreExtraElements]
    public class TransactionMongoEntity : MongoEntity
    {
        public TransactionMongoEntity()
        {
            Vouts = new List<MongoVout>();
            Vins = new List<MongoVin>();
        }

        [BsonElement("txid")]
        public string TxId { set; get; }

        [BsonElement("blockhash")]
        public string BlockHash { set; get; }

        [BsonElement("blockindex")]
        public long BlockIndex { set; get; }

        [BsonElement("timestamp")]
        public long Timestamp { set; get; }

        [BsonElement("total")]
        public decimal Total { set; get; }

        [BsonElement("vout")]
        public List<MongoVout> Vouts { set; get; }

        [BsonElement("vin")]
        public List<MongoVin> Vins { set; get; }

        [BsonElement("wasprocessed")]
        public bool WasProcessed { set; get; }
    }

    public class MongoVout
    {
        [BsonElement("amount")]
        public long Amount { set; get; }

        [BsonElement("addresses")]
        public string Addresses { set; get; }
    }

    public class MongoVin
    {
        [BsonElement("amount")]
        public long Amount { set; get; }

        [BsonElement("addresses")]
        public string Addresses { set; get; }
    }
}
