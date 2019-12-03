using System;

namespace OnvifDiscovery.Exceptions
{
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
	}
}
