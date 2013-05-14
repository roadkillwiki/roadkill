using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Mvc.ViewModels
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
		/// The virtual path of the directory, e.g. /folder1/folder2
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectorySummary"/> class.
		/// </summary>
		/// <param name="name">The directory name.</param>
		public DirectorySummary(string name)
		{
			Name = name;
			Path = "";
			Files = new List<FileSummary>();
			ChildFolders = new List<DirectorySummary>();
		}
	}
}
