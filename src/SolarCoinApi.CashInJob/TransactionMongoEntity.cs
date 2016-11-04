using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace SolarCoinApi.CashInJob
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
            Vouts = new List<Vout>();
            Vins = new List<Vin>();
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
        public List<Vout> Vouts { set; get; }

        [BsonElement("vin")]
        public List<Vin> Vins { set; get; }
    }

    public class Vout
    {
        [BsonElement("amount")]
        public long Amount { set; get; }

        [BsonElement("addresses")]
        public string Addresses { set; get; }
    }

    public class Vin
    {
        [BsonElement("amount")]
        public long Amount { set; get; }

        [BsonElement("addresses")]
        public string Addresses { set; get; }
    }
}
