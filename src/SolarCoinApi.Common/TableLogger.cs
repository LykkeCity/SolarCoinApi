using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Microsoft.Extensions.Options;
using SolarCoinApi.Core;
using SolarCoinApi.Core.Options;

namespace SolarCoinApi.Common
{
    public class TableLogger : ILog
    {
        private readonly INoSQLTableStorage<LogEntity> _errorTableStorage;
        private readonly INoSQLTableStorage<LogEntity> _warningTableStorage;
        private readonly INoSQLTableStorage<LogEntity> _infoTableStorage;
        private readonly IOptions<LoggerOptions> _options;
        private readonly ISlackNotifier _notifier;
        private bool _verbose;

        public TableLogger(ISlackNotifier notifier, IOptions<LoggerOptions> options, bool verbose = false)
        {
            _options = options;

            _notifier = notifier;

            _errorTableStorage = new AzureTableStorage<LogEntity>(_options.Value.ConnectionString, _options.Value.ErrorTableName, null);
            _warningTableStorage = new AzureTableStorage<LogEntity>(_options.Value.ConnectionString, _options.Value.WarningTableName, null);
            _infoTableStorage = new AzureTableStorage<LogEntity>(_options.Value.ConnectionString, _options.Value.InfoTableName, null);

            _verbose = verbose;
        }


        private async Task Insert(string level, string component, string process, string context, string type, string stack,
            string msg, DateTime? dateTime)
        {
            var dt = dateTime ?? DateTime.UtcNow;
            var newEntity = LogEntity.Create(level, component, process, context, type, stack, msg, dt);

            if (level == "error" || level == "fatalerror")
                await _errorTableStorage.InsertAndGenerateRowKeyAsTimeAsync(newEntity, dt);
            if (level == "warning")
                await _warningTableStorage.InsertAndGenerateRowKeyAsTimeAsync(newEntity, dt);
            if (level == "info")
                await _infoTableStorage.InsertAndGenerateRowKeyAsTimeAsync(newEntity, dt);
        }

        public Task WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            return _verbose ?
                Insert("info", component, process, context, null, null, info, dateTime)
                : Task.CompletedTask;
        }

        public Task WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            return Insert("warning", component, process, context, null, null, info, dateTime);
        }

        public async Task WriteErrorAsync(string component, string process, string context, Exception type, DateTime? dateTime = null)
        {
            await TryWriteErrorToSlack("SolarCoinApi: " + component, type.Message);
            await Insert("error", component, process, context, type.GetType().ToString(), type.StackTrace, type.Message, dateTime);
        }

        public async Task WriteFatalErrorAsync(string component, string process, string context, Exception type, DateTime? dateTime = null)
        {
            await TryWriteErrorToSlack("SolarCoinApi: " + component, type.Message);
            await Insert("fatalerror", component, process, context, type.GetType().ToString(), type.StackTrace, type.Message, dateTime);
        }

        private Task TryWriteErrorToSlack(string component, string message)
        {
            return _notifier.Notify(new SlackMessage { Sender = component, Type = "Errors", Message = message });
        }
    }
}
