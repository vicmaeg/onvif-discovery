using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using OnvifDiscovery.Interfaces;

namespace OnvifDiscovery.Client
{
	internal class UdpClientFactory : IUdpClientFactory
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

				if (!IsValidAdapter (adapter)) { continue; }
				IPInterfaceProperties adapterProperties = adapter.GetIPProperties ();
				foreach (var ua in adapterProperties.UnicastAddresses) {
					if (ua.Address.AddressFamily == AddressFamily.InterNetwork) {
						IPEndPoint myLocalEndPoint = new IPEndPoint (ua.Address, 0); // port does not matter
						try {
							IOnvifUdpClient client = CreateClient (myLocalEndPoint);
							clients.Add (client);
						} catch (SocketException) {
							// Discard clients that produces a SocketException when created
						}
					}
				}
			}
			return clients;
		}

		bool IsValidAdapter (NetworkInterface adapter)
		{
			// Only select interfaces that are Ethernet type and support IPv4 (important to minimize waiting time)
			if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
				!adapter.NetworkInterfaceType.ToString ().ToLower ().StartsWith ("wireless")) return false;
			if (adapter.OperationalStatus == OperationalStatus.Down) { return false; }
			if (!adapter.Supports (NetworkInterfaceComponent.IPv4)) { return false; }
			return true;
		}
	}
}
