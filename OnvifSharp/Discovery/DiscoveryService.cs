using System.Collections.ObjectModel;
using OnvifSharp.Discovery.Interfaces;
using OnvifSharp.Discovery.Models;

namespace OnvifSharp.Discovery
{
	public class DiscoveryService : IDiscoveryService
	{
		public DiscoveryService ()
		{
			DiscoveredDevices = new ObservableCollection<DiscoveryDevice> ();
		}

		public ObservableCollection<DiscoveryDevice> DiscoveredDevices { get; }

		public void Start ()
		{
			throw new System.NotImplementedException ();
		}

		public void Stop ()
		{
			throw new System.NotImplementedException ();
		}
	}
}
