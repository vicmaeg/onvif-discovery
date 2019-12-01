using System.Collections.Generic;
using System.Net;

namespace OnvifSharp.Discovery.Interfaces
{
	public interface IUdpClientFactory
	{
		IUdpClient CreateClient (IPEndPoint endpoint);

		IEnumerable<IUdpClient> CreateClientForeachInterface ();
	}
}

