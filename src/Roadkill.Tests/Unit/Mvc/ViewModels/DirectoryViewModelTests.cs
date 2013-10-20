using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	// Slightly lame property tests
	[TestFixture]
	[Category("Unit")]
	public class DirectoryViewModelTests
	{
		[Test]
		public void Constructor_Should_Fill_Properties()
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
		public void Constructor_Should_Create_Empty_Files_And_ChildFolders()
		{
			// Arrange + Act
			DirectoryViewModel model = new DirectoryViewModel("", "");

			// Assert
			Assert.That(model.Files.Count, Is.EqualTo(0));
			Assert.That(model.ChildFolders.Count, Is.EqualTo(0));
		}
	}
}
