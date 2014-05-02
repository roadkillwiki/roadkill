using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Raven.Client;
using Raven.Client.Document;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;
using Roadkill.Core.Plugins;
using StructureMap;
using StructureMap.Attributes;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Core.Database.RavenDB
{
	public class RavenDBRepository : IRepository
	{
		private ApplicationSettings _settings;

		public virtual IDocumentSession Session
		{
			get
			{
				IDocumentSession session = ObjectFactory.GetInstance<IDocumentSession>();
				if (session == null)
					throw new DatabaseException("The IDocumentSession for RavenDB is null - has Startup() been called?", null);

				return session;
			}
		}

		public RavenDBRepository(ApplicationSettings settings)
		{
			_settings = settings;
		}

		#region IRepository Members

		public void Startup(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			ObjectFactory.Configure(x =>
			{
				// Create a singleton DocumentStore to open sessions with
				DocumentStore documentStore = new DocumentStore();
				documentStore.ParseConnectionString(connectionString);
				documentStore.Conventions.FindIdentityProperty = (propInfo) => { return propInfo.DeclaringType.Name == "ObjectId"; };
				documentStore.Initialize();

				x.For<DocumentStore>().Singleton().Use(documentStore);

				// Open a sessions for each new HTTP Request/Thread
				x.For<IDocumentSession>().HybridHttpOrThreadLocalScoped().Use(ctx => ctx.GetInstance<DocumentStore>().OpenSession());
			});
		}

		public void TestConnection(DataStoreType dataStoreType, string connectionString)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IPageRepository Members

		public PageContent AddNewPage(Page page, string text, string editedBy, DateTime editedOn)
		{
			// Get the most recent page ID from the database
			int newId = 1;
			Page newestPage = Session.Query<Page>().OrderByDescending(x => x.Id).FirstOrDefault();
			if (newestPage != null)
				newId = newestPage.Id + 1;

			page.Id = newId;

			// Save the page
			Session.Store(page);

			// Save the page's contents
			PageContent pageContent = new PageContent()
			{
				Id = Guid.NewGuid(),
				Page = page,
				Text = text,
				EditedBy = editedBy,
				EditedOn = editedOn,
				VersionNumber = 1,
			};
			Session.Store(pageContent);
			Session.SaveChanges();

			return pageContent;
		}

		public PageContent AddNewPageContentVersion(Page page, string text, string editedBy, DateTime editedOn, int version)
		{
			page.ModifiedOn = editedOn;
			page.ModifiedBy = editedBy;
			Session.Store(page);

			PageContent pageContent = new PageContent()
			{
				Id = Guid.NewGuid(),
				Page = page,
				Text = text,
				EditedBy = editedBy,
				EditedOn = editedOn,
				VersionNumber = version,
			};
			Session.Store(pageContent);
			Session.SaveChanges();

			return pageContent;
		}

		public IEnumerable<Page> AllPages()
		{
			return Session.Query<Page>().ToList();
		}

		public IEnumerable<PageContent> AllPageContents()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<string> AllTags()
		{
			throw new NotImplementedException();
		}

		public void DeletePage(Page page)
		{
			throw new NotImplementedException();
		}

		public void DeletePageContent(PageContent pageContent)
		{
			throw new NotImplementedException();
		}

		public void DeleteAllPages()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Page> FindPagesCreatedBy(string username)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Page> FindPagesModifiedBy(string username)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Page> FindPagesContainingTag(string tag)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<PageContent> FindPageContentsByPageId(int pageId)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<PageContent> FindPageContentsEditedBy(string username)
		{
			throw new NotImplementedException();
		}

		public PageContent GetLatestPageContent(int pageId)
		{
			throw new NotImplementedException();
		}

		public Page GetPageById(int id)
		{
			return Session.Load<Page>(id);
		}

		public Page GetPageByTitle(string title)
		{
			throw new NotImplementedException();
		}

		public PageContent GetPageContentById(Guid id)
		{
			throw new NotImplementedException();
		}

		public PageContent GetPageContentByPageIdAndVersionNumber(int id, int versionNumber)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<PageContent> GetPageContentByEditedBy(string username)
		{
			throw new NotImplementedException();
		}

		public Page SaveOrUpdatePage(Page page)
		{
			throw new NotImplementedException();
		}

		public void UpdatePageContent(PageContent content)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IUserRepository Members

		public void DeleteAllUsers()
		{
			throw new NotImplementedException();
		}

		public void DeleteUser(User user)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<User> FindAllEditors()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<User> FindAllAdmins()
		{
			throw new NotImplementedException();
		}

		public User GetAdminById(Guid id)
		{
			throw new NotImplementedException();
		}

		public User GetUserByActivationKey(string key)
		{
			throw new NotImplementedException();
		}

		public User GetEditorById(Guid id)
		{
			throw new NotImplementedException();
		}

		public User GetUserByEmail(string email, bool? isActivated = null)
		{
			throw new NotImplementedException();
		}

		public User GetUserById(Guid id, bool? isActivated = null)
		{
			throw new NotImplementedException();
		}

		public User GetUserByPasswordResetKey(string key)
		{
			throw new NotImplementedException();
		}

		public User GetUserByUsername(string username)
		{
			throw new NotImplementedException();
		}

		public User GetUserByUsernameOrEmail(string username, string email)
		{
			throw new NotImplementedException();
		}

		public User SaveOrUpdateUser(User user)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region ISettingsRepository Members

		public void Install(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			foreach (SiteSettings siteSettings in Session.Query<SiteSettings>())
			{
				Session.Delete(siteSettings);
			}

			foreach (PageContent pageContent in Session.Query<PageContent>())
			{
				Session.Delete(pageContent);
			}

			foreach (Page page in Session.Query<Page>())
			{
				Session.Delete(page);
			}

			foreach (User user in Session.Query<User>())
			{
				Session.Delete(user);
			}

			Session.SaveChanges();
		}

		public void Upgrade(ApplicationSettings applicationSettings)
		{
			throw new NotImplementedException();
		}

		public void SaveSiteSettings(SiteSettings siteSettings)
		{
			throw new NotImplementedException();
		}

		public SiteSettings GetSiteSettings()
		{
			throw new NotImplementedException();
		}

		public void SaveTextPluginSettings(TextPlugin plugin)
		{
			throw new NotImplementedException();
		}

		public PluginSettings GetTextPluginSettings(Guid databaseId)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
