using OnvifDiscovery.Exceptions;
using Shouldly;
using Xunit;

namespace OnvifDiscovery.Tests.Exceptions;

public class DiscoveryExceptionTests
{
    [Fact]
    public void DiscriminatorNotAvailableException_default_ctor()
    {
        // Arrange
        const string expectedMessage = "Exception of type 'OnvifDiscovery.Exceptions.DiscoveryException' was thrown.";

        // Act
        var sut = new DiscoveryException();

        // Assert
        sut.InnerException.ShouldBeNull();
        sut.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void DiscriminatorNotAvailableException_ctor_string()
    {
        // Arrange
        const string expectedMessage = "message";

        // Act
        var sut = new DiscoveryException(expectedMessage);

        // Assert
        sut.InnerException.ShouldBeNull();
        sut.Message.ShouldBe(expectedMessage);
    }

    [Fact]
    public void DiscriminatorNotAvailableException_ctor_string_ex()
    {
        // Arrange
        const string expectedMessage = "message";
        var innerEx = new Exception("foo");

        // Act
        var sut = new DiscoveryException(expectedMessage, innerEx);

        // Assert
        sut.InnerException.ShouldBe(innerEx);
        sut.Message.ShouldBe(expectedMessage);
    }
}
