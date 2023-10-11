using System.Threading.Channels;
using OnvifDiscovery.Models;

namespace OnvifDiscovery;

/// <summary>
///     Onvif Discovery, has the logic to discover onvif compliant devices on the network
/// </summary>
public interface IDiscovery
{
    /// <summary>
    ///     Discover new onvif cameras by returning an async enumerable
    /// </summary>
    /// <param name="timeout">A timeout in seconds to wait for onvif devices</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns></returns>
    IAsyncEnumerable<DiscoveryDevice> DiscoverAsync(int timeout, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Discover new onvif cameras by passing a channel writer and a timeout
    /// </summary>
    /// <param name="channelWriter">Channel Writer that this method will use to write new discovered cameras</param>
    /// <param name="timeout">A timeout in seconds to wait for onvif devices</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns></returns>
    Task DiscoverAsync(ChannelWriter<DiscoveryDevice> channelWriter, int timeout,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Discover new onvif devices on the network passing a callback
    ///     to retrieve devices as they reply
    /// </summary>
    /// <param name="timeout">A timeout in seconds to wait for onvif devices</param>
    /// <param name="onDeviceDiscovered">A method that is called each time a new device replies.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The Task to be awaited</returns>
    [Obsolete("Use one of the DiscoverAsync methods, this method will be removed in next major release")]
    Task Discover(int timeout, Action<DiscoveryDevice> onDeviceDiscovered,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Discover new onvif devices on the network
    /// </summary>
    /// <param name="timeout">A timeout in seconds to wait for onvif devices</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>a list of <see cref="DiscoveryDevice" /></returns>
    /// <remarks>
    ///     Use the <see cref="Discover(int, Action{DiscoveryDevice}, CancellationToken)" />
    ///     overload (with an action as a parameter) if you want to retrieve devices as they reply.
    /// </remarks>
    [Obsolete("Use one of the DiscoverAsync methods, this method will be removed in next major release")]
    Task<IEnumerable<DiscoveryDevice>> Discover(int timeout,
        CancellationToken cancellationToken = default);
}