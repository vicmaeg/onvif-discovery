
using OnvifDiscovery.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace OnvifDiscovery.Interfaces
{
	public interface IDiscoveryService
	{
		ObservableCollection<DiscoveryDevice> DiscoveredDevices { get; }
		Task Start ();
		void Stop ();
	}
}
