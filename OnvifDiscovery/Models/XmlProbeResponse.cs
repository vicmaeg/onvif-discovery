using System.Xml.Serialization;

namespace OnvifDiscovery.Models;

/// <summary>
///     The probe response
/// </summary>
[XmlRoot("Envelope", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
public class XmlProbeResponse
{
    /// <summary>
    ///     The Header of the probe response
    /// </summary>
    [XmlElement(Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public Header Header { get; init; } = new();

    /// <summary>
    ///     The Body of the probe response
    /// </summary>
    [XmlElement(Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public Body Body { get; init; } = new();
}

/// <summary>
///     The Header of the probe response
/// </summary>
public class Header
{
    /// <summary>
    ///     The message id
    /// </summary>
    [XmlElement(Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
    // ReSharper disable once InconsistentNaming
    public string MessageID { get; init; } = string.Empty;

    /// <summary>
    ///     The message id that relates to
    /// </summary>
    [XmlElement(Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
    public string RelatesTo { get; init; } = string.Empty;

    /// <summary>
    ///     To
    /// </summary>
    [XmlElement(Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
    public string To { get; init; } = string.Empty;

    /// <summary>
    ///     App sequence
    /// </summary>
    [XmlElement(Namespace = "http://schemas.xmlsoap.org/ws/2005/04/discovery")]
    public string AppSequence { get; init; } = string.Empty;
}

/// <summary>
///     The Body of the probe response
/// </summary>
public class Body
{
    /// <summary>
    ///     An array of probe matches
    /// </summary>
    [XmlArray(Namespace = "http://schemas.xmlsoap.org/ws/2005/04/discovery")]
    public ProbeMatch[] ProbeMatches { get; init; } = Array.Empty<ProbeMatch>();
}

/// <summary>
///     A probe match
/// </summary>
public class ProbeMatch
{
    /// <summary>
    ///     The endpoint reference
    /// </summary>
    [XmlElement(Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
    public EndpointReference EndpointReference { get; init; } = new();

    /// <summary>
    ///     The types
    /// </summary>
    [XmlElement(Namespace = "http://schemas.xmlsoap.org/ws/2005/04/discovery")]
    public string Types { get; init; } = string.Empty;

    /// <summary>
    ///     The scopes
    /// </summary>
    [XmlElement(Namespace = "http://schemas.xmlsoap.org/ws/2005/04/discovery")]
    public string Scopes { get; init; } = string.Empty;

    /// <summary>
    ///     The XAddrs
    /// </summary>
    [XmlElement(Namespace = "http://schemas.xmlsoap.org/ws/2005/04/discovery")]
    public string XAddrs { get; init; } = string.Empty;

    /// <summary>
    ///     The metadata version
    /// </summary>
    [XmlElement(Namespace = "http://schemas.xmlsoap.org/ws/2005/04/discovery")]
    public string MetadataVersion { get; init; } = string.Empty;
}

/// <summary>
///     The endpoint reference
/// </summary>
public class EndpointReference
{
    /// <summary>
    ///     The address
    /// </summary>
    [XmlElement(Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
    public string Address { get; init; } = string.Empty;
}