using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using FluentAssertions;
using Moq;
using OnvifDiscovery.Exceptions;
using OnvifDiscovery.Models;
using OnvifDiscovery.Tests.TestHelpers;
using OnvifDiscovery.Udp;
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
    public async Task DiscoverAsync_ClientFactoryMissingClients_ThrowsException()
    {
        // Arrange
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface()).Returns(new List<UdpClientWrapper>());

        // Act
        var act = async () => await wSDiscovery.DiscoverAsync(5).ToListAsync();

        // Assert
        await act.Should().ThrowAsync<DiscoveryException>();
    }

    [Fact]
    public async Task DiscoverAsync_OneClient_TwoCameras_ReturnsListWithTwoCameras()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        TestCamera? camera1 = null;
        TestCamera? camera2 = null;
        var udpClientMock = new Mock<IUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface())
            .Returns(new List<IUdpClient> { udpClientMock.Object });
        udpClientMock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<byte[], IPEndPoint>((datagram, _) => { messageId = ParseMessageId(datagram); });
        udpClientMock.Setup(udp => udp.ReceiveResultsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                camera1 = new TestCamera(messageId, Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.12");
                camera2 = new TestCamera(messageId, Guid.NewGuid(), "SamsungHardware", "SamsungName", "192.168.1.14");
                return new UdpReceiveResult[]
                {
                    new(Utils.CreateProbeResponse(camera1), IPEndPoint.Parse("192.168.1.12")),
                    new(Utils.CreateProbeResponse(camera2), IPEndPoint.Parse("192.168.1.14"))
                }.ToAsyncEnumerable();
            });

        // Act
        var discoveredDevices = await wSDiscovery.DiscoverAsync(1).ToListAsync();

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
        udpClientMock.Verify(c => c.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()), Times.Once);
        udpClientMock.Verify(c => c.ReceiveResultsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DiscoverAsync_TwoClients_OneCameraPerclient_ReturnsListWithTwoCameras()
    {
        // Arrange
        var messageId1 = Guid.NewGuid();
        var messageId2 = Guid.NewGuid();
        TestCamera? camera1A = null;
        TestCamera? camera1B = null;
        TestCamera? camera2A = null;
        TestCamera? camera2B = null;
        var udpClient1Mock = new Mock<IUdpClient>();
        var udpClient2Mock = new Mock<IUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface()).Returns(new List<IUdpClient>
            { udpClient1Mock.Object, udpClient2Mock.Object });
        udpClient1Mock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<byte[], IPEndPoint>((datagram, _) => { messageId1 = ParseMessageId(datagram); });
        udpClient2Mock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<byte[], IPEndPoint>((datagram, _) => { messageId2 = ParseMessageId(datagram); });
        udpClient1Mock.Setup(udp => udp.ReceiveResultsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                camera1A = new TestCamera(messageId1, Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.12");
                camera1B = new TestCamera(messageId1, Guid.NewGuid(), "SamsungHardware", "SamsungName",
                    "192.168.1.16");
                return new UdpReceiveResult[]
                {
                    new(Utils.CreateProbeResponse(camera1A), IPEndPoint.Parse("192.168.1.12")),
                    new(Utils.CreateProbeResponse(camera1B), IPEndPoint.Parse("192.168.1.16"))
                }.ToAsyncEnumerable();
            });
        udpClient2Mock.Setup(udp => udp.ReceiveResultsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                camera2A = new TestCamera(messageId2, Guid.NewGuid(), "SamsungHardware", "SamsungName",
                    "192.168.1.14");
                camera2B = new TestCamera(messageId2, Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.18");
                return new UdpReceiveResult[]
                {
                    new(Utils.CreateProbeResponse(camera2A), IPEndPoint.Parse("192.168.1.14")),
                    new(Utils.CreateProbeResponse(camera2B), IPEndPoint.Parse("192.168.1.18"))
                }.ToAsyncEnumerable();
            });

        // Act
        var discoveredDevices = await wSDiscovery.DiscoverAsync(1).ToListAsync();

        // Assert
        var discoveryDevices = discoveredDevices.ToList();
        discoveryDevices.Should().HaveCount(4);
        var firstDevice = discoveryDevices.ElementAt(0);
        firstDevice.Address.Should().Be(camera1A!.IP);
        firstDevice.Model.Should().Be(camera1A.Model);
        firstDevice.Mfr.Should().Be(camera1A.Manufacturer);
        var secondDevice = discoveryDevices.ElementAt(1);
        secondDevice.Address.Should().Be(camera1B!.IP);
        secondDevice.Model.Should().Be(camera1B.Model);
        secondDevice.Mfr.Should().Be(camera1B.Manufacturer);
        var thirdDevice = discoveryDevices.ElementAt(2);
        thirdDevice.Address.Should().Be(camera2A!.IP);
        thirdDevice.Model.Should().Be(camera2A.Model);
        thirdDevice.Mfr.Should().Be(camera2A.Manufacturer);
        var fourthDevice = discoveryDevices.ElementAt(3);
        fourthDevice.Address.Should().Be(camera2B!.IP);
        fourthDevice.Model.Should().Be(camera2B.Model);
        fourthDevice.Mfr.Should().Be(camera2B.Manufacturer);

        udpClientFactoryMock.Verify(cf => cf.CreateClientForeachInterface(), Times.Once);
        udpClient1Mock.Verify(c => c.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()), Times.Once);
        udpClient2Mock.Verify(c => c.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()), Times.Once);
        udpClient1Mock.Verify(c => c.ReceiveResultsAsync(It.IsAny<CancellationToken>()), Times.Once);
        udpClient2Mock.Verify(c => c.ReceiveResultsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DiscoverAsync_CancelBeforeFinish_ThrowsOperationCanceledException()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var udpClientMock = new Mock<IUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface())
            .Returns(new List<IUdpClient> { udpClientMock.Object });
        udpClientMock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<byte[], IPEndPoint>((datagram, _) => { messageId = ParseMessageId(datagram); });
        udpClientMock.Setup(udp => udp.ReceiveResultsAsync(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken ct) => GetTestCamerasEvery200Milliseconds(messageId, ct));

        // Act
        var cancellation =
            new CancellationTokenSource(
                600); // Cancel after 600ms. Since cameras are received every 500ms, we will receive only one

        var act = async () => await wSDiscovery.DiscoverAsync(5, cancellation.Token).ToListAsync();

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task DiscoverAsync_DuplicatedResultsFromMultipleInterfaces_ResultDoesNotHaveDuplications()
    {
        // Arrange
        var cameraA = new TestCamera(Guid.NewGuid(), Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.12", 1000);
        var cameraB = new TestCamera(Guid.NewGuid(), Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.12", 1000);
        var udpClientAMock = new Mock<IUdpClient>();
        var udpClientBMock = new Mock<IUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface()).Returns(new List<IUdpClient>
            { udpClientAMock.Object, udpClientBMock.Object });
        udpClientAMock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<byte[], IPEndPoint>((datagram, _) =>
            {
                cameraA = cameraA with { MessageId = ParseMessageId(datagram) };
            });
        udpClientBMock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<byte[], IPEndPoint>((datagram, _) =>
            {
                cameraB = cameraB with { MessageId = ParseMessageId(datagram) };
            });
        udpClientAMock.Setup(udp => udp.ReceiveResultsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                return new UdpReceiveResult[] { new(Utils.CreateProbeResponse(cameraA), IPEndPoint.Parse(cameraA.IP)) }
                    .ToAsyncEnumerable();
            });
        udpClientBMock.Setup(udp => udp.ReceiveResultsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                return new UdpReceiveResult[] { new(Utils.CreateProbeResponse(cameraB), IPEndPoint.Parse(cameraB.IP)) }
                    .ToAsyncEnumerable();
            });

        // Act
        var discoveredDevices = await wSDiscovery.DiscoverAsync(1).ToListAsync();

        // Arrange
        discoveredDevices.Should().HaveCount(1);
    }

    [Fact]
    public async Task DiscoverAsync_ResultsFromMultipleInterfacesAndSameIpButDifferentPortForXAddress_ResultDoesNotHaveDuplications()
    {
        // Arrange
        var cameraA = new TestCamera(Guid.NewGuid(), Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.12", 1000);
        var cameraB = new TestCamera(Guid.NewGuid(), Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.12", 1001);
        var udpClientAMock = new Mock<IUdpClient>();
        var udpClientBMock = new Mock<IUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface()).Returns(new List<IUdpClient>
            { udpClientAMock.Object, udpClientBMock.Object });
        udpClientAMock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<byte[], IPEndPoint>((datagram, _) =>
            {
                cameraA = cameraA with { MessageId = ParseMessageId(datagram) };
            });
        udpClientBMock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<byte[], IPEndPoint>((datagram, _) =>
            {
                cameraB = cameraB with { MessageId = ParseMessageId(datagram) };
            });
        udpClientAMock.Setup(udp => udp.ReceiveResultsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                return new UdpReceiveResult[] { new(Utils.CreateProbeResponse(cameraA), IPEndPoint.Parse(cameraA.IP)) }
                    .ToAsyncEnumerable();
            });
        udpClientBMock.Setup(udp => udp.ReceiveResultsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                return new UdpReceiveResult[] { new(Utils.CreateProbeResponse(cameraB), IPEndPoint.Parse(cameraB.IP)) }
                    .ToAsyncEnumerable();
            });

        // Act
        var discoveredDevices = await wSDiscovery.DiscoverAsync(1).ToListAsync();

        // Arrange
        discoveredDevices.Should().HaveCount(2);
    }

    [Fact]
    public async Task DiscoverAsync_ResponseWithNoHeader_DiscardsAndNotCrash()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        TestCamera? camera = null;
        var udpClientMock = new Mock<IUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface())
            .Returns(new List<IUdpClient> { udpClientMock.Object });
        udpClientMock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<byte[], IPEndPoint>((datagram, _) => { messageId = ParseMessageId(datagram); });
        udpClientMock.Setup(udp => udp.ReceiveResultsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                camera = new TestCamera(messageId, Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.12");
                var cam = new TestCamera(messageId, Guid.NewGuid(), "SamsungHardware", "SamsungName", "192.168.1.14");
                return new UdpReceiveResult[]
                {
                    new(Utils.CreateProbeResponse(camera), IPEndPoint.Parse("192.168.1.12")),
                    new(Utils.CreateProbeResponseWithNullHeader(cam), IPEndPoint.Parse("192.168.1.14"))
                }.ToAsyncEnumerable();
            });

        // Act
        var discoveredDevices = await wSDiscovery.DiscoverAsync(1).ToListAsync();

        // Assert
        var discoveryDevices = discoveredDevices.ToList();
        discoveryDevices.Should().HaveCount(1);
        var firstDevice = discoveryDevices[0];
        firstDevice.Address.Should().Be(camera!.IP);
        firstDevice.Model.Should().Be(camera.Model);
        firstDevice.Mfr.Should().Be(camera.Manufacturer);

        udpClientFactoryMock.Verify(cf => cf.CreateClientForeachInterface(), Times.Once);
        udpClientMock.Verify(c => c.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()), Times.Once);
        udpClientMock.Verify(c => c.ReceiveResultsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DiscoverAsync_ContinuouslyDiscoversNewCameras_UntilTimeout_Every200Milliseconds()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var udpClientMock = new Mock<IUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface())
            .Returns(new List<IUdpClient> { udpClientMock.Object });
        udpClientMock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<byte[], IPEndPoint>((datagram, _) => { messageId = ParseMessageId(datagram); });
        udpClientMock.Setup(udp => udp.ReceiveResultsAsync(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken ct) => GetTestCamerasEvery200Milliseconds(messageId, ct));

        // Act and Assert
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var numCamerasDiscovered = 0;
        await foreach (var device in wSDiscovery.DiscoverAsync(1))
        {
            numCamerasDiscovered++;
            stopWatch.ElapsedMilliseconds.Should()
                .BeCloseTo(200 * numCamerasDiscovered, 80 * (ulong)numCamerasDiscovered);
            var ip = $"192.168.1.{numCamerasDiscovered}";
            device.Address.Should().Be(ip);
            device.Model.Should().Be("AxisHardware");
            device.Mfr.Should().Be("AxisName");
        }

        stopWatch.Stop();
        stopWatch.ElapsedMilliseconds.Should().BeCloseTo(1000, 100);
        numCamerasDiscovered.Should().Be(4);
    }

    [Fact]
    public async Task DiscoverAsync_ThrowsException_WhenSendingProbeMessage_ThrowsException()
    {
        // Arrange
        var exceptionMessage = "Error sending upd message";
        var udpClientMock = new Mock<IUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface())
            .Returns(new List<IUdpClient> { udpClientMock.Object });
        udpClientMock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var act = async () => await wSDiscovery.DiscoverAsync(5).ToListAsync();

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage(exceptionMessage);
    }

    [Fact]
    public async Task DiscoverAsync_WithChannelWriter_TwoClients_OneCameraPerclient_ReturnsListWithTwoCameras()
    {
        // Arrange
        var channel = Channel.CreateUnbounded<DiscoveryDevice>();
        var messageId1 = Guid.NewGuid();
        var messageId2 = Guid.NewGuid();
        TestCamera? camera1A = null;
        TestCamera? camera1B = null;
        TestCamera? camera2A = null;
        TestCamera? camera2B = null;
        var udpClient1Mock = new Mock<IUdpClient>();
        var udpClient2Mock = new Mock<IUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface()).Returns(new List<IUdpClient>
            { udpClient1Mock.Object, udpClient2Mock.Object });
        udpClient1Mock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<byte[], IPEndPoint>((datagram, _) => { messageId1 = ParseMessageId(datagram); });
        udpClient2Mock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<byte[], IPEndPoint>((datagram, _) => { messageId2 = ParseMessageId(datagram); });
        udpClient1Mock.Setup(udp => udp.ReceiveResultsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                camera1A = new TestCamera(messageId1, Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.12");
                camera1B = new TestCamera(messageId1, Guid.NewGuid(), "SamsungHardware", "SamsungName",
                    "192.168.1.16");
                return new UdpReceiveResult[]
                {
                    new(Utils.CreateProbeResponse(camera1A), IPEndPoint.Parse("192.168.1.12")),
                    new(Utils.CreateProbeResponse(camera1B), IPEndPoint.Parse("192.168.1.16"))
                }.ToAsyncEnumerable();
            });
        udpClient2Mock.Setup(udp => udp.ReceiveResultsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                camera2A = new TestCamera(messageId2, Guid.NewGuid(), "SamsungHardware", "SamsungName",
                    "192.168.1.14");
                camera2B = new TestCamera(messageId2, Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.18");
                return new UdpReceiveResult[]
                {
                    new(Utils.CreateProbeResponse(camera2A), IPEndPoint.Parse("192.168.1.14")),
                    new(Utils.CreateProbeResponse(camera2B), IPEndPoint.Parse("192.168.1.18"))
                }.ToAsyncEnumerable();
            });

        // Act
        var task = wSDiscovery.DiscoverAsync(channel.Writer, 1);
        var discoveredDevices = await channel.Reader.ReadAllAsync().ToListAsync();
        await task;

        // Assert
        var discoveryDevices = discoveredDevices.ToList();
        discoveryDevices.Should().HaveCount(4);
        var firstDevice = discoveryDevices.ElementAt(0);
        firstDevice.Address.Should().Be(camera1A!.IP);
        firstDevice.Model.Should().Be(camera1A.Model);
        firstDevice.Mfr.Should().Be(camera1A.Manufacturer);
        var secondDevice = discoveryDevices.ElementAt(1);
        secondDevice.Address.Should().Be(camera1B!.IP);
        secondDevice.Model.Should().Be(camera1B.Model);
        secondDevice.Mfr.Should().Be(camera1B.Manufacturer);
        var thirdDevice = discoveryDevices.ElementAt(2);
        thirdDevice.Address.Should().Be(camera2A!.IP);
        thirdDevice.Model.Should().Be(camera2A.Model);
        thirdDevice.Mfr.Should().Be(camera2A.Manufacturer);
        var fourthDevice = discoveryDevices.ElementAt(3);
        fourthDevice.Address.Should().Be(camera2B!.IP);
        fourthDevice.Model.Should().Be(camera2B.Model);
        fourthDevice.Mfr.Should().Be(camera2B.Manufacturer);

        udpClientFactoryMock.Verify(cf => cf.CreateClientForeachInterface(), Times.Once);
        udpClient1Mock.Verify(c => c.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()), Times.Once);
        udpClient2Mock.Verify(c => c.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()), Times.Once);
        udpClient1Mock.Verify(c => c.ReceiveResultsAsync(It.IsAny<CancellationToken>()), Times.Once);
        udpClient2Mock.Verify(c => c.ReceiveResultsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Obsolete("Testing obsolete method")]
    public async Task Discover_TwoClients_OneCameraPerclient_ReturnsListWithTwoCameras()
    {
        // Arrange
        var messageId1 = Guid.NewGuid();
        var messageId2 = Guid.NewGuid();
        TestCamera? camera1A = null;
        TestCamera? camera1B = null;
        TestCamera? camera2A = null;
        TestCamera? camera2B = null;
        var udpClient1Mock = new Mock<IUdpClient>();
        var udpClient2Mock = new Mock<IUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface()).Returns(new List<IUdpClient>
            { udpClient1Mock.Object, udpClient2Mock.Object });
        udpClient1Mock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<byte[], IPEndPoint>((datagram, _) => { messageId1 = ParseMessageId(datagram); });
        udpClient2Mock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<byte[], IPEndPoint>((datagram, _) => { messageId2 = ParseMessageId(datagram); });
        udpClient1Mock.Setup(udp => udp.ReceiveResultsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                camera1A = new TestCamera(messageId1, Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.12");
                camera1B = new TestCamera(messageId1, Guid.NewGuid(), "SamsungHardware", "SamsungName",
                    "192.168.1.16");
                return new UdpReceiveResult[]
                {
                    new(Utils.CreateProbeResponse(camera1A), IPEndPoint.Parse("192.168.1.12")),
                    new(Utils.CreateProbeResponse(camera1B), IPEndPoint.Parse("192.168.1.16"))
                }.ToAsyncEnumerable();
            });
        udpClient2Mock.Setup(udp => udp.ReceiveResultsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                camera2A = new TestCamera(messageId2, Guid.NewGuid(), "SamsungHardware", "SamsungName",
                    "192.168.1.14");
                camera2B = new TestCamera(messageId2, Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.18");
                return new UdpReceiveResult[]
                {
                    new(Utils.CreateProbeResponse(camera2A), IPEndPoint.Parse("192.168.1.14")),
                    new(Utils.CreateProbeResponse(camera2B), IPEndPoint.Parse("192.168.1.18"))
                }.ToAsyncEnumerable();
            });

        // Act
        var discoveredDevices = await wSDiscovery.Discover(1);

        // Assert
        var discoveryDevices = discoveredDevices.ToList();
        discoveryDevices.Should().HaveCount(4);
        var firstDevice = discoveryDevices.ElementAt(0);
        firstDevice.Address.Should().Be(camera1A!.IP);
        firstDevice.Model.Should().Be(camera1A.Model);
        firstDevice.Mfr.Should().Be(camera1A.Manufacturer);
        var secondDevice = discoveryDevices.ElementAt(1);
        secondDevice.Address.Should().Be(camera1B!.IP);
        secondDevice.Model.Should().Be(camera1B.Model);
        secondDevice.Mfr.Should().Be(camera1B.Manufacturer);
        var thirdDevice = discoveryDevices.ElementAt(2);
        thirdDevice.Address.Should().Be(camera2A!.IP);
        thirdDevice.Model.Should().Be(camera2A.Model);
        thirdDevice.Mfr.Should().Be(camera2A.Manufacturer);
        var fourthDevice = discoveryDevices.ElementAt(3);
        fourthDevice.Address.Should().Be(camera2B!.IP);
        fourthDevice.Model.Should().Be(camera2B.Model);
        fourthDevice.Mfr.Should().Be(camera2B.Manufacturer);

        udpClientFactoryMock.Verify(cf => cf.CreateClientForeachInterface(), Times.Once);
        udpClient1Mock.Verify(c => c.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()), Times.Once);
        udpClient2Mock.Verify(c => c.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()), Times.Once);
        udpClient1Mock.Verify(c => c.ReceiveResultsAsync(It.IsAny<CancellationToken>()), Times.Once);
        udpClient2Mock.Verify(c => c.ReceiveResultsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Obsolete("Testing obsolete method")]
    public async Task DiscoverWithCallback_TwoClients_OneCameraPerclient_ReturnsListWithTwoCameras()
    {
        // Arrange
        var messageId1 = Guid.NewGuid();
        var messageId2 = Guid.NewGuid();
        TestCamera? camera1A = null;
        TestCamera? camera1B = null;
        TestCamera? camera2A = null;
        TestCamera? camera2B = null;
        var udpClient1Mock = new Mock<IUdpClient>();
        var udpClient2Mock = new Mock<IUdpClient>();
        udpClientFactoryMock.Setup(cf => cf.CreateClientForeachInterface()).Returns(new List<IUdpClient>
            { udpClient1Mock.Object, udpClient2Mock.Object });
        udpClient1Mock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<byte[], IPEndPoint>((datagram, _) => { messageId1 = ParseMessageId(datagram); });
        udpClient2Mock.Setup(cl => cl.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()))
            .ReturnsAsync(200)
            .Callback<byte[], IPEndPoint>((datagram, _) => { messageId2 = ParseMessageId(datagram); });
        udpClient1Mock.Setup(udp => udp.ReceiveResultsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                camera1A = new TestCamera(messageId1, Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.12");
                camera1B = new TestCamera(messageId1, Guid.NewGuid(), "SamsungHardware", "SamsungName",
                    "192.168.1.16");
                return new UdpReceiveResult[]
                {
                    new(Utils.CreateProbeResponse(camera1A), IPEndPoint.Parse("192.168.1.12")),
                    new(Utils.CreateProbeResponse(camera1B), IPEndPoint.Parse("192.168.1.16"))
                }.ToAsyncEnumerable();
            });
        udpClient2Mock.Setup(udp => udp.ReceiveResultsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                camera2A = new TestCamera(messageId2, Guid.NewGuid(), "SamsungHardware", "SamsungName",
                    "192.168.1.14");
                camera2B = new TestCamera(messageId2, Guid.NewGuid(), "AxisHardware", "AxisName", "192.168.1.18");
                return new UdpReceiveResult[]
                {
                    new(Utils.CreateProbeResponse(camera2A), IPEndPoint.Parse("192.168.1.14")),
                    new(Utils.CreateProbeResponse(camera2B), IPEndPoint.Parse("192.168.1.18"))
                }.ToAsyncEnumerable();
            });

        // Act
        var discoveryDevices = new List<DiscoveryDevice>();
        await wSDiscovery.Discover(1, device => discoveryDevices.Add(device));

        // Assert
        discoveryDevices.Should().HaveCount(4);
        var firstDevice = discoveryDevices.ElementAt(0);
        firstDevice.Address.Should().Be(camera1A!.IP);
        firstDevice.Model.Should().Be(camera1A.Model);
        firstDevice.Mfr.Should().Be(camera1A.Manufacturer);
        var secondDevice = discoveryDevices.ElementAt(1);
        secondDevice.Address.Should().Be(camera1B!.IP);
        secondDevice.Model.Should().Be(camera1B.Model);
        secondDevice.Mfr.Should().Be(camera1B.Manufacturer);
        var thirdDevice = discoveryDevices.ElementAt(2);
        thirdDevice.Address.Should().Be(camera2A!.IP);
        thirdDevice.Model.Should().Be(camera2A.Model);
        thirdDevice.Mfr.Should().Be(camera2A.Manufacturer);
        var fourthDevice = discoveryDevices.ElementAt(3);
        fourthDevice.Address.Should().Be(camera2B!.IP);
        fourthDevice.Model.Should().Be(camera2B.Model);
        fourthDevice.Mfr.Should().Be(camera2B.Manufacturer);

        udpClientFactoryMock.Verify(cf => cf.CreateClientForeachInterface(), Times.Once);
        udpClient1Mock.Verify(c => c.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()), Times.Once);
        udpClient2Mock.Verify(c => c.SendAsync(It.IsAny<byte[]>(), It.IsAny<IPEndPoint>()), Times.Once);
        udpClient1Mock.Verify(c => c.ReceiveResultsAsync(It.IsAny<CancellationToken>()), Times.Once);
        udpClient2Mock.Verify(c => c.ReceiveResultsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static async IAsyncEnumerable<UdpReceiveResult> GetTestCamerasEvery200Milliseconds(Guid messageId,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var cameraNumber = 1;
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(200, ct).ConfigureAwait(false);
            var ip = $"192.168.1.{cameraNumber}";
            cameraNumber++;
            var camera = new TestCamera(messageId, Guid.NewGuid(), "AxisHardware", "AxisName", ip);
            yield return new UdpReceiveResult(Utils.CreateProbeResponse(camera), IPEndPoint.Parse(ip));
        }
    }

    private static Guid ParseMessageId(byte[] datagram)
    {
        var message = Encoding.UTF8.GetString(datagram);
        var uuidIndex = message.IndexOf("uuid:", StringComparison.Ordinal);
        return Guid.Parse(message.AsSpan(uuidIndex + 5, Guid.NewGuid().ToString().Length));
    }
}