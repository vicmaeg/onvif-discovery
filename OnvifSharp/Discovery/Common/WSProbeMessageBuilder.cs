using System;
using System.Text;

namespace OnvifSharp.Discovery.Common
{
	public class WSProbeMessageBuilder
	{
		public static byte[] NewProbeMessage (Guid messageId)
		{
			var probeMessagewithguid = string.Format (Constants.WS_PROBE_MESSAGE, messageId.ToString ());
			return Encoding.ASCII.GetBytes (probeMessagewithguid);
		}
	}
}
