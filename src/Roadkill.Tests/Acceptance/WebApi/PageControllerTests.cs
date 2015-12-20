using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Acceptance.WebApi
{
	[TestFixture]
	[Category("Acceptance")]
	public class PageControllerTests : WebApiTestBase
	{
		[Test]
		public void get_should_return_all_pages()
		{
			// Arrange
			AddPage("test", "this is page 1");
			AddPage("page 2", "this is page 2");

			WebApiClient apiclient = new WebApiClient();
			apiclient.Login();

			// Act
			WebApiResponse<List<PageViewModel>> response = apiclient.Get<List<PageViewModel>>("Pages");

			// Assert
			IEnumerable<PageViewModel> pages = response.Result;
			Assert.That(pages.Count(), Is.EqualTo(2), response);
		}

		[Test]
		public void get_with_id_should_return_correct_page()
		{
			// Arrange
			PageContent expectedPage = AddPage("test", "this is page 1");
			var queryString = new Dictionary<string, string>()
			{ 
				{ "Id", expectedPage.Page.Id.ToString() }
			};
			WebApiClient apiclient = new WebApiClient();
			apiclient.Login();

			// Act
			WebApiResponse<PageViewModel> response = apiclient.Get<PageViewModel>("Pages", queryString);

			// Assert
			PageViewModel actualPage = response.Result;
			Assert.That(actualPage, Is.Not.Null, response.ToString());
			Assert.That(actualPage.Id, Is.EqualTo(expectedPage.Page.Id), response.ToString());
		}

		[Test]
		public void post_should_add_page()
		{
			// Arrange
			PageViewModel page = new PageViewModel()
			{
				Title = "Hello",
				CreatedBy = "admin",
				CreatedOn = DateTime.UtcNow,
				Content = "some content",
				RawTags = "tag1,tag2"
			};

			WebApiClient apiclient = new WebApiClient();
			apiclient.Login();

			// Act
			WebApiResponse response = apiclient.Post<PageViewModel>("Pages", page);

			// Assert
			IPageRepository repository = GetRepository();
			IEnumerable<Page> pages = repository.AllPages();
			Assert.That(pages.Count(), Is.EqualTo(1), response);
		}

		[Test]
		public void put_should_update_page()
		{
			// Arrange
			PageContent pageContent = AddPage("test", "this is page 1");
			PageViewModel viewModel = new PageViewModel(pageContent.Page);
			viewModel.Title = "New title";
			
			WebApiClient apiclient = new WebApiClient();
			apiclient.Login();

			// Act
			WebApiResponse response = apiclient.Put<PageViewModel>("Pages/Put", viewModel);

			// Assert
			IPageRepository repository = GetRepository();
			Page page = repository.AllPages().FirstOrDefault();
			Assert.That(page.Title, Is.EqualTo("New title"), response);
		}
	}
}
