using System.Net;
using System.Net.Sockets;
using FluentAssertions;
using Moq;
using OnvifDiscovery.Client;
using OnvifDiscovery.Exceptions;
using OnvifDiscovery.Interfaces;
using OnvifDiscovery.Tests.TestHelpers;
using Xunit;

namespace OnvifDiscovery.Tests;

public class OnvifDiscoveryTests
{
    private readonly Mock<IUdpClientFactory> udpClientFactoryMock;
    private readonly Discovery wSDiscovery;

    public OnvifDiscoveryTests()
    {
        udpClientFactoryMock = new Mock<IUdpClientFactory>();
        wSDiscovery = new Discovery(udpClientFactoryMock.Object);
    }

    [Fact]
    public async Task Discover_ClientFactoryMissingClients_ThrowsException()
    {
        // Arrange
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface()).Returns(new List<OnvifUdpClient>());

        // Act
        var act = async () => { await wSDiscovery.Discover(5); };

        // Assert
        await act.Should().ThrowAsync<DiscoveryException>();
    }

    [Fact]
    public async Task Discover_OneClient_TwoCameras_ReturnsListWithTwoCameras()
    {
        // Arrange
        var firstTime = true;
        var messageId = Guid.NewGuid();
        TestCamera? camera1 = null;
        TestCamera? camera2 = null;
        var udpClientMock = new Mock<IOnvifUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface())
            .Returns(new List<IOnvifUdpClient> { udpClientMock.Object });
        udpClientMock.Setup(cl => cl.SendProbeAsync(It.IsAny<Guid>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<Guid, IPEndPoint>((mId, _) => { messageId = mId; });
        udpClientMock.Setup(udp => udp.ReceiveAsync())
            .ReturnsAsync(() =>
            {
                if (firstTime)
                {
                    firstTime = false;
                    camera1 = new TestCamera(messageId, Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.12");
                    return new UdpReceiveResult(Utils.CreateProbeResponse(camera1),
                        IPEndPoint.Parse("192.168.1.12"));
                }

                camera2 = new TestCamera(messageId, Guid.NewGuid(), "SamsungHardware", "SamsungName", "192.168.1.14");
                return new UdpReceiveResult(Utils.CreateProbeResponse(camera2), IPEndPoint.Parse("192.168.1.14"));
            });

        // Act
        var discoveredDevices = await wSDiscovery.Discover(1);

        // Assert
        var discoveryDevices = discoveredDevices.ToList();
        discoveryDevices.Should().HaveCount(2);
        var firstDevice = discoveryDevices.ElementAt(0);
        firstDevice.Address.Should().Be(camera1!.IP);
        firstDevice.Model.Should().Be(camera1.Model);
        firstDevice.Mfr.Should().Be(camera1.Manufacturer);
        var secondDevice = discoveryDevices.ElementAt(1);
        secondDevice.Address.Should().Be(camera2!.IP);
        secondDevice.Model.Should().Be(camera2.Model);
        secondDevice.Mfr.Should().Be(camera2.Manufacturer);

        udpClientFactoryMock.Verify(cf => cf.CreateClientForeachInterface(), Times.Once);
        udpClientMock.Verify(c => c.SendProbeAsync(It.IsAny<Guid>(), It.IsAny<IPEndPoint>()), Times.Once);
        udpClientMock.Verify(c => c.ReceiveAsync(), Times.AtLeast(2));
    }

    [Fact]
    public async Task Discover_TwoClients_OneCameraPerclient_ReturnsListWithTwoCameras()
    {
        // Arrange
        var messageId1 = Guid.NewGuid();
        var messageId2 = Guid.NewGuid();
        TestCamera? camera1 = null;
        TestCamera? camera2 = null;
        var udpClient1Mock = new Mock<IOnvifUdpClient>();
        var udpClient2Mock = new Mock<IOnvifUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface()).Returns(new List<IOnvifUdpClient>
            { udpClient1Mock.Object, udpClient2Mock.Object });
        udpClient1Mock.Setup(cl => cl.SendProbeAsync(It.IsAny<Guid>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<Guid, IPEndPoint>((mId, _) => { messageId1 = mId; });
        udpClient2Mock.Setup(cl => cl.SendProbeAsync(It.IsAny<Guid>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<Guid, IPEndPoint>((mId, _) => { messageId2 = mId; });
        udpClient1Mock.Setup(udp => udp.ReceiveAsync())
            .ReturnsAsync(() =>
            {
                camera1 = new TestCamera(messageId1, Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.12");
                return new UdpReceiveResult(Utils.CreateProbeResponse(camera1), IPEndPoint.Parse("192.168.1.12"));
            });
        udpClient2Mock.Setup(udp => udp.ReceiveAsync())
            .ReturnsAsync(() =>
            {
                camera2 = new TestCamera(messageId2, Guid.NewGuid(), "SamsungHardware", "SamsungName",
                    "192.168.1.14");
                return new UdpReceiveResult(Utils.CreateProbeResponse(camera2), IPEndPoint.Parse("192.168.1.14"));
            });


        // Act
        var discoveredDevices = await wSDiscovery.Discover(1);

        // Assert
        var discoveryDevices = discoveredDevices.ToList();
        discoveryDevices.Should().HaveCount(2);
        var firstDevice = discoveryDevices.ElementAt(0);
        firstDevice.Address.Should().Be(camera1!.IP);
        firstDevice.Model.Should().Be(camera1.Model);
        firstDevice.Mfr.Should().Be(camera1.Manufacturer);
        var secondDevice = discoveryDevices.ElementAt(1);
        secondDevice.Address.Should().Be(camera2!.IP);
        secondDevice.Model.Should().Be(camera2.Model);
        secondDevice.Mfr.Should().Be(camera2.Manufacturer);

        udpClientFactoryMock.Verify(cf => cf.CreateClientForeachInterface(), Times.Once);
        udpClient1Mock.Verify(c => c.SendProbeAsync(It.IsAny<Guid>(), It.IsAny<IPEndPoint>()), Times.Once);
        udpClient1Mock.Verify(c => c.ReceiveAsync(), Times.AtLeast(2));
    }

    [Fact]
    public async Task Discover_CancelBeforeFinish_ReturnsPartialList()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var udpClientMock = new Mock<IOnvifUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface())
            .Returns(new List<IOnvifUdpClient> { udpClientMock.Object });
        udpClientMock.Setup(cl => cl.SendProbeAsync(It.IsAny<Guid>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<Guid, IPEndPoint>((mId, _) => { messageId = mId; });
        var cameraNumber = 1;
        udpClientMock.Setup(udp => udp.ReceiveAsync())
            .Returns(async () =>
            {
                await Task.Delay(500).ConfigureAwait(false);
                var ip = $"192.168.1.{cameraNumber}";
                cameraNumber++;
                var camera = new TestCamera(messageId, Guid.NewGuid(), "AxisHardware", "AxisName", ip);
                return new UdpReceiveResult(Utils.CreateProbeResponse(camera), IPEndPoint.Parse(ip));
            });

        // Act
        var cancellation =
            new CancellationTokenSource(
                600); // Cancel after 600ms. Since cameras are received every 500ms, we will receive only one
        var discoveredDevices = await wSDiscovery.Discover(10, cancellation.Token);

        // Assert
        discoveredDevices.Should().HaveCount(1);
    }

    [Fact]
    public async Task Discover_Exception_In_ReceiveAsync_Do_Not_Throws_And_Returns_List()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var udpClientMock = new Mock<IOnvifUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface())
            .Returns(new List<IOnvifUdpClient> { udpClientMock.Object });
        udpClientMock.Setup(cl => cl.SendProbeAsync(It.IsAny<Guid>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<Guid, IPEndPoint>((mId, _) => { messageId = mId; });
        udpClientMock.SetupSequence(udp => udp.ReceiveAsync())
            .ReturnsAsync(() =>
            {
                var ip = "192.168.1.1";
                var camera = new TestCamera(messageId, Guid.NewGuid(), "AxisHardware", "AxisName", ip);
                return new UdpReceiveResult(Utils.CreateProbeResponse(camera), IPEndPoint.Parse(ip));
            })
            .ThrowsAsync(new Exception())
            .Returns(async () =>
            {
                var ip = "192.168.1.2";
                await Task.Delay(700);
                var camera = new TestCamera(messageId, Guid.NewGuid(), "AxisHardware", "AxisName", ip);
                return new UdpReceiveResult(Utils.CreateProbeResponse(camera), IPEndPoint.Parse(ip));
            })
            .Returns(async () =>
            {
                var ip = "192.168.1.3";
                await Task.Delay(400);
                var camera = new TestCamera(messageId, Guid.NewGuid(), "AxisHardware", "AxisName", ip);
                return new UdpReceiveResult(Utils.CreateProbeResponse(camera), IPEndPoint.Parse(ip));
            });

        // Act
        var discoveredDevices = await wSDiscovery.Discover(1);

        // Arrange
        discoveredDevices.Should().HaveCount(2);
    }

    [Fact]
    public async Task Discover_DuplicatedResultsFromMultipleInterfaces_ResultDoesNotHaveDuplications()
    {
        // Arrange
        var cameraA = new TestCamera(Guid.NewGuid(), Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.12");
        var cameraB = new TestCamera(Guid.NewGuid(), Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.12");
        var udpClientAMock = new Mock<IOnvifUdpClient>();
        var udpClientBMock = new Mock<IOnvifUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface()).Returns(new List<IOnvifUdpClient>
            { udpClientAMock.Object, udpClientBMock.Object });
        udpClientAMock.Setup(cl => cl.SendProbeAsync(It.IsAny<Guid>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<Guid, IPEndPoint>((mId, _) => { cameraA.MessageId = mId; });
        udpClientBMock.Setup(cl => cl.SendProbeAsync(It.IsAny<Guid>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<Guid, IPEndPoint>((mId, _) => { cameraB.MessageId = mId; });
        udpClientAMock.Setup(udp => udp.ReceiveAsync())
            .ReturnsAsync(() =>
            {
                return new UdpReceiveResult(Utils.CreateProbeResponse(cameraA), IPEndPoint.Parse(cameraA.IP));
            });
        udpClientBMock.Setup(udp => udp.ReceiveAsync())
            .ReturnsAsync(() =>
            {
                return new UdpReceiveResult(Utils.CreateProbeResponse(cameraB), IPEndPoint.Parse(cameraB.IP));
            });

        // Act
        var discoveredDevices = await wSDiscovery.Discover(1);

        // Arrange
        discoveredDevices.Should().HaveCount(1);
    }

    [Fact]
    public async Task Discover_ResponseWithNoHeader_DiscardsAndNotCrash()
    {
        // Arrange
        var firstTime = true;
        var messageId = Guid.NewGuid();
        TestCamera? camera = null;
        var udpClientMock = new Mock<IOnvifUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface())
            .Returns(new List<IOnvifUdpClient> { udpClientMock.Object });
        udpClientMock.Setup(cl => cl.SendProbeAsync(It.IsAny<Guid>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<Guid, IPEndPoint>((mId, _) => { messageId = mId; });
        udpClientMock.Setup(udp => udp.ReceiveAsync())
            .ReturnsAsync(() =>
            {
                if (firstTime)
                {
                    firstTime = false;
                    camera = new TestCamera(messageId, Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.12");
                    return new UdpReceiveResult(Utils.CreateProbeResponse(camera), IPEndPoint.Parse("192.168.1.12"));
                }

                var cam = new TestCamera(messageId, Guid.NewGuid(), "SamsungHardware", "SamsungName", "192.168.1.14");
                return new UdpReceiveResult(Utils.CreateProbeResponseWithNullHeader(cam),
                    IPEndPoint.Parse("192.168.1.14"));
            });

        // Act
        var discoveredDevices = await wSDiscovery.Discover(1);

        // Assert
        var discoveryDevices = discoveredDevices.ToList();
        discoveryDevices.Should().HaveCount(1);
        var firstDevice = discoveryDevices[0];
        firstDevice.Address.Should().Be(camera!.IP);
        firstDevice.Model.Should().Be(camera.Model);
        firstDevice.Mfr.Should().Be(camera.Manufacturer);

        udpClientFactoryMock.Verify(cf => cf.CreateClientForeachInterface(), Times.Once);
        udpClientMock.Verify(c => c.SendProbeAsync(It.IsAny<Guid>(), It.IsAny<IPEndPoint>()), Times.Once);
        udpClientMock.Verify(c => c.ReceiveAsync(), Times.AtLeast(2));
    }
}