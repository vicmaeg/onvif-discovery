using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using OnvifSharp.Discovery;
using OnvifSharp.Discovery.Client;
using OnvifSharp.Discovery.Exceptions;
using OnvifSharp.Discovery.Interfaces;
using Xunit;

namespace OnvifSharp.Tests.Discovery
{
	public class WSDiscoveryTests
	{
		WSDiscovery wSDiscovery;
		Mock<IUdpClientFactory> udpClientFactoryMock;

		public WSDiscoveryTests ()
		{
			udpClientFactoryMock = new Mock<IUdpClientFactory> ();
			wSDiscovery = new WSDiscovery (udpClientFactoryMock.Object);
		}

		[Fact]
		public async Task Discover_ClientFactoryMissingClients_ThrowsException ()
		{
			// Arrange
			udpClientFactoryMock.Setup (cf => cf.CreateClientForeachInterface ()).Returns (new List<UdpClientWrapper> ());

			// Act
			Func<Task> act = async () => { await wSDiscovery.Discover (5); };

			// Assert
			await act.Should ().ThrowAsync<DiscoveryException> ();
		}

		[Fact]
		public async Task Discover_OneClient_TwoCameras_ReturnsListWithTwoCameras ()
		{

		}

		[Fact]
		public async Task Discover_TwoClients_OneCameraPerclient_ReturnsListWithTwoCameras ()
		{

		}

		[Fact]
		public async Task Discover_CancelBeforeFinish_ReturnsEmptyList ()
		{

		}
	}
}
