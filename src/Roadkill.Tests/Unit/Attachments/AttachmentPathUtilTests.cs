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
	public class AttachmentPathUtilTests
	{
		private ApplicationSettings _settings;
		private AttachmentPathUtil _attachmentPathUtil;

		[SetUp]
		public void Setup()
		{
			_settings = new ApplicationSettings();
			_settings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Attachments");
			_attachmentPathUtil = new AttachmentPathUtil(_settings);

			try
			{
				// Delete any existing attachments folder

				// Remove the files 1st
				if (Directory.Exists(_settings.AttachmentsFolder))
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(_settings.AttachmentsFolder);
					foreach (FileInfo file in directoryInfo.GetFiles())
					{
						File.Delete(file.FullName);
					}

					if (directoryInfo.Exists)
					{
						directoryInfo.Attributes = FileAttributes.Normal;
						directoryInfo.Delete(true);
					}
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
			string actualPath = _attachmentPathUtil.ConvertUrlPathToPhysicalPath(relativePath);

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
			string actualPath = _attachmentPathUtil.ConvertUrlPathToPhysicalPath(relativePath);

			// Assert
			Assert.That(actualPath, Is.EqualTo(expectedPath));
		}

		[Test]
		public void IsAttachmentPathValid_Should_Be_True_For_Valid_SubDirectory()
		{
			// Arrange
			string physicalPath = Path.Combine(_settings.AttachmentsDirectoryPath, "images", "test");
			Directory.CreateDirectory(physicalPath);
			bool expectedResult = true;

			// Act
			bool actualResult = _attachmentPathUtil.IsAttachmentPathValid(physicalPath);

			// Assert
			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void IsAttachmentPathValid_Should_Be_False_For_Valid_Path_That_Does_Not_Exist()
		{
			// Arrange
			string physicalPath = Path.Combine(_settings.AttachmentsDirectoryPath, "folder100", "folder99");
			bool expectedResult = false;

			// Act
			bool actualResult = _attachmentPathUtil.IsAttachmentPathValid(physicalPath);

			// Assert
			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void IsAttachmentPathValid_Should_Be_Case_Insensitive()
		{
			// Arrange
			string physicalPath = Path.Combine(_settings.AttachmentsDirectoryPath, "images", "test");
			physicalPath = physicalPath.Replace("Attachments", "aTTacHMentS");
			Directory.CreateDirectory(physicalPath);

			bool expectedResult = true;

			// Act
			bool actualResult = _attachmentPathUtil.IsAttachmentPathValid(physicalPath);

			// Assert
			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		[TestCase("{attachmentsfolder}", true)]
		[TestCase(@"{attachmentsfolder}folder1", true)]
		[TestCase(@"{attachmentsfolder}folder1\folder2", true)]
		public void IsAttachmentPathValid_Should_Be_True_For_Valid_Paths(string physicalPath, bool expectedResult)
		{
			// Arrange
			physicalPath = physicalPath.Replace("{attachmentsfolder}", _settings.AttachmentsDirectoryPath);
			Directory.CreateDirectory(physicalPath);

			// Act
			bool actualResult = _attachmentPathUtil.IsAttachmentPathValid(physicalPath);

			// Assert
			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void IsAttachmentPathValid_Should_Be_True_For_EmptyString()
		{
			// Arrange
			bool expectedResult = true;

			// Act
			bool actualResult = _attachmentPathUtil.IsAttachmentPathValid("");

			// Assert
			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		[TestCase(@".\", false)]
		[TestCase(@"\.", false)]
		[TestCase(@"\..", false)]
		[TestCase(@"..\", false)]
		[TestCase(@"{attachmentsfolder}.\", false)]
		[TestCase(@"{attachmentsfolder}\.", false)]
		[TestCase(@"{attachmentsfolder}\..", false)]
		[TestCase(@"{attachmentsfolder}..\", false)]
		[TestCase(@"{attachmentsfolder}.\..\", false)]
		[TestCase(@"{attachmentsfolder}..\.\", false)]
		[TestCase(@"./../", false)]
		[TestCase("/", false)]
		[TestCase("/folder1", false)]
		[TestCase("/folder1/folder2", false)]
		public void IsAttachmentPathValid_Should_Be_False_For_Invalid_Paths(string physicalPath, bool expectedResult)
		{
			// Arrange
			physicalPath = physicalPath.Replace("{attachmentsfolder}", _settings.AttachmentsDirectoryPath);

			// Act
			bool actualResult = _attachmentPathUtil.IsAttachmentPathValid(physicalPath);

			// Assert
			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void AttachmentFolderExistsAndWriteable_Should_Return_Empty_String_For_Writeable_Folder()
		{
			// Arrange
			string directory = AppDomain.CurrentDomain.BaseDirectory;
			string expectedMessage = "";

			// Act
			string actualMessage = AttachmentPathUtil.AttachmentFolderExistsAndWriteable(directory, null);

			// Assert
			Assert.That(actualMessage, Is.EqualTo(expectedMessage));
		}

		[Test]
		public void AttachmentFolderExistsAndWriteable_Should_Return_Error_For_Empty_Folder()
		{
			// Arrange
			string directory = "";
			string expectedMessage = "The folder name is empty";

			// Act
			string actualMessage = AttachmentPathUtil.AttachmentFolderExistsAndWriteable(directory, null);

			// Assert
			Assert.That(actualMessage, Is.EqualTo(expectedMessage));
		}

		[Test]
		public void AttachmentFolderExistsAndWriteable_Should_Return_Error_For_Missing_Folder()
		{
			// Arrange
			string directory = @"c:\87sd9f7dssdds3232";
			string expectedMessage = "The directory does not exist, please create it first";

			// Act
			string actualMessage = AttachmentPathUtil.AttachmentFolderExistsAndWriteable(directory, null);

			// Assert
			Assert.That(actualMessage, Is.EqualTo(expectedMessage));
		}
	}
}
