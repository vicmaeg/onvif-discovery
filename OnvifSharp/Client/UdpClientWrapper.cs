using OnvifDiscovery.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace OnvifDiscovery.Client
{
	/// <summary>
	/// A simple <see cref="System.Net.Sockets.UdpClient"/> wrapper
	/// </summary>
	public class UdpClientWrapper : IUdpClient
	{
		UdpClient client;

		public UdpClientWrapper (IPEndPoint localpoint)
		{
			client = new UdpClient (localpoint) {
				EnableBroadcast = true
			};
		}

		public async Task<int> SendAsync (byte[] datagram, int bytes, IPEndPoint endPoint)
		{
			return await client.SendAsync (datagram, bytes, endPoint);
		}

		public async Task<UdpReceiveResult> ReceiveAsync ()
		{
			return await client.ReceiveAsync ();
		}

		public void Close ()
		{
			client.Close ();
		}
	}
}
