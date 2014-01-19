using System;
using System.Configuration;
using System.Net.Http;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Logging;
using Roadkill.Core.Mvc.Controllers.Api;

namespace Roadkill.Tests.Integration.WebApi
{
	[TestFixture]
	[Category("Integration")]
	public abstract class WebApiTestBase
	{
		private IISExpress _iisExpress;

		protected static readonly string ADMIN_EMAIL = Settings.ADMIN_EMAIL;
		protected static readonly string ADMIN_PASSWORD = Settings.ADMIN_PASSWORD;
		protected static readonly Guid ADMIN_ID = Settings.ADMIN_ID;
		protected string BaseUrl;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_iisExpress = new IISExpress();
			_iisExpress.Start();

			string url = ConfigurationManager.AppSettings["url"];
			if (string.IsNullOrEmpty(url))
				url = "http://localhost:9876";
			BaseUrl = url;
		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			if (_iisExpress != null)
			{
				_iisExpress.Dispose();
			}
		}

		[SetUp]
		public void Setup()
		{
			ConfigFileManager.CopyWebConfig();
			ConfigFileManager.CopyConnectionStringsConfig();
			ConfigFileManager.CopyRoadkillConfig();
			SqlExpressSetup.RecreateLocalDbData();
		}

		protected IRepository GetRepository()
		{
			ApplicationSettings appSettings = new ApplicationSettings();
			appSettings.DataStoreType = DataStoreType.SqlServer2012;
			appSettings.ConnectionString = SqlExpressSetup.ConnectionString;
			appSettings.LoggingTypes = "none";
			appSettings.UseBrowserCache = false;
			Log.ConfigureLogging(appSettings);

			LightSpeedRepository repository = new LightSpeedRepository(appSettings);
			repository.Startup(appSettings.DataStoreType, appSettings.ConnectionString, false);
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
