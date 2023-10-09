using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace OnvifDiscovery.Udp;

internal class UdpClientFactory : IUdpClientFactory
{
    public IEnumerable<IUdpClient> CreateClientForeachInterface()
    {
        var clients = new List<IUdpClient>();

        foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (!IsValidAdapter(adapter))
            {
                continue;
            }

            foreach (var ua in adapter.GetIPProperties().UnicastAddresses)
            {
                if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    IPEndPoint myLocalEndPoint = new(ua.Address, 0); // port does not matter
                    try
                    {
                        clients.Add(new UdpClientWrapper(myLocalEndPoint));
                    } catch (SocketException)
                    {
                        // Discard clients that produces a SocketException when created
                    }
                }
            }
        }

        return clients;
    }

    private static bool IsValidAdapter(NetworkInterface adapter) =>
        // Only select interfaces that are Ethernet type and support IPv4 (important to minimize waiting time)
        (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
         adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) &&
        adapter.OperationalStatus != OperationalStatus.Down &&
        adapter.Supports(NetworkInterfaceComponent.IPv4);
}