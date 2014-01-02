using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zip;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Export;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Plugins;
using Roadkill.Core.Services;

namespace Roadkill.Core.Domain.Export
{
	public class WikiExporter
	{
		private readonly ApplicationSettings _applicationSettings;
		private readonly PageService _pageService;
		private readonly SqlExportBuilder _sqlExportBuilder;

		public string ExportFolder { get; set; }

		public WikiExporter(ApplicationSettings applicationSettings, PageService pageService, IRepository repository, IPluginFactory pluginFactory)
		{
			if (applicationSettings == null)
				throw new ArgumentNullException("applicationSettings");

			if (pageService == null)
				throw new ArgumentNullException("pageService");

			_applicationSettings = applicationSettings;
			_pageService = pageService;
			_sqlExportBuilder = new SqlExportBuilder(repository, pluginFactory);

			ExportFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Export");
		}

		public Stream ExportAsXml()
		{
			string xml = _pageService.ExportToXml();

			// Don't dispose the stream (as the FileStreamResult will need it open)
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(xml);
			writer.Flush();
			stream.Position = 0;

			return stream;
		}

		public Stream ExportAsSql()
		{
			string sql = _sqlExportBuilder.Export();

			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(sql);
			writer.Flush();
			stream.Position = 0;

			return stream;
		}

		public void ExportAttachments(string filename)
		{
			if (!Directory.Exists(ExportFolder))
				Directory.CreateDirectory(ExportFolder);

			string zipFullPath = Path.Combine(ExportFolder, filename);
			using (ZipFile zip = new ZipFile(zipFullPath))
			{
				zip.AddDirectory(_applicationSettings.AttachmentsDirectoryPath, "Attachments");
				zip.Save();
			}
		}

		public void ExportAsWikiFiles(string filename)
		{
			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			IEnumerable<PageViewModel> pages = _pageService.AllPages();
			char[] invalidChars = Path.GetInvalidFileNameChars();

			if (!Directory.Exists(ExportFolder))
				Directory.CreateDirectory(ExportFolder);

			string zipFullPath = Path.Combine(ExportFolder, filename);

			if (File.Exists(zipFullPath))
				File.Delete(zipFullPath);

			using (ZipFile zip = new ZipFile(zipFullPath))
			{
				int index = 0;
				List<string> filenames = new List<string>();

				foreach (PageViewModel summary in pages.OrderBy(p => p.Title))
				{
					// Double check for blank titles, as the API can add
					// pages with blanks titles even though the UI doesn't allow it.
					if (string.IsNullOrEmpty(summary.Title))
						summary.Title = "(No title -" + summary.Id + ")";

					string filePath = summary.Title;

					// Ensure the filename is unique as its title based.
					// Simply replace invalid path characters with a '-'
					foreach (char item in invalidChars)
					{
						filePath = filePath.Replace(item, '-');
					}

					if (filenames.Contains(filePath))
						filePath += (++index) + "";
					else
						index = 0;

					filenames.Add(filePath);

					filePath = Path.Combine(ExportFolder, filePath);
					filePath += ".wiki";
					string content = "Tags:" + summary.SpaceDelimitedTags() + "\r\n" + summary.Content;

					Console.WriteLine(filePath);
					File.WriteAllText(filePath, content);
					zip.AddFile(filePath, "");
				}

				zip.Save();
			}
		}
	}
}
