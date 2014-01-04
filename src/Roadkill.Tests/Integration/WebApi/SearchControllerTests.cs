using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Integration.WebApi
{
	[TestFixture]
	[Category("Integration")]
	public class SearchControllerTests : WebApiTestBase
	{
		[Test]
		public void Search_Should_Return_Result_Based_On_Query()
		{
			// Arrange
			IRepository repository = GetRepository();
			AddPage(repository, "test", "this is page 1");
			AddPage(repository, "page 2", "this is page 2");

			HttpClient client = Login();
			string indexUrl = GetFullUrl("Search/CreateIndex");
			string url = GetFullUrl("Search?query=test");

			// Act
			var x = client.GetAsync(indexUrl).Result;
			HttpResponseMessage response = client.GetAsync(url).Result;
			IEnumerable<SearchResultViewModel> results = response.Content.ReadAsAsync<IEnumerable<SearchResultViewModel>>().Result;

			// Assert
			Assert.That(results.Count(), Is.EqualTo(1), response.Content.ReadAsStringAsync().Result);
		}
	}
}
