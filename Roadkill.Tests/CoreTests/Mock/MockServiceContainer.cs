using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Moq;
using NHibernate;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Domain;
using Roadkill.Core.Search;
using StructureMap;

namespace Roadkill.Tests
{
	public class MockServiceContainer
	{
		public static string AdminEmail = "admin@localhost";
		public static string AdminUsername = "admin";
		public static string AdminPassword = "password";

		private User _testUser;
		private List<PageContent> _contentList;
		private List<Page> _pageList;

		private Mock<IRepository> _mockRepository;
		private Mock<SearchManager> _mockSearchManager;
		private Mock<UserManager> _mockUserManager;
		private IConfigurationContainer _configurationContainer;

		public MockServiceContainer()
		{
			_configurationContainer = new RoadkillSettings();
			_configurationContainer.ApplicationSettings = new ApplicationSettings();
			_configurationContainer.SitePreferences = new SitePreferences() { MarkupType = "Creole" };

			_contentList = new List<PageContent>();
			_pageList = new List<Page>();

			// Mock up a usermanager
			Mock<User> mockUser = new Mock<User>();
			mockUser.SetupProperty<string>(x => x.Email, AdminEmail);
			mockUser.SetupProperty<string>(x => x.Username, AdminUsername);
			_testUser = mockUser.Object;
			RoadkillContext.Current.CurrentUser = AdminEmail;

			_mockUserManager = new Mock<UserManager>();
			_mockUserManager.Setup(x => x.GetUser(_testUser.Email)).Returns(mockUser.Object);
			_mockUserManager.Setup(x => x.Authenticate(_testUser.Email, "")).Returns(true);
			_mockUserManager.Setup(x => x.GetLoggedInUserName(It.IsAny<HttpContextBase>())).Returns(_testUser.Username);

			//
			// Repository mocks
			//
			_mockRepository = new Mock<IRepository>();
			_mockRepository.Setup(x => x.Queryable<Page>()).Returns(_pageList.AsQueryable());
			_mockRepository.Setup(x => x.Queryable<PageContent>()).Returns(_contentList.AsQueryable());

			// Searchmanager mock
			_mockSearchManager = new Mock<SearchManager>();
		}

		public Page AddMockPage(int id, string createdBy, string title, string tags, string textContent = "")
		{
			return AddAndGetMockPage(id, createdBy, title, tags, DateTime.Today, textContent).Object;
		}

		public Page AddMockPage(int id, string createdBy, string title, string tags, DateTime createdOn, string textContent = "")
		{
			return AddAndGetMockPage(id, createdBy, title, tags, createdOn, textContent).Object;
		}

		/// <summary>
		/// Adds a page to the mock repository
		/// </summary>
		public Mock<Page> AddAndGetMockPage(int id, string createdBy, string title, string tags, DateTime createdOn, string textContent = "")
		{
			var pageMock = new Mock<Page>() { CallBase = true };
			pageMock.SetupProperty(x => x.Id, id);
			pageMock.SetupProperty(x => x.CreatedBy, createdBy);
			pageMock.SetupProperty(x => x.Title, title);
			pageMock.SetupProperty(x => x.Tags, tags);
			pageMock.SetupProperty(x => x.CreatedOn, createdOn);

			PageContent pageContent = new PageContent();
			pageContent.Page = pageMock.Object;
			pageContent.Id = Guid.NewGuid();
			pageContent.VersionNumber = 1;
			pageContent.EditedBy = createdBy;
			pageContent.EditedOn = DateTime.Now;

			if (string.IsNullOrEmpty(textContent))
				pageContent.Text = title + "'s text";
			else
				pageContent.Text = textContent;

			_mockRepository.Setup(x => x.Delete(pageMock.Object)).Callback(() => _pageList.Remove(pageMock.Object));
			_mockRepository.Setup(x => x.Delete(pageContent)).Callback(() => _contentList.Remove(pageContent));

			_contentList.Add(pageContent);
			_pageList.Add(pageMock.Object);

			return pageMock;
		}
	}
}
