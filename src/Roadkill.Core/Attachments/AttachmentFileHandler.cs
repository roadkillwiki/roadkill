using Roadkill.Core.Configuration;
using Roadkill.Core.Services;
using System.IO;
using System.Web;

namespace Roadkill.Core.Attachments
{
	/// <summary>
	/// A <see cref="IHttpHandler"/> that serves all uploaded files.
	/// </summary>
	public class AttachmentFileHandler : IHttpHandler
	{
		private ApplicationSettings _settings;
		private readonly IFileService _fileService;

		public bool IsReusable
		{
			get { return true; }
		}

		public AttachmentFileHandler(ApplicationSettings settings, IFileService fileService)
		{
			_settings = settings;
			_fileService = fileService;
		}

		/// <summary>
		/// IHttpHandler implementation.
		/// </summary>
		/// <param name="context"></param>
		public void ProcessRequest(HttpContext context)
		{
			ResponseWrapper wrapper = new ResponseWrapper(new HttpResponseWrapper(context.Response));

			WriteResponse(context.Request.Url.LocalPath, 
							context.Request.ApplicationPath, 
							context.Request.Headers["If-Modified-Since"], 
							wrapper, context);
		}

		/// <summary>
		/// Writes out a status code, cache response and response (binary or text) for the 
		/// localpath file request.
		/// </summary>
		/// <param name="localPath">The request's local path, which is a url such as /Attachments/foo.jpg</param>
		/// <param name="applicationPath">The application path e.g. /wiki/, if the app is running under one.
		/// If the app is running from the root then this will be "/".</param>
		/// <param name="url">The url for the request</param>
		/// <param name="modifiedSinceHeader">The modified since header (or null if there isn't one) - this is a date in ISO format.</param>
		/// <param name="responseWrapper">A wrapper for the HttpResponse object, to cater for ASP.NET being untestable.</param>
		public void WriteResponse(string localPath, string applicationPath, string modifiedSinceHeader, 
									IResponseWrapper responseWrapper, HttpContext context)
		{
			_fileService.WriteResponse( localPath,  applicationPath,  modifiedSinceHeader, 
									 responseWrapper, context);


		}

		public string TranslateUrlPathToFilePath(string urlPath, string applicationPath)
		{
			if (string.IsNullOrEmpty(urlPath))
				return "";

			if (!urlPath.StartsWith("/"))
				urlPath = "/" + urlPath;

			// Get rid of the route from the path
			// This replacement assumes the url is case sensitive (e.g. '/Attachments' is replaced, '/attachments' isn't)
			string filePath = urlPath.Replace(string.Format("/{0}", _settings.AttachmentsRoutePath), "");

			if (!string.IsNullOrEmpty(applicationPath) && applicationPath != "/" && filePath.StartsWith(applicationPath))
				filePath = filePath.Replace(applicationPath, "");

			// urlPath/LocalPath uses "/" and a Windows filepath is "\"
			// ignoring Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar for now.
			filePath = filePath.Replace('/', Path.DirectorySeparatorChar);

			// Get rid of the \ at the start of the file path
			if (filePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
				filePath = filePath.Remove(0, 1);

			// THe attachmentFolder has a trailing slash
			string fullPath = _settings.AttachmentsDirectoryPath + filePath;
			return fullPath;
		}
	}
}