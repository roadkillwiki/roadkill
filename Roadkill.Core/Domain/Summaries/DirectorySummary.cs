using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Roadkill.Core.Configuration;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides directory information for a folder in the attachments folder.
	/// </summary>
	public class DirectorySummary
	{
		/// <summary>
		/// All files in the directory
		/// </summary>
		public List<FileSummary> Files { get; set; }

		/// <summary>
		/// All child folders in the directory.
		/// </summary>
		public List<DirectorySummary> ChildFolders { get; set; }

		/// <summary>
		/// The name of the folder.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The url path, e.g. /Attachments/Folder1/ab.jpg
		/// </summary>
		public string UrlPath { get; set; }

		/// <summary>
		/// A full filesystem path to the directory, including the attachments portion of the filepath.
		/// </summary>
		public string DiskPath { get; set; }

		/// <summary>
		/// Base64'd version of the path
		/// </summary>
		public string SafePath
		{
			get
			{
				return UrlPath.ToBase64();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectorySummary"/> class.
		/// </summary>
		/// <param name="diskPath">A full disk path, including the attachments folder.</param>
		public DirectorySummary(IConfigurationContainer config, string diskPath)
		{
			Name = Path.GetFileName(diskPath);
			DiskPath = diskPath;
			UrlPath = DiskPath.Replace(config.ApplicationSettings.AttachmentsFolder, "");
			UrlPath = UrlPath.Replace(@"\", "/");

			Files = new List<FileSummary>();
			ChildFolders = new List<DirectorySummary>();
		}

		public static DirectorySummary FromBase64UrlPath(IConfigurationContainer config, string base64Path)
		{
			string path = "";

			if (!string.IsNullOrEmpty(base64Path))
				path = base64Path.FromBase64();

			path = config.ApplicationSettings.AttachmentsFolder + path;

			DirectorySummary summary = new DirectorySummary(config, path);
			return summary;
		}
	}

	/// <summary>
	/// A single file in a folder in the attachments folder.
	/// </summary>
	public class FileSummary
	{
		public string Name { get; set; }
		public string DiskPath { get; set; }
		public string UrlPath { get; set; }
		public string Extension { get; set; }

		/// <summary>
		/// Base64'd version of the path
		/// </summary>
		public string SafePath
		{
			get
			{
				return UrlPath.ToBase64();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FileSummary"/> class.
		/// </summary>
		/// <param name="diskPath">A full disk path, including the attachments folder.</param>
		public FileSummary(string diskPath, IConfigurationContainer config)
		{
			Name = Path.GetFileName(diskPath);
			DiskPath = diskPath;
			UrlPath = diskPath.Replace(config.ApplicationSettings.AttachmentsFolder, "");
			UrlPath = UrlPath.Replace(@"\", "/");
			Extension = Path.GetExtension(diskPath).Replace(".", "");
		}

		public static FileSummary FromBase64UrlPath(string base64Path, IConfigurationContainer config)
		{
			string path = "";

			if (!string.IsNullOrEmpty(base64Path))
				path = base64Path.FromBase64();

			path = config.ApplicationSettings.AttachmentsFolder + path;

			FileSummary summary = new FileSummary(path, config);
			return summary;
		}
	}
}
