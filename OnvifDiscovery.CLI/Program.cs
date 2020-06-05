using System;
using System.Threading;
using System.Threading.Tasks;
using OnvifDiscovery.Models;

namespace OnvifDiscovery.CLI
{
	class Program
	{
		static async Task Main ()
		{
			Console.WriteLine ("Starting Discover ONVIF cameras for 10 seconds, press Ctrl+C to abort\n");

			var cts = new CancellationTokenSource ();
			Console.CancelKeyPress += (s, e) => {
				e.Cancel = true;
				cts.Cancel ();
			};
			var discovery = new Discovery ();
			await discovery.Discover (10, OnNewDevice, cts.Token);
			Console.WriteLine ("ONVIF Discovery finished");
		}

		private static void OnNewDevice (DiscoveryDevice device)
		{
			// Multiple events could be received at the same time.
			// The lock is here to avoid messing the console.
			lock (Console.Out)
			{
				Console.WriteLine (
					$"Device model {device.Model} from manufacturer {device.Mfr} has address {device.Address}");
				Console.Write ($"Urls to device: ");
				foreach (var address in device.XAdresses) {
					Console.Write ($"{address}, ");
				}

				Console.WriteLine ("\n");
			}
		}
	}
}
