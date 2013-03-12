using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Unit
{
	internal class RepositoryStub : IRepository
	{
		private List<Page> _pages;
		private List<PageContent> _pagesContent;

		public RepositoryStub()
		{
			_pages = new List<Page>();
			_pagesContent = new List<PageContent>();
		}

		#region IRepository Members

		public void DeletePage(Page page)
		{
			throw new NotImplementedException();
		}

		public void DeletePageContent(PageContent pageContent)
		{
			throw new NotImplementedException();
		}

		public void DeleteUser(User user)
		{
			throw new NotImplementedException();
		}

		public void DeleteAllPages()
		{
			throw new NotImplementedException();
		}

		public void DeleteAllPageContent()
		{
			throw new NotImplementedException();
		}

		public void DeleteAllUsers()
		{
			throw new NotImplementedException();
		}

		public void SaveOrUpdatePage(Page page)
		{
			throw new NotImplementedException();
		}

		public PageContent AddNewPage(Page page, string text, string editedBy, DateTime editedOn)
		{
			throw new NotImplementedException();
		}

		public PageContent AddNewPageContentVersion(Page page, string text, string editedBy, DateTime editedOn, int version)
		{
			throw new NotImplementedException();
		}

		public void UpdatePageContent(PageContent content)
		{
			throw new NotImplementedException();
		}

		public void SaveOrUpdateUser(User user)
		{
			throw new NotImplementedException();
		}

		public void SaveSitePreferences(SitePreferences preferences)
		{
			throw new NotImplementedException();
		}

		public SitePreferences GetSitePreferences()
		{
			throw new NotImplementedException();
		}

		public void Startup(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			throw new NotImplementedException();
		}

		public void Install(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			throw new NotImplementedException();
		}

		public void Test(DataStoreType dataStoreType, string connectionString)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IPageRepository Members

		public IEnumerable<Page> AllPages()
		{
			throw new NotImplementedException();
		}

		public Page GetPageById(int id)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Page> FindPagesByCreatedBy(string username)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Page> FindPagesByModifiedBy(string username)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Page> FindPagesContainingTag(string tag)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<string> AllTags()
		{
			throw new NotImplementedException();
		}

		public Page GetPageByTitle(string title)
		{
			throw new NotImplementedException();
		}

		public PageContent GetLatestPageContent(int pageId)
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

		public PageContent GetPageContentByVersionId(Guid versionId)
		{
			throw new NotImplementedException();
		}

		public PageContent GetPageContentByEditedBy(string username)
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

		public IEnumerable<PageContent> AllPageContents()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IUserRepository Members

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

		public User GetUserByEmail(string email, bool IsActivated = true)
		{
			throw new NotImplementedException();
		}

		public User GetUserById(Guid id, bool IsActivated = true)
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

		public IEnumerable<User> FindAllEditors()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<User> FindAllAdmins()
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
