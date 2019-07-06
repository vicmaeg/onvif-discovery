using OnvifSharp.Discovery;
using System;
using System.Threading.Tasks;

namespace OnvifSharp.CLI
{
	class Program
	{
		static async Task Main ()
		{
			Console.WriteLine ("Starting Discover ONVIF cameras!");
			var discovery = new WSDiscovery ();
			var devices = await discovery.Discover (1);
			foreach(var device in devices) {
				Console.WriteLine ($"Device model {device.Model} has address {device.Address}");
			}
			Console.WriteLine ("ONVIF Discovery finished");
		}
	}
}
