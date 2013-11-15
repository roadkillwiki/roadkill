using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class ExtensionsTests
	{
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
	}
}
