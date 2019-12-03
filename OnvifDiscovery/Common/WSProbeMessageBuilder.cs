using System;
using System.Text;

namespace OnvifDiscovery.Common
{
	internal static class WSProbeMessageBuilder
	{
		public static byte[] NewProbeMessage (Guid messageId)
		{
			if (messageId == Guid.Empty) {
				throw new ArgumentException ("messageId could not be Empty");
			}
			var probeMessagewithguid = string.Format (Constants.WS_PROBE_MESSAGE, messageId.ToString ());
			return Encoding.ASCII.GetBytes (probeMessagewithguid);
		}
	}
}
