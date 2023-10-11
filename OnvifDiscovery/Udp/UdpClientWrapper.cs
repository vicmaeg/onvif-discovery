using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using OnvifDiscovery.Common;

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

    public async Task<int> SendAsync(byte[] datagram, IPEndPoint endPoint)
        => await client.SendAsync(datagram, datagram.Length, endPoint);

    public async IAsyncEnumerable<UdpReceiveResult> ReceiveResultsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            UdpReceiveResult? response = null;
            try
            {
                response = await client.ReceiveAsync(cancellationToken);
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

    public async Task<int> SendProbeAsync(Guid messageId, IPEndPoint endPoint)
    {
        var datagram = WSProbeMessageBuilder.NewProbeMessage(messageId);
        return await client.SendAsync(datagram, datagram.Length, endPoint);
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