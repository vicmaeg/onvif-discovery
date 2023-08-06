using OnvifDiscovery;
using OnvifDiscovery.Models;

Console.WriteLine("Starting Discover ONVIF cameras for 10 seconds, press Ctrl+C to abort\n");

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};
var discovery = new Discovery();
await discovery.Discover(10, onNewDevice, cts.Token);
Console.WriteLine("ONVIF Discovery finished");

void onNewDevice(DiscoveryDevice device)
{
    // Multiple events could be received at the same time.
    // The lock is here to avoid messing the console.
    lock (Console.Out)
    {
        Console.WriteLine(
            $"Device model {device.Model} from manufacturer {device.Mfr} has address {device.Address}");
        Console.Write("Urls to device: ");
        foreach (var address in device.XAddresses)
        {
            Console.Write($"{address}, ");
        }

        Console.WriteLine("\n");
    }
}