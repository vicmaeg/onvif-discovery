using System.Collections.Generic;

namespace OnvifDiscovery.Models
{
	/// <summary>
	/// A discovered onvif device
	/// </summary>
	public class DiscoveryDevice
	{
		/// <summary>
		/// The types of this onvif device. ex: NetworkVideoTransmitter
		/// </summary>
		public IEnumerable<string> Types { get; internal set; }

		/// <summary>
		/// The XAddresses of this device, the url on which the device has the webservices.
		/// Normally in the form of: http://{IP}:{Port}/onvif/device_service
		/// </summary>
		public IEnumerable<string> XAdresses { get; internal set; }

		/// <summary>
		/// The onvif device model
		/// </summary>
		public string Model { get; internal set; }

		/// <summary>
		/// The device manufacturer
		/// </summary>
		public string Mfr { get; internal set; }

		/// <summary>
		/// The device IP address
		/// </summary>
		public string Address { get; internal set; }

		/// <summary>
		/// The device scopes
		/// </summary>
		public IEnumerable<string> Scopes { get; internal set; }
	}
}
