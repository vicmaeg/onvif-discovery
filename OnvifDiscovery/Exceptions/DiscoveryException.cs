using System;
using System.Runtime.Serialization;

namespace OnvifDiscovery.Exceptions
{
	/// <summary>
	/// Exceptions related to discovery onvif compliant devices
	/// </summary>
	[Serializable]
	public class DiscoveryException : Exception
	{
		internal DiscoveryException ()
		{
		}

		internal DiscoveryException (string message)
			: base (message)
		{
		}

		internal DiscoveryException (string message, Exception inner)
			: base (message, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the System.Exception class with serialized data.
		/// </summary>
		/// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized 
		/// object data about the exception being thrown.</param>
		/// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information
		/// about the source or destination</param>
		protected DiscoveryException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

	}
}
