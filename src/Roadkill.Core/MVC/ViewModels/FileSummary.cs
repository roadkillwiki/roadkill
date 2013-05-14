using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Mvc.ViewModels
{
	/// <summary>
	/// A single file in a folder in the attachments folder.
	/// </summary>
	public class FileSummary
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public string Extension { get; set; }
		public long Size { get; set; }
		public string CreateDate { get; set; }
		public string Folder { get; set; }

		/// <summary>
		/// The full url path, e.g. /Attachments/Folder1/mypic.png.
		/// </summary>
		public string FullPath { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FileSummary"/> class.
		/// </summary>
		/// <param name="relativePath">The relative path of the filename</param>
		public FileSummary(string name, string directoryPath)
		{
			Name = name;

			//FileInfo fileInfo = new FileInfo(relativePath);
			//Size = fileInfo.Length;
			//CreateDate = fileInfo.CreationTime.ToShortDateString();
			//Folder = fileInfo.Directory.Name;
		}
	}
}
