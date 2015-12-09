using System;
using System.IO;
using NUnit.Framework;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;

namespace Roadkill.Tests.Unit.Attachments
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
		public void isattachmentpathvalid_should_be_true_for_valid_subdirectory()
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
		public void isattachmentpathvalid_should_be_false_for_valid_path_that_does_not_exist()
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
		public void isattachmentpathvalid_should_be_case_insensitive()
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
		public void isattachmentpathvalid_should_be_true_for_emptystring()
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
		public void attachmentfolderexistsandwriteable_should_return_empty_string_for_writeable_folder()
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
		public void attachmentfolderexistsandwriteable_should_return_error_for_empty_folder()
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
		public void attachmentfolderexistsandwriteable_should_return_error_for_missing_folder()
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
