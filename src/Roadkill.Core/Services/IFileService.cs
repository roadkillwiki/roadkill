using Roadkill.Core.Attachments;
using Roadkill.Core.Mvc.ViewModels;
using System.Web;

namespace Roadkill.Core.Services
{
	public interface IFileService
	{
		void Delete(string filePath, string fileName);
		void DeleteFolder(string folderPath);
		bool CreateFolder(string parentPath, string folderName);
		DirectoryViewModel FolderInfo(string dir);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="destinationPath">The relative path of the folder to store the file.</param>
		/// <param name="files"></param>
		/// <returns></returns>
		string Upload(string destinationPath, HttpFileCollectionBase files);

		void WriteResponse(string localPath, string applicationPath, string modifiedSinceHeader, IResponseWrapper responseWrapper, HttpContext context);
	}
}
