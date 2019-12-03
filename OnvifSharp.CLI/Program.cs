using OnvifDiscovery;
using System;
using System.Threading.Tasks;

namespace OnvifDiscovery.CLI
{
	class Program
	{
		static async Task Main ()
		{
			Console.WriteLine ("Starting Discover ONVIF cameras!\n");
			var discovery = new OnvifDiscovery ();
			var devices = await discovery.Discover (1);
			foreach (var device in devices) {
				Console.WriteLine ($"Device model {device.Model} from manufacturer {device.Mfr} has address {device.Address}");
				Console.Write ($"Urls to device: ");
				foreach (var address in device.XAdresses) {
					Console.Write ($"{address}, ");
				}
				Console.WriteLine ("\n");
			}
			Console.WriteLine ("ONVIF Discovery finished");
		}
	}
}
