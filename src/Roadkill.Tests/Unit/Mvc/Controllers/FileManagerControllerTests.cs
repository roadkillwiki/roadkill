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
			_settings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Attachments");
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

				Directory.CreateDirectory(_settings.AttachmentsFolder);
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

		[Test]
		public void DeleteFile_Should_Return_Ok_Json_Status_And_Delete_File()
		{
			// Arrange
			string testFile1Path = CreateTestFileInAttachments("test.txt");
			string dirPath = CreateTestDirectoryInAttachments("test");
			string testFile2Path = Path.Combine(dirPath, "test.txt");
			File.WriteAllText(testFile2Path, "test");
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.DeleteFile("/test/", "test.txt") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			string json = result.Data.ToString();
			Assert.That(json, Is.EqualTo("{ status = ok, message =  }"));
			Assert.That(File.Exists(testFile2Path), Is.False);
			Assert.That(File.Exists(testFile1Path), Is.True);
		}

		[Test]
		public void DeleteFile_In_Subfolder_Should_Return_Ok_Json_Status_And_Delete_File()
		{
			// Arrange
			string fullPath = CreateTestFileInAttachments("test.txt");
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.DeleteFile("/", "test.txt") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			string json = result.Data.ToString();
			Assert.That(json, Is.EqualTo("{ status = ok, message =  }"));
			Assert.That(File.Exists(fullPath), Is.False);
		}

		[Test]
		public void DeleteFile_Missing_File_Returns_Ok()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.DeleteFile("/", "doesntexist.txt") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");

			string json = result.Data.ToString();
			Assert.That(json, Is.EqualTo("{ status = ok, message =  }"));
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void DeleteFile_Bad_Paths_Throws_Exception()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.DeleteFile("/.././", "hacker.txt") as JsonResult;

			// Assert
		}

		[Test]
		public void DeleteFolder_Should_Return_Ok_Json_Status_And_Delete_Folder()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			string fullPath = CreateTestDirectoryInAttachments("folder1");

			// Act
			JsonResult result = _filesController.DeleteFolder("folder1");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			string json = result.Data.ToString();
			Assert.That(json, Is.EqualTo("{ status = ok, message =  }"));
			Assert.That(Directory.Exists(fullPath), Is.False);
		}

		[Test]
		public void DeleteFolder_With_SubDirectory_Should_Return_Ok_Json_Status_And_Delete_Folder()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			string fullPath = CreateTestDirectoryInAttachments("folder1");
			string subPath = Path.Combine(fullPath, "subfolder1");
			string subsubPath = Path.Combine(subPath, "subsubfolder");
			Directory.CreateDirectory(subPath);
			Directory.CreateDirectory(subsubPath);

			// Act
			JsonResult result = _filesController.DeleteFolder("folder1/subfolder1/subsubfolder");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");

			string json = result.Data.ToString();
			Assert.That(json, Is.EqualTo("{ status = ok, message =  }"));
			Assert.That(Directory.Exists(subsubPath), Is.False);
			Assert.That(Directory.Exists(subPath), Is.True);
			Assert.That(Directory.Exists(fullPath), Is.True);
		}

		[Test]
		public void DeleteFolder_Missing_Folder_Should_Return_Error()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.DeleteFolder("folder1");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			string json = result.Data.ToString();
			Assert.That(json, Is.StringStarting("{ status = error, message ="));
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void DeleteFolder_Hacky_Path()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act/Assert
			JsonResult result = _filesController.DeleteFolder(@".\..\");
		}

		[Test]
		public void DeleteFolder_Empty_Folder_Argument_Returns_Error()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.DeleteFolder("");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			string json = result.Data.ToString();
			Assert.That(json, Is.StringStarting("{ status = error, message ="));
		}

		[Test]
		public void DeleteFolder_Containing_Files_Should_Return_Error()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			CreateTestDirectoryInAttachments("folder1");
			string fullPath = Path.Combine(_settings.AttachmentsDirectoryPath, "folder1", "test.txt");
			File.WriteAllText(fullPath, "test");

			// Act
			JsonResult result = _filesController.DeleteFolder("folder1");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			string json = result.Data.ToString();
			Assert.That(json, Is.StringStarting("{ status = error, message ="));
		}

		[Test]
		public void DeleteFolder_Has_Subdirectories_Should_Return_Error()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			string fullpath = CreateTestDirectoryInAttachments("folder1");
			Directory.CreateDirectory(Path.Combine(fullpath, "subfolder1"));

			// Act
			JsonResult result = _filesController.DeleteFolder("folder1");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			string json = result.Data.ToString();
			Assert.That(json, Is.StringStarting("{ status = error, message ="));
		}

		// [X] DeleteFile
		// [X] DeleteFolder
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

		private string CreateTestFileInAttachments(string filename)
		{
			string fullPath = Path.Combine(_settings.AttachmentsDirectoryPath, filename);
			File.WriteAllText(fullPath, "test");

			return fullPath;
		}

		private string CreateTestDirectoryInAttachments(string directoryName)
		{
			string fullPath = Path.Combine(_settings.AttachmentsDirectoryPath, directoryName);
			Directory.CreateDirectory(fullPath);

			return fullPath;
		}
	}
}
