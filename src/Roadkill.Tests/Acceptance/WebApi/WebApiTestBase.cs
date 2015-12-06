using System;
using System.Configuration;
using Mindscape.LightSpeed;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using IRepository = Roadkill.Core.Database.IRepository;

namespace Roadkill.Tests.Acceptance.WebApi
{
	[TestFixture]
	[Category("Acceptance")]
	public abstract class WebApiTestBase
	{
		private IIS _iis;

		protected static readonly string ADMIN_EMAIL = Settings.ADMIN_EMAIL;
		protected static readonly string ADMIN_PASSWORD = Settings.ADMIN_PASSWORD;
		protected static readonly Guid ADMIN_ID = Settings.ADMIN_ID;
		protected string BaseUrl;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_iis = new IIS();
			_iis.Start();

			string url = ConfigurationManager.AppSettings["url"];
			if (string.IsNullOrEmpty(url))
				url = "http://localhost:9876";
			BaseUrl = url;
		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			if (_iis != null)
			{
				_iis.Dispose();
			}
		}

		[SetUp]
		public void Setup()
		{
			ConfigFileManager.CopyWebConfig();
			ConfigFileManager.CopyConnectionStringsConfig();
			ConfigFileManager.CopyRoadkillConfig();
			SqlServerSetup.RecreateLocalDbData();
		}

		protected IRepository GetRepository()
		{
			ApplicationSettings appSettings = new ApplicationSettings();
			appSettings.DatabaseName = "SqlServer2008";
			appSettings.ConnectionString = SqlServerSetup.ConnectionString;
			appSettings.LoggingTypes = "none";
			appSettings.UseBrowserCache = false;

			LightSpeedRepository repository = new LightSpeedRepository(DataProvider.SqlServer2008, SqlServerSetup.ConnectionString);
			return repository;
		}

		protected PageContent AddPage(string title, string content)
		{
			using (IRepository repository = GetRepository())
			{
				Page page = new Page();
				page.Title = title;
				page.Tags = "tag1, tag2";
				page.CreatedBy = "admin";
				page.CreatedOn = DateTime.UtcNow;
				page.ModifiedOn = DateTime.UtcNow;
				page.ModifiedBy = "admin";

				return repository.AddNewPage(page, content, "admin", DateTime.UtcNow);
			}
		}
	}
}
