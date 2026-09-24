using Roadkill.Core.Attachments;
using Roadkill.Core.Mvc.ViewModels;
using Microsoft.AspNetCore.Http;

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
		string Upload(string destinationPath, IFormFileCollection files);

		/// <summary>
		/// Writes the file for the attachment url to the response.
		/// </summary>
		/// <exception cref="Roadkill.Core.Exceptions.HttpStatusException">The file doesn't exist (404) or can't be read (500).</exception>
		void WriteResponse(string localPath, string applicationPath, string modifiedSinceHeader, IResponseWrapper responseWrapper);
	}
}
