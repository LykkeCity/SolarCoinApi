﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using SolarCoinApi.Core.Log;
using SolarCoinApi.Core.Timers.Interfaces;

namespace SolarCoinApi.Core.Timers
{
    // Таймер, который исполняет метод Execute через определенный интервал после окончания исполнения метода Execute
    public abstract class TimerPeriod : IStarter, ITimerCommand
    {
        private readonly string _componentName;
        private readonly int _periodMs;
        protected readonly ILog _log;

        protected TimerPeriod(string componentName, int periodMs, ILog log)
        {
            _componentName = componentName;

            _periodMs = periodMs;
            _log = log;
        }

        public bool Working { get; private set; }

        private void LogFatalError(Exception exception)
        {
            try
            {
                _log.WriteFatalErrorAsync(_componentName, "Loop", "", exception).Wait();
            }
            catch (Exception)
            {
            }
        }

        public abstract Task Execute();

        private async Task ThreadMethod()
        {
            while (Working)
            {
                try
                {
                    await Execute();
                }
                catch (Exception exception)
                {
                    LogFatalError(exception);
                }
                await Task.Delay(_periodMs);
            }
        }

        public virtual void Start()
        {

            if (Working)
                return;

            Working = true;
            Task.Run(async () => { await ThreadMethod(); });

        }

        public void Stop()
        {
            Working = false;
        }

        public string GetComponentName()
        {
            return _componentName;
        }
    }
}
