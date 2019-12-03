using System.Collections.Generic;

namespace OnvifDiscovery.Models
{
	public class DiscoveryDevice
	{
		public IEnumerable<string> Types { get; internal set; }
		public IEnumerable<string> XAdresses { get; internal set; }
		public string Model { get; internal set; }
		public string Mfr { get; internal set; }
		public string Address { get; internal set; }
	}
}
