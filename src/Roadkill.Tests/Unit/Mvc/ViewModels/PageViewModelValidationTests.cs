using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit.Mvc.ViewModels
{
	[TestFixture]
	[Category("Unit")]
	public class PageViewModelValidationTests
	{
		[Test]
		public void verifyrawtags_with_ok_characters_should_succeed()
		{
			// Arrange
			PageViewModel model = new PageViewModel();
			model.RawTags = "tagone, anothertag, tag-2, code, c++";

			// Act
			ValidationResult result = PageViewModel.VerifyRawTags(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void verifyrawtags_with_empty_string_should_succeed()
		{
			// Arrange
			PageViewModel model = new PageViewModel();
			model.RawTags = "";

			// Act
			ValidationResult result = PageViewModel.VerifyRawTags(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void verifyrawtags_with_bad_characters_should_fail()
		{
			// Arrange
			PageViewModel model = new PageViewModel();
			model.RawTags = "&&amp+some,tags,only,^^,!??malicious,#monkey,would,use";

			// Act
			ValidationResult result = PageViewModel.VerifyRawTags(model, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}
	}
}
