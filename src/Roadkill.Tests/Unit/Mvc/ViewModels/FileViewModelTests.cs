using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	// Not much to see here - lame property tests
	[TestFixture]
	[Category("Unit")]
	public class FileViewModelTests
	{
		[Test]
		public void Constructor_Should_Fill_Properties_And_Create_Empty_Files_And_ChildFolders()
		{
			// Arrange
			string name = "random.jpg";
			string extension = "jpg";
			long size = 1241241;
			DateTime createDate = DateTime.Today;
			string folder = "/Home/MyDirectory";

			// Act
			FileViewModel model = new FileViewModel(name, extension, size, createDate, folder);		

			// Assert
			Assert.That(model.Name, Is.EqualTo(name));
			Assert.That(model.Extension, Is.EqualTo(extension));
			Assert.That(model.Size, Is.EqualTo(size));
			Assert.That(model.CreateDate, Is.EqualTo(createDate.ToShortDateString()));
			Assert.That(model.Folder, Is.EqualTo(folder));
		}
	}
}
