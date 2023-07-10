namespace OnvifDiscovery.Tests.TestHelpers;

internal class ScopesTestData
{
    public IEnumerable<ScopeTestData> Devices { get; set; }
}

public class ScopeTestData
{
    public IEnumerable<string> Scopes { get; set; }
    public string ExpectedModel { get; set; }
    public string ExpectedMfr { get; set; }
}