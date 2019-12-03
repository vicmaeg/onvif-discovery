using System;
using System.Runtime.Serialization;

namespace OnvifDiscovery.Exceptions
{
	[Serializable]
	public class DiscoveryException : Exception
	{
		public DiscoveryException ()
		{
		}

		public DiscoveryException (string message)
			: base (message)
		{
		}

		public DiscoveryException (string message, Exception inner)
			: base (message, inner)
		{
		}

		protected DiscoveryException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

	}
}
