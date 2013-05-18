using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using Roadkill.Core.Configuration;
using Roadkill.Core.Localization.Resx;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;
using Roadkill.Core.Managers;
using Roadkill.Core.Attachments;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Provides file manager functionality for wiki page editing.
	/// </summary>
	[AdminRequired]
	public class FileManagerController : ControllerBase
	{
		private AttachmentFileHandler _attachmentHandler;

		/// <summary>
		/// Constructor for the file manager.
		/// </summary>
		/// <remarks>This action requires editor rights.</remarks>
		public FileManagerController(ApplicationSettings settings, UserManagerBase userManager, IUserContext context, SettingsManager siteSettingsManager, AttachmentFileHandler attachment)
			: base(settings, userManager, context, siteSettingsManager)
		{
			_attachmentHandler = attachment;
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
		[EditorRequired]
		[HttpPost]
		public JsonResult DeleteFile(string filePath, string fileName)
		{
			try
			{
				string physicalPath = _attachmentHandler.ConvertUrlPathToPhysicalPath(filePath);
				string physicalFilePath = Path.Combine(physicalPath, fileName);

				if (!_attachmentHandler.IsAttachmentPathValid(physicalPath))
				{
					throw new SecurityException("Attachment Path invalid for DeleteFile", null);
				}

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
		[EditorRequired]
		[HttpPost]
		public JsonResult DeleteFolder(string folder)
		{
			try
			{
				if (string.IsNullOrEmpty(folder))
				{
					return Json(new { status = "error", message = "SiteStrings.FileManager_Error_BaseFolderDelete" });
				}

				string physicalPath = _attachmentHandler.ConvertUrlPathToPhysicalPath(folder);

				if (!_attachmentHandler.IsAttachmentPathValid(physicalPath, false))
				{
					throw new SecurityException("Attachment Path invalid", null);
				}

				var info = new DirectoryInfo(physicalPath);

				if (info.Exists && info.GetDirectories().Length == 0 && info.GetFiles().Length == 0)
				{
					info.Delete();
				}
				else
				{
					return Json(new { status = "error", message = "SiteStrings.FileManager_Error_FolderDelete" });
				}

				return Json(new { status = "ok", message = "" });
			}
			catch (IOException e)
			{
				return Json(new { status = "error", message = e.Message });
			}
		}

		/// <summary>
		/// Returns a JSON representation of a DirectorySummary object, including files that
		/// can be used with the jQuery to display in one or more formats, i.e. list or detail views.
		/// </summary>
		/// <param name="dir">The current directory to display</param>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		public ActionResult FolderInfo(string dir)
		{
			string folder = dir;
			folder = Server.UrlDecode(folder);

			string physicalPath = _attachmentHandler.ConvertUrlPathToPhysicalPath(folder);

			if (!_attachmentHandler.IsAttachmentPathValid(physicalPath))
			{
				throw new Exception("Attachment Path invalid");
			}

			string currentFolderName = dir;
			if (!string.IsNullOrEmpty(currentFolderName) && currentFolderName != "/")
				currentFolderName = Path.GetFileName(dir);

			DirectorySummary summary = new DirectorySummary(currentFolderName, dir);
			if (Directory.Exists(physicalPath))
			{
				foreach (string directory in Directory.GetDirectories(physicalPath))
				{
					DirectoryInfo info = new DirectoryInfo(directory);
					string fullPath = info.FullName;
					fullPath = fullPath.Replace(ApplicationSettings.AttachmentsDirectoryPath, "");
					fullPath = fullPath.Replace(Path.DirectorySeparatorChar.ToString(), "/");
					fullPath = "/" + fullPath; // removed in the 1st replace

					DirectorySummary childSummary = new DirectorySummary(info.Name, fullPath);
					summary.ChildFolders.Add(childSummary);
				}

				foreach (string file in Directory.GetFiles(physicalPath))
				{
					FileInfo info = new FileInfo(file);
					string filename = info.Name;
					string urlPath = Path.Combine(dir, filename);
					FileSummary fileSummary = new FileSummary(info.Name, info.Extension.Replace(".",""), info.Length, info.CreationTime, dir, "");
					summary.Files.Add(fileSummary);
				}
			}

			return Json(summary);
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
			try
			{
				var physicalPath = _attachmentHandler.ConvertUrlPathToPhysicalPath(currentFolderPath);

				if (!_attachmentHandler.IsAttachmentPathValid(physicalPath))
				{
					throw new Exception("Attachment Path invalid");
				}

				var newPath = Path.Combine(physicalPath, newFolderName);

				if (!Directory.Exists(newPath))
					Directory.CreateDirectory(newPath);
				else
					return Json(new { status = "error", message = String.Format("SiteStrings.FileManager_Error_AddFolder", newFolderName) });
			}
			catch (Exception e)
			{
				return Json(new { status = "error", message = e.Message });
			}

			return Json(new { status = "ok", FolderName = newFolderName, SafePath = newFolderName.ToBase64() });
		}

		/// <summary>
		/// Attempts to upload a file provided.
		/// </summary>
		/// <returns>Returns Json object containing status and either message or fileList.</returns>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		[HttpPost]
		public JsonResult FileUpload()
		{
			try
			{
				string destinationFolder = Request.Form["destination_folder"];
				string physicalPath = _attachmentHandler.ConvertUrlPathToPhysicalPath(destinationFolder);

				if (!_attachmentHandler.IsAttachmentPathValid(physicalPath))
				{
					throw new Exception("Attachment Path invalid");
				}

				string fileName = "";
				for (int i = 0; i < Request.Files.Count; i++)
				{
					HttpPostedFileBase sourceFile = Request.Files[i];
					string fullFilePath = Path.Combine(physicalPath, sourceFile.FileName);
					sourceFile.SaveAs(fullFilePath);

					fileName = sourceFile.FileName;
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
	}
}
