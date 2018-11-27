using OnvifSharp.Discovery.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnvifSharp.Discovery
{
	public class WSDiscovery : IWSDiscovery
	{

		public Task<IEnumerable<string>> Discover (int Timeout)
		{
			SendProbe ();
		}


		public void SendProbe ()
		{
			Console.WriteLine ("Probe Sended");
		}
	}
}
