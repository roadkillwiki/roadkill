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

		public void ProcessRequest(HttpContext context)
		{
			string fileExtension = Path.GetExtension(context.Request.Url.LocalPath);
			string attachmentFolder = _settings.AttachmentsDirectoryPath;

			using (ServerManager serverManager = new ServerManager())
			{
				// Get the mimetype from the IIS settings (configurable in the mimetypes.xml file in the site)
				string mimeType = MimeTypes.GetMimeType(fileExtension, serverManager);

				byte[] buffer = null;
				try
				{
					// LocalPath uses "/" and a Windows filepath is \
					string filePath = context.Request.Url.LocalPath.Replace(string.Format("/{0}", _settings.AttachmentsRoutePath), "");
					filePath = filePath.Replace(context.Request.ApplicationPath, "");
					filePath = filePath.Replace('/', Path.DirectorySeparatorChar);

					if (attachmentFolder.EndsWith(Path.DirectorySeparatorChar.ToString()))
						attachmentFolder = attachmentFolder.Remove(attachmentFolder.Length -1, 1);

					if (filePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
						filePath = filePath.Remove(0, 1);

					// Ignoring Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar for now.
					string fullPath = attachmentFolder + Path.DirectorySeparatorChar + filePath;

					// Should this return a 404?
					if (!File.Exists(fullPath))
						throw new FileNotFoundException(string.Format("The url {0} (translated to {1}) does not exist on the server", context.Request.Url.LocalPath, fullPath));

					AddStatusCodeForCache(context, fullPath);
					if (context.Response.StatusCode != 304)
					{
						// Serve the file
						buffer = File.ReadAllBytes(fullPath);
						context.Response.ContentType = mimeType;
						context.Response.BinaryWrite(buffer);
					}

					context.Response.End();
				}
				catch (FileNotFoundException ex)
				{
					Log.Error(ex, "Unable to find the attachment file");
					context.Response.StatusCode = 404;
					context.Response.End();
				}
				catch (IOException ex)
				{
					Log.Error(ex, "There was a problem opening the file {0}", context.Request.Url);
					context.Response.Write("There was a problem opening the file (see the error logs)");
					context.Response.StatusCode = 500;
					context.Response.End();
				}
			}
		}

		private void AddStatusCodeForCache(HttpContext context, string fullPath)
		{
			// https://developers.google.com/speed/docs/best-practices/caching
			context.Response.AddFileDependency(fullPath);

			FileInfo info = new FileInfo(fullPath);
			context.Response.Cache.SetCacheability(HttpCacheability.Public);
			context.Response.Headers.Add("Expires", "-1"); // always followed by the browser
			context.Response.Cache.SetLastModifiedFromFileDependencies(); // sometimes followed by the browser
			context.Response.StatusCode = context.GetStatusCodeForCache(info.LastWriteTimeUtc);
		}
	}
}