using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Exceptions;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;

namespace Roadkill.Tests.Unit.Services
{
	[TestFixture]
	[Category("Unit")]
	public class LocalFileServiceTests
	{
		private MocksAndStubsContainer _container;
		private ApplicationSettings _applicationSettings;
		private SettingsService _settingsService;
		private IFileService _fileService;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();
			_applicationSettings = _container.ApplicationSettings;
			_settingsService = _container.SettingsService;
			_fileService = new LocalFileService(_applicationSettings, _settingsService);

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
				Assert.Fail("Unable to delete the attachments folder " + _applicationSettings.AttachmentsFolder + ", does it have a lock/explorer window open?" + e);
			}
			catch (ArgumentException e)
			{
				Assert.Fail("Unable to delete the attachments folder " + _applicationSettings.AttachmentsFolder + ", is EasyMercurial open?" + e);
			}
		}

		[Test]
		public void delete_should_delete_given_file()
		{
			// Arrange
			string fullPath = CreateTestFileInAttachments("test.txt");

			// Act
			_fileService.Delete("/", "test.txt");

			// Assert
			Assert.That(File.Exists(fullPath), Is.False);
		}

		[Test]
		public void delete_should_not_throw_exception_when_file_doesnt_exist()
		{
			// Arrange + Act + Assert
			_fileService.Delete("/", "this.file.doesnt.exist.txt");
		}

		[Test]
		public void delete_in_subfolder_should_delete_file()
		{
			// Arrange
			string testFile1Path = CreateTestFileInAttachments("test.txt");
			string dirPath = CreateTestDirectoryInAttachments("test");
			string testFile2Path = Path.Combine(dirPath, "test.txt");
			File.WriteAllText(testFile2Path, "test");

			// Act
			_fileService.Delete("/test/", "test.txt");

			// Assert
			Assert.That(File.Exists(testFile2Path), Is.False);
			Assert.That(File.Exists(testFile1Path), Is.True);
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void Delete_With_Bad_Paths_Throws_Exception()
		{
			// Arrange + Act + Assert
			_fileService.Delete("/.././", "hacker.txt");
		}

		[Test]
		public void deletefolder_should_delete_given_folder()
		{
			// Arrange
			string fullPath = CreateTestDirectoryInAttachments("folder1");

			// Act
			_fileService.DeleteFolder("folder1");

			// Assert
			Assert.That(Directory.Exists(fullPath), Is.False);
		}

		[Test]
		public void deletefolder_with_subdirectory_should_delete_given_subfolder_not_other_children_or_parent()
		{
			// Arrange
			string fullPath = CreateTestDirectoryInAttachments("folder1");
			string subPath = Path.Combine(fullPath, "subfolder1");
			string subsubPath = Path.Combine(subPath, "subsubfolder");
			Directory.CreateDirectory(subPath);
			Directory.CreateDirectory(subsubPath);

			// Act
			_fileService.DeleteFolder("folder1/subfolder1/subsubfolder");

			// Assert
			Assert.That(Directory.Exists(subsubPath), Is.False);
			Assert.That(Directory.Exists(subPath), Is.True);
			Assert.That(Directory.Exists(fullPath), Is.True);
		}

		[Test]
		[ExpectedException(typeof(FileException))]
		public void DeleteFolder_Empty_Folder_Argument_Should_Throw_Exception()
		{
			// Arrange, Act, Assert
			_fileService.DeleteFolder("");
		}

		[Test]
		[ExpectedException(typeof(FileException))]
		public void DeleteFolder_Containing_Files_Should_Throw_FileException()
		{
			// Arrange
			CreateTestDirectoryInAttachments("folder1");
			string fullPath = Path.Combine(_applicationSettings.AttachmentsDirectoryPath, "folder1", "test.txt");
			File.WriteAllText(fullPath, "test");

			// Act, Assert
			_fileService.DeleteFolder("folder1");
		}

		[Test]
		[ExpectedException(typeof(FileException))]
		public void DeleteFolder_With_Folder_That_Has_Subdirectories_Should_Throw_FileException()
		{
			// Arrange
			string fullpath = CreateTestDirectoryInAttachments("folder1");
			Directory.CreateDirectory(Path.Combine(fullpath, "subfolder1"));

			// Act, Assert
			_fileService.DeleteFolder("folder1");
		}

		[Test]
		[ExpectedException(typeof(FileException))]
		public void DeleteFolder_With_Missing_Directory_Should_Throw_FileException()
		{
			// Arrange, Act, Assert
			_fileService.DeleteFolder("folder1/folder2");
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void DeleteFolder_With_Bad_Paths_Should_Throw_Exception()
		{
			// Arrange + Act + Assert
			_fileService.DeleteFolder("/../../folder1");
		}

		[Test]
		public void folderinfo_with_empty_path_should_return_model_with_root()
		{
			// Arrange
			CreateTestDirectoryInAttachments("blah");
			CreateTestFileInAttachments("blah.png");

			// Act
			DirectoryViewModel model = _fileService.FolderInfo("");

			// Assert
			Assert.That(model, Is.Not.Null, "DirectoryViewModel is null");
			Assert.That(model.ChildFolders.Count, Is.EqualTo(1));
			Assert.That(model.Files.Count, Is.EqualTo(1));
			Assert.That(model.Name, Is.EqualTo(""));
			Assert.That(model.UrlPath, Is.EqualTo(""));
		}

		[Test]
		public void folderinfo_with_root_should_return_model()
		{
			// Arrange
			CreateTestDirectoryInAttachments("blah");
			CreateTestFileInAttachments("blah.png");

			// Act
			DirectoryViewModel model = _fileService.FolderInfo("");

			// Assert
			Assert.That(model, Is.Not.Null, "DirectoryViewModel is null");
			Assert.That(model.ChildFolders.Count, Is.EqualTo(1));
			Assert.That(model.Files.Count, Is.EqualTo(1));
			Assert.That(model.Name, Is.EqualTo(""));
			Assert.That(model.UrlPath, Is.EqualTo(""));
		}

		[Test]
		public void folderinfo_with_subfolder_should_return_model()
		{
			// Arrange
			CreateTestDirectoryInAttachments(@"blah\blah2\blah3");
			CreateTestDirectoryInAttachments(@"blah\blah2\blah3\blah4");
			CreateTestFileInAttachments(@"blah\blah2\blah3\something.png");
			CreateTestFileInAttachments(@"blah\blah2\blah3\something2.png");
			CreateTestFileInAttachments(@"blah\blah2\blah3\something3.png");

			// Act
			DirectoryViewModel model = _fileService.FolderInfo("/blah/blah2/blah3");

			// Assert
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
			// Arrange + Act + Assert
			_fileService.FolderInfo("/missingfolder");
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void FolderInfo_With_Bad_Folder_Path_Should_Throw_Exception()
		{
			// Arrange + Act + Assert
			_fileService.FolderInfo(".././");
		}

		[Test]
		public void createfolder_in_root_folder_should_create_folder_and_return_ok_json_status()
		{
			// Arrange
			string folderName = "newfolder with spaces in it";
			string fullPath = Path.Combine(_applicationSettings.AttachmentsDirectoryPath, folderName);

			// Act
			bool result = _fileService.CreateFolder("/", folderName);

			// Assert
			Assert.That(result, Is.True);
		}

		[Test]
		public void createfolder_with_subdirectory_should_create_folder_and_return_ok_json_status()
		{
			// Arrange
			string fullPath = CreateTestDirectoryInAttachments("folder1");
			string subPath = Path.Combine(fullPath, "subfolder1");
			string subsubPath = Path.Combine(subPath, "subsubfolder");
			Directory.CreateDirectory(subPath);

			// Act
			bool result = _fileService.CreateFolder("/folder1/subfolder1/", "subsubfolder");

			// Assert
			Assert.That(result, Is.True);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CreateFolder_With_Empty_FolderName_Argument_Should_Throw_ArgumentNullException()
		{
			// Arrange + Act + Assert
			_fileService.CreateFolder("/", "");
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void CreateFolder_With_Missing_Parent_Directory_Should_Return_Error()
		{
			// Arrange + Act + Assert
			_fileService.CreateFolder("folder1/folder2", "newfolder");
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void CreateFolder_With_Bad_Folder_Path_Should_Throw_Exception()
		{
			// Arrange + Act + Assert
			_fileService.CreateFolder("/../../folder1", "../cheeky/path");
		}

		[Test]
		public void upload_with_single_file_to_root_should_save_file_to_disk_and_return_filename()
		{
			// Arrange
			string file1FullPath = Path.Combine(_applicationSettings.AttachmentsDirectoryPath, "file1.png");
			HttpFileCollectionBase fileCollection = CreateFileCollection("file1.png");

			// Act
			string filename = _fileService.Upload("/", fileCollection);

			// Assert
			Assert.That(filename, Is.EqualTo("file1.png"));
			Assert.That(File.Exists(file1FullPath), Is.True);
		}

		[Test]
		public void upload_should_overwrite_existing_file_whenoverwritefiles_setting_is_true()
		{
			// Arrange
			SiteSettings siteSettings = _settingsService.GetSiteSettings();
			siteSettings.OverwriteExistingFiles = true;

			CreateTestFileInAttachments("file1.png", "the original file");
			string file1FullPath = Path.Combine(_applicationSettings.AttachmentsDirectoryPath, "file1.png");

			HttpFileCollectionBase fileCollection = CreateFileCollection("file1.png");

			// Act
			string filename = _fileService.Upload("/", fileCollection);

			// Assert
			Assert.That(filename, Is.EqualTo("file1.png"));
			string fileContent = File.ReadAllText(file1FullPath);
			Assert.That(fileContent, Is.EqualTo("test contents"));
		}

		[Test]
		public void upload_should_be_case_insensitive()
		{
			// Arrange
			HttpFileCollectionBase fileCollection = CreateFileCollection("file1.PNG");
			string file1FullPath = Path.Combine(_applicationSettings.AttachmentsDirectoryPath, "file1.PNG");

			// Act
			string filename = _fileService.Upload("/", fileCollection);

			// Assert
			Assert.That(filename, Is.EqualTo("file1.PNG"));
			Assert.That(File.Exists(file1FullPath), Is.True);
		}


		[Test]
		public void upload_with_multiple_files_to_root_should_save_files_to_disk_and_return_last_uploaded_file()
		{
			// Arrange
			HttpFileCollectionBase fileCollection = CreateFileCollection("file1.png", "file2.png");
			string file1FullPath = Path.Combine(_applicationSettings.AttachmentsDirectoryPath, "file1.png");
			string file2FullPath = Path.Combine(_applicationSettings.AttachmentsDirectoryPath, "file2.png");

			// Act
			string filename = _fileService.Upload("/", fileCollection);

			// Assert
			Assert.That(filename, Is.EqualTo("file2.png"));
			Assert.That(File.Exists(file1FullPath), Is.True);
			Assert.That(File.Exists(file2FullPath), Is.True);
		}

		[Test]
		public void upload_with_multiple_files_to_subfolder_should_save_files_and_return_last_uploaded_file()
		{
			// Arrange
			HttpFileCollectionBase fileCollection = CreateFileCollection("file1.png", "file2.png");

			string fullPath = CreateTestDirectoryInAttachments("folder1");
			string subPath = Path.Combine(fullPath, "folder2");
			Directory.CreateDirectory(subPath);

			string file1FullPath = Path.Combine(subPath, "file1.png");
			string file2FullPath = Path.Combine(subPath, "file2.png");

			// Act
			string filename = _fileService.Upload("/folder1/folder2", fileCollection);

			// Assert
			Assert.That(filename, Is.EqualTo("file2.png"));
			Assert.That(File.Exists(file1FullPath), Is.True);
			Assert.That(File.Exists(file2FullPath), Is.True);
		}

		[Test]
		public void upload_with_no_files_to_root_should_return_empty_filename()
		{
			// Arrange
			HttpFileCollectionBase fileCollection = CreateFileCollection();

			// Act
			string filename = _fileService.Upload("/", fileCollection);

			// Assert
			Assert.That(filename, Is.EqualTo(""));
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void FileUpload_With_Bad_Folder_Path_Should_Throw_Exception()
		{
			// Arrange
			HttpFileCollectionBase fileCollection = CreateFileCollection();

			// Act + Assert
			_fileService.Upload("/../../bad/path", fileCollection);
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void FileUpload_With_Missing_Folder_Should_Throw_SecurityException()
		{
			// Arrange
			HttpFileCollectionBase fileCollection = CreateFileCollection();

			// Act + Assert
			_fileService.Upload("/missingfolder", fileCollection);
		}

		[Test]
		[ExpectedException(typeof(FileException))]
		public void FileUpload_Should_Throw_FileException_When_File_Has_Bad_Extension()
		{
			// Arrange
			HttpFileCollectionBase fileCollection = CreateFileCollection("virus.exe");
			string file1FullPath = Path.Combine(_applicationSettings.AttachmentsDirectoryPath, "virus.exe");

			// Act + Assert
			_fileService.Upload("/", fileCollection);
		}

		[Test]
		[ExpectedException(typeof(FileException))]
		public void FileUpload_Should_Throw_FileException_When_File_Exists_And_OverWriteFiles_Setting_Is_False()
		{
			// Arrange
			SiteSettings siteSettings = _settingsService.GetSiteSettings();
			siteSettings.OverwriteExistingFiles = false;
			CreateTestFileInAttachments("file1.png", "the original file");

			HttpFileCollectionBase fileCollection = CreateFileCollection("file1.png.exe");
			string file1FullPath = Path.Combine(_applicationSettings.AttachmentsDirectoryPath, "file1.png");

			// Act + Assert
			_fileService.Upload("/", fileCollection);
		}

		[Test]
		public void fileupload_should_throw_fileexception_when_file_exists_and_overwritefiles_setting_is_false_for_multiple_files()
		{
			// Arrange
			SiteSettings siteSettings = _settingsService.GetSiteSettings();
			siteSettings.OverwriteExistingFiles = false;
			CreateTestFileInAttachments("file3.png", "the original file"); // just 1 existing file

			HttpFileCollectionBase fileCollection = CreateFileCollection("file1.png", "file2.png", "file3.png", "file4.png", "file5.png");

			string file1FullPath = Path.Combine(_applicationSettings.AttachmentsDirectoryPath, "file1.png");
			string file2FullPath = Path.Combine(_applicationSettings.AttachmentsDirectoryPath, "file2.png");
			string file3FullPath = Path.Combine(_applicationSettings.AttachmentsDirectoryPath, "file3.png");
			string file4FullPath = Path.Combine(_applicationSettings.AttachmentsDirectoryPath, "file4.png");
			string file5FullPath = Path.Combine(_applicationSettings.AttachmentsDirectoryPath, "file5.png");

			// Act
			try
			{
				_fileService.Upload("/", fileCollection);

				Assert.Fail("FileException was not thrown");

			}
			catch (FileException)
			{
				// Check the files that didn't previously exist were uploaded.
				Assert.That(File.Exists(file1FullPath), Is.True);
				Assert.That(File.Exists(file2FullPath), Is.True);

				// The files after file3.png won't be uploaded.
				Assert.That(File.Exists(file4FullPath), Is.False);
				Assert.That(File.Exists(file5FullPath), Is.False);
			}
		}

		// Helpers
		private string CreateTestFileInAttachments(string filename, string filecontent = "test")
		{
			string fullPath = Path.Combine(_applicationSettings.AttachmentsDirectoryPath, filename);
			File.WriteAllText(fullPath, filecontent);

			return fullPath;
		}

		private string CreateTestDirectoryInAttachments(string directoryName)
		{
			string fullPath = Path.Combine(_applicationSettings.AttachmentsDirectoryPath, directoryName);
			Directory.CreateDirectory(fullPath);

			return fullPath;
		}

		/// <summary>
		/// Sets up all the Request object's various properties to mock a file being uploaded. This sets the 
		/// file size to 8192 bytes, and writes each file name to disk when SaveAs() is called, with the content "test contents"
		/// </summary>
		private HttpFileCollectionBase CreateFileCollection(params string[] fileNames)
		{
			// Add all the files provided so they save as an empty file to the file path
			Mock<HttpFileCollectionBase> fileCollection = new Mock<HttpFileCollectionBase>();
			fileCollection.Setup(x => x.Count).Returns(fileNames.Length);

			List<HttpPostedFileBase> files = new List<HttpPostedFileBase>();
			for (int i = 0; i < fileNames.Length; i++)
			{
				Mock<HttpPostedFileBase> postedfile = new Mock<HttpPostedFileBase>();
				postedfile.Setup(f => f.ContentLength).Returns(8192);
				postedfile.Setup(f => f.FileName).Returns(fileNames[i]);
				postedfile.Setup(f => f.SaveAs(It.IsAny<string>())).Callback<string>(filename => File.WriteAllText(Path.Combine(_applicationSettings.AttachmentsDirectoryPath, filename), "test contents"));

				// Setup the files[i] indexer
				fileCollection.SetupGet(x => x[i]).Returns(postedfile.Object);
			}

			return fileCollection.Object;
		}
	}
}
