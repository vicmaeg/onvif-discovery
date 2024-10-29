using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace OnvifDiscovery.Udp;

/// <summary>
///     A simple Udp client that wraps <see cref="System.Net.Sockets.UdpClient" />
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class UdpClientWrapper : IUdpClient
{
    private readonly UdpClient client;
    private bool disposedValue;

    public UdpClientWrapper(IPEndPoint localEndpoint)
    {
        client = new UdpClient(localEndpoint)
        {
            EnableBroadcast = true
        };
    }

    public async Task<int> SendAsync(byte[] datagram, IPEndPoint endPoint, CancellationToken cancellationToken)
        => await client.SendAsync(datagram, datagram.Length, endPoint);

    public async IAsyncEnumerable<UdpReceiveResult> ReceiveResultsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            UdpReceiveResult? response = null;
            try
            {
#if NET48
                response = await client.ReceiveAsync();
#else
                response = await client.ReceiveAsync(cancellationToken);
#endif
            } catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                throw;
            } catch (Exception)
            {
                // we catch all other exceptions !
                // Something might be bad in the response of a camera when call ReceiveAsync (BeginReceive in socket) fail
            }

            if (response.HasValue)
            {
                yield return response.Value;
            }
        }
    }

    public void Close()
    {
        client.Close();
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                client.Close();
                client.Dispose();
            }

            disposedValue = true;
        }
    }
}