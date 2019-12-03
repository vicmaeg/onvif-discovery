using System.Xml.Serialization;

namespace OnvifDiscovery.Models
{
	/// <summary>
	/// The probe response
	/// </summary>
	[XmlRoot ("Envelope", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
	public class XmlProbeReponse
	{
		/// <summary>
		/// The Header of the probe response
		/// </summary>
		[XmlElement (Namespace = "http://www.w3.org/2003/05/soap-envelope")]
		public Header Header { get; set; }

		/// <summary>
		/// The Body of the probe response
		/// </summary>
		[XmlElement (Namespace = "http://www.w3.org/2003/05/soap-envelope")]
		public Body Body { get; set; }
	}

	/// <summary>
	/// The Header of the probe response
	/// </summary>
	public class Header
	{
		/// <summary>
		/// The message id
		/// </summary>
		[XmlElement (Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		public string MessageID { get; set; }

		/// <summary>
		/// The message id that relates to
		/// </summary>
		[XmlElement (Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		public string RelatesTo { get; set; }

		/// <summary>
		/// To
		/// </summary>
		[XmlElement (Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		public string To { get; set; }

		/// <summary>
		/// App sequence
		/// </summary>
		[XmlElement (Namespace = "http://schemas.xmlsoap.org/ws/2005/04/discovery")]
		public string AppSequence { get; set; }
	}
	/// <summary>
	/// The Body of the probe response
	/// </summary>
	public class Body
	{
		/// <summary>
		/// An array of probe matches
		/// </summary>
		[XmlArray (Namespace = "http://schemas.xmlsoap.org/ws/2005/04/discovery")]
		public ProbeMatch[] ProbeMatches { get; set; }
	}

	/// <summary>
	/// A probe match
	/// </summary>
	public class ProbeMatch
	{
		/// <summary>
		/// The endpoint reference
		/// </summary>
		[XmlElement (Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		public EndpointReference EndpointReference { get; set; }

		/// <summary>
		/// The types
		/// </summary>
		[XmlElement (Namespace = "http://schemas.xmlsoap.org/ws/2005/04/discovery")]
		public string Types { get; set; }

		/// <summary>
		/// The scopes
		/// </summary>
		[XmlElement (Namespace = "http://schemas.xmlsoap.org/ws/2005/04/discovery")]
		public string Scopes { get; set; }

		/// <summary>
		/// The XAddrs
		/// </summary>
		[XmlElement (Namespace = "http://schemas.xmlsoap.org/ws/2005/04/discovery")]
		public string XAddrs { get; set; }

		/// <summary>
		/// The metadata version
		/// </summary>
		[XmlElement (Namespace = "http://schemas.xmlsoap.org/ws/2005/04/discovery")]
		public string MetadataVersion { get; set; }
	}

	/// <summary>
	/// The endpoint reference
	/// </summary>
	public class EndpointReference
	{
		/// <summary>
		/// The address
		/// </summary>
		[XmlElement (Namespace = "http://schemas.xmlsoap.org/ws/2004/08/addressing")]
		public string Address { get; set; }
	}
}
