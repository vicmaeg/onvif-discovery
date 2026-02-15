using System.Collections;
using System.Text.Json;
using Xunit;

namespace OnvifDiscovery.Tests.TestHelpers;

public class ScopesTestDataGenerator : IEnumerable<TheoryDataRow<string[], string, string>>
{
    private readonly ScopesTestData scopesTestData;

    public ScopesTestDataGenerator()
    {
        var scopesTestDataText = File.ReadAllText("Resources/ScopesTestData.json");
        scopesTestData = JsonSerializer.Deserialize<ScopesTestData>(scopesTestDataText)!;
    }

    public IEnumerator<TheoryDataRow<string[], string, string>> GetEnumerator()
    {
        foreach (var scopeTestData in scopesTestData.Devices)
        {
            yield return new TheoryDataRow<string[], string, string>(
                scopeTestData.Scopes.ToArray(),
                scopeTestData.ExpectedModel,
                scopeTestData.ExpectedMfr);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}