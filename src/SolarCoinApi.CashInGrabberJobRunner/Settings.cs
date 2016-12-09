using SolarCoinApi.Common;
using SolarCoinApi.Core.Options;
using SolarCoinApi.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.CashInGrabberJobRunner
{
    public class CashInGrabberSettings : IValidatable
    {
        public LoggerSettings Logger { set; get; }
        public MongoSettings Mongo { set; get; }
        public QueueSettings TransitQueue { set; get; }
        public TableSettings Monitoring { set; get; }
        public bool VerboseLogging { set; get; }
        public int Threshold { get; set; }
        public int Period { get; set; }

        public void Validate()
        {
            if (Logger == null)
                throw new Exception("Logger section should be present");
            if (Mongo == null)
                throw new Exception("Mongo section should be present");
            if (TransitQueue == null)
                throw new Exception("Transit Queue section should be present");

            if (string.IsNullOrWhiteSpace(Logger.ConnectionString))
                throw new Exception("Logger Connection String should be present");
            if (string.IsNullOrWhiteSpace(Logger.ErrorTableName))
                throw new Exception("Logger Error Table Name should be present");
            if (string.IsNullOrWhiteSpace(Logger.InfoTableName))
                throw new Exception("Logger Info Table Name should be present");
            if (string.IsNullOrWhiteSpace(Logger.WarningTableName))
                throw new Exception("Logger Warning Table Name should be present");

            if (string.IsNullOrWhiteSpace(TransitQueue.ConnectionString))
                throw new Exception("Transit Queue Connection String should be present");
            if (string.IsNullOrWhiteSpace(TransitQueue.Name))
                throw new Exception("Transit Queue should be present");

            if (string.IsNullOrWhiteSpace(Monitoring.Name))
                throw new Exception("Monitoring Connection String should be present");
            if (string.IsNullOrWhiteSpace(Monitoring.ConnectionString))
                throw new Exception("Monitoring Name should be present");


            if (string.IsNullOrWhiteSpace(Mongo.CollectionName))
                throw new Exception("Mongo Collection Name should be present");
            if (string.IsNullOrWhiteSpace(Mongo.DbName))
                throw new Exception("Mongo Db Name should be present");
            if (string.IsNullOrWhiteSpace(Mongo.Host))
                throw new Exception("Mongo Host should be present");
            if (string.IsNullOrWhiteSpace(Mongo.Port))
                throw new Exception("Mongo Port should be present");
        }
    }
}
