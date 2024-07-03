namespace OnvifDiscovery.Exceptions;

/// <summary>
///     Exceptions related to discovery onvif compliant devices
/// </summary>
public class DiscoveryException : Exception
{
    internal DiscoveryException()
    { }

    internal DiscoveryException(string message)
        : base(message)
    { }

    internal DiscoveryException(string message, Exception inner)
        : base(message, inner)
    { }
}
