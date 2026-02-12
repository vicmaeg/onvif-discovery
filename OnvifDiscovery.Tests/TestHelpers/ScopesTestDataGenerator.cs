using System.Collections;
using System.Text.Json;
using Xunit;

namespace OnvifDiscovery.Tests.TestHelpers;

public class ScopesTestDataGenerator : IEnumerable<ITheoryDataRow>
{
    private readonly ScopesTestData scopesTestData;

    public ScopesTestDataGenerator()
    {
        var scopesTestDataText = File.ReadAllText("Resources/ScopesTestData.json");
        scopesTestData = JsonSerializer.Deserialize<ScopesTestData>(scopesTestDataText)!;
    }

    public IEnumerator<ITheoryDataRow> GetEnumerator()
    {
        foreach (var scopeTestData in scopesTestData.Devices)
        {
            yield return new TheoryDataRow(scopeTestData.Scopes, scopeTestData.ExpectedModel, scopeTestData.ExpectedMfr);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}