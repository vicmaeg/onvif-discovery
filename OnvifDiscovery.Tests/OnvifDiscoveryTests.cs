using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using OnvifDiscovery.Client;
using OnvifDiscovery.Exceptions;
using OnvifDiscovery.Interfaces;
using OnvifDiscovery.Tests.TestHelpers;
using Xunit;

namespace OnvifDiscovery.Tests
{
	public class OnvifDiscoveryTests
	{
		Discovery wSDiscovery;
		Mock<IUdpClientFactory> udpClientFactoryMock;

		public OnvifDiscoveryTests ()
		{
			udpClientFactoryMock = new Mock<IUdpClientFactory> ();
			wSDiscovery = new Discovery (udpClientFactoryMock.Object);
		}

		[Fact]
		public async Task Discover_ClientFactoryMissingClients_ThrowsException ()
		{
			// Arrange
			udpClientFactoryMock.Setup (cf => cf.CreateClientForeachInterface ()).Returns (new List<OnvifUdpClient> ());

			// Act
			Func<Task> act = async () => { await wSDiscovery.Discover (5); };

			// Assert
			await act.Should ().ThrowAsync<DiscoveryException> ();
		}

		[Fact]
		public async Task Discover_OneClient_TwoCameras_ReturnsListWithTwoCameras ()
		{
			// Arrange
			bool firstTime = true;
			Guid messageId = Guid.NewGuid ();
			TestCamera camera1 = null;
			TestCamera camera2 = null;
			Mock<IOnvifUdpClient> udpClientMock = new Mock<IOnvifUdpClient> ();
			udpClientFactoryMock.Setup (cf => cf.CreateClientForeachInterface ()).Returns (new List<IOnvifUdpClient> { udpClientMock.Object });
			udpClientMock.Setup (cl => cl.SendProbeAsync (It.IsAny<Guid> (), It.IsAny<IPEndPoint> ()))
				.ReturnsAsync (200)
				.Callback<Guid, IPEndPoint> ((mId, endpoint) => {
					messageId = mId;
				});
			udpClientMock.Setup (udp => udp.ReceiveAsync ())
						.ReturnsAsync (() => {
							if (firstTime) {
								firstTime = false;
								camera1 = new TestCamera (messageId, Guid.NewGuid (), "AxisHardware", "AxisName", "192.168.1.12");
								return new UdpReceiveResult (Utils.CreateProbeResponse (camera1), IPEndPoint.Parse ("192.168.1.12"));
							} else {
								camera2 = new TestCamera (messageId, Guid.NewGuid (), "SamsungHardware", "SamsungName", "192.168.1.14");
								return new UdpReceiveResult (Utils.CreateProbeResponse (camera2), IPEndPoint.Parse ("192.168.1.14"));
							}
						});

			// Act
			var discoveredDevices = await wSDiscovery.Discover (1);

			// Assert
			discoveredDevices.Should ().HaveCount (2);
			var firstDevice = discoveredDevices.ElementAt (0);
			firstDevice.Address.Should ().Be (camera1.IP);
			firstDevice.Model.Should ().Be (camera1.Model);
			firstDevice.Mfr.Should ().Be (camera1.Manufacturer);
			var secondDevice = discoveredDevices.ElementAt (1);
			secondDevice.Address.Should ().Be (camera2.IP);
			secondDevice.Model.Should ().Be (camera2.Model);
			secondDevice.Mfr.Should ().Be (camera2.Manufacturer);

			udpClientFactoryMock.Verify (cf => cf.CreateClientForeachInterface (), Times.Once);
			udpClientMock.Verify (c => c.SendProbeAsync (It.IsAny<Guid> (), It.IsAny<IPEndPoint> ()), Times.Once);
			udpClientMock.Verify (c => c.ReceiveAsync (), Times.AtLeast (2));
		}

