using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OnvifDiscovery.Tests
{
	public static class Utils
	{
		public static byte[] CreateProbeResponse (TestCamera camera)
		{
			string templateResponse = File.ReadAllText ("response.txt");
			string modifiedResponse = String.Format (templateResponse, camera.MessageId, camera.Address,
				camera.Model, camera.Manufacturer, camera.IP);

			return Encoding.ASCII.GetBytes (modifiedResponse);
		}
	}
}
