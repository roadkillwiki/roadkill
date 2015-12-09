using System.Collections.Generic;
using NUnit.Framework;
using Roadkill.Core.Import;

namespace Roadkill.Tests.Unit.Import
{
	[TestFixture]
	[Category("Unit")]
	public class ScrewTurnImportExtensionTests
	{
		[TestCase("Before [http://roadkill.com|Roadkill Wiki] After", "Before [[http://roadkill.com|Roadkill Wiki]] After")]
		[TestCase("Before [http://roadkill.com] After", "Before [[http://roadkill.com]] After")]
		[TestCase("Before [PageName] After", "Before [[PageTitle]] After")]
		[TestCase("Before [{UP}file.zip|zip file] After", "Before [[/file.zip|zip file]] After")]
		public void ReplaceHyperlinks_Should_Return_String_With_Expected_Format(string input, string expected)
		{
			// Arrange
			Dictionary<string, string> nameTitleMapping = new Dictionary<string, string> {{"PageName", "PageTitle"}};

			// Act
			string actual = input.ReplaceHyperlinks(nameTitleMapping);

			// Assert
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test]
		public void replacebr_should_return_string_with_expected_format()
		{
			// Arrange
			const string expected = "Before \n\n After";

			// Act
			string actual = "Before {br}{BR} After".ReplaceBr();

			// Assert
			Assert.That(actual, Is.EqualTo(expected));
		}

		[TestCase("Before [image|Image caption|{UP}file.jpg] After", "Before [[File:/file.jpg|Image caption]] After")]
		[TestCase("Before [image||{UP}file.jpg] After", "Before [[File:/file.jpg]] After")]
		[TestCase("Before [imageleft||{UP}file.jpg] After", "Before [[File:/file.jpg]] After")]
		[TestCase("Before [imageright||{UP}file.jpg] After", "Before [[File:/file.jpg]] After")]
		[TestCase("Before [imageauto||{UP}file.jpg] After", "Before [[File:/file.jpg]] After")]
		public void ReplaceImageLinks_Should_Return_String_With_Expected_Format(string input, string expected)
		{
			// Arrange

			// Act
			string actual = input.ReplaceHyperlinks(new Dictionary<string, string>()).ReplaceImageLinks();

			// Assert
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test]
		public void replaceblockcode_should_return_string_with_expected_format()
		{
			// Arrange
			const string expected = "Before [[[code lang=|This is \r\n block code]]] After";

			// Act
			string actual = "Before @@This is \r\n block code@@ After".ReplaceBlockCode();

			// Assert
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test]
		public void replaceinlinecode_should_return_string_with_expected_format()
		{
			// Arrange
			const string expected = "Before <code>This is \r\n inline code</code> After";

			// Act
			string actual = "Before {{This is \r\n inline code}} After".ReplaceInlineCode();

			// Assert
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test]
		public void replaceboxmarkup_should_return_string_with_expected_format()
		{
			// Arrange
			const string expected = "Before @@infobox:This is \r\n in a box@@ After";

			// Act
			string actual = "Before (((This is \r\n in a box))) After".ReplaceBoxMarkup();

			// Assert
			Assert.That(actual, Is.EqualTo(expected));
		}
	}
}