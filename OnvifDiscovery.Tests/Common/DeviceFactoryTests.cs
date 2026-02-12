using System.Net;
using OnvifDiscovery.Common;
using OnvifDiscovery.Models;
using OnvifDiscovery.Tests.TestHelpers;
using Shouldly;
using Xunit;

namespace OnvifDiscovery.Tests.Common;

public class DeviceFactoryTests
{
    [Theory]
    [ClassData(typeof(ScopesTestDataGenerator))]
    public void CreateDevice_Model_And_Manufacturer_Parsed_Correctly_From_Scopes(IEnumerable<string> scopes,
        string expectedModel, string expectedMfr)
    {
        // Arrange
        var probeMatch = new ProbeMatch
        {
            Scopes = string.Join(" ", scopes),
            XAddrs = string.Empty,
            Types = string.Empty
        };
        var remoteEndpoint = IPEndPoint.Parse("127.0.0.1");

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
        var types = string.Join(" ", "dn:NetworkVideoTransmitter", "tds:Device");
        var probeMatch = new ProbeMatch
        {
            Scopes = string.Empty,
            XAddrs = string.Empty,
            Types = types
        };
        var remoteEndpoint = IPEndPoint.Parse("127.0.0.1");

        // Act
        var device = DeviceFactory.CreateDevice(probeMatch, remoteEndpoint);

        // Assert
        device.Types.Count().ShouldBe(2);
        device.Types.ShouldBe(new[] { "dn:NetworkVideoTransmitter", "tds:Device" });
    }

    [Fact]
    public void CreateDevice_XAddress_Parsed()
    {
        // Arrange
        var device1 = "https://192.168.1.1/onvif/device_service";
        var device2 = "https://192.168.1.2/onvif/device_service";
        var xaddress = string.Join(" ", device1, device2);
        var probeMatch = new ProbeMatch
        {
            Scopes = string.Empty,
            XAddrs = xaddress,
            Types = string.Empty
        };
        var remoteEndpoint = IPEndPoint.Parse("127.0.0.1");

        // Act
        var device = DeviceFactory.CreateDevice(probeMatch, remoteEndpoint);

        // Assert
        device.XAddresses.Count().ShouldBe(2);
        device.XAddresses.ShouldBe(new[] { device1, device2 });
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
        var remoteEndpoint = IPEndPoint.Parse("127.0.0.1");

        // Act
        var device = DeviceFactory.CreateDevice(probeMatch, remoteEndpoint);

        // Assert
        device.Address.ShouldBe("127.0.0.1");
    }
}
