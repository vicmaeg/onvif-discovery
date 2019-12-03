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
		/// <summary>
		/// Send a probe message on the network
		/// </summary>
		/// <param name="messageId">The messageId, to filter the responses when received</param>
		/// <param name="endPoint">The endpoint to send the probe, normally a Multicast</param>
		/// <returns></returns>
		Task<int> SendProbeAsync (Guid messageId, IPEndPoint endPoint);

		/// <summary>
		/// Receive a <see cref="UdpReceiveResult"/>
		/// </summary>
		/// <returns>the udp receive result</returns>
		Task<UdpReceiveResult> ReceiveAsync ();

		/// <summary>
		/// Close the socket
		/// </summary>
		void Close ();
	}
}
