using System;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Extensions;

namespace Roadkill.Tests.Unit.Extensions
{
	[TestFixture]
	[Category("Unit")]
	public class ExtensionsTests
	{
		[Test]
		public void clearmilliseconds_should_set_milliseconds_to_zero()
		{
			// Arrange
			DateTime now = DateTime.UtcNow;
			DateTime expectedDate = now;
			expectedDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, 0);

			// Act
			DateTime actualDate = now.ClearMilliseconds();

			// Assert
			Assert.That(actualDate, Is.EqualTo(expectedDate));
		}

		[Test]
		public void tobase64_should_encode_string_with_expected_format()
		{
			// Arrange
			string expected = "dGhpcyBoYXMgYSBzcGFjZQ==";

			// Act
			string actual = "this has a space".ToBase64(); 

			// Assert
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test]
		public void frombase64_should_dencode_string_to_plain_text()
		{
			// Arrange
			string base64String = "dGhpcyBoYXMgYSBzcGFjZQ==";
			string expected = "this has a space";

			// Act
			string actual = base64String.FromBase64();

			// Assert
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test]
		public void append_should_add_tabs_to_line()
		{
			// Arrange
			StringBuilder builder = new StringBuilder();
			string expected = "\t\t" + "here is some text" +Environment.NewLine;

			// Act
			string actual = builder.AppendLine("here is some text", 2).ToString();

			// Assert
			Assert.That(expected, Is.EquivalentTo(actual));
		}

		[Test]
		public void append_should_add_tabs_text()
		{
			// Arrange
			StringBuilder builder = new StringBuilder();
			string expected = "\t\t\t" + "here is some text";

			// Act
			string actual = builder.Append("here is some text", 3).ToString();

			// Assert
			Assert.That(expected, Is.EquivalentTo(actual));
		}
	}
}
