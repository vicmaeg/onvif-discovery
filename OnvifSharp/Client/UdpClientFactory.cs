using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using OnvifDiscovery.Interfaces;

namespace OnvifDiscovery.Client
{
	public class UdpClientFactory : IUdpClientFactory
	{
		public IOnvifUdpClient CreateClient (IPEndPoint endpoint)
		{
			return new OnvifUdpClient (endpoint);
		}

		public IEnumerable<IOnvifUdpClient> CreateClientForeachInterface ()
		{
			var clients = new List<IOnvifUdpClient> ();

			NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces ();
			foreach (NetworkInterface adapter in nics) {
				// Only select interfaces that are Ethernet type and support IPv4 (important to minimize waiting time)
				if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
					!adapter.NetworkInterfaceType.ToString ().ToLower ().StartsWith ("wireless")) continue;
				if (adapter.OperationalStatus == OperationalStatus.Down) { continue; }
				if (!adapter.Supports (NetworkInterfaceComponent.IPv4)) { continue; }

				IPInterfaceProperties adapterProperties = adapter.GetIPProperties ();
				foreach (var ua in adapterProperties.UnicastAddresses) {
					if (ua.Address.AddressFamily == AddressFamily.InterNetwork) {
						IPEndPoint myLocalEndPoint = new IPEndPoint (ua.Address, 0); // port does not matter
						try {
							IOnvifUdpClient client = CreateClient (myLocalEndPoint);
							clients.Add (client);
						} catch (SocketException) { }
					}
				}
			}
			return clients;
		}
	}
}
