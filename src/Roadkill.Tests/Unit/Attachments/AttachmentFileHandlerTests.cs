using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class AttachmentFileHandlerTests
	{
		private ApplicationSettings _settings;
		private AttachmentFileHandler _fileHandler;

		[SetUp]
		public void Setup()
		{
			_settings = new ApplicationSettings();
			_settings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Attachments");
			_fileHandler = new AttachmentFileHandler(_settings);

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
				Assert.Fail("Unable to delete the attachments folder " + _settings.AttachmentsFolder + ", does it have a lock?" + e.ToString());
			}
		}

		[Test]
		[TestCase("")]
		[TestCase("/")]
		[TestCase("///")]
		public void ConvertUrlPathToPhysicalPath_Should_Strip_Empty_And_Redundant_Seperators(string relativePath)
		{
			// Arrange
			string expectedPath = _settings.AttachmentsDirectoryPath;

			// Act
			string actualPath = _fileHandler.ConvertUrlPathToPhysicalPath(relativePath);

			// Assert
			Assert.That(actualPath, Is.EqualTo(expectedPath));
		}

		[Test]
		[TestCase("/folder1", @"folder1\")]
		[TestCase("/folder1/folder2", @"folder1\folder2\")]
		[TestCase("/folder1/folder2/", @"folder1\folder2\")]
		public void ConvertUrlPathToPhysicalPath_Should_Combine_Paths_And_Contain_Trailing_Slash(string relativePath, string expectedPath)
		{
			// Arrange
			expectedPath = Path.Combine(_settings.AttachmentsDirectoryPath, expectedPath);
			Directory.CreateDirectory(expectedPath);

			// Act
			string actualPath = _fileHandler.ConvertUrlPathToPhysicalPath(relativePath);

			// Assert
			Assert.That(actualPath, Is.EqualTo(expectedPath));
		}

		[Test]
		public void IsAttachmentPathValid_Should_Be_True_For_Valid_For_Full_Path()
		{
			// Arrange
			string physicalPath = Path.Combine(_settings.AttachmentsDirectoryPath, "images", "test");
			Directory.CreateDirectory(physicalPath);

			// Act
			bool actualResult = _fileHandler.IsAttachmentPathValid(physicalPath);

			// Assert
			Assert.That(actualResult, Is.True);
		}

		[Test]
		public void IsAttachmentPathValid_Should_Be_Case_Insensitive()
		{
			// Arrange
			string physicalPath = Path.Combine(_settings.AttachmentsDirectoryPath, "images", "test");
			physicalPath = physicalPath.Replace("Attachments", "aTTacHMentS");
			Directory.CreateDirectory(physicalPath);

			// Act
			bool actualResult = _fileHandler.IsAttachmentPathValid(physicalPath);

			// Assert
			Assert.That(actualResult, Is.True);
		}

		[Test]
		[TestCase("", true)]
		[TestCase(@"folder1", true)]
		[TestCase(@"folder1\folder2", true)]
		public void IsAttachmentPathValid_Should_Be_True_For_Valid_Paths(string physicalPath, bool isValid)
		{
			// Arrange
			physicalPath = Path.Combine(_settings.AttachmentsDirectoryPath, physicalPath);
			Directory.CreateDirectory(physicalPath);

			// Act
			bool actualResult = _fileHandler.IsAttachmentPathValid(physicalPath);

			// Assert
			Assert.That(actualResult, Is.EqualTo(isValid));
		}

		[Test]
		[TestCase(@".\", false)]
		[TestCase(@"\.", false)]
		[TestCase(@"\..", false)]
		[TestCase(@"..\", false)]
		[TestCase(@".\..\", false)]
		[TestCase(@"..\.\", false)]
		[TestCase(@"./../", false)]
		[TestCase("/", false)]
		[TestCase("/folder1", false)]
		[TestCase("/folder1/folder2", false)]
		public void IsAttachmentPathValid_Should_Be_False_For_Invalid_Paths(string physicalPath, bool isValid)
		{
			// Arrange

			// Act
			bool actualResult = _fileHandler.IsAttachmentPathValid(physicalPath);

			// Assert
			Assert.That(actualResult, Is.EqualTo(isValid));
		}
	}
}
