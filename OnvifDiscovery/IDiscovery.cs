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

}