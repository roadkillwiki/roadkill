using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit.Mvc.ViewModels
{
	[TestFixture]
	[Category("Unit")]
	public class LanguageViewModelTests
	{
		[Test]
		public void constructor_should_set_properties()
		{
			// Arrange
			string code = "en-GB";
			string name = "Her Majesty's British English";

			// Act
			LanguageViewModel model = new LanguageViewModel(code, name);	

			// Assert
			Assert.That(model.Code, Is.EqualTo(code));
			Assert.That(model.Name, Is.EqualTo(name));
		}

		[Test]
		public void supportedlocales_should_return_list_of_languages()
		{
			// Arrange + Act
			IEnumerable<LanguageViewModel> languages = LanguageViewModel.SupportedLocales();

			// Assert
			Assert.That(languages.Count(), Is.EqualTo(12));
		}
	}
}
