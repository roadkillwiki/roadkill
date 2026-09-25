using System;
using System.IO;
using Roadkill.Core.Exceptions;
using NUnit.Framework;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Attachments
{
	[TestFixture]
	[Category("Unit")]
	public class AttachmentWriteResponseTests
	{
		private ApplicationSettings _applicationSettings;
		private LocalFileService _fileService;

		[SetUp]
		public void Setup()
		{
			_applicationSettings = new ApplicationSettings();
			_applicationSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Unit", "Attachments");
			_applicationSettings.AttachmentsRoutePath = "Attachments";

			_fileService = new LocalFileService(_applicationSettings, new SettingsService(new RepositoryFactoryMock(), _applicationSettings));
		}

		[Test]
		public void writeresponse_should_set_200_status_and_mimetype_and_write_bytes()
		{
			// Arrange

			string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Unit", "Attachments", "afile.jpg");
			File.WriteAllText(fullPath, "fake content");
			byte[] expectedBytes = File.ReadAllBytes(fullPath);
			string expectedMimeType = "image/jpeg";

			string localPath = "/wiki/Attachments/afile.jpg";
			string applicationPath = "/wiki";
			string modifiedSince = "";

			ResponseWrapperMock wrapper = new ResponseWrapperMock();

			// Act
			_fileService.WriteResponse(localPath, applicationPath, modifiedSince, wrapper);

			// Assert
			Assert.That(wrapper.StatusCode, Is.EqualTo(200));
			Assert.That(wrapper.ContentType, Is.EqualTo(expectedMimeType));
			Assert.That(wrapper.Buffer, Is.EqualTo(expectedBytes));
		}

		[Test]
		public void writeresponse_should_not_serve_files_outside_the_attachments_folder()
		{
			// Arrange (an existing file, next to the attachments folder)
			string outsideFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Unit", "outside-attachments.txt");
			File.WriteAllText(outsideFile, "secret");
			ResponseWrapperMock wrapper = new ResponseWrapperMock();

			// Act + Assert
			HttpStatusException exception = Assert.Throws<HttpStatusException>(() =>
				_fileService.WriteResponse("/wiki/Attachments/../outside-attachments.txt", "/wiki", "", wrapper));

			Assert.That(exception.StatusCode, Is.EqualTo(404));
			Assert.That(wrapper.Buffer, Is.Null.Or.Empty);
		}

		[Test]
		public void writeresponse_should_throw_404_exception_for_missing_file()
		{
			// Arrange

			string localPath = "/wiki/Attachments/doesntexist404.jpg";
			string applicationPath = "/wiki";
			string modifiedSince = "";

			ResponseWrapperMock wrapper = new ResponseWrapperMock();

			try
			{
				// Act + Assert
				_fileService.WriteResponse(localPath, applicationPath, modifiedSince, wrapper);

				Assert.Fail("No 404 HttpException thrown");
			}
			catch (HttpStatusException e)
			{
				Assert.That(e.StatusCode, Is.EqualTo(404));
			}
		}

		[Test]
		public void writeresponse_should_throw_404_exception_for_bad_application_path()
		{
			// Arrange

			string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Unit", "Attachments", "afile.jpg");
			File.WriteAllText(fullPath, "fake content");

			string localPath = "/wiki/Attachments/afile.jpg";
			string applicationPath = "/wookie";
			string modifiedSince = "";

			ResponseWrapperMock wrapper = new ResponseWrapperMock();

			try
			{
				// Act + Assert
				_fileService.WriteResponse(localPath, applicationPath, modifiedSince, wrapper);

				Assert.Fail("No 500 HttpException thrown");
			}
			catch (HttpStatusException e)
			{
				Assert.That(e.StatusCode, Is.EqualTo(404));
			}
		}

		[Test]
		public void translatelocalpathtofilepath_should_be_case_sensitive()
		{
			// Arrange
			_applicationSettings.AttachmentsFolder = Path.Combine(Path.GetTempPath(), "attachments") + Path.DirectorySeparatorChar;

			// Act
			string actualPath = _fileService.TranslateUrlPathToFilePath("/Attachments/a.jpg", "/");

			// Assert
			Assert.That(actualPath, Is.Not.EqualTo(Path.Combine(Path.GetTempPath(), "ATTACHMENTS", "a.jpg")), "TranslateLocalPathToFilePath does a case sensitive url" +
																			 " replacement (this is for Apache compatibility");
		}

		[TestCase("/Attachments/a.jpg", "", "a.jpg")] // should tolerate 'bad' application paths
		[TestCase("Attachments/a.jpg", "/", "a.jpg")] // should tolerate url not beginning with /
		[TestCase("/Attachments/a.jpg", "/", "a.jpg")]
		[TestCase("/Attachments/folder1/folder2/a.jpg", "/wiki/", "folder1/folder2/a.jpg")]
		[TestCase("/wiki/Attachments/a.jpg", "/wiki/", "a.jpg")]
		[TestCase("/wiki/Attachments/a.jpg", "/wiki", "a.jpg")]
		[TestCase("/wiki/wiki2/Attachments/a.jpg", "/wiki/wiki2/", "a.jpg")]
		public void TranslateLocalPathToFilePath(string localPath, string appPath, string expectedPath)
		{
			// Arrange
			string attachmentsFolder = Path.Combine(Path.GetTempPath(), "Attachments") + Path.DirectorySeparatorChar;
			_applicationSettings.AttachmentsFolder = attachmentsFolder;
			expectedPath = attachmentsFolder + expectedPath.Replace('/', Path.DirectorySeparatorChar);

			// Act
			string actualPath = _fileService.TranslateUrlPathToFilePath(localPath, appPath);

			// Assert
			Assert.That(actualPath, Is.EqualTo(expectedPath), $"Failed with {localPath} {appPath} {expectedPath}");
		}
	}
}
