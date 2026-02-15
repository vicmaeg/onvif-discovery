using System.Net;
using OnvifDiscovery.Common;
using OnvifDiscovery.Models;
using OnvifDiscovery.Tests.TestHelpers;
using Shouldly;
using Xunit;

namespace OnvifDiscovery.Tests.Common;

public class DeviceFactoryTests
{
    private static readonly string[] deviceTypes = { "dn:NetworkVideoTransmitter", "tds:Device" };
    private static readonly string[] xAddresses =
    {
        "https://192.168.1.1/onvif/device_service",
        "https://192.168.1.2/onvif/device_service"
    };
    private const string IpLoopback = "127.0.0.1";

    [Theory]
    [ClassData(typeof(ScopesTestDataGenerator))]
    public void CreateDevice_Model_And_Manufacturer_Parsed_Correctly_From_Scopes(string[] scopes,
        string expectedModel, string expectedMfr)
    {
        // Arrange
        var probeMatch = new ProbeMatch
        {
            Scopes = string.Join(" ", scopes),
            XAddrs = string.Empty,
            Types = string.Empty
        };
        var remoteEndpoint = IPEndPoint.Parse(IpLoopback);

        // Act
        var device = DeviceFactory.CreateDevice(probeMatch, remoteEndpoint);

        //Assert
        device.Model.ShouldBe(expectedModel);
        device.Mfr.ShouldBe(expectedMfr);
        device.Scopes.ShouldBe(scopes);
    }

    [Fact]
    public void CreateDevice_Types_Parsed()
    {
        // Arrange
        var types = string.Join(" ", deviceTypes);
        var probeMatch = new ProbeMatch
        {
            Scopes = string.Empty,
            XAddrs = string.Empty,
            Types = types
        };
        var remoteEndpoint = IPEndPoint.Parse(IpLoopback);

        // Act
        var device = DeviceFactory.CreateDevice(probeMatch, remoteEndpoint);

        // Assert
        device.Types.Count().ShouldBe(2);
        device.Types.ShouldBe(deviceTypes);
    }

    [Fact]
    public void CreateDevice_XAddress_Parsed()
    {
        // Arrange
        var xaddress = string.Join(" ", xAddresses);
        var probeMatch = new ProbeMatch
        {
            Scopes = string.Empty,
            XAddrs = xaddress,
            Types = string.Empty
        };
        var remoteEndpoint = IPEndPoint.Parse(IpLoopback);

        // Act
        var device = DeviceFactory.CreateDevice(probeMatch, remoteEndpoint);

        // Assert
        device.XAddresses.Count().ShouldBe(2);
        device.XAddresses.ShouldBe(xAddresses);
    }

    [Fact]
    public void CreateDevice_Address_Parsed()
    {
        // Arrange
        var probeMatch = new ProbeMatch
        {
            Scopes = string.Empty,
            XAddrs = string.Empty,
            Types = string.Empty
        };
        var remoteEndpoint = IPEndPoint.Parse(IpLoopback);

        // Act
        var device = DeviceFactory.CreateDevice(probeMatch, remoteEndpoint);

        // Assert
        device.Address.ShouldBe(IpLoopback);
    }
}