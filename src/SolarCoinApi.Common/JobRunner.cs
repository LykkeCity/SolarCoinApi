using SolarCoinApi.Core.Timers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Loader;
using System.Threading;

namespace SolarCoinApi.Common
{
    public class JobsRunner
    {
        private List<TimerPeriodEx> _jobs;
        private bool _startedWatching = false;

        public JobsRunner()
        {
            _jobs = new List<TimerPeriodEx>();
        }

        public void AddJob(TimerPeriodEx job)
        {
            if (_startedWatching)
                throw new Exception($"Could not add {job.GetComponentName()} to the list of to-watch jobs: already watching. Use ${nameof(AddJob)} before ${nameof(StartAndWatch)}!");
            
            _jobs.Add(job);
        }

        public async Task StartAndWatch()
        {
            try
            {
                if (_startedWatching)
                    throw new Exception("Already watching jobs");

                if (_jobs.Count() == 0)
                    throw new Exception($"No jobs to watch. Add jobs using {nameof(AddJob)}");

                _startedWatching = true;

                foreach (var job in _jobs)
                {
                    Console.WriteLine($"Staring {job.GetComponentName()}");
                    job.Start();
                }

                var SIGTERM = new ManualResetEvent(false);
                var end = new ManualResetEvent(false);
                
                AssemblyLoadContext.Default.Unloading += ctx =>
                {
                    Console.WriteLine("SIGTERM recieved");
                    SIGTERM.Set();

                    end.WaitOne();
                };

                Console.WriteLine("Waiting for SIGTERM");
                SIGTERM.WaitOne();

                Console.WriteLine("Stopping all jobs...");

                foreach (var job in _jobs)
                {
                    job.Stop();
                }

                Console.WriteLine("Waiting for all jobs to complete...");

                await Task.WhenAll(_jobs.Select(x => x.CurrentIteration ?? Task.CompletedTask).ToArray());

                Console.WriteLine("All jobs stopped and completed.");

                end.Set();

                return;
            }
            catch (Exception e)
            {
                var a = 234;
                throw;
            }

        }
    }
}
