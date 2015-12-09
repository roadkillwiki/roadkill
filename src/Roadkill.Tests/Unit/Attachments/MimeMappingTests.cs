using NUnit.Framework;
using Roadkill.Core.Attachments;

namespace Roadkill.Tests.Unit.Attachments
{
	[TestFixture]
	[Category("Unit")]
	public class MimeMappingTests
	{
		[Test]
		public void should_return_application_mimetype_for_empty_extension()
		{
			// Arrange
			string expected = "application/octet-stream";

			// Act
			string actual = MimeTypes.GetMimeType("");

			// Assert
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test]
		public void should_return_application_mimetype_for_unknown_extension()
		{
			// Arrange
			string expected = "application/octet-stream";

			// Act
			string actual = MimeTypes.GetMimeType(".blah");

			// Assert
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test]
		public void should_ignore_case_for_extension()
		{
			// Arrange
			string expected = "image/jpeg";

			// Act
			string actual = MimeTypes.GetMimeType(".JPEG");

			// Assert
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test]
		[TestCase("image/jpeg",".jpg")]
		[TestCase("image/png", ".png")]
		[TestCase("image/gif", ".gif")]
		[TestCase("application/x-shockwave-flash", ".swf")]
		[TestCase("application/pdf", ".pdf")]
		public void Should_Return_Known_Types_Common_Extension(string expectedMimeType, string extension)
		{
			// Arrange

			// Act
			string actual = MimeTypes.GetMimeType(extension);

			// Assert
			Assert.That(actual, Is.EqualTo(expectedMimeType));
		}
	}
}
