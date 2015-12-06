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
		public void VerifyRawTags_With_Ok_Characters_Should_Succeed()
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
		public void VerifyRawTags_With_Empty_String_Should_Succeed()
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
		public void VerifyRawTags_With_Bad_Characters_Should_Fail()
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
