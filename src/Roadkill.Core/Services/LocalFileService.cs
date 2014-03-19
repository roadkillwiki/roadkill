using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.Exceptions;
using Roadkill.Core.Localization;
using Roadkill.Core.Mvc.ViewModels;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

namespace Roadkill.Core.Services
{
	class LocalFileService : IFileService
	{
		#region private properties

		private readonly AttachmentPathUtil _attachmentPathUtil;
		private readonly ApplicationSettings _applicationSettings;
		private readonly SettingsService _settingsService;
		private static readonly string[] FilesToExclude = new string[] { "emptyfile.txt", "_installtest.txt" }; // installer/publish files

		#endregion

		#region constructors

		public LocalFileService(ApplicationSettings settings, SettingsService settingsService)
		{
			_applicationSettings = settings;
			_settingsService = settingsService;
			_attachmentPathUtil = new AttachmentPathUtil(settings);
		}

		#endregion

		#region public methods

		public void Delete(string filePath, string fileName)
		{
			string physicalPath = _attachmentPathUtil.ConvertUrlPathToPhysicalPath(filePath);
			string physicalFilePath = Path.Combine(physicalPath, fileName);

			if (!_attachmentPathUtil.IsAttachmentPathValid(physicalPath))
			{
				throw new SecurityException(null, "Attachment path was invalid when deleting {0}", fileName);
			}

			try
			{
				if (System.IO.File.Exists(physicalFilePath))
					System.IO.File.Delete(physicalFilePath);
			}
			catch (IOException e)
			{
				throw new FileException(e, "Unable to delete {0} from {1}", fileName, filePath);
			}
		}

		public void DeleteFolder(string folderPath)
		{
			if (string.IsNullOrEmpty(folderPath))
			{
				throw new FileException(null, SiteStrings.FileManager_Error_DeleteFolder);
			}

			string physicalPath = _attachmentPathUtil.ConvertUrlPathToPhysicalPath(folderPath);

			if (!_attachmentPathUtil.IsAttachmentPathValid(physicalPath, false))
			{
				throw new SecurityException(null, "Attachment path was invalid when deleting the folder {0}", folderPath);
			}

			try
			{
				DirectoryInfo info = new DirectoryInfo(physicalPath);

				if (info.Exists && info.GetDirectories().Length == 0 && info.GetFiles().Length == 0)
				{
					info.Delete();
				}
				else
				{
					throw new FileException("The folder is not empty.", null);
				}
			}
			catch (IOException e)
			{
				throw new FileException(e, "Unable to delete {0} from {1}", folderPath);
			}
		}

		public bool CreateFolder(string parentPath, string folderName)
		{
			var physicalPath = _attachmentPathUtil.ConvertUrlPathToPhysicalPath(parentPath);

			if (!_attachmentPathUtil.IsAttachmentPathValid(physicalPath))
			{
				throw new SecurityException(null, "Attachment path was invalid when creating folder {0}", folderName);
			}

			var newPath = Path.Combine(physicalPath, folderName);

			if (!Directory.Exists(newPath)){
				Directory.CreateDirectory(newPath);
				return true;
			}
			throw new FileException(SiteStrings.FileManager_Error_CreateFolder + " " + folderName, null);
		}

		public DirectoryViewModel FolderInfo(string dir)
		{
			if (!Directory.Exists(_applicationSettings.AttachmentsDirectoryPath))
			{
				throw new SecurityException("The attachments directory does not exist - please create it.",null);
			}

			string folder = dir;
			folder = HttpUtility.UrlDecode(folder);

			string physicalPath = _attachmentPathUtil.ConvertUrlPathToPhysicalPath(folder);

			if (!_attachmentPathUtil.IsAttachmentPathValid(physicalPath))
			{
				throw new SecurityException(null, "Attachment path was invalid when getting the folder {0}", dir);
			}

			try
			{
				string currentFolderName = dir;
				if (!string.IsNullOrEmpty(currentFolderName) && currentFolderName != "/")
					currentFolderName = Path.GetFileName(dir);

				DirectoryViewModel directoryModel = new DirectoryViewModel(currentFolderName, dir);
				if (Directory.Exists(physicalPath))
				{
					foreach (string directory in Directory.GetDirectories(physicalPath))
					{
						DirectoryInfo info = new DirectoryInfo(directory);
						string fullPath = info.FullName;
						fullPath = fullPath.Replace(_applicationSettings.AttachmentsDirectoryPath, "");
						fullPath = fullPath.Replace(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture), "/");
						fullPath = "/" + fullPath; // removed in the 1st replace

						DirectoryViewModel childModel = new DirectoryViewModel(info.Name, fullPath);
						directoryModel.ChildFolders.Add(childModel);
					}

					foreach (string file in Directory.GetFiles(physicalPath))
					{

						FileInfo info = new FileInfo(file);
						string filename = info.Name;

						if (!FilesToExclude.Contains(info.Name))
						{
							string urlPath = Path.Combine(dir, filename);
							FileViewModel fileModel = new FileViewModel(info.Name, info.Extension.Replace(".", ""), info.Length, info.CreationTime, dir);
							directoryModel.Files.Add(fileModel);
						}
					}
				}

				return directoryModel;
			}
			catch (IOException e)
			{
				throw new FileException("An unhandled error occurred: " + e.Message, e);
			}
		}

		public string Upload(string destination, HttpFileCollectionBase files)
		{
			//string destination = Request.Form["destination_folder"];
			string physicalPath = _attachmentPathUtil.ConvertUrlPathToPhysicalPath(destination);

			if (!_attachmentPathUtil.IsAttachmentPathValid(physicalPath))
			{
				throw new SecurityException("Attachment path was invalid when uploading.", null);
			}

			try
			{
				// Get the allowed files types
				string fileName = "";

				// For checking the setting to overwrite existing files
				SiteSettings siteSettings = _settingsService.GetSiteSettings();
				IEnumerable<string> allowedExtensions = siteSettings.AllowedFileTypesList
													.Select(x => x.ToLower());

				for (int i = 0; i < files.Count; i++)
				{
					// Find the file's extension
					HttpPostedFileBase sourceFile = files[i];
					string extension = Path.GetExtension(sourceFile.FileName).Replace(".", "");

					if (!string.IsNullOrEmpty(extension))
						extension = extension.ToLower();

					// Check if it's an allowed extension
					if (allowedExtensions.Contains(extension))
					{
						string fullFilePath = Path.Combine(physicalPath, sourceFile.FileName);

						// Check if it exists on disk already
						if (!siteSettings.OverwriteExistingFiles)
						{
							if (System.IO.File.Exists(fullFilePath))
							{
								string errorMessage = string.Format(SiteStrings.FileManager_Upload_FileAlreadyExists, sourceFile.FileName);
								throw new FileException(errorMessage, null);
							}
						}

						sourceFile.SaveAs(fullFilePath);
						fileName = sourceFile.FileName;
					}
					else
					{
						string allowedExtensionsCsv = string.Join(",", allowedExtensions);
						string errorMessage = string.Format(SiteStrings.FileManager_Extension_Not_Supported, allowedExtensionsCsv);
						throw new FileException(errorMessage,null);
					}
				}

				return fileName;
			}
			catch (IOException e)
			{
				throw new FileException(e.Message,e);
			}
		}

		#endregion
	}
}




