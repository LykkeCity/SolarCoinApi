using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolarCoinApi.Core.Timers.Interfaces
{
	public interface IStarter
	{
		void Start();
	    void Stop();
	    string GetComponentName();
	}
}
