using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace OnvifDiscovery.Interfaces
{
	/// <summary>
	/// UDP Client interface to wrapp UdpClient and easily mock <see cref="System.Net.Sockets.UdpClient"/> in tests
	/// </summary>
	public interface IOnvifUdpClient
	{
		Task<int> SendProbeAsync (Guid messageId, IPEndPoint endPoint);
		Task<UdpReceiveResult> ReceiveAsync ();
		void Close ();
	}
}
