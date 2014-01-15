using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;
using Roadkill.Core.Configuration;
using Roadkill.Core.Localization;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using Roadkill.Core.Attachments;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Provides file manager functionality for wiki page editing.
	/// </summary>
	[EditorRequired]
	public class FileManagerController : ControllerBase
	{
		private AttachmentFileHandler _attachmentHandler;
		private AttachmentPathUtil _attachmentPathUtil;
		private static string[] _filesToExclude = new string[] { "emptyfile.txt", "_installtest.txt" }; // installer/publish files

		/// <summary>
		/// Constructor for the file manager.
		/// </summary>
		/// <remarks>This action requires editor rights.</remarks>
		public FileManagerController(ApplicationSettings settings, UserServiceBase userManager, IUserContext context,
			SettingsService settingsService, AttachmentFileHandler attachment)
			: base(settings, userManager, context, settingsService)
		{
			_attachmentHandler = attachment;
			_attachmentPathUtil = new AttachmentPathUtil(settings);
		}

		/// <summary>
		/// Displays the default file manager.
		/// </summary>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		public ActionResult Index()
		{
			return View();
		}

		/// <summary>
		/// Attempts to delete a file in the attachments folder.
		/// </summary>
		/// <param name="filePath">A relative file path that contains the file to be deleted.</param>
		/// <param name="fileName">The name of the file to be deleted.</param>
		/// <remarks>This action requires editor rights.</remarks>
		[AdminRequired]
		[HttpPost]
		public JsonResult DeleteFile(string filePath, string fileName)
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

				return Json(new { status = "ok", message = "" });
			}
			catch (IOException e)
			{
				return Json(new { status = "error", message = e.Message });
			}
		}

		/// <summary>
		/// Attempts to delete a child Folder of the Attachments folder.
		/// </summary>
		/// <param name="folder">A Folder name, which is a relative URL and contains full path from Attachments folder.</param>
		/// <remarks>This action requires editor rights.</remarks>
		[AdminRequired]
		[HttpPost]
		public JsonResult DeleteFolder(string folder)
		{
			if (string.IsNullOrEmpty(folder))
			{
				return Json(new { status = "error", message = SiteStrings.FileManager_Error_DeleteFolder });
			}

			string physicalPath = _attachmentPathUtil.ConvertUrlPathToPhysicalPath(folder);

			if (!_attachmentPathUtil.IsAttachmentPathValid(physicalPath, false))
			{
				throw new SecurityException(null, "Attachment path was invalid when deleting the folder {0}", folder);
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
					return Json(new { status = "error", message = "The folder is not empty." });
				}

				return Json(new { status = "ok", message = "" });
			}
			catch (IOException e)
			{
				return Json(new { status = "error", message = e.Message });
			}
		}

		/// <summary>
		/// Returns a JSON representation of a DirectoryViewModel object, including files that
		/// can be used with the jQuery to display in one or more formats, i.e. list or detail views.
		/// </summary>
		/// <param name="dir">The current directory to display</param>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		[HttpPost]
		public ActionResult FolderInfo(string dir)
		{
			if (!Directory.Exists(ApplicationSettings.AttachmentsDirectoryPath))
				return Json(new { status = "error", message = "The attachments directory does not exist - please create it." });

			string folder = dir;
			folder = Server.UrlDecode(folder);

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
						fullPath = fullPath.Replace(ApplicationSettings.AttachmentsDirectoryPath, "");
						fullPath = fullPath.Replace(Path.DirectorySeparatorChar.ToString(), "/");
						fullPath = "/" + fullPath; // removed in the 1st replace

						DirectoryViewModel childModel = new DirectoryViewModel(info.Name, fullPath);
						directoryModel.ChildFolders.Add(childModel);
					}

					foreach (string file in Directory.GetFiles(physicalPath))
					{

						FileInfo info = new FileInfo(file);
						string filename = info.Name;

						if (!_filesToExclude.Contains(info.Name))
						{
							string urlPath = Path.Combine(dir, filename);
							FileViewModel fileModel = new FileViewModel(info.Name, info.Extension.Replace(".", ""), info.Length, info.CreationTime, dir);
							directoryModel.Files.Add(fileModel);
						}
					}
				}

				return Json(directoryModel);
			}
			catch (IOException e)
			{
				return Json(new { status = "error", message = "An unhandled error occurred: "+e.Message });
			}
		}

		/// <summary>
		/// Attempts to create a new folder in the attachments folder, using the relative path provided.
		/// </summary>
		/// <param name="currentFolderPath">The path relative to the attachments folder to add to.</param>
		/// <param name="newFolderName">The new folder name to create.</param>
		/// <returns>Redirects to the Index action. If an error occurred the TempData["Error"] object contains the message.</returns>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		[HttpPost]
		public JsonResult NewFolder(string currentFolderPath, string newFolderName)
		{
			var physicalPath = _attachmentPathUtil.ConvertUrlPathToPhysicalPath(currentFolderPath);

			if (!_attachmentPathUtil.IsAttachmentPathValid(physicalPath))
			{
				throw new SecurityException(null, "Attachment path was invalid when creating folder {0}", newFolderName);
			}

			try
			{
				var newPath = Path.Combine(physicalPath, newFolderName);

				if (!Directory.Exists(newPath))
					Directory.CreateDirectory(newPath);
				else
					return Json(new { status = "error", message = SiteStrings.FileManager_Error_CreateFolder + " " + newFolderName });
			}
			catch (Exception e)
			{
				return Json(new { status = "error", message = e.Message });
			}

			return Json(new { status = "ok", FolderName = newFolderName });
		}

		/// <summary>
		/// Attempts to upload a file provided.
		/// </summary>
		/// <returns>Returns Json object containing status and either message or fileList.</returns>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		[HttpPost]
		public JsonResult Upload()
		{
			string destinationFolder = Request.Form["destination_folder"];
			string physicalPath = _attachmentPathUtil.ConvertUrlPathToPhysicalPath(destinationFolder);

			if (!_attachmentPathUtil.IsAttachmentPathValid(physicalPath))
			{
				throw new SecurityException("Attachment path was invalid when uploading.", null);
			}

			try
			{
				// Get the allowed files types
				string fileName = "";
				IEnumerable<string> allowedExtensions = SettingsService.GetSiteSettings().AllowedFileTypesList
													.Select(x => x.ToLower());

				// For checking the setting to overwrite existing files
				SiteSettings siteSettings = SettingsService.GetSiteSettings();

				for (int i = 0; i < Request.Files.Count; i++)
				{
					// Find the file's extension
					HttpPostedFileBase sourceFile = Request.Files[i];
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
								 return Json(new { status = "error", message = errorMessage }, "text/plain");
							 }
						}

						sourceFile.SaveAs(fullFilePath);
						fileName = sourceFile.FileName;
					}
					else
					{
						string allowedExtensionsCsv = string.Join(",", allowedExtensions);
						string errorMessage = string.Format(SiteStrings.FileManager_Extension_Not_Supported, allowedExtensionsCsv);
						
						return Json(new { status = "error", message = errorMessage }, "text/plain");
					}
				}

				return Json(new { status = "ok", filename = fileName }, "text/plain");
			}
			catch (Exception e)
			{
				return Json(new { status = "error", message = e.Message }, "text/plain");
			}
		}

		/// <summary>
		/// Provides a File Navigator view in order to select a file.
		/// </summary>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		public ActionResult Select()
		{
			return View();
		}

		protected override void OnException(ExceptionContext filterContext)
		{
			base.OnException(filterContext);

			Console.WriteLine(filterContext.Exception);
		}
	}
}
