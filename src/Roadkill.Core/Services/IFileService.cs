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
		string Upload(string destination, HttpFileCollectionBase files);
		void WriteResponse(string localPath, string applicationPath, string modifiedSinceHeader, IResponseWrapper responseWrapper, HttpContext context);
	}
}
