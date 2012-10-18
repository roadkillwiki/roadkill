using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Moq;
using NHibernate;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Search;

namespace Roadkill.Tests
{
	public class MoqTestBase
	{
		protected User _testUser;
		protected List<PageContent> _contentList;
		protected List<Page> _pageList;
		protected Mock<NHibernateRepository> _mockRepository;
		protected Mock<IQuery> _queryMock;
		protected Mock<SearchManager> _mockSearchManager;
		protected string _userEmail = "admin@localhost";
		protected string _username = "admin";

		[SetUp]
		public void Setup()
		{
			// Required by the indexer to parser the markup
			SiteConfiguration.Initialize(new SiteConfiguration() { MarkupType = "Creole" }); 

			RoadkillContext.IsWeb = false;
			_contentList = new List<PageContent>();
			_pageList = new List<Page>();

			// Mock up a usermanager
			Mock<User> mockUser = new Mock<User>();
			mockUser.SetupProperty<string>(x => x.Email, _userEmail);
			mockUser.SetupProperty<string>(x => x.Username, _username);
			_testUser = mockUser.Object;
			RoadkillContext.Current.CurrentUser = _userEmail;

			Mock<UserManager> mockuserManager = new Mock<UserManager>();
			mockuserManager.Setup(x => x.GetUser(_testUser.Email)).Returns(mockUser.Object);
			mockuserManager.Setup(x => x.Authenticate(_testUser.Email, "")).Returns(true);
			mockuserManager.Setup(x => x.GetLoggedInUserName(It.IsAny<HttpContextBase>())).Returns(_testUser.Username);
			UserManager.Initialize(mockuserManager.Object);

			//
			// NHibernate sessionfactory mocks
			//
			_queryMock = new Mock<IQuery>();
			Mock<ISessionFactory> sessionFactoryMock = new Mock<ISessionFactory>();
			Mock<ISession> sessionMock = new Mock<ISession>();
			sessionFactoryMock.Setup(x => x.OpenSession()).Returns(sessionMock.Object);
			sessionMock.Setup(x => x.CreateQuery(It.IsAny<string>())).Returns(_queryMock.Object);

			//
			// NHibernate repo mocks
			//
			_mockRepository = new Mock<NHibernateRepository>();
			_mockRepository.Setup(x => x.Queryable<Page>()).Returns(_pageList.AsQueryable());
			_mockRepository.Setup(x => x.Queryable<PageContent>()).Returns(_contentList.AsQueryable());
			_mockRepository.SetupProperty(x => x.SessionFactory, sessionFactoryMock.Object);
			NHibernateRepository.Initialize(_mockRepository.Object);

			// Searchmanager mock
			_mockSearchManager = new Mock<SearchManager>();
		}

		protected Page AddMockPage(int id, string createdBy, string title, string tags, string textContent = "")
		{
			return AddAndGetMockPage(id, createdBy, title, tags, DateTime.Today, textContent).Object;
		}

		protected Page AddMockPage(int id, string createdBy, string title, string tags, DateTime createdOn, string textContent = "")
		{
			return AddAndGetMockPage(id, createdBy, title, tags, createdOn, textContent).Object;
		}

		/// <summary>
		/// Adds a page to the mock repository
		/// </summary>
		protected Mock<Page> AddAndGetMockPage(int id, string createdBy, string title, string tags, DateTime createdOn, string textContent = "")
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

			pageMock.Setup(x => x.CurrentContent()).Returns(pageContent);
			_mockRepository.Setup(x => x.Delete(pageMock.Object)).Callback(() => _pageList.Remove(pageMock.Object));
			_mockRepository.Setup(x => x.Delete(pageContent)).Callback(() => _contentList.Remove(pageContent));

			_contentList.Add(pageContent);
			_pageList.Add(pageMock.Object);

			return pageMock;
		}
	}
}
