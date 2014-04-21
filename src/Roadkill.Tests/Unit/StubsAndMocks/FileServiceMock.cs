using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Roadkill.Core.Attachments;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class FileServiceMock : IFileService
	{
		// This class is a candidate for Moq

		/// <summary>
		/// If this property is set, all methods called will throw it.
		/// </summary>
		public Exception CustomException { get; set; }

		public void Delete(string filePath, string fileName)
		{
			if (CustomException != null)
				throw CustomException;
		}

		public void DeleteFolder(string folderPath)
		{
			if (CustomException != null)
				throw CustomException;
		}

		public bool CreateFolder(string parentPath, string folderName)
		{
			if (CustomException != null)
				throw CustomException;

			return true;
		}

		public DirectoryViewModel FolderInfo(string dir)
		{
			if (CustomException != null)
				throw CustomException;

			DirectoryViewModel model = new DirectoryViewModel(dir, dir+ "urlpath");
			model.Files.Add(new FileViewModel("file1.txt", "txt", 12345, DateTime.UtcNow, ""));
			model.ChildFolders.Add(new DirectoryViewModel("child", "childurlpath"));

			return model;
		}

		public string Upload(string destinationPath, HttpFileCollectionBase files)
		{
			if (CustomException != null)
				throw CustomException;

			if (files.Count > 0)
				return files[files.Count - 1].FileName;
			else
				return "";
		}

		public void WriteResponse(string localPath, string applicationPath, string modifiedSinceHeader, IResponseWrapper responseWrapper, HttpContext context)
		{
			if (CustomException != null)
				throw CustomException;
		}
	}
}
