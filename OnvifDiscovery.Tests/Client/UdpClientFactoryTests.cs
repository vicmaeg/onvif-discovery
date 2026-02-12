using OnvifDiscovery.Udp;
using Shouldly;
using Xunit;

namespace OnvifDiscovery.Tests.Client;

public class UdpClientFactoryTests
{
    [Fact]
    public void CreateClientForeachInterface_InstancesReturned()
    {
        // Arrange
        var factory = new UdpClientFactory();

        // Act
        var clients = factory.CreateClientForeachInterface().ToArray();

        // Assert
        clients.Length.ShouldBeGreaterThanOrEqualTo(1);
        clients.ShouldAllBe(c => c is UdpClientWrapper);
    }
}
