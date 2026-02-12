using System.Collections.Concurrent;
using System.Net;
using System.Threading.Channels;
using OnvifDiscovery.Common;
using OnvifDiscovery.Exceptions;
using OnvifDiscovery.Models;
using OnvifDiscovery.Udp;

namespace OnvifDiscovery;

/// <summary>
///     Onvif Discovery, has the logic to discover onvif compliant devices on the network
/// </summary>
public class Discovery : IDiscovery
{
    private readonly IUdpClientFactory clientFactory;

    /// <summary>
    ///     Creates an instance of <see cref="Discovery" />
    /// </summary>
    public Discovery() : this(new UdpClientFactory())
    { }

    /// <summary>
    ///     Creates an instance of <see cref="Discovery" />
    /// </summary>
    /// <param name="clientFactory">An UDP client factory instance</param>
    public Discovery(IUdpClientFactory clientFactory)
    {
        this.clientFactory = clientFactory;
    }

    /// <summary>
    ///     Discover new onvif cameras by returning an async enumerable
    /// </summary>
    /// <param name="timeout">A timeout in seconds to wait for onvif devices</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns></returns>
    public IAsyncEnumerable<DiscoveryDevice> DiscoverAsync(int timeout, CancellationToken cancellationToken = default)
    {
        var channel = Channel.CreateUnbounded<DiscoveryDevice>();
        _ = DiscoverFromAllInterfaces(channel.Writer, timeout, cancellationToken);
        return channel.Reader.ReadAllAsync(cancellationToken);
    }

    /// <summary>
    ///     Discover new onvif cameras by passing a channel writer and a timeout
    /// </summary>
    /// <param name="channelWriter">Channel Writer that this method will use to write new discovered cameras</param>
    /// <param name="timeout">A timeout in seconds to wait for onvif devices</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns></returns>
    public Task DiscoverAsync(ChannelWriter<DiscoveryDevice> channelWriter, int timeout,
        CancellationToken cancellationToken = default) =>
        DiscoverFromAllInterfaces(channelWriter, timeout, cancellationToken);

    private async Task DiscoverFromAllInterfaces(ChannelWriter<DiscoveryDevice> channelWriter, int timeout,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        try
        {
            var clients = clientFactory.CreateClientForeachInterface().ToArray();
            if (!clients.Any())
            {
                throw new DiscoveryException("Missing valid NetworkInterfaces, UdpClients could not be created");
            }

            var discoveredDevicesAddresses = new ConcurrentDictionary<string, bool>();
            var discoveries = clients.Select(client =>
                DiscoverFromSingleInterface(channelWriter, client, discoveredDevicesAddresses, cts.Token));
            await Task.WhenAll(discoveries);
        } catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException &&
                                     timeoutCts.IsCancellationRequested)
        {
            // If cancellation is from timeout source then just catch it
        } catch (Exception ex)
        {
            channelWriter.TryComplete(ex);
            throw;
        } finally
        {
            channelWriter.TryComplete();
        }
    }

    private static async Task DiscoverFromSingleInterface(ChannelWriter<DiscoveryDevice> channelWriter,
        IUdpClient client, ConcurrentDictionary<string, bool> discoveredDevicesAddresses,
        CancellationToken cancellationToken)
    {
        try
        {
            var messageId = Guid.NewGuid();
            var probeTask = SendProbeMessages(client, messageId, cancellationToken);
            var discoverMessagesTask =
                ReceiveDiscoverMessages(channelWriter, client, discoveredDevicesAddresses, messageId,
                    cancellationToken);
            await Task.WhenAll(probeTask, discoverMessagesTask);
        } finally
        {
            client.Close();
        }
    }

    private static async Task SendProbeMessages(IUdpClient client, Guid messageId, CancellationToken cancellationToken)
    {
        var multicastEndpoint =
            new IPEndPoint(IPAddress.Parse(Constants.WS_MULTICAST_ADDRESS), Constants.WS_MULTICAST_PORT);
        var datagram = WSProbeMessageBuilder.NewProbeMessage(messageId);
        while (!cancellationToken.IsCancellationRequested)
        {
            await client.SendAsync(datagram, multicastEndpoint, cancellationToken);
            await Task.Delay(500, cancellationToken);
        }
    }

    private static async Task ReceiveDiscoverMessages(ChannelWriter<DiscoveryDevice> channelWriter,
        IUdpClient client, ConcurrentDictionary<string, bool> discoveredDevicesAddresses, Guid messageId,
        CancellationToken cancellationToken)
    {
        await foreach (var response in client.ReceiveResultsAsync(cancellationToken))
        {
            var discoveredDevice = ProbeMessageProcessor.ProcessResponse(response, messageId);
            if (discoveredDevice is null || discoveredDevice.XAddresses.All(discoveredDevicesAddresses.ContainsKey))
            {
                continue;
            }

            foreach (var xAddress in discoveredDevice.XAddresses)
            {
                discoveredDevicesAddresses.TryAdd(xAddress, true);
            }

            await channelWriter.WriteAsync(discoveredDevice, cancellationToken);
        }
    }
}