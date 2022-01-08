using OnvifDiscovery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace OnvifDiscovery.Common
{
	internal static class DeviceFactory
	{
		internal static DiscoveryDevice CreateDevice(ProbeMatch probeMatch, IPEndPoint remoteEndpoint)
		{
			return new DiscoveryDevice {
				Address = remoteEndpoint.Address.ToString (),
				Model = ParseModelFromScopes (probeMatch.Scopes),
				Mfr = ParseMfrFromScopes (probeMatch.Scopes),
				XAdresses = ConvertToList (probeMatch.XAddrs),
				Types = ConvertToList (probeMatch.Types),
				Scopes = ConvertToList (probeMatch.Scopes)
			};
		}

		private static string ParseModelFromScopes(string scopes)
		{
			var model = Regex.Match (scopes, "(?<=hardware/).*?(?= )")?.Value ?? string.Empty;
			return Uri.UnescapeDataString (model);
		}

		private static string ParseMfrFromScopes (string scopes)
		{
			var scopesArray = scopes.Split ();
			var nameQuery = scopesArray.Where (scope => scope.Contains ("name/")).ToArray ();
			var mfrQuery = scopesArray.Where (scope => scope.Contains ("mfr/") || scope.Contains ("manufacturer/")).ToArray ();
			if (mfrQuery.Length > 0) {
				var match = Regex.Match (mfrQuery[0], Constants.PATTERN);
				return Uri.UnescapeDataString(match.Groups[6].Value);
			}
			if (nameQuery.Length > 0) {
				var match = Regex.Match (nameQuery[0], Constants.PATTERN);
				string temp = Uri.UnescapeDataString(match.Groups[6].Value);
				if (temp.Contains (' ')) {
					temp = temp.Split ()[0];
				}
				return temp;
			}
			return string.Empty;
		}

		private static IEnumerable<string> ConvertToList (string spacedListString)
		{
			var strings = spacedListString.Split ();
			foreach (var str in strings) {
				yield return str.Trim ();
			}
		}
	}
}
