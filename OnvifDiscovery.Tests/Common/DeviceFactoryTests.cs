using FluentAssertions;
using OnvifDiscovery.Common;
using OnvifDiscovery.Models;
using OnvifDiscovery.Tests.TestHelpers;
using System.Net;
using Xunit;

namespace OnvifDiscovery.Tests.Common;

public class DeviceFactoryTests
{
	[Theory]
	[ClassData (typeof (ScopesTestDataGenerator))]
	public void CreateDevice_Model_And_Manufacturer_Parsed_Correctly_From_Scopes (IEnumerable<string> scopes, string expectedModel, string expectedMfr)
	{
		// Arrange
		var probeMatch = new ProbeMatch {
			Scopes = string.Join (" ", scopes),
			XAddrs = string.Empty,
			Types = string.Empty
		};
		var remoteEndpoint = IPEndPoint.Parse ("127.0.0.1");

		// Act
		var device = DeviceFactory.CreateDevice (probeMatch, remoteEndpoint);

		//Assert
		device.Model.Should ().Be (expectedModel);
		device.Mfr.Should ().Be (expectedMfr);
		device.Scopes.Should ().BeEquivalentTo (scopes);
	}

	[Fact]
	public void CreateDevice_Types_Parsed ()
	{
		// Arrange
		var types = string.Join (" ", "dn:NetworkVideoTransmitter", "tds:Device");
		var probeMatch = new ProbeMatch {
			Scopes = string.Empty,
			XAddrs = string.Empty,
			Types = types
		};
		var remoteEndpoint = IPEndPoint.Parse ("127.0.0.1");

		// Act
		var device = DeviceFactory.CreateDevice (probeMatch, remoteEndpoint);

		// Assert
		device.Types.Should ().HaveCount (2);
		device.Types.Should ().BeEquivalentTo (new string[] { "dn:NetworkVideoTransmitter", "tds:Device" });
	}

	[Fact]
	public void CreateDevice_XAddress_Parsed ()
	{
		// Arrange
		var device1 = "https://192.168.1.1/onvif/device_service";
		var device2 = "https://192.168.1.2/onvif/device_service";
		var xaddress = string.Join (" ", device1, device2);
		var probeMatch = new ProbeMatch {
			Scopes = string.Empty,
			XAddrs = xaddress,
			Types = string.Empty
		};
		var remoteEndpoint = IPEndPoint.Parse ("127.0.0.1");

		// Act
		var device = DeviceFactory.CreateDevice (probeMatch, remoteEndpoint);

		// Assert
		device.XAdresses.Should ().HaveCount (2);
		device.XAdresses.Should ().BeEquivalentTo (new string[] { device1, device2 });
	}

	[Fact]
	public void CreateDevice_Address_Parsed ()
	{
		// Arrange
		var probeMatch = new ProbeMatch {
			Scopes = string.Empty,
			XAddrs = string.Empty,
			Types = string.Empty
		};
		var remoteEndpoint = IPEndPoint.Parse ("127.0.0.1");

		// Act
		var device = DeviceFactory.CreateDevice (probeMatch, remoteEndpoint);

		// Assert
		device.Address.Should ().Be ("127.0.0.1");
	}
}
