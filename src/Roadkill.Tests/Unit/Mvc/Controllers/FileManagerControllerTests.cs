using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Managers;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Attachments;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class FileManagerControllerTests
	{
		private ApplicationSettings _settings;
		private UserManagerBase _userManager;
		private IUserContext _context;
		private RepositoryMock _repository;
		private SettingsManager _settingsManager;
		private AttachmentFileHandler _attachmentFileHandler;
		private FileManagerController _filesController;

		[SetUp]
		public void Setup()
		{
			// File-specific settings
			_context = new Mock<IUserContext>().Object;
			_settings = new ApplicationSettings();
			_settings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "attachments");
			_repository = new RepositoryMock();
			_attachmentFileHandler = new AttachmentFileHandler(_settings);
			_settingsManager = new SettingsManager(_settings, _repository);
			_filesController = new FileManagerController(_settings, _userManager, _context, _settingsManager, _attachmentFileHandler);

			try
			{
				// Delete any existing attachments folder
				DirectoryInfo directoryInfo = new DirectoryInfo(_settings.AttachmentsFolder);
				if (directoryInfo.Exists)
				{
					directoryInfo.Attributes = FileAttributes.Normal;
					directoryInfo.Delete(true);
				}
			}
			catch (IOException e)
			{
				Assert.Fail("Unable to delete the attachments folder "+_settings.AttachmentsFolder+", does it have a lock?" + e.ToString());
			}

			_userManager = new Mock<UserManagerBase>(_settings, null).Object;
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

		[Test]
		public void Index()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			ActionResult result = _filesController.Index();
			
			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
		}

		// DeleteFile
		// DeleteFolder
		// FolderInfo
		// NewFolder
		// FileUpload

		[Test]
		public void Select()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			ActionResult result = _filesController.Select();

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
		}
	}
}
