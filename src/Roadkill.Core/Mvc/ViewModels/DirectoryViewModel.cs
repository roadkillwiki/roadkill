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
	public class DirectoryViewModel
	{
		/// <summary>
		/// All files in the directory
		/// </summary>
		public List<FileViewModel> Files { get; set; }

		/// <summary>
		/// All child folders in the directory.
		/// </summary>
		public List<DirectoryViewModel> ChildFolders { get; set; }

		/// <summary>
		/// The name of the folder.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The virtual path of the directory, e.g. /folder1/folder2
		/// </summary>
		public string UrlPath { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryViewModel"/> class.
		/// </summary>
		/// <param name="name">The directory name.</param>
		public DirectoryViewModel(string name, string urlPath)
		{
			Name = name;
			UrlPath = urlPath;
			Files = new List<FileViewModel>();
			ChildFolders = new List<DirectoryViewModel>();
		}
	}
}
