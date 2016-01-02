using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Acceptance.Headless.WebApi
{
	[TestFixture]
	[Category("Acceptance")]
	public class SearchControllerTests : WebApiTestBase
	{
		[Test]
		public void search_should_return_result_based_on_query()
		{
			// Arrange
			AddPage("test", "this is page 1");
			AddPage("page 2", "this is page 2");
			var queryString = new Dictionary<string, string>()
			{ 
				{ "query", "test" }
			};

			WebApiClient apiclient = new WebApiClient();

			// Act
			apiclient.Get("Search/CreateIndex");
			WebApiResponse<List<PageViewModel>> response = apiclient.Get<List<PageViewModel>>("Search", queryString);

			// Assert
			IEnumerable<PageViewModel> pages = response.Result;
			Assert.That(pages.Count(), Is.EqualTo(1), response);
		}
	}
}
