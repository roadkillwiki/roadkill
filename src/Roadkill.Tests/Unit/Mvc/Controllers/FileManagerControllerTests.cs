using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Roadkill.Tests.Unit.StubsAndMocks;
using Roadkill.Core.Exceptions;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class FileManagerControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;
		private UserServiceMock _userService;
		private PageService _pageService;
		private PageHistoryService _historyService;
		private SettingsService _settingsService;
		private PluginFactoryMock _pluginFactory;
		private AttachmentFileHandler _attachmentFileHandler;
		private MvcMockContainer _mvcMockContainer;
		private FileServiceMock _fileService;

		private FileManagerController _filesController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_context = _container.UserContext;
			_repository = _container.Repository;
			_pluginFactory = _container.PluginFactory;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_historyService = _container.HistoryService;
			_pageService = _container.PageService;
			_attachmentFileHandler = new AttachmentFileHandler(_applicationSettings,_container.FileService);
			_fileService = _container.FileService as FileServiceMock;

			try
			{
				// Delete any existing attachments folder
				DirectoryInfo directoryInfo = new DirectoryInfo(_applicationSettings.AttachmentsFolder);
				if (directoryInfo.Exists)
				{
					directoryInfo.Attributes = FileAttributes.Normal;
					directoryInfo.Delete(true);
				}

				Directory.CreateDirectory(_applicationSettings.AttachmentsFolder);
			}
			catch (IOException e)
			{
				Assert.Fail("Unable to delete the attachments folder " + _applicationSettings.AttachmentsFolder + ", does it have a lock/explorer window open, or Mercurial open?" + e.ToString());
			}
			catch (ArgumentException e)
			{
				Assert.Fail("Unable to delete the attachments folder " + _applicationSettings.AttachmentsFolder + ", is EasyMercurial open?" + e.ToString());
			}

			_filesController = new FileManagerController(_applicationSettings, _userService, _context, _settingsService, _attachmentFileHandler, _fileService);
			_mvcMockContainer = _filesController.SetFakeControllerContext();
		}

		[Test]
		public void Index_Should_Return_View()
		{
			// Arrange

			// Act
			ActionResult result = _filesController.Index();
			
			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
		}

		[Test]
		public void Select_Should_Return_View()
		{
			// Arrange

			// Act
			ActionResult result = _filesController.Select();

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
		}

		[Test]
		public void DeleteFile_Should_Return_Ok_Json_Status()
		{
			// Arrange

			// Act
			JsonResult result = _filesController.DeleteFile("/", "test.txt") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.message, Is.EqualTo(""));
		}

		[Test]
		public void DeleteFile_Should_Return_Json_Error_Status_When_FileException_Is_Thrown()
		{
			// Arrange
			_fileService.CustomException = new FileException("It didn't delete", null);

			// Act
			JsonResult result = _filesController.DeleteFile("/", "test.txt") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.EqualTo("It didn't delete"));
		}

		[Test]
		public void DeleteFolder_Should_Return_Ok_Json_Status()
		{
			// Arrange

			// Act
			JsonResult result = _filesController.DeleteFolder("myfolder") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.message, Is.EqualTo(""));
		}

		[Test]
		public void DeleteFolder_Should_Return_Json_Error_Status_When_FileException_Is_Thrown()
		{
			// Arrange
			_fileService.CustomException = new FileException("It didn't delete the folder", null);

			// Act
			JsonResult result = _filesController.DeleteFolder("myfolder") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.EqualTo("It didn't delete the folder"));
		}

		[Test]
		public void FolderInfo_Should_Return_Model()
		{
			// Arrange

			// Act
			JsonResult result = _filesController.FolderInfo("myfolder") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult was not returned");

			DirectoryViewModel model = result.Data as DirectoryViewModel;
			Assert.That(model, Is.Not.Null, "DirectoryViewModel is null");
			Assert.That(model.ChildFolders.Count, Is.EqualTo(1));
			Assert.That(model.Files.Count, Is.EqualTo(1));
			Assert.That(model.Name, Is.EqualTo("myfolder"));
			Assert.That(model.UrlPath, Is.EqualTo("myfolderurlpath"));
		}

		[Test]
		public void FolderInfo_Should_Return_Json_Error_Status_When_FileException_Is_Thrown()
		{
			// Arrange
			_fileService.CustomException = new FileException("It didn't get the folder info", null);

			// Act
			JsonResult result = _filesController.FolderInfo("myfolder") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.EqualTo("It didn't get the folder info"));
		}

		[Test]
		public void NewFolder_Should_Return_Ok_Json_Status_And_New_FolderName()
		{
			// Arrange

			// Act
			JsonResult result = _filesController.NewFolder("currentfolder", "newfolder") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.FolderName, Is.EqualTo("newfolder"));
		}

		[Test]
		public void NewFolder_Should_Return_Json_Error_Status_When_FileException_Is_Thrown()
		{
			// Arrange
			_fileService.CustomException = new FileException("It didn't create the folder", null);

			// Act
			JsonResult result = _filesController.NewFolder("currentfolder", "newfolder") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.EqualTo("It didn't create the folder"));
		}

		[Test]
		public void Upload_Should_Return_Ok_Json_Status_And_Last_Filename_Uploaded_With_Text_Plain_Content_Type()
		{
			// Arrange
			MvcMockContainer container = _filesController.SetFakeControllerContext();
			SetupMockPostedFiles(container, "/", "file1.png");

			// Act
			JsonResult result = _filesController.Upload() as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.ContentType, Is.EqualTo("text/plain"), "JsonResult content type");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("ok"));
			Assert.That(jsonObject.filename, Is.EqualTo("file1.png"));
		}

		[Test]
		public void Upload_Should_Return_Json_Error_Status_When_FileException_Is_Thrown()
		{
			// Arrange
			MvcMockContainer container = _filesController.SetFakeControllerContext();
			SetupMockPostedFiles(container, "/", "file1.png");

			_fileService.CustomException = new FileException("It didn't upload a file", null);

			// Act
			JsonResult result = _filesController.NewFolder("currentfolder", "newfolder") as JsonResult;

			// Assert
			Assert.That(result, Is.Not.Null, "JsonResult");
			Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.DenyGet));

			dynamic jsonObject = result.Data;
			Assert.That(jsonObject.status, Is.EqualTo("error"));
			Assert.That(jsonObject.message, Is.EqualTo("It didn't upload a file"));
		}

		[Test]
		public void Upload_Should_Accept_HttpPost_Only()
		{
			// Arrange, Act and Assert
			_filesController.AssertHttpPostOnly(x => x.Upload());
		}

		[Test]
		public void DeleteFile_Should_Accept_HttpPost_Only()
		{
			// Arrange, Act and Assert
			_filesController.AssertHttpPostOnly(x => x.DeleteFile("",""));
		}

		[Test]
		public void DeleteFolder_Should_Accept_HttpPost_Only()
		{
			// Arrange, Act and Assert
			_filesController.AssertHttpPostOnly(x => x.DeleteFolder(""));
		}

		[Test]
		public void FolderInfo_Should_Accept_HttpPost_Only()
		{
			// Arrange, Act and Assert
			_filesController.AssertHttpPostOnly(x => x.FolderInfo(""));
		}

		[Test]
		public void NewFolder_Should_Accept_HttpPost_Only()
		{
			// Arrange, Act and Assert
			_filesController.AssertHttpPostOnly(x => x.NewFolder("",""));
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
					postedfile.Setup(f => f.SaveAs(It.IsAny<string>())).Callback<string>(filename => File.WriteAllText(Path.Combine(_applicationSettings.AttachmentsDirectoryPath, filename), "test contents"));
					container.Request.Setup(x => x.Files[i]).Returns(postedfile.Object);
				}
			}
		}
	}
}
