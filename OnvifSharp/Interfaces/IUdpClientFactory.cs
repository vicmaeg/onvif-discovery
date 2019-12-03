using System.Collections.Generic;
using System.Net;

namespace OnvifDiscovery.Interfaces
{
	public interface IUdpClientFactory
	{
		IUdpClient CreateClient (IPEndPoint endpoint);

		IEnumerable<IUdpClient> CreateClientForeachInterface ();
	}
}

