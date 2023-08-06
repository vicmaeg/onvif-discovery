namespace OnvifDiscovery.Tests.TestHelpers;

internal record ScopesTestData(IEnumerable<ScopeTestDataItem> Devices);

internal record ScopeTestDataItem(IEnumerable<string> Scopes, string ExpectedModel, string ExpectedMfr);