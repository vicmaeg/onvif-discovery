using FluentAssertions;
using OnvifDiscovery.Exceptions;
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
        sut.InnerException.Should().BeNull();
        sut.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public void DiscriminatorNotAvailableException_ctor_string()
    {
        // Arrange
        const string expectedMessage = "message";

        // Act
        var sut = new DiscoveryException(expectedMessage);

        // Assert
        sut.InnerException.Should().BeNull();
        sut.Message.Should().Be(expectedMessage);
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
        sut.InnerException.Should().Be(innerEx);
        sut.Message.Should().Be(expectedMessage);
    }
}