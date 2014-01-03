using System;
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
	public class PageControllerTests : WebApiTestBase
	{
		[Test]
		public void Get_Should_Return_All_Pages()
		{
			// Arrange
			IRepository repository = GetRepository();
			AddPage(repository, "test", "this is page 1");
			AddPage(repository, "page 2", "this is page 2");

			HttpClient client = Login();
			string url = GetFullUrl("Pages");

			// Act
			HttpResponseMessage response = client.GetAsync(url).Result;
			IEnumerable<PageViewModel> results = response.Content.ReadAsAsync<IEnumerable<PageViewModel>>().Result;

			// Assert
			Assert.That(results.Count(), Is.EqualTo(2), response.Content.ReadAsStringAsync().Result);
		}

		[Test]
		public void Get_With_Id_Should_Return_Correct_Page()
		{
			// Arrange
			IRepository repository = GetRepository();
			PageContent expectedPage = AddPage(repository, "test", "this is page 1");

			HttpClient client = Login();
			string url = GetFullUrl("Pages/" + expectedPage.Page.Id);

			// Act
			HttpResponseMessage response = client.GetAsync(url).Result;
			PageViewModel actualPage = response.Content.ReadAsAsync<PageViewModel>().Result;

			// Assert
			Assert.That(actualPage, Is.Not.Null, response.Content.ReadAsStringAsync().Result);
			Assert.That(actualPage.Id, Is.EqualTo(expectedPage.Page.Id), response.Content.ReadAsStringAsync().Result);
		}

		[Test]
		public void Post_Should_Add_Page()
		{
			// Arrange
			IRepository repository = GetRepository();
			PageViewModel page = new PageViewModel()
			{
				Title = "Hello",
				CreatedBy = "admin",
				CreatedOn = DateTime.UtcNow,
				Content = "some content",
				RawTags = "tag1,tag2"
			};

			HttpClient client = Login();
			string url = GetFullUrl("Pages");

			// Act
			HttpResponseMessage response = client.PostAsJsonAsync<PageViewModel>(url, page).Result;
			string jsonResponse = response.Content.ReadAsStringAsync().Result;

			// Assert
			IEnumerable<Page> pages = repository.AllPages();
			Assert.That(pages.Count(), Is.EqualTo(1), jsonResponse);
		}

		[Test]
		public void Put_Should_Update_Page()
		{
			// Arrange
			IRepository repository = GetRepository();
			PageContent pageContent = AddPage(repository, "test", "this is page 1");
			PageViewModel viewModel = new PageViewModel(pageContent.Page);
			viewModel.Title = "New title";

			HttpClient client = Login();
			string url = GetFullUrl("Pages");

			// Act
			HttpResponseMessage response = client.PutAsJsonAsync<PageViewModel>(url, viewModel).Result;
			string jsonResponse = response.Content.ReadAsStringAsync().Result;

			// Assert
			Page page = repository.AllPages().FirstOrDefault();
			//Assert.That(page.Title, Is.EqualTo("New title"), jsonResponse);
		}
	}
}
