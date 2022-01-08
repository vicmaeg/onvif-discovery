using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OnvifDiscovery.Tests.TestHelpers
{
	public class ScopesTestDataGenerator : IEnumerable<object[]>
	{
		private readonly ScopesTestData scopesTestData;

		public ScopesTestDataGenerator ()
		{
			var scopesTestDataText = File.ReadAllText ("Resources/ScopesTestData.json");
			scopesTestData = JsonSerializer.Deserialize<ScopesTestData> (scopesTestDataText);
		}

		public IEnumerator<object[]> GetEnumerator ()
		{
			foreach (var scopeTestData in scopesTestData.Devices) {
				yield return new object[] { scopeTestData.Scopes, scopeTestData.ExpectedModel, scopeTestData.ExpectedMfr };
			}
		}

		IEnumerator IEnumerable.GetEnumerator () => GetEnumerator ();
	}
}
