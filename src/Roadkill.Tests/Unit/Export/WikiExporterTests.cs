using System;
using System.IO;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Domain.Export;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Export
{
	[TestFixture]
	[Category("Unit")]
	public class WikiExporterTests
	{
		private MocksAndStubsContainer _container;
		private ApplicationSettings _applicationSettings;
		private RepositoryMock _repository;
		private PageService _pageService;
		private PluginFactoryMock _pluginFactory;
		private WikiExporter _wikiExporter;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();
			_applicationSettings = _container.ApplicationSettings;
			_repository = _container.Repository;
			_pageService = _container.PageService;
			_pluginFactory = _container.PluginFactory;

			_wikiExporter = new WikiExporter(_applicationSettings, _pageService, _repository, _pluginFactory);
			_wikiExporter.ExportFolder = AppDomain.CurrentDomain.BaseDirectory;
		}

		[Test]
		public void exportasxml_should_return_non_empty_stream()
		{
			// Arrange
			_repository.AddNewPage(new Page() { Id = 1 }, "text", "admin", DateTime.UtcNow);
			_repository.AddNewPage(new Page() { Id = 2 }, "text", "admin", DateTime.UtcNow);

			// Act
			Stream stream = _wikiExporter.ExportAsXml();

			// Assert
			Assert.That(stream.Length, Is.GreaterThan(1));
		}

		[Test]
		public void exportassql_should_return_non_empty_stream()
		{
			// Arrange
			_repository.AddNewPage(new Page() { Id = 1 }, "text", "admin", DateTime.UtcNow);
			_repository.AddNewPage(new Page() { Id = 2 }, "text", "admin", DateTime.UtcNow);

			// Act
			Stream stream = _wikiExporter.ExportAsSql();

			// Assert
			Assert.That(stream.Length, Is.GreaterThan(1));
		}

		[Test]
		public void exportaswikifiles_should_save_zip_file_to_export_directory()
		{
			// Arrange
			string filename = string.Format("export-{0}.zip", DateTime.Now.Ticks);
			string zipFullPath = Path.Combine(_wikiExporter.ExportFolder, filename);

			_repository.AddNewPage(new Page() { Id = 1 }, "text", "admin", DateTime.UtcNow);
			_repository.AddNewPage(new Page() { Id = 2 }, "text", "admin", DateTime.UtcNow);

			// Act
			_wikiExporter.ExportAsWikiFiles(filename);

			// Assert
			Assert.That(File.Exists(zipFullPath), Is.True);

			FileInfo file = new FileInfo(zipFullPath);
			Assert.That(file.Length, Is.GreaterThan(1));
		}

		[Test]
		public void exportattachments_should_save_zip_file_to_export_directory()
		{
			// Arrange
			string filename = string.Format("attachments-{0}.zip", DateTime.Now.Ticks);
			string zipFullPath = Path.Combine(_wikiExporter.ExportFolder, filename);
			_applicationSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Attachments");

			string filename1 = Path.Combine(_applicationSettings.AttachmentsFolder, "somefile1.txt");
			string filename2 = Path.Combine(_applicationSettings.AttachmentsFolder, "somefile2.txt");

			File.WriteAllText(filename1, "sample content");
			File.WriteAllText(filename2, "sample content");

			// Act
			_wikiExporter.ExportAttachments(filename);

			// Assert
			Assert.That(File.Exists(zipFullPath), Is.True);

			FileInfo file = new FileInfo(zipFullPath);
			Assert.That(file.Length, Is.GreaterThan(1));
		}
	}
}
