using OnvifDiscovery.Common;
using OnvifDiscovery.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace OnvifDiscovery.Client;

/// <summary>
/// A simple Udp client that wraps <see cref="System.Net.Sockets.UdpClient"/>
/// It creates the probe messages also
/// </summary>
internal class OnvifUdpClient : IOnvifUdpClient
{
	private readonly UdpClient client;

	public OnvifUdpClient (IPEndPoint localEndpoint)
	{
		client = new UdpClient (localEndpoint) {
			EnableBroadcast = true
		};
	}

	public async Task<int> SendProbeAsync (Guid messageId, IPEndPoint endPoint)
	{
		var datagram = WSProbeMessageBuilder.NewProbeMessage (messageId);
		return await client.SendAsync (datagram, datagram.Length, endPoint);
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
