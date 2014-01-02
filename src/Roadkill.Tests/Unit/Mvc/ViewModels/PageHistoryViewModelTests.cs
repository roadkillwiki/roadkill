using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Plugins;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class PageHistoryViewModelTests
	{
		[Test]
		public void Constructor_Should_Fill_Properties()
		{
			// Arrange
			PageContent content = new PageContent();
			content.Page = new Page() { Id = 7, IsLocked = true };
			content.EditedOn = DateTime.Today;
			content.EditedBy = "adude";
			content.VersionNumber = 9;

			// Act
			PageHistoryViewModel model = new PageHistoryViewModel(content);

			// Assert
			Assert.That(model.Id, Is.EqualTo(content.Id));
			Assert.That(model.EditedBy, Is.EqualTo(content.EditedBy));
			Assert.That(model.EditedOn, Is.EqualTo(content.EditedOn));
			Assert.That(model.EditedOnWithOffset, Is.Not.Empty);
			Assert.That(model.IsPageAdminOnly, Is.EqualTo(content.Page.IsLocked));
			Assert.That(model.PageId, Is.EqualTo(content.Page.Id));
			Assert.That(model.VersionNumber, Is.EqualTo(content.VersionNumber));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_Should_Throw_Exception_When_PageContent_Page_Is_Null()
		{
			// Arrange
			PageContent content = new PageContent();

			// Act + Assert
			PageHistoryViewModel model = new PageHistoryViewModel(content);
		}

		[Test]
		public void EditedOnWithOffset_Should_Be_RFC_Format_And_Not_Include_Time_Zone()
		{
			// Arrange
			PageContent content = new PageContent();
			content.Page = new Page() { Id = 1 };
			content.EditedOn = DateTime.Today;

			PageHistoryViewModel model = new PageHistoryViewModel(content);
			string expectedEditedOn = DateTime.Today.ToString("s") +"Z";

			// Act
			string actualEditedOn = model.EditedOnWithOffset;

			// Assert
			Assert.That(actualEditedOn, Is.EqualTo(expectedEditedOn));
		}
	}
}
