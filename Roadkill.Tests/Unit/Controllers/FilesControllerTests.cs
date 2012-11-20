using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Controllers;
using Roadkill.Core.Domain;
using Roadkill.Tests.Integration;
using StructureMap;

namespace Roadkill.Tests.Unit.Controllers
{
	[TestFixture]
	[Category("Unit")]
	public class FilesControllerTests
	{
		private IConfigurationContainer _config;
		private UserManager _userManager;

		[TestFixtureSetUp]
		public void Init()
		{
			_config = new RoadkillSettings();
			_config.ApplicationSettings = new ApplicationSettings();
			_config.SitePreferences = new SitePreferences() { AllowedFileTypes = "png, jpg" };

			_userManager = new Mock<UserManager>(_config, null, null).Object;
		}

		[Test]
		public void UploadFile_Should_Save_To_Filesystem_With_No_Errors_And_Redirect()
		{
			// Arrange
			_config.ApplicationSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "attachments");

			FilesController filesController = new FilesController(_config, _userManager);
			MvcMockContainer mocksContainer =  filesController.SetFakeControllerContext();
			SetupMockPostedFile(mocksContainer);

			SettingsSummary validConfigSettings = new SettingsSummary()
			{
				AllowedExtensions = "jpg, png, gif",
				AllowUserSignup = true,
				EnableRecaptcha = true,
				MarkupType = "markuptype",
				RecaptchaPrivateKey = "privatekey",
				RecaptchaPublicKey = "publickey",
				SiteName = "sitename",
				SiteUrl = "siteurl",
				Theme = "theme",
			};

			// Act

			// All incoming paths are based 64 encoded. 
			// This path is relative to the attachments directory root.
			string pathAsBase64 = @"\".ToBase64();
			ActionResult result = filesController.UploadFile(pathAsBase64) as ActionResult;
			
			// Assert
			string expectedFilePath = Path.Combine(_config.ApplicationSettings.AttachmentsFolder, "test.png");
			Assert.That(File.Exists(expectedFilePath), "Filepath: " + expectedFilePath);
			Assert.That(filesController.TempData["Error"], Is.Null, "Errors");
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>(), "RedirectToRouteResult");
		}

		private void SetupMockPostedFile(MvcMockContainer container)
		{
			Mock<HttpFileCollectionBase> postedfilesKeyCollection = new Mock<HttpFileCollectionBase>();
			List<string> fakeFileKeys = new List<string>() { "uploadFile" };
			Mock<HttpPostedFileBase> postedfile = new Mock<HttpPostedFileBase>();

			container.Request.Setup(req => req.Files).Returns(postedfilesKeyCollection.Object);
			postedfilesKeyCollection.Setup(keys => keys.GetEnumerator()).Returns(fakeFileKeys.GetEnumerator());
			postedfilesKeyCollection.Setup(keys => keys["uploadFile"]).Returns(postedfile.Object);

			postedfile.Setup(f => f.ContentLength).Returns(8192);
			postedfile.Setup(f => f.FileName).Returns("test.png");
			postedfile.Setup(f => f.SaveAs(It.IsAny<string>())).Callback<string>(filename => File.WriteAllText(filename, "test contents"));
		}
	}
}
