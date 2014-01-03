using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Mvc.Controllers.Api;
using Roadkill.Tests.Acceptance;

namespace Roadkill.Tests.Integration.WebApi
{
	[TestFixture]
	[Category("Unit")]
	public class WebApiTestBase
	{
		private IISExpress _iisExpress;

		protected static readonly string ADMIN_EMAIL = Settings.ADMIN_EMAIL;
		protected static readonly string ADMIN_PASSWORD = Settings.ADMIN_PASSWORD;
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

		protected string GetUrl(string fullPath)
		{
			return string.Format("{0}/api/{1}", BaseUrl, fullPath);
		}
	}
}
