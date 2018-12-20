using OnvifSharp.Discovery;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OnvifSharp.CLI
{
	class Program
	{
		static async Task Main (string[] args)
		{
			Console.WriteLine ("Starting Discover ONVIF cameras!");
			var discovery = new WSDiscovery ();
			var devices = await discovery.Discover (5);
			Console.WriteLine ($"Devices Discovered: {devices.Count ()}");
			int i = 1;
			foreach (var device in devices) {
				Console.Write ($"{i}) Name: {device.Name} Model: {device.Model} ");
				Console.Write ($"XAddresses: ");
				foreach (var address in device.XAdresses) {
					Console.Write ($"{address}, ");
				}
				i++;
				Console.WriteLine ("");
			}
			Console.WriteLine ("ONVIF Discovery finnished!");
		}
	}
}
