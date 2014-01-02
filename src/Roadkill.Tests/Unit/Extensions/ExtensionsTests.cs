using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Extensions;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class ExtensionsTests
	{
		[Test]
		public void ClearMilliseconds_Should_Set_Milliseconds_To_Zero()
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
		public void ToBase64_Should_Encode_String_With_Expected_Format()
		{
			// Arrange
			string expected = "dGhpcyBoYXMgYSBzcGFjZQ==";

			// Act
			string actual = "this has a space".ToBase64(); 

			// Assert
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test]
		public void FromBase64_Should_Dencode_String_To_Plain_Text()
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
		public void Append_Should_Add_Tabs_To_Line()
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
		public void Append_Should_Add_Tabs_Text()
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
