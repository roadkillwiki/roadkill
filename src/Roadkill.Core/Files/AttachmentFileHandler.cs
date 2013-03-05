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

namespace Roadkill.Core.Files
{
	/// <summary>
	/// A <see cref="IHttpHandler"/> that serves all uploaded files.
	/// </summary>
	public class AttachmentFileHandler : IHttpHandler
	{
		private IConfigurationContainer _config;

		public bool IsReusable
		{
			get { return true; }
		}

		public AttachmentFileHandler(IConfigurationContainer config)
		{
			_config = config;
		}

		public void ProcessRequest(HttpContext context)
		{
			string fileExtension = Path.GetExtension(context.Request.Url.LocalPath);
			string attachmentFolder = _config.ApplicationSettings.AttachmentsFolder;

			using (ServerManager serverManager = new ServerManager())
			{
				// Get the mimetype from the IIS settings (configurable in the mimetypes.xml file in the site)
				string mimeType = GetMimeType(fileExtension, serverManager);

				byte[] buffer = null;
				try
				{
					// LocalPath uses "/" and a Windows filepath is \
					string filePath = context.Request.Url.LocalPath.Replace(string.Format("/{0}", _config.ApplicationSettings.AttachmentsRoutePath), "");
					filePath = filePath.Replace('/', Path.DirectorySeparatorChar);

					if (attachmentFolder.EndsWith(Path.DirectorySeparatorChar.ToString()))
						attachmentFolder = attachmentFolder.Remove(attachmentFolder.Length, 1);

					if (filePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
						filePath = filePath.Remove(0, 1);

					// Ignoring Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar for now.
					string fullPath = attachmentFolder + Path.DirectorySeparatorChar + filePath;

					if (!File.Exists(fullPath))
						throw new FileNotFoundException(string.Format("The url {0} (translated to {1}) does not exist on the server", context.Request.Url.LocalPath, fullPath));

					// Caching: Google recommend an Expires of 1 month, ETag over Last-Modified
					context.Response.AddFileDependency(fullPath);

					FileInfo info = new FileInfo(fullPath);
					TimeSpan expires = TimeSpan.FromDays(28);
					context.Response.Cache.SetLastModifiedFromFileDependencies();
					context.Response.Cache.SetETag(info.LastWriteTimeUtc.GetHashCode().ToString());
					context.Response.Cache.SetExpires(DateTime.UtcNow.Add(expires));
					context.Response.Cache.SetMaxAge(expires);
					context.Response.Cache.SetCacheability(HttpCacheability.Public);

					// Serve the file
					buffer = File.ReadAllBytes(fullPath);
					context.Response.ContentType = mimeType;
					context.Response.BinaryWrite(buffer);
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

		private string GetMimeType(string fileExtension, ServerManager serverManager)
		{
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
				return MimeMapping.GetMimeMapping("." +fileExtension);
			}
		}

		internal static string GetAttachmentsPath(IConfigurationContainer configuration)
		{
			string attachmentsPath = configuration.ApplicationSettings.AttachmentsUrlPath;
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
	}
}
