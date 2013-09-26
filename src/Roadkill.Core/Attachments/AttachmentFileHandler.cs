using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.Web.Administration;
using System.IO;
using System.Web.Routing;
using System.Reflection;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;
using System.Collections.Specialized;

namespace Roadkill.Core.Attachments
{
	/// <summary>
	/// A <see cref="IHttpHandler"/> that serves all uploaded files.
	/// </summary>
	public class AttachmentFileHandler : IHttpHandler
	{
		private ApplicationSettings _settings;

		public bool IsReusable
		{
			get { return true; }
		}

		public AttachmentFileHandler(ApplicationSettings settings)
		{
			_settings = settings;
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
							wrapper);
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
									IResponseWrapper responseWrapper)
		{
			// Get the mimetype from the IIS settings (configurable in the mimetypes.xml file in the site)
			// n.b. debug mode skips using IIS to avoid complications with testing.
			string fileExtension = Path.GetExtension(localPath);
			string mimeType = MimeTypes.GetMimeType(fileExtension);

			try
			{
				string fullPath = TranslateUrlPathToFilePath(localPath, applicationPath);

				if (File.Exists(fullPath))
				{
					responseWrapper.AddStatusCodeForCache(fullPath, modifiedSinceHeader);
					if (responseWrapper.StatusCode != 304)
					{
						// Serve the file in the body
						byte[] buffer = File.ReadAllBytes(fullPath);
						responseWrapper.ContentType = mimeType;
						responseWrapper.BinaryWrite(buffer);
					}

					responseWrapper.End();
				}
				else
				{
					// 404
					Log.Warn("The url {0} (translated to {1}) does not exist on the server.", localPath, fullPath);

					// Throw so the web.config catches it
					throw new HttpException(404, string.Format("{0} does not exist on the server.", localPath));
				}
			}
			catch (IOException ex)
			{
				// 500
				Log.Error(ex, "There was a problem opening the file {0}.", localPath);

				// Throw so the web.config catches it				
				throw new HttpException(500, "There was a problem opening the file (see the error logs)");
			}
		}

		/// <summary>
		/// Takes a request's local file path (e.g. /attachments/a.jpg 
		/// and translates it into the correct attachment file path.
		/// </summary>
		/// <param name="urlPath">The request's url/local path, which is a url such as /Attachments/foo.jpg.
		/// This should always begin with "/".</param>
		/// <param name="applicationPath">The application path e.g. /wiki/, if the app is running under one.
		/// If the app is running from the root then this will just be "/".</param>
		/// <returns>A full operating system file path.</returns>
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