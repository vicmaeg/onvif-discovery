using System;
using System.IO;
using System.Text;

namespace OnvifDiscovery.Tests.TestHelpers
{
	public static class Utils
	{
		public static byte[] CreateProbeResponse (TestCamera camera)
		{
			string templateResponse = File.ReadAllText ("Resources/response.txt");
			string modifiedResponse = String.Format (templateResponse, camera.MessageId, camera.Address,
				camera.Model, camera.Manufacturer, camera.IP);

			return Encoding.ASCII.GetBytes (modifiedResponse);
		}

		public static byte[] CreateProbeResponseWithNullHeader (TestCamera camera)
		{
			string templateResponse = File.ReadAllText ("Resources/response_no_header.txt");
			string modifiedResponse = String.Format (templateResponse, camera.Address,
				camera.Model, camera.Manufacturer, camera.IP);

			return Encoding.ASCII.GetBytes (modifiedResponse);
		}
	}
}
