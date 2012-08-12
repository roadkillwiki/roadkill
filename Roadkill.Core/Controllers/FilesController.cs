using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;
using System.Web.Management;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using Roadkill.Core.Localization.Resx;

namespace Roadkill.Core.Controllers
{
	/// <summary>
	/// Provides file manager functionality for wiki page editing.
	/// </summary>
	public class FilesController : ControllerBase
	{
		/// <summary>
		/// Displays the default file manager.
		/// </summary>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		public ActionResult Index(string id)
		{
			return View();
		}

		/// <summary>
		/// Attempts to delete a file in the attachments folder.
		/// </summary>
		/// <param name="filePath">A file path including the filename, which is a relative URL.</param>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		[HttpPost]
		public ActionResult DeleteFile(string filePath)
		{
			try
			{
				string folder = RoadkillSettings.AttachmentsFolder;
				string path = string.Format(@"{0}\{1}", folder, filePath);

				if (System.IO.File.Exists(path))
					System.IO.File.Delete(path);
			}
			catch (IOException e)
			{
				TempData["Error"] = string.Format(SiteStrings.FileExplorer_Error_DeleteFile, e.Message);
			}

			return RedirectToAction("Index");
		}

		/// <summary>
		/// Displays a HTML representation of a folder, including files that
		/// can be used with the jQuery File Tree plugin.
		/// </summary>
		/// <param name="dir">The current directory to display</param>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		public ActionResult Folder(string dir)
		{
			string folder = dir;
			folder = Server.UrlDecode(folder);

			if (string.IsNullOrEmpty(folder) || folder == "/")
			{
				// GetFilesAndFolders expects a base64 encoded folder, so as wacky as it looks
				// we base64 the attachments folder, so it can decode it correctly
				folder = folder.ToBase64();
			}

			return PartialView(GetFilesAndFolders(folder));
		}

		/// <summary>
		/// Returns a plain text string containing the full attachments URL path.
		/// </summary>
		/// <param name="id">A Base64 representation of a path relative to the Attachments folder.</param>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		public ActionResult GetPath(string id)
		{
			FileSummary summary = FileSummary.FromBase64UrlPath(id);
			return Content(summary.UrlPath);
		}	

		/// <summary>
		/// Gets a lists of all files and folders (as a <see cref="DirectorySummary"/> for the relative path provided.
		/// </summary>
		private DirectorySummary GetFilesAndFolders(string folder)
		{
			DirectorySummary summary = DirectorySummary.FromBase64UrlPath(folder);
			string fullPath = summary.DiskPath;

			if (Directory.Exists(fullPath))
			{
				foreach (string item in Directory.GetDirectories(fullPath))
				{
					summary.ChildFolders.Add(new DirectorySummary(item));
				}
				foreach (string item in Directory.GetFiles(fullPath))
				{
					summary.Files.Add(new FileSummary(item));
				}
			}

			return summary;
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
		public ActionResult NewFolder(string currentFolderPath, string newFolderName)
		{
			try
			{
				DirectorySummary summary = DirectorySummary.FromBase64UrlPath(currentFolderPath);
				string newPath = string.Format("{0}\\{1}", summary.DiskPath, newFolderName);
				if (!Directory.Exists(newPath))
					Directory.CreateDirectory(newPath);
			}
			catch (IOException e)
			{
				TempData["Error"] = string.Format(SiteStrings.FileExplorer_Error_NewDirectory, e.Message);
			}

			return RedirectToAction("Index");
		}

		/// <summary>
		/// Attempts to upload a file provided by the 'uploadFile' POST var.
		/// </summary>
		/// <param name="currentUploadFolderPath">The current path relative to the attachments folder to save to.</param>
		/// <returns>Redirects to the Index action. If an error occurred the TempData["Error"] object contains the message.</returns>
		/// <remarks>This action requires editor rights.</remarks>
		[EditorRequired]
		[HttpPost]
		public ActionResult UploadFile(string currentUploadFolderPath)
		{	
			string filename = Request.Files["uploadFile"].FileName;
			if (string.IsNullOrEmpty(filename))
				RedirectToAction("Index");

			string extension = Path.GetExtension(filename).Replace(".","");
	
			if (RoadkillSettings.AllowedFileTypes.Contains(extension))
			{
				try
				{
					DirectorySummary summary = DirectorySummary.FromBase64UrlPath(currentUploadFolderPath);

					if (!Directory.Exists(RoadkillSettings.AttachmentsFolder))
						Directory.CreateDirectory(RoadkillSettings.AttachmentsFolder);

					string filePath = string.Format(@"{0}\{1}", summary.DiskPath, filename);
					HttpPostedFileBase postedFile = Request.Files["uploadFile"] as HttpPostedFileBase;
					postedFile.SaveAs(filePath);
				}
				catch (Exception e)
				{
					TempData["Error"] = string.Format(SiteStrings.FileExplorer_Error_FileUpload, e.Message);
				}
			}
			else
			{
				TempData["Error"] = string.Format(SiteStrings.FileExplorer_Error_BadExtension, extension);
			}
			

			return RedirectToAction("Index");
		}		
	}
}
