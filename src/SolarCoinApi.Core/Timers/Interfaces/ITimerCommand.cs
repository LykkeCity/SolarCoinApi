using System.Threading.Tasks;

namespace SolarCoinApi.Core.Timers.Interfaces
{
	public interface ITimerCommand
	{
		Task Execute();
	}
}