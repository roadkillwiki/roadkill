using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.Exceptions;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using System;
using System.Web.Mvc;

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
		private readonly IFileService _fileService;

		/// <summary>
		/// Constructor for the file manager.
		/// </summary>
		/// <remarks>This action requires editor rights.</remarks>
		public FileManagerController(ApplicationSettings settings, UserServiceBase userManager, IUserContext context,
			SettingsService settingsService, AttachmentFileHandler attachment, IFileService fileService)
			: base(settings, userManager, context, settingsService)
		{
			_attachmentHandler = attachment;
			_attachmentPathUtil = new AttachmentPathUtil(settings);
			_fileService = fileService;
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
			try
			{
				_fileService.Delete(filePath, fileName);
				return Json(new { status = "ok", message = "" });
			}
			catch (FileException e)
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
			try
			{
				_fileService.DeleteFolder(folder);
				return Json(new { status = "ok", message = "" });
			}
			catch (FileException e)
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
			try
			{
				return Json(_fileService.FolderInfo(dir));
			}
			catch (FileException e)
			{
				return Json(new { status = "error", message = e.Message });
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
			try
			{
				_fileService.CreateFolder(currentFolderPath, newFolderName);
				return Json(new { status = "ok", FolderName = newFolderName });
			}
			catch (FileException e)
			{
				return Json(new { status = "error", message = e.Message });
			}
		}

		/// <summary>
		/// Attempts to upload a file provided.
		/// </summary>
		/// <returns>Returns Json object containing status and the last uploaded file name.</returns>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		[HttpPost]
		public JsonResult Upload()
		{
			try
			{
				string destinationFolder = Request.Form["destination_folder"];
				string fileName = _fileService.Upload(destinationFolder, Request.Files);
				return Json(new { status = "ok", filename = fileName }, "text/plain");
			}
			catch (FileException e)
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
