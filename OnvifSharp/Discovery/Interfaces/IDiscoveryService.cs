
using OnvifSharp.Discovery.Models;
using System.Collections.ObjectModel;

namespace OnvifSharp.Discovery.Interfaces
{
    interface IDiscoveryService
    {
        ObservableCollection<DiscoveryDevice> DiscoveredDevices { get; }
        void Start();
        void Stop();
    }
}
