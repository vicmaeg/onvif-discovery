using System.Text;
using OnvifDiscovery.Common;
using Shouldly;
using Xunit;

namespace OnvifDiscovery.Tests.Common;

public class WSProbeMessageBuilderTests
{
    [Fact]
    public void NewProbeMessage_MessageIdValid_AddsMessageId()
    {
        var messageId = Guid.NewGuid();
        var messageBytes = WSProbeMessageBuilder.NewProbeMessage(messageId);
        var message = Encoding.UTF8.GetString(messageBytes);
        message.ShouldContain(messageId.ToString());
    }

    [Fact]
    public void NewProbeMessage_MessageIdEmpty_ThrowsInvalidArgumentException()
    {
        Action act = () => WSProbeMessageBuilder.NewProbeMessage(Guid.Empty);

        var exception = Should.Throw<ArgumentException>(act);
        exception.Message.ShouldContain("messageId could not be Empty");
    }
}
