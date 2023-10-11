namespace OnvifDiscovery.Tests.TestHelpers;

public record TestCamera(Guid MessageId, Guid Address, string Model, string Manufacturer, string IP, int? Port = null)
{
    public string XAddressIp => Port is null ? $"{IP}" : $"{IP}:{Port}";
}