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
	public class FileViewModel
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public string Extension { get; set; }
		public long Size { get; set; }
		public string CreateDate { get; set; }
		public string Folder { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FileViewModel"/> class.
		/// </summary>
		/// <param name="relativePath">The relative path of the filename</param>
		public FileViewModel(string name, string extension, long size, DateTime createDate, string folder)
		{
			Name = name;
			Extension = extension;
			Size = size;
			CreateDate = createDate.ToShortDateString();
			Folder = folder;
		}
	}
}
