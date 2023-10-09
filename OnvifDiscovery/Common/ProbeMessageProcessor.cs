using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using OnvifDiscovery.Models;

namespace OnvifDiscovery.Common;

internal static class ProbeMessageProcessor
{
    internal static DiscoveryDevice? ProcessResponse(UdpReceiveResult response, Guid messageId)
    {
        var strResponse = Encoding.UTF8.GetString(response.Buffer);
        var xmlResponse = DeserializeResponse(strResponse);
        if (IsFromProbeMessage(messageId, xmlResponse)
            && xmlResponse!.Body.ProbeMatches.Any()
            && !string.IsNullOrEmpty(xmlResponse.Body.ProbeMatches[0].Scopes))
        {
            return DeviceFactory.CreateDevice(xmlResponse.Body.ProbeMatches[0], response.RemoteEndPoint);
        }

        return null;
    }

    private static XmlProbeResponse? DeserializeResponse(string xml)
    {
        var serializer = new XmlSerializer(typeof(XmlProbeResponse));
        var settings = new XmlReaderSettings();
        using var textReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(textReader, settings);
        return serializer.Deserialize(xmlReader) as XmlProbeResponse;
    }

    private static bool IsFromProbeMessage(Guid messageId, XmlProbeResponse? response) =>
        response?.Header.RelatesTo.Contains(messageId.ToString()) ?? false;
}