		[Fact]
		public async Task Discover_TwoClients_OneCameraPerclient_ReturnsListWithTwoCameras ()
		{
			// Arrange
			Guid messageId1 = Guid.NewGuid ();
			Guid messageId2 = Guid.NewGuid ();
			TestCamera camera1 = null;
			TestCamera camera2 = null;
			Mock<IOnvifUdpClient> udpClient1Mock = new Mock<IOnvifUdpClient> ();
			Mock<IOnvifUdpClient> udpClient2Mock = new Mock<IOnvifUdpClient> ();
			udpClientFactoryMock.Setup (cf => cf.CreateClientForeachInterface ()).Returns (new List<IOnvifUdpClient> { udpClient1Mock.Object, udpClient2Mock.Object });
			udpClient1Mock.Setup (cl => cl.SendProbeAsync (It.IsAny<Guid> (), It.IsAny<IPEndPoint> ()))
				.ReturnsAsync (200)
				.Callback<Guid, IPEndPoint> ((mId, endpoint) => {
					messageId1 = mId;
				});
			udpClient2Mock.Setup (cl => cl.SendProbeAsync (It.IsAny<Guid> (), It.IsAny<IPEndPoint> ()))
				.ReturnsAsync (200)
				.Callback<Guid, IPEndPoint> ((mId, endpoint) => {
					messageId2 = mId;
				});
			udpClient1Mock.Setup (udp => udp.ReceiveAsync ())
						.ReturnsAsync (() => {
							camera1 = new TestCamera (messageId1, Guid.NewGuid (), "AxisHardware", "AxisName", "192.168.1.12");
							return new UdpReceiveResult (Utils.CreateProbeResponse (camera1), IPEndPoint.Parse ("192.168.1.12"));
						});
			udpClient2Mock.Setup (udp => udp.ReceiveAsync ())
						.ReturnsAsync (() => {
							camera2 = new TestCamera (messageId2, Guid.NewGuid (), "SamsungHardware", "SamsungName", "192.168.1.14");
							return new UdpReceiveResult (Utils.CreateProbeResponse (camera2), IPEndPoint.Parse ("192.168.1.14"));
						});



			// Act
			var discoveredDevices = await wSDiscovery.Discover (1);

			// Assert
			discoveredDevices.Should ().HaveCount (2);
			var firstDevice = discoveredDevices.ElementAt (0);
			firstDevice.Address.Should ().Be (camera1.IP);
			firstDevice.Model.Should ().Be (camera1.Model);
			firstDevice.Mfr.Should ().Be (camera1.Manufacturer);
			var secondDevice = discoveredDevices.ElementAt (1);
			secondDevice.Address.Should ().Be (camera2.IP);
			secondDevice.Model.Should ().Be (camera2.Model);
			secondDevice.Mfr.Should ().Be (camera2.Manufacturer);

			udpClientFactoryMock.Verify (cf => cf.CreateClientForeachInterface (), Times.Once);
			udpClient1Mock.Verify (c => c.SendProbeAsync (It.IsAny<Guid> (), It.IsAny<IPEndPoint> ()), Times.Once);
			udpClient1Mock.Verify (c => c.ReceiveAsync (), Times.AtLeast (2));
		}

		[Fact]
		public async Task Discover_CancelBeforeFinish_ReturnsPartialList ()
		{
			// Arrange
			Guid messageId = Guid.NewGuid ();
			TestCamera camera = null;
			Mock<IOnvifUdpClient> udpClientMock = new Mock<IOnvifUdpClient> ();
			udpClientFactoryMock.Setup (cf => cf.CreateClientForeachInterface ()).Returns (new List<IOnvifUdpClient> { udpClientMock.Object });
			udpClientMock.Setup (cl => cl.SendProbeAsync (It.IsAny<Guid> (), It.IsAny<IPEndPoint> ()))
				.ReturnsAsync (200)
				.Callback<Guid, IPEndPoint> ((mId, endpoint) => {
					messageId = mId;
				});
			var cameraNumber = 1;
			udpClientMock.Setup (udp => udp.ReceiveAsync ())
						.Returns (async () => {
							await Task.Delay (500).ConfigureAwait (false);
							var ip = $"192.168.1.{cameraNumber}";
							cameraNumber++;
							camera = new TestCamera (messageId, Guid.NewGuid (), "AxisHardware", "AxisName", ip);
							return new UdpReceiveResult (Utils.CreateProbeResponse (camera), IPEndPoint.Parse (ip));
						});

			// Act
			CancellationTokenSource cancellation = new CancellationTokenSource (600); // Cancel after 600ms. Since cameras are received every 500ms, we will receive only one
			var discoveredDevices = await wSDiscovery.Discover (10, cancellation.Token);

			// Assert
			discoveredDevices.Should ().HaveCount (1);
		}

		[Fact]
		public async Task Discover_Exception_In_ReceiveAsync_Do_Not_Throws_And_Returns_List ()
		{
			// Arrange
			Guid messageId = Guid.NewGuid ();
			TestCamera camera = null;
			Mock<IOnvifUdpClient> udpClientMock = new Mock<IOnvifUdpClient> ();
			udpClientFactoryMock.Setup (cf => cf.CreateClientForeachInterface ()).Returns (new List<IOnvifUdpClient> { udpClientMock.Object });
			udpClientMock.Setup (cl => cl.SendProbeAsync (It.IsAny<Guid> (), It.IsAny<IPEndPoint> ()))
				.ReturnsAsync (200)
				.Callback<Guid, IPEndPoint> ((mId, endpoint) => {
					messageId = mId;
				});
			udpClientMock.SetupSequence (udp => udp.ReceiveAsync ())
						.ReturnsAsync (() => {
							var ip = $"192.168.1.1";
							camera = new TestCamera (messageId, Guid.NewGuid (), "AxisHardware", "AxisName", ip);
							return new UdpReceiveResult (Utils.CreateProbeResponse (camera), IPEndPoint.Parse (ip));
						})
						.ThrowsAsync (new Exception())
						.Returns (async () => {
							var ip = $"192.168.1.2";
							await Task.Delay (700);
							camera = new TestCamera (messageId, Guid.NewGuid (), "AxisHardware", "AxisName", ip);
							return new UdpReceiveResult (Utils.CreateProbeResponse (camera), IPEndPoint.Parse (ip));
						})
						.Returns (async () => {
							var ip = $"192.168.1.3";
							await Task.Delay (400);
							camera = new TestCamera (messageId, Guid.NewGuid (), "AxisHardware", "AxisName", ip);
							return new UdpReceiveResult (Utils.CreateProbeResponse (camera), IPEndPoint.Parse (ip));
						 });

			// Act
			var discoveredDevices = await wSDiscovery.Discover (1);

			// Arrange
			discoveredDevices.Should().NotBeEmpty();
			discoveredDevices.Should().HaveCount(2);
		}
	}
}
