namespace OnvifDiscovery.Models;

/// <summary>
///     A discovered onvif device
/// </summary>
public record DiscoveryDevice(
    IEnumerable<string> Types,
    IEnumerable<string> XAddresses,
    string Model,
    string Mfr,
    string Address,
    IEnumerable<string> Scopes
)
{
    /// <summary>
    ///     The types of this onvif device. ex: NetworkVideoTransmitter
    /// </summary>
    public IEnumerable<string> Types { get; init; } = Types;

    /// <summary>
    ///     The XAddresses of this device, the url on which the device has the webservices.
    ///     Normally in the form of: http://{IP}:{Port}/onvif/device_service
    /// </summary>
    public IEnumerable<string> XAddresses { get; init; } = XAddresses;

    /// <summary>
    ///     The onvif device model
    /// </summary>
    public string Model { get; init; } = Model;

    /// <summary>
    ///     The device manufacturer
    /// </summary>
    public string Mfr { get; init; } = Mfr;

    /// <summary>
    ///     The device IP address
    /// </summary>
    public string Address { get; init; } = Address;

    /// <summary>
    ///     The device scopes
    /// </summary>
    public IEnumerable<string> Scopes { get; init; } = Scopes;
}