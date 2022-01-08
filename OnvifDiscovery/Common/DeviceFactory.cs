using OnvifDiscovery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace OnvifDiscovery.Common
{
	internal class DeviceFactory
	{
		internal static DiscoveryDevice CreateDevice(ProbeMatch probeMatch, IPEndPoint remoteEndpoint)
		{
			var discoveryDevice = new DiscoveryDevice ();
			string scopes = probeMatch.Scopes;
			discoveryDevice.Address = remoteEndpoint.Address.ToString ();
			discoveryDevice.Model = Regex.Match (scopes, "(?<=hardware/).*?(?= )")?.Value;
			discoveryDevice.Mfr = ParseMfrFromScopes (scopes);
			discoveryDevice.XAdresses = ConvertToList (probeMatch.XAddrs);
			discoveryDevice.Types = ConvertToList (probeMatch.Types);
			return discoveryDevice;
		}

		private static string ParseMfrFromScopes (string scopes)
		{
			var nameQuery = scopes.Split (' ').Where (scope => scope.Contains ("name/")).ToArray ();
			var mfrQuery = scopes.Split (' ').Where (scope => scope.Contains ("mfr/")).ToArray ();
			if (mfrQuery.Length > 0) {
				var match = Regex.Match (mfrQuery[0], Constants.PATTERN);
				return Uri.UnescapeDataString(match.Groups[6].Value);
			}
			if (nameQuery.Length > 0) {
				var match = Regex.Match (nameQuery[0], Constants.PATTERN);
				string temp = Uri.UnescapeDataString(match.Groups[6].Value);
				if (temp.Contains (" ")) {
					temp = temp.Split (' ')[0];
				}
				return temp;
			}
			return string.Empty;
		}

		private static IEnumerable<string> ConvertToList (string spacedListString)
		{
			var strings = spacedListString.Split (null);
			foreach (var str in strings) {
				yield return str.Trim ();
			}
		}
	}
}
