using System;

namespace OnvifDiscovery.Tests.TestHelpers
{
	public class TestCamera
	{
		public TestCamera (Guid messageId, Guid address, string model, string manufacturer, string ip)
		{
			MessageId = messageId;
			Address = address;
			Model = model;
			Manufacturer = manufacturer;
			IP = ip;
		}

		public Guid MessageId { get; }

		public Guid Address { get; }

		public string Model { get; }

		public string Manufacturer { get; }

		public string IP { get; }
	}
}
