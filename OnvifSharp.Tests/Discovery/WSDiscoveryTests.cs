using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using OnvifDiscovery;
using OnvifDiscovery.Client;
using OnvifDiscovery.Exceptions;
using OnvifDiscovery.Interfaces;
using Xunit;

namespace OnvifDiscovery.Tests
{
	public class WSDiscoveryTests
	{
		OnvifDiscovery wSDiscovery;
		Mock<IUdpClientFactory> udpClientFactoryMock;

		public WSDiscoveryTests ()
		{
			udpClientFactoryMock = new Mock<IUdpClientFactory> ();
			wSDiscovery = new OnvifDiscovery (udpClientFactoryMock.Object);
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
