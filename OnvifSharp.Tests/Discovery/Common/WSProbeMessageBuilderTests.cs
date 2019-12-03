using System;
using System.IO;
using System.Text;
using System.Xml;
using FluentAssertions;
using OnvifDiscovery.Common;
using Xunit;

namespace OnvifDiscovery.Tests.Common
{
	public class WSProbeMessageBuilderTests
	{
		[Fact]
		public void NewProbeMessage_MessageIdValid_AddsMessageId ()
		{
			Guid messageId = Guid.NewGuid ();
			var messageBytes = WSProbeMessageBuilder.NewProbeMessage (messageId);
			var message = Encoding.UTF8.GetString (messageBytes);
			message.Should ().Contain (messageId.ToString ());
		}

		[Fact]
		public void NewProbeMessage_MessageIdEmpty_ThrowsInvalidArgumentException ()
		{
			Action act = () => WSProbeMessageBuilder.NewProbeMessage (Guid.Empty);

			act.Should ().Throw<ArgumentException> ().WithMessage ("messageId could not be Empty");
		}
	}
}
