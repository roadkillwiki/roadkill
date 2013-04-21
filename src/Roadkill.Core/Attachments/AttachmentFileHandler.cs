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
				string mimeType = GetMimeType(fileExtension, serverManager);

				byte[] buffer = null;
				try
				{
					// LocalPath uses "/" and a Windows filepath is \
					string filePath = context.Request.Url.LocalPath.Replace(string.Format("/{0}", _settings.AttachmentsRoutePath), "");
					filePath = filePath.Replace('/', Path.DirectorySeparatorChar);

					if (attachmentFolder.EndsWith(Path.DirectorySeparatorChar.ToString()))
						attachmentFolder = attachmentFolder.Remove(attachmentFolder.Length, 1);

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

		private string GetMimeType(string fileExtension, ServerManager serverManager)
		{
#if MONO
			return MimeMapping.GetMimeMapping(fileExtension);
#endif

			try
			{
				string mimeType = "text/plain";

				Microsoft.Web.Administration.Configuration config = serverManager.GetApplicationHostConfiguration();
				ConfigurationSection staticContentSection = config.GetSection("system.webServer/staticContent");
				ConfigurationElementCollection mimemaps = staticContentSection.GetCollection();

				ConfigurationElement element = mimemaps.FirstOrDefault(m => m.Attributes["fileExtension"].Value.ToString() == fileExtension);

				if (element != null)
					mimeType = element.Attributes["mimeType"].Value.ToString();

				return mimeType;
			}
			catch (UnauthorizedAccessException)
			{
				// Shared hosting won't have access to the applicationhost.config file
				return MimeMapping.GetMimeMapping(fileExtension);
			}
		}

		internal static string GetAttachmentsPath(ApplicationSettings settings)
		{
			string attachmentsPath = settings.AttachmentsUrlPath;
			if (HttpContext.Current != null)
			{
				string applicationPath = HttpContext.Current.Request.ApplicationPath;
				if (!applicationPath.EndsWith("/"))
					applicationPath += "/";

				if (attachmentsPath.StartsWith("/"))
					attachmentsPath = attachmentsPath.Remove(0, 1);

				attachmentsPath = applicationPath + attachmentsPath;
			}

			return attachmentsPath;
		}

		/// <summary>
		/// Tests if the attachments folder provided can be written to, by writing a file to the folder.
		/// </summary>
		/// <param name="folder">The folder path which should include "~/" at the start.</param>
		/// <returns>Any error messages or an empty string if no errors occurred.</returns>
		public static string TestAttachmentsFolder(string folder)
		{
			string errors = "";
			if (string.IsNullOrEmpty(folder))
			{
				errors = "The folder name is empty";
			}
			else
			{
				try
				{
					string directory = folder;
					if (folder.StartsWith("~") && HttpContext.Current != null)
						directory = HttpContext.Current.Server.MapPath(folder);

					if (Directory.Exists(directory))
					{
						string path = Path.Combine(directory, "_installtest.txt");
						System.IO.File.WriteAllText(path, "created by the installer to test the attachments folder");
					}
					else
					{
						errors = "The directory does not exist, please create it first";
					}
				}
				catch (Exception e)
				{
					errors = e.ToString();
				}
			}

			return errors;
		}
	}
}
