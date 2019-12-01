using System.Net;
using FluentAssertions;
using OnvifSharp.Discovery.Client;
using Xunit;

namespace OnvifSharp.Tests.Discovery.Client
{
	public class UdpClientFactoryTests
	{
		[Fact]
		public void CreateClient_WithEndpoint_InstanceReturned ()
		{
			// Arrange
			var factory = new UdpClientFactory ();
			var endpoint = new IPEndPoint (IPAddress.Loopback, 0);

			// Act
			var client = factory.CreateClient (endpoint);

			// Assert
			client.Should ().NotBeNull ();
			client.Should ().BeOfType<UdpClientWrapper> ();
		}

		[Fact]
		public void CreateClientForeachInterface_InstancesReturned ()
		{
			// Arrange
			var factory = new UdpClientFactory ();

			// Act
			var clients = factory.CreateClientForeachInterface ();

			// Assert
			clients.Should ().HaveCountGreaterOrEqualTo (1);
			clients.Should ().AllBeOfType<UdpClientWrapper> ();
		}
	}
}
