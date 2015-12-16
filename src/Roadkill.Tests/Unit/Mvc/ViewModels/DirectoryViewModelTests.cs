using NUnit.Framework;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit.Mvc.ViewModels
{
	// Slightly lame property tests
	[TestFixture]
	[Category("Unit")]
	public class DirectoryViewModelTests
	{
		[Test]
		public void constructor_should_fill_properties()
		{
			// Arrange
			string name = "MyDirectory";
			string urlPath = "/Home/MyDirectory";

			// Act
			DirectoryViewModel model = new DirectoryViewModel(name, urlPath);			

			// Assert
			Assert.That(model.Name, Is.EqualTo(name));
			Assert.That(model.UrlPath, Is.EqualTo(urlPath));
		}

		[Test]
		public void constructor_should_create_empty_files_and_childfolders()
		{
			// Arrange + Act
			DirectoryViewModel model = new DirectoryViewModel("", "");

			// Assert
			Assert.That(model.Files.Count, Is.EqualTo(0));
			Assert.That(model.ChildFolders.Count, Is.EqualTo(0));
		}
	}
}
