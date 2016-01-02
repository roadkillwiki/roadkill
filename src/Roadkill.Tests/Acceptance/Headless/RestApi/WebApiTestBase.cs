using System;
using System.Configuration;
using Mindscape.LightSpeed;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;

namespace Roadkill.Tests.Acceptance.Headless.RestApi
{
	[TestFixture]
	[Category("Acceptance")]
	public abstract class WebApiTestBase
	{
		protected static readonly string ADMIN_EMAIL = TestConstants.ADMIN_EMAIL;
		protected static readonly string ADMIN_PASSWORD = TestConstants.ADMIN_PASSWORD;
		protected static readonly Guid ADMIN_ID = TestConstants.ADMIN_ID;
		protected string BaseUrl;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			TestHelpers.CreateIisTestSite();

			string url = ConfigurationManager.AppSettings["url"];
			if (string.IsNullOrEmpty(url))
				url = TestConstants.WEB_BASEURL;
			BaseUrl = url;
		}

		[SetUp]
		public void Setup()
		{
			TestHelpers.CopyDevWebConfigFromLibFolder();
			TestHelpers.CopyDevConnectionStringsConfig();
			TestHelpers.CopyDevRoadkillConfig();
			TestHelpers.SqlServerSetup.RecreateTables();
		}

		protected IPageRepository GetRepository()
		{
			ApplicationSettings appSettings = new ApplicationSettings();
			appSettings.DatabaseName = "SqlServer2008";
			appSettings.ConnectionString = TestConstants.CONNECTION_STRING;
			appSettings.UseBrowserCache = false;

			var context = new LightSpeedContext();
			context.ConnectionString = TestConstants.CONNECTION_STRING;
			context.DataProvider = DataProvider.SqlServer2008;

			LightSpeedPageRepository repository = new LightSpeedPageRepository(context.CreateUnitOfWork());
			return repository;
		}

		protected PageContent AddPage(string title, string content)
		{
			using (IPageRepository repository = GetRepository())
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
