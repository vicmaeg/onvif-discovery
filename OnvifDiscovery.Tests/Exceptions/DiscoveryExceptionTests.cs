using System.Runtime.Serialization.Formatters.Binary;
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

    [Fact]
    public void DiscriminatorNotAvailableException_serialization_deserialization_test()
    {
        // Arrange
        var innerEx = new Exception("foo");
        var originalException = new DiscoveryException("message", innerEx);
        var buffer = new byte[4096];
        var ms = new MemoryStream(buffer);
        var ms2 = new MemoryStream(buffer);
        var formatter = new BinaryFormatter();

        // Act
        formatter.Serialize(ms, originalException);
        var deserializedException = (DiscoveryException)formatter.Deserialize(ms2);

        // Assert
        deserializedException.Should().NotBeNull();
        deserializedException.InnerException.Should().NotBeNull();
        deserializedException.Message.Should().Be(originalException.Message);
    }
}