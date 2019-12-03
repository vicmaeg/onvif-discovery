using System.Collections.Generic;
using System.Net;

namespace OnvifDiscovery.Interfaces
{
	/// <summary>
	/// Factory to create <see cref="IOnvifUdpClient"/> clients
	/// </summary>
	public interface IUdpClientFactory
	{
		/// <summary>
		/// Creates an <see cref="IOnvifUdpClient"/> based on the endpoint
		/// </summary>
		/// <param name="endpoint">The endpoint this client is created</param>
		/// <returns>An instance of <see cref="IOnvifUdpClient"/></returns>
		IOnvifUdpClient CreateClient (IPEndPoint endpoint);

		/// <summary>
		/// Creates an <see cref="IOnvifUdpClient"/> for each valid network interface
		/// </summary>
		/// <returns>A list of <see cref="IOnvifUdpClient"/></returns>
		IEnumerable<IOnvifUdpClient> CreateClientForeachInterface ();
	}
}

