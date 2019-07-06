namespace OnvifSharp.Discovery.Models
{
	public class DiscoveryDevice
	{
		public string Model { get; set; }
		public string Address { get; set; }
		public string Mfr { get; set; }
		public string Mac { get; set; }
		public int Port { get; set; }
	}
}
