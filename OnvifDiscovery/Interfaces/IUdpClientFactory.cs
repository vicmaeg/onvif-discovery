using System.Collections.Generic;
using System.Net;

namespace OnvifDiscovery.Interfaces
{
	public interface IUdpClientFactory
	{
		IOnvifUdpClient CreateClient (IPEndPoint endpoint);

		IEnumerable<IOnvifUdpClient> CreateClientForeachInterface ();
	}
}

