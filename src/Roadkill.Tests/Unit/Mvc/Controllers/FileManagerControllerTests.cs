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
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Attachments;
using System.Collections.Specialized;
using System.Reflection;
using Roadkill.Core.Mvc.Attributes;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class FileManagerControllerTests
	{
		private ApplicationSettings _settings;
		private UserServiceBase _userService;
		private IUserContext _context;
		private RepositoryMock _repository;
		private SettingsService _settingsService;
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
			_settingsService = new SettingsService(_settings, _repository);
			_filesController = new FileManagerController(_settings, _userService, _context, _settingsService, _attachmentFileHandler);

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
				Assert.Fail("Unable to delete the attachments folder "+_settings.AttachmentsFolder+", does it have a lock/explorer window open?" + e.ToString());
			}

			_userService = new Mock<UserServiceBase>(_settings, null).Object;
		}

		[Test]
		public void Index_Should_Return_View()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			ActionResult result = _filesController.Index();
			
			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
		}

		[Test]
		public void Select_Should_Return_View()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			ActionResult result = _filesController.Select();

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

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.message, Is.EqualTo(""));

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

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.message, Is.EqualTo(""));

			Assert.That(File.Exists(fullPath), Is.False);
		}

		[Test]
		public void DeleteFile_Missing_File_Should_Return_Json_Status_Ok()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.DeleteFile("/", "doesntexist.txt") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.message, Is.EqualTo(""));
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void DeleteFile_With_Bad_Paths_Throws_Exception()
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

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.message, Is.EqualTo(""));

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

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.message, Is.EqualTo(""));

			Assert.That(Directory.Exists(subsubPath), Is.False);
			Assert.That(Directory.Exists(subPath), Is.True);
			Assert.That(Directory.Exists(fullPath), Is.True);
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

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.Not.Null.Or.Empty);
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

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.Not.Null.Or.Empty);
		}

		[Test]
		public void DeleteFolder_With_Folder_That_Has_Subdirectories_Should_Return_Error()
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

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.Not.Null.Or.Empty);
		}

		[Test]
		public void DeleteFolder_With_Missing_Directory_Should_Return_Error()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.DeleteFolder("folder1/folder2");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.Not.Null.Or.Empty);
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void DeleteFolder_With_Bad_Paths_Should_Throw_Exception()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.DeleteFolder("/../../folder1") as JsonResult;

			// Assert
		}

		[Test]
		public void FolderInfo_With_Empty_Path_Should_Contain_Model_With_Root()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			CreateTestDirectoryInAttachments("blah");
			CreateTestFileInAttachments("blah.png");

			// Act
			JsonResult result = _filesController.FolderInfo("") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult was not returned");

			DirectoryViewModel model = result.Data as DirectoryViewModel;
			Assert.That(model, Is.Not.Null, "DirectoryViewModel is null");
			Assert.That(model.ChildFolders.Count, Is.EqualTo(1));
			Assert.That(model.Files.Count, Is.EqualTo(1));
			Assert.That(model.Name, Is.EqualTo(""));
			Assert.That(model.UrlPath, Is.EqualTo(""));
		}

		[Test]
		public void FolderInfo_With_Root_Should_Contain_Model()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			CreateTestDirectoryInAttachments("blah");
			CreateTestFileInAttachments("blah.png");

			// Act
			JsonResult result = _filesController.FolderInfo("") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult was not returned");

			DirectoryViewModel model = result.Data as DirectoryViewModel;
			Assert.That(model, Is.Not.Null, "DirectoryViewModel is null");
			Assert.That(model.ChildFolders.Count, Is.EqualTo(1));
			Assert.That(model.Files.Count, Is.EqualTo(1));
			Assert.That(model.Name, Is.EqualTo(""));
			Assert.That(model.UrlPath, Is.EqualTo(""));
		}

		[Test]
		public void FolderInfo_With_SubFolder_Should_Contain_Model()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			CreateTestDirectoryInAttachments(@"blah\blah2\blah3");
			CreateTestDirectoryInAttachments(@"blah\blah2\blah3\blah4");
			CreateTestFileInAttachments(@"blah\blah2\blah3\something.png");
			CreateTestFileInAttachments(@"blah\blah2\blah3\something2.png");
			CreateTestFileInAttachments(@"blah\blah2\blah3\something3.png");

			// Act
			JsonResult result = _filesController.FolderInfo("/blah/blah2/blah3") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult was not returned");

			DirectoryViewModel model = result.Data as DirectoryViewModel;
			Assert.That(model, Is.Not.Null, "DirectoryViewModel is null");
			Assert.That(model.ChildFolders.Count, Is.EqualTo(1));
			Assert.That(model.Files.Count, Is.EqualTo(3));
			Assert.That(model.Name, Is.EqualTo("blah3"));
			Assert.That(model.UrlPath, Is.EqualTo("/blah/blah2/blah3"));
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void FolderInfo_With_Missing_Directory_Should_Throw_Exception()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.FolderInfo("/missingfolder") as JsonResult;

			// Assert
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void FolderInfo_With_Bad_Folder_Path_Should_Throw_Exception()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.FolderInfo(".././") as JsonResult;

			// Assert
		}

		[Test]
		public void NewFolder_In_Root_Folder_Should_Create_Folder_And_Return_Ok_Json_Status()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			string folderName = "newfolder with spaces in it";
			string fullPath = Path.Combine(_settings.AttachmentsDirectoryPath, folderName);

			// Act
			JsonResult result = _filesController.NewFolder("/", folderName);

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.FolderName, Is.EqualTo(folderName));
			Assert.That(Directory.Exists(fullPath), Is.True);
		}

		[Test]
		public void NewFolder_With_SubDirectory_Should_Create_Folder_And_Return_Ok_Json_Status()
		{
			// Arrange
			_filesController.SetFakeControllerContext();
			string fullPath = CreateTestDirectoryInAttachments("folder1");
			string subPath = Path.Combine(fullPath, "subfolder1");
			string subsubPath = Path.Combine(subPath, "subsubfolder");
			Directory.CreateDirectory(subPath);

			// Act
			JsonResult result = _filesController.NewFolder("/folder1/subfolder1/", "subsubfolder");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.FolderName, Is.EqualTo("subsubfolder"));
			Assert.That(Directory.Exists(subsubPath), Is.True);
		}

		[Test]
		public void NewFolder_With_Empty_FolderName_Argument_Should_Return_Error()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.NewFolder("/","");

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.Not.Null.Or.Empty);
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void NewFolder_With_Missing_Directory_Should_Return_Error()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.NewFolder("folder1/folder2", "newfolder");

			// Assert
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void NewFolder_With_Bad_Folder_Path_Should_Throw_Exception()
		{
			// Arrange
			_filesController.SetFakeControllerContext();

			// Act
			JsonResult result = _filesController.NewFolder("/../../folder1","../cheeky/path") as JsonResult;

			// Assert
		}

		[Test]
		public void FileUpload_With_Single_File_To_Root_Should_Save_File_To_Disk_And_Return_Ok_Json_Status()
		{
			// Arrange
			MvcMockContainer container = _filesController.SetFakeControllerContext();
			SetupMockPostedFiles(container, "/", "file1.png");
			string file1FullPath = Path.Combine(_settings.AttachmentsDirectoryPath, "file1.png");

			// Act
			JsonResult result = _filesController.Upload();

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.filename, Is.EqualTo("file1.png"));

			Assert.That(File.Exists(file1FullPath), Is.True);
		}

		[Test]
		public void FileUpload_Should_OverWrite_Existing_File_WhenOverWriteFiles_Setting_Is_True()
		{
			// Arrange
			_repository.SiteSettings.OverwriteExistingFiles = true;
			CreateTestFileInAttachments("file1.png", "the original file");

			MvcMockContainer container = _filesController.SetFakeControllerContext();
			SetupMockPostedFiles(container, "/", "file1.png");
			string file1FullPath = Path.Combine(_settings.AttachmentsDirectoryPath, "file1.png");

			// Act
			JsonResult result = _filesController.Upload();

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.filename, Is.EqualTo("file1.png"));

			string fileContent = File.ReadAllText(file1FullPath);
			Assert.That(fileContent, Is.EqualTo("test contents"));
		}

		[Test]
		public void FileUpload_Should_Be_Case_Insensitive()
		{
			// Arrange
			MvcMockContainer container = _filesController.SetFakeControllerContext();
			SetupMockPostedFiles(container, "/", "file1.PNG");
			string file1FullPath = Path.Combine(_settings.AttachmentsDirectoryPath, "file1.PNG");

			// Act
			JsonResult result = _filesController.Upload();

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.filename, Is.EqualTo("file1.PNG"));

			Assert.That(File.Exists(file1FullPath), Is.True);
		}


		[Test]
		public void FileUpload_With_Multiple_Files_To_Root_Should_Save_Files_To_Disk_And_Return_Ok_Json_Status()
		{
			// Arrange
			MvcMockContainer container = _filesController.SetFakeControllerContext();
			SetupMockPostedFiles(container, "/", "file1.png", "file2.png");
			string file1FullPath = Path.Combine(_settings.AttachmentsDirectoryPath, "file1.png");
			string file2FullPath = Path.Combine(_settings.AttachmentsDirectoryPath, "file2.png");

			// Act
			JsonResult result = _filesController.Upload();

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.filename, Is.EqualTo("file2.png"));

			Assert.That(File.Exists(file1FullPath), Is.True);
			Assert.That(File.Exists(file2FullPath), Is.True);
		}

		[Test]
		public void FileUpload_With_Multiple_Files_To_SubFolder_Should_Save_Files_To_Disk_And_Return_Ok_Json_Status()
		{
			// Arrange
			MvcMockContainer container = _filesController.SetFakeControllerContext();
			SetupMockPostedFiles(container, "/folder1/folder2", "file1.png", "file2.png");

			string fullPath = CreateTestDirectoryInAttachments("folder1");
			string subPath = Path.Combine(fullPath, "folder2");
			Directory.CreateDirectory(subPath);

			string file1FullPath = Path.Combine(subPath, "file1.png");
			string file2FullPath = Path.Combine(subPath, "file2.png");

			// Act
			JsonResult result = _filesController.Upload();

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.filename, Is.EqualTo("file2.png"));

			Assert.That(File.Exists(file1FullPath), Is.True);
			Assert.That(File.Exists(file2FullPath), Is.True);
		}

		[Test]
		public void FileUpload_With_No_Files_To_Root_Should_Return_Ok_Json_Status()
		{
			// Arrange
			MvcMockContainer container = _filesController.SetFakeControllerContext();
			SetupMockPostedFiles(container, "/");

			// Act
			JsonResult result = _filesController.Upload();

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.filename, Is.EqualTo(""));
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void FileUpload_With_Bad_Folder_Path_Should_Throw_Exception()
		{
			// Arrange
			MvcMockContainer container = _filesController.SetFakeControllerContext();
			SetupMockPostedFiles(container, "/../../bad/path");

			// Act
			JsonResult result = _filesController.Upload();

			// Assert
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void FileUpload_With_Missing_Folder_Should_Throw_Exception()
		{
			// Arrange
			MvcMockContainer container = _filesController.SetFakeControllerContext();
			SetupMockPostedFiles(container, "/missingfolder");

			// Act
			JsonResult result = _filesController.Upload();

			// Assert
		}

		[Test]
		public void FileUpload_Should_Return_Error_Json_Status_When_File_Has_Bad_Extension()
		{
			// Arrange
			MvcMockContainer container = _filesController.SetFakeControllerContext();
			SetupMockPostedFiles(container, "/", "file1.exe");
			string file1FullPath = Path.Combine(_settings.AttachmentsDirectoryPath, "file1.exe");

			// Act
			JsonResult result = _filesController.Upload();

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.Not.Null.Or.Empty);

			Assert.That(File.Exists(file1FullPath), Is.False);
		}

		[Test]
		public void FileUpload_Should_Return_Error_Json_Status_When_File_Exists_And_OverWriteFiles_Setting_Is_False()
		{
			// Arrange
			_repository.SiteSettings.OverwriteExistingFiles = false;
			CreateTestFileInAttachments("file1.png", "the original file");

			MvcMockContainer container = _filesController.SetFakeControllerContext();
			SetupMockPostedFiles(container, "/", "file1.png");
			string file1FullPath = Path.Combine(_settings.AttachmentsDirectoryPath, "file1.png");

			// Act
			JsonResult result = _filesController.Upload();

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.Not.Null.Or.Empty);

			string fileContent = File.ReadAllText(file1FullPath);
			Assert.That(fileContent, Is.EqualTo("the original file"));
		}


		[Test]
		public void FileUpload_Should_Return_Error_Json_Status_When_File_Exists_And_OverWriteFiles_Setting_Is_False_For_Multiple_Files()
		{
			// Arrange
			_repository.SiteSettings.OverwriteExistingFiles = false;
			CreateTestFileInAttachments("file3.png", "the original file");

			MvcMockContainer container = _filesController.SetFakeControllerContext();
			SetupMockPostedFiles(container, "/", "file1.png", "file2.png", "file3.png", "file4.png", "file5.png");

			string file1FullPath = Path.Combine(_settings.AttachmentsDirectoryPath, "file1.png");
			string file2FullPath = Path.Combine(_settings.AttachmentsDirectoryPath, "file2.png");
			string file3FullPath = Path.Combine(_settings.AttachmentsDirectoryPath, "file3.png");
			string file4FullPath = Path.Combine(_settings.AttachmentsDirectoryPath, "file4.png");
			string file5FullPath = Path.Combine(_settings.AttachmentsDirectoryPath, "file5.png");

			// Act
			JsonResult result = _filesController.Upload();

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.Not.Null.Or.Empty);

			string fileContent = File.ReadAllText(file3FullPath);
			Assert.That(fileContent, Is.EqualTo("the original file"));

			Assert.That(File.Exists(file1FullPath), Is.True);
			Assert.That(File.Exists(file2FullPath), Is.True);
			Assert.That(File.Exists(file4FullPath), Is.False);
			Assert.That(File.Exists(file5FullPath), Is.False);
		}

		[Test]
		[TestCase("Upload")]
		[TestCase("DeleteFile")]
		[TestCase("DeleteFolder")]
		[TestCase("FolderInfo")]
		[TestCase("NewFolder")]
		public void Actions_Should_Be_HttpPost(string actionName)
		{
			// Arrange
			MvcMockContainer container = _filesController.SetFakeControllerContext();
			MethodInfo actionMethod = typeof(FileManagerController).GetMethod(actionName);

			// Act
			HttpPostAttribute postAttribute = actionMethod.GetCustomAttributes(typeof(HttpPostAttribute), false)
							 .Cast<HttpPostAttribute>()
							 .SingleOrDefault();

			HttpGetAttribute getAttribute = actionMethod.GetCustomAttributes(typeof(HttpGetAttribute), false)
							 .Cast<HttpGetAttribute>()
							 .SingleOrDefault();

			// Assert
			Assert.That(postAttribute, Is.Not.Null, "Couldn't find the HttpPostAttribute");
			Assert.That(getAttribute, Is.Null, "The HttpGetAttribute was not null (and should be)");
		}

		[Test]
		public void Controller_Should_Have_EditorRequired_Attribute()
		{
			// Arrange


			// Act
			EditorRequiredAttribute editorAttribute = Attribute.GetCustomAttribute(typeof(FileManagerController), typeof(EditorRequiredAttribute))
												as EditorRequiredAttribute;

			// Assert
			Assert.That(editorAttribute, Is.Not.Null, "Couldn't find the EditorRequiredAttribute");
		}

		[Test]
		[TestCase("Index")]
		[TestCase("Select")]
		[TestCase("FolderInfo")]
		[TestCase("NewFolder")]
		[TestCase("Upload")]
		public void Actions_Should_Have_EditorRequired_Attribute(string actionName)
		{
			// Arrange
			MethodInfo actionMethod = typeof(FileManagerController).GetMethod(actionName);

			// Act
			EditorRequiredAttribute editorAttribute = actionMethod.GetCustomAttributes(typeof(EditorRequiredAttribute), false)
							 .Cast<EditorRequiredAttribute>()
							 .SingleOrDefault();

			// Assert
			Assert.That(editorAttribute, Is.Not.Null, "Couldn't find the EditorRequiredAttribute");
		}

		[Test]
		[TestCase("DeleteFolder")]
		[TestCase("DeleteFile")]
		public void Actions_Should_Have_AdminRequired_Attribute(string actionName)
		{
			// Arrange
			MethodInfo actionMethod = typeof(FileManagerController).GetMethod(actionName);

			// Act
			AdminRequiredAttribute adminAttribute = actionMethod.GetCustomAttributes(typeof(AdminRequiredAttribute), false)
							 .Cast<AdminRequiredAttribute>()
							 .SingleOrDefault();

			// Assert
			Assert.That(adminAttribute, Is.Not.Null, "Couldn't find the AdminRequiredAttribute");
		}

		// Helpers

		private string CreateTestFileInAttachments(string filename, string filecontent = "test")
		{
			string fullPath = Path.Combine(_settings.AttachmentsDirectoryPath, filename);
			File.WriteAllText(fullPath, filecontent);

			return fullPath;
		}

		private string CreateTestDirectoryInAttachments(string directoryName)
		{
			string fullPath = Path.Combine(_settings.AttachmentsDirectoryPath, directoryName);
			Directory.CreateDirectory(fullPath);

			return fullPath;
		}

		/// <summary>
		/// Sets up all the Request object's various properties to mock a file being uploaded. This sets the 
		/// file size to 8192 bytes, and writes each file name to disk when SaveAs() is called, with the content "test contents"
		/// </summary>
		private void SetupMockPostedFiles(MvcMockContainer container, string destinationFolder, params string[] fileNames)
		{
			// Mock the folder the files are saved to
			container.Request.Setup(x => x.Form).Returns(delegate()
			{
				var values = new NameValueCollection();
				values.Add("destination_folder", destinationFolder);
				return values;
			});

			// Add all the files provided so they save as an empty file to the file path
			Mock<HttpFileCollectionBase> postedfilesKeyCollection = new Mock<HttpFileCollectionBase>();
			container.Request.Setup(req => req.Files).Returns(postedfilesKeyCollection.Object);

			if (fileNames != null)
			{
				List<HttpPostedFileBase> files = new List<HttpPostedFileBase>();
				container.Request.Setup(x => x.Files.Count).Returns(fileNames.Length);
				for (int i = 0; i < fileNames.Length; i++)
				{
					Mock<HttpPostedFileBase> postedfile = new Mock<HttpPostedFileBase>();
					postedfile.Setup(f => f.ContentLength).Returns(8192);
					postedfile.Setup(f => f.FileName).Returns(fileNames[i]);
					postedfile.Setup(f => f.SaveAs(It.IsAny<string>())).Callback<string>(filename => File.WriteAllText(Path.Combine(_settings.AttachmentsDirectoryPath, filename), "test contents"));
					container.Request.Setup(x => x.Files[i]).Returns(postedfile.Object);
				}
			}
		}
	}
}
