using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using Roadkill.Core.Configuration;
using Roadkill.Core.Localization.Resx;
using Roadkill.Core.Files;

namespace Roadkill.Core.Controllers
{
    /// <summary>
    /// Provides file manager functionality for wiki page editing.
    /// </summary>
    public class FileManagerController : ControllerBase
    {
        /// <summary>
        /// Constructor for the file manager.
        /// </summary>
        /// <remarks>This action requires editor rights.</remarks>
        public FileManagerController(IConfigurationContainer configuration, UserManager userManager, IRoadkillContext context)
			: base(configuration, userManager, context) 
		{
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
        /// <param name="filePath">A file path including the filename, which is a relative URL.</param>
        /// <remarks>This action requires editor rights.</remarks>
        [EditorRequired]
        [HttpPost]
        public JsonResult DeleteFile(string filePath, string fileName)
        {
            try
            {
                string path = Path.Combine(AttachmentFileHandler.CombineAbsoluteAttachmentsFolder(Configuration, filePath), fileName);

                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

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
                if (folder == "" || folder == "/Attachments")
                {
                    return Json(new { status = "error", message = SiteStrings.FileManager_Error_BaseFolderDelete });
                }

                string fullPath = AttachmentFileHandler.CombineAbsoluteAttachmentsFolder(Configuration, folder);

                var info = new DirectoryInfo(fullPath);

                if (info.Exists && info.GetDirectories().Length == 0 && info.GetFiles().Length == 0)
                {
                    info.Delete();
                }
                else
                {
                    return Json(new { status = "error", message = SiteStrings.FileManager_Error_FolderDelete });
                }

                return Json(new { status = "ok", message = "" });
            }
            catch (IOException e)
            {
                return Json(new { status = "error", message = e.Message });
            }

        }

        /// <summary>
        /// Displays a HTML representation of a folder, including files that
        /// can be used with the jQuery File Tree plugin.
        /// </summary>
        /// <param name="dir">The current directory to display</param>
        /// <remarks>This action requires editor rights.</remarks>
        //[EditorRequired]
        //public ActionResult Folder(string dir)
        //{
        //    string folder = dir;
        //    folder = Server.UrlDecode(folder);

        //    if (string.IsNullOrEmpty(folder) || folder == "/")
        //    {
        //        // GetFilesAndFolders expects a base64 encoded folder, so as wacky as it looks
        //        // we base64 the attachments folder, so it can decode it correctly
        //        folder = folder.ToBase64();
        //    }

        //    return PartialView(GetFilesAndFolders(folder));
        //}

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

            if (string.IsNullOrEmpty(folder) || folder == "/")
            {
                // GetFilesAndFolders expects a base64 encoded folder, so as wacky as it looks
                // we base64 the attachments folder, so it can decode it correctly
                folder = folder.ToBase64();
            }

            return Json(GetFilesAndFolders(folder));
        }


        /// <summary>
        /// Returns a plain text string containing the full attachments URL path.
        /// </summary>
        /// <param name="id">A Base64 representation of a path relative to the Attachments folder.</param>
        /// <remarks>This action requires editor rights.</remarks>
        //[EditorRequired]
        //public ActionResult GetPath(string id)
        //{
        //    FileSummary summary = FileSummary.FromBase64UrlPath(id, Configuration);
        //    return Content(summary.UrlPath);
        //}

        /// <summary>
        /// Gets a lists of all files and folders (as a <see cref="DirectorySummary"/> for the relative path provided.
        /// </summary>
        private DirectorySummary GetFilesAndFolders(string folder)
        {
            DirectorySummary summary = DirectorySummary.FromBase64UrlPath(Configuration, folder);
            string fullPath = summary.DiskPath;

            if (Directory.Exists(fullPath))
            {
                foreach (string item in Directory.GetDirectories(fullPath))
                {
                    summary.ChildFolders.Add(new DirectorySummary(Configuration, item));
                }
                foreach (string item in Directory.GetFiles(fullPath))
                {
                    summary.Files.Add(new FileSummary(item, Configuration));
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
        public JsonResult NewFolder(string currentFolderPath, string newFolderName)
        {
            try
            {
                if (currentFolderPath.StartsWith("/Attachments"))
                {
                    currentFolderPath = currentFolderPath.Replace("/Attachments", "");
                }
                currentFolderPath = currentFolderPath.ToBase64();
                DirectorySummary summary = DirectorySummary.FromBase64UrlPath(Configuration, currentFolderPath);
                string newPath = string.Format("{0}\\{1}", summary.DiskPath, newFolderName);
                if (!Directory.Exists(newPath))
                    Directory.CreateDirectory(newPath);
                else
                    return Json(new { status = "error", message = String.Format(SiteStrings.FileManager_Error_AddFolder, newFolderName) });
            }
            catch (Exception e)
            {
                return Json(new { status = "error", message = e.Message });
            }

            return Json(new { status = "ok", FolderName = newFolderName, SafePath = newFolderName.ToBase64() });
        }

        /// <summary>
        /// Attempts to upload a file provided by the 'uploadFile' POST var.
        /// </summary>
        /// <param name="currentUploadFolderPath">The current path relative to the attachments folder to save to.</param>
        /// <returns>Redirects to the Index action. If an error occurred the TempData["Error"] object contains the message.</returns>
        /// <remarks>This action requires editor rights.</remarks>
        [EditorRequired]
        [HttpPost]
        public JsonResult FileUpload()
        {
            List<object> filesList = new List<object>();
            HttpPostedFileBase SourceFile;
            string destinationFolder;
            string physicalPath;
            string fullFilePath;

            try
            {
                destinationFolder = Request.Form["destination_folder"];

                if (destinationFolder == "/Attachments")
                {
                    physicalPath = Configuration.ApplicationSettings.AttachmentsFolder;
                }
                else
                {
                    destinationFolder = destinationFolder.Replace("/Attachments/", "");
                    destinationFolder = destinationFolder.Replace("/", @"\");
                    physicalPath = Path.GetFullPath(Path.Combine(Configuration.ApplicationSettings.AttachmentsFolder, destinationFolder));
                }

                for (int i = 0; i < Request.Files.Count; i++)
                {
                    SourceFile = Request.Files[i];
                    fullFilePath = Path.Combine(physicalPath, SourceFile.FileName);
                    SourceFile.SaveAs(fullFilePath);

                    filesList.Add(new FileSummary(fullFilePath, Configuration));

                }

                return Json(new { status = "ok", files = filesList }, "text/plain");
            }
            catch (Exception e)
            {
                return Json(new { status = "error", message = e.Message }, "text/plain");
            }
        }

        //[EditorRequired]
        //public ActionResult Select()
        //{
        //    return View();
        //}

    }
}
