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
			DirectoryViewModel summary = new DirectoryViewModel(name, urlPath);			

			// Assert
			Assert.That(summary.Name, Is.EqualTo(name));
			Assert.That(summary.UrlPath, Is.EqualTo(urlPath));
		}

		[Test]
		public void Constructor_Should_Create_Empty_Files_And_ChildFolders()
		{
			// Arrange

			// Act
			DirectoryViewModel summary = new DirectoryViewModel("", "");

			// Assert
			Assert.That(summary.Files.Count, Is.EqualTo(0));
			Assert.That(summary.ChildFolders.Count, Is.EqualTo(0));
		}
	}
}
