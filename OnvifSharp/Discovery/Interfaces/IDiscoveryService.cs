
using OnvifSharp.Discovery.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace OnvifSharp.Discovery.Interfaces
{
	public interface IDiscoveryService
	{
		ObservableCollection<DiscoveryDevice> DiscoveredDevices { get; }
		Task Start ();
		void Stop ();
	}
}
