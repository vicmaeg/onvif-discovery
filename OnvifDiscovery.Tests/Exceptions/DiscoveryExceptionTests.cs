using OnvifDiscovery.Exceptions;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace OnvifDiscovery.Tests.Exceptions
{
	public class DiscoveryExceptionTests
	{
		[Fact]
		public void DiscriminatorNotAvailableException_default_ctor ()
		{
			// Arrange
			const string expectedMessage = "Exception of type 'OnvifDiscovery.Exceptions.DiscoveryException' was thrown.";

			// Act
			var sut = new DiscoveryException ();

			// Assert
			Assert.Null (sut.InnerException);
			Assert.Equal (expectedMessage, sut.Message);
		}

		[Fact]
		public void DiscriminatorNotAvailableException_ctor_string ()
		{
			// Arrange
			const string expectedMessage = "message";

			// Act
			var sut = new DiscoveryException (expectedMessage);

			// Assert
			Assert.Null (sut.InnerException);
			Assert.Equal (expectedMessage, sut.Message);
		}

		[Fact]
		public void DiscriminatorNotAvailableException_ctor_string_ex ()
		{
			// Arrange
			const string expectedMessage = "message";
			var innerEx = new Exception ("foo");

			// Act
			var sut = new DiscoveryException (expectedMessage, innerEx);

			// Assert
			Assert.Equal (innerEx, sut.InnerException);
			Assert.Equal (expectedMessage, sut.Message);
		}

		[Fact]
		public void DiscriminatorNotAvailableException_serialization_deserialization_test ()
		{
			// Arrange
			var innerEx = new Exception ("foo");
			var originalException = new DiscoveryException ("message", innerEx);
			var buffer = new byte[4096];
			var ms = new MemoryStream (buffer);
			var ms2 = new MemoryStream (buffer);
			var formatter = new BinaryFormatter ();

			// Act
			formatter.Serialize (ms, originalException);
			var deserializedException = (DiscoveryException)formatter.Deserialize (ms2);

			// Assert
			Assert.Equal (originalException.InnerException.Message, deserializedException.InnerException.Message);
			Assert.Equal (originalException.Message, deserializedException.Message);
		}
	}
}
