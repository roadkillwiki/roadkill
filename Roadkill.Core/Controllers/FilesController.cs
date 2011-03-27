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

namespace Roadkill.Core.Controllers
{
	public class FilesController : ControllerBase
    {
		[EditorRequired]
		public ActionResult Index(string id)
		{
			return View();
		}

		[EditorRequired]
		public ActionResult GetPath(string id)
		{
			FileSummary summary = FileSummary.FromBase64UrlPath(id);
			return Content(summary.UrlPath);
		}

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

					if (!Directory.Exists(Server.MapPath(RoadkillSettings.AttachmentsFolder)))
						Directory.CreateDirectory(Server.MapPath(RoadkillSettings.AttachmentsFolder));

					string filePath = string.Format(@"{0}\{1}", summary.DiskPath, filename);
					HttpPostedFileBase postedFile = Request.Files["uploadFile"] as HttpPostedFileBase;
					postedFile.SaveAs(filePath);
				}
				catch (Exception e)
				{
					TempData["Error"] = string.Format("An error occured uploading the file: {0}", e.Message);
				}
			}
			else
			{
				TempData["Error"] = string.Format("Files with the extension '{0}' are not allowed to be uploaded", extension);
			}
			

			return RedirectToAction("Index");
		}

		[EditorRequired]
		[HttpPost]
		public ActionResult NewFolder(string currentFolderPath,string newFolderName)
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
				TempData["Error"] = string.Format("An error occured creating the directory: {0}",e.Message);
			}

			return RedirectToAction("Index");
		}

		[EditorRequired]
		[HttpPost]
		public ActionResult DeleteFile(string filePath)
		{
			try
			{
				string folder = Server.MapPath(RoadkillSettings.AttachmentsFolder);
				string path = string.Format(@"{0}\{1}", folder, filePath);

				if (System.IO.File.Exists(path))
					System.IO.File.Delete(path);
			}
			catch (IOException e)
			{
				TempData["Error"] = string.Format("An error occured deleting the file: {0}",e.Message);
			}

			return RedirectToAction("Index");
		}
    }
}
