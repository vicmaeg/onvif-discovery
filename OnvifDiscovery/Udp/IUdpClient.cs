using System.Net;
using System.Net.Sockets;

namespace OnvifDiscovery.Udp;

/// <summary>
///     UDP Client interface that wraps UdpClient and easily mock <see cref="System.Net.Sockets.UdpClient" /> in tests
/// </summary>
public interface IUdpClient : IDisposable
{
    /// <summary>
    ///     Sends a UDP datagram asynchronously to a remote host.
    /// </summary>
    /// <param name="datagram">
    ///     An array of type System.Byte that specifies the UDP datagram that you intend
    ///     to send represented as an array of bytes.
    /// </param>
    /// <param name="endPoint">An System.Net.IPEndPoint that represents the host and port to which to send the datagram.</param>
    /// <returns>The Task</returns>
    Task<int> SendAsync(byte[] datagram, IPEndPoint endPoint, CancellationToken cancellationToken);

    /// <summary>
    ///     Continuously receives udp results using async enumerable feature until the token is cancelled
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel </param>
    /// <returns></returns>
    IAsyncEnumerable<UdpReceiveResult> ReceiveResultsAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Close the socket
    /// </summary>
    void Close();
}