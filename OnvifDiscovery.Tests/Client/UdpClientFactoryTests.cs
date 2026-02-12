using FluentAssertions;
using OnvifDiscovery.Udp;
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
        clients.Should().HaveCountGreaterThanOrEqualTo(1);
        clients.Should().AllBeOfType<UdpClientWrapper>();
    }
}