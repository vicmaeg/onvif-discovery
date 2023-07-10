using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using OnvifDiscovery.Interfaces;

namespace OnvifDiscovery.Client;

internal class UdpClientFactory : IUdpClientFactory
{
    public IOnvifUdpClient CreateClient(IPEndPoint endpoint) => new OnvifUdpClient(endpoint);

    public IEnumerable<IOnvifUdpClient> CreateClientForeachInterface()
    {
        var clients = new List<IOnvifUdpClient>();

        var nics = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var adapter in nics)
        {
            if (!IsValidAdapter(adapter))
            {
                continue;
            }

            var adapterProperties = adapter.GetIPProperties();
            foreach (var address in adapterProperties.UnicastAddresses.Select(ua => ua.Address))
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    var myLocalEndPoint = new IPEndPoint(address, 0); // port does not matter
                    try
                    {
                        var client = CreateClient(myLocalEndPoint);
                        clients.Add(client);
                    } catch (SocketException)
                    {
                        // Discard clients that produces a SocketException when created
                    }
                }
            }
        }

        return clients;
    }

    private bool IsValidAdapter(NetworkInterface adapter)
    {
        // Only select interfaces that are Ethernet type and support IPv4 (important to minimize waiting time)
        if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
            !adapter.NetworkInterfaceType.ToString().ToLower().StartsWith("wireless"))
        {
            return false;
        }

        if (adapter.OperationalStatus == OperationalStatus.Down)
        {
            return false;
        }

        if (!adapter.Supports(NetworkInterfaceComponent.IPv4))
        {
            return false;
        }

        return true;
    }
}