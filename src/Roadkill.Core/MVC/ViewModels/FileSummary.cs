using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;

namespace Roadkill.Core
{
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
		public FileSummary(string diskPath, ApplicationSettings settings)
		{
			Name = Path.GetFileName(diskPath);
			DiskPath = diskPath;
			UrlPath = diskPath.Replace(settings.AttachmentsDirectoryPath, "");
			UrlPath = UrlPath.Replace(@"\", "/");
			Extension = Path.GetExtension(diskPath).Replace(".", "");
		}

		public static FileSummary FromBase64UrlPath(string base64Path, ApplicationSettings settings)
		{
			string path = "";

			if (!string.IsNullOrEmpty(base64Path))
				path = base64Path.FromBase64();

			path = settings.AttachmentsDirectoryPath + path;

			FileSummary summary = new FileSummary(path, settings);
			return summary;
		}
	}
}
