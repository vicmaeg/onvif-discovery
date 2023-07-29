using System.Net;
using System.Text.RegularExpressions;
using OnvifDiscovery.Models;

namespace OnvifDiscovery.Common;

internal static class DeviceFactory
{
    private const int RegexProcessingTimeoutInMs = 500;

    internal static DiscoveryDevice CreateDevice(ProbeMatch probeMatch, IPEndPoint remoteEndpoint) =>
        new(
            ConvertToList(probeMatch.Types),
            ConvertToList(probeMatch.XAddrs),
            ParseModelFromScopes(probeMatch.Scopes),
            ParseMfrFromScopes(probeMatch.Scopes),
            remoteEndpoint.Address.ToString(),
            ConvertToList(probeMatch.Scopes)
        );

    private static string ParseModelFromScopes(string scopes)
    {
        var model = Regex.Match(scopes, "(?<=hardware/).*?(?= )", RegexOptions.None,
            TimeSpan.FromMilliseconds(RegexProcessingTimeoutInMs)).Value;
        return Uri.UnescapeDataString(model);
    }

    private static string ParseMfrFromScopes(string scopes)
    {
        var scopesArray = scopes.Split();
        var nameQuery = scopesArray.Where(scope => scope.Contains("name/")).ToArray();
        var mfrQuery = scopesArray.Where(scope => scope.Contains("mfr/") || scope.Contains("manufacturer/"))
            .ToArray();
        if (mfrQuery.Length > 0)
        {
            var mfrMatch = Regex.Match(mfrQuery[0], Constants.PATTERN, RegexOptions.None,
                TimeSpan.FromMilliseconds(RegexProcessingTimeoutInMs));
            return Uri.UnescapeDataString(mfrMatch.Groups[6].Value);
        }

        if (nameQuery.Length <= 0)
        {
            return string.Empty;
        }

        var nameMatch = Regex.Match(nameQuery[0], Constants.PATTERN, RegexOptions.None,
            TimeSpan.FromMilliseconds(RegexProcessingTimeoutInMs));
        var mfr = Uri.UnescapeDataString(nameMatch.Groups[6].Value);
        if (mfr.Contains(' '))
        {
            mfr = mfr.Split()[0];
        }

        return mfr;
    }

    private static IEnumerable<string> ConvertToList(string spacedListString)
    {
        var strings = spacedListString.Split();
        foreach (var str in strings)
        {
            yield return str.Trim();
        }
    }
}