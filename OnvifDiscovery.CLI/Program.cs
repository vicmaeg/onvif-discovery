using OnvifDiscovery;

Console.WriteLine("Starting Discover ONVIF cameras for 10 seconds, press Ctrl+C to abort\n");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    var discovery = new Discovery();
    await foreach (var device in discovery.DiscoverAsync(10, cts.Token))
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

    Console.WriteLine("ONVIF Discovery finished");
} catch (OperationCanceledException)
{
    Console.WriteLine("ONVIF Discovery canceled");
}