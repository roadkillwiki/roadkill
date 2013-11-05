using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Plugins;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class LanguageViewModelTests
	{
		[Test]
		public void Constructor_Should_Set_Properties()
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
		public void SupportedLocales_Should_Return_List_Of_Languages()
		{
			// Arrange + Act
			IEnumerable<LanguageViewModel> languages = LanguageViewModel.SupportedLocales();

			// Assert
			Assert.That(languages.Count(), Is.EqualTo(11));
		}
	}
}
