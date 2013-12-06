using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class AttachmentFileHandlerTests
	{
		private ApplicationSettings _applicationSettings;

		[SetUp]
		public void Setup()
		{
			_applicationSettings = new ApplicationSettings();
			_applicationSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Unit", "Attachments");
			_applicationSettings.AttachmentsRoutePath = "Attachments";
		}

		[Test]
		public void WriteResponse_Should_Set_200_Status_And_MimeType_And_Write_Bytes()
		{
			// Arrange
			AttachmentFileHandler handler = new AttachmentFileHandler(_applicationSettings);

			string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Unit", "Attachments", "afile.jpg");
			File.WriteAllText(fullPath, "fake content");
			byte[] expectedBytes = File.ReadAllBytes(fullPath);
			string expectedMimeType = "image/jpeg";

			string localPath = "/wiki/Attachments/afile.jpg";
			string applicationPath = "/wiki";
			string modifiedSince = "";

			ResponseWrapperMock wrapper = new ResponseWrapperMock();

			// Act
			handler.WriteResponse(localPath, applicationPath, modifiedSince, wrapper);

			// Assert
			Assert.That(wrapper.StatusCode, Is.EqualTo(200));
			Assert.That(wrapper.ContentType, Is.EqualTo(expectedMimeType));
			Assert.That(wrapper.Buffer, Is.EqualTo(expectedBytes));
		}

		[Test]
		public void WriteResponse_Should_Throw_404_Exception_For_Missing_File()
		{
			// Arrange
			AttachmentFileHandler handler = new AttachmentFileHandler(_applicationSettings);

			string localPath = "/wiki/Attachments/doesntexist404.jpg";
			string applicationPath = "/wiki";
			string modifiedSince = "";

			ResponseWrapperMock wrapper = new ResponseWrapperMock();

			try
			{
				// Act + Assert
				handler.WriteResponse(localPath, applicationPath, modifiedSince, wrapper);

				Assert.Fail("No 404 HttpException thrown");
			}
			catch (HttpException e)
			{
				Assert.That(e.GetHttpCode(), Is.EqualTo(404));
			}
		}

		[Test]
		public void WriteResponse_Should_Throw_404_Exception_For_Bad_Application_Path()
		{
			// Arrange
			AttachmentFileHandler handler = new AttachmentFileHandler(_applicationSettings);

			string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Unit", "Attachments", "afile.jpg");
			File.WriteAllText(fullPath, "fake content");

			string localPath = "/wiki/Attachments/afile.jpg";
			string applicationPath = "/wookie";
			string modifiedSince = "";

			ResponseWrapperMock wrapper = new ResponseWrapperMock();

			try
			{
				// Act + Assert
				handler.WriteResponse(localPath, applicationPath, modifiedSince, wrapper);

				Assert.Fail("No 500 HttpException thrown");
			}
			catch (HttpException e)
			{
				Assert.That(e.GetHttpCode(), Is.EqualTo(404));
			}
		}

		[Test]
		public void TranslateLocalPathToFilePath_Should_Be_Case_Sensitive()
		{
			// Arrange
			_applicationSettings.AttachmentsFolder = @"C:\attachments\";
			AttachmentFileHandler handler = new AttachmentFileHandler(_applicationSettings);

			// Act
			string actualPath = handler.TranslateUrlPathToFilePath("/Attachments/a.jpg", "/");

			// Assert
			Assert.That(actualPath, Is.Not.EqualTo(@"c:\Attachments\a.jpg"), "TranslateLocalPathToFilePath does a case sensitive url" +
																			 " replacement (this is for Apache compatibility");
		}

		[TestCase("/Attachments/a.jpg", "", @"C:\Attachments\a.jpg")] // should tolerate 'bad' application paths
		[TestCase("Attachments/a.jpg", "/", @"C:\Attachments\a.jpg")] // should tolerate url not beginning with /
		[TestCase("/Attachments/a.jpg", "/", @"C:\Attachments\a.jpg")]
		[TestCase("/Attachments/folder1/folder2/a.jpg", "/wiki/", @"C:\Attachments\folder1\folder2\a.jpg")]
		[TestCase("/wiki/Attachments/a.jpg", "/wiki/", @"C:\Attachments\a.jpg")]
		[TestCase("/wiki/Attachments/a.jpg", "/wiki", @"C:\Attachments\a.jpg")]
		[TestCase("/wiki/wiki2/Attachments/a.jpg", "/wiki/wiki2/", @"C:\Attachments\a.jpg")]
		public void TranslateLocalPathToFilePath(string localPath, string appPath, string expectedPath)
		{
			// Arrange
			_applicationSettings.AttachmentsFolder = @"C:\Attachments\";
			AttachmentFileHandler handler = new AttachmentFileHandler(_applicationSettings);

			// Act
			string actualPath = handler.TranslateUrlPathToFilePath(localPath, appPath);

			// Assert
			Assert.That(actualPath, Is.EqualTo(expectedPath), "Failed with {0} {1} {2}", localPath, appPath, expectedPath);
		}
	}
}
