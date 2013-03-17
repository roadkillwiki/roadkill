using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Unit
{
	internal class RepositoryStub : IRepository
	{
		internal List<Page> Pages { get; private set; }
		internal List<PageContent> PageContents { get; private set; }
		internal List<User> Users { get; private set; }
		internal SitePreferences SitePreferences { get; private set; }

		public RepositoryStub()
		{
			Pages = new List<Page>();
			PageContents = new List<PageContent>();
			Users = new List<User>();
			SitePreferences = new SitePreferences();
		}

		#region IRepository Members

		public void DeletePage(Page page)
		{
			Pages.Remove(page);
		}

		public void DeletePageContent(PageContent pageContent)
		{
			PageContents.Remove(pageContent);
		}

		public void DeleteUser(User user)
		{
			Users.Remove(user);
		}

		public void DeleteAllPages()
		{
			Pages = new List<Page>();
		}

		public void DeleteAllPageContent()
		{
			PageContents = new List<PageContent>();
		}

		public void DeleteAllUsers()
		{
			Users = new List<User>();
		}

		public void SaveOrUpdatePage(Page page)
		{
			Page existingPage = Pages.FirstOrDefault(x => x.Id == page.Id);

			if (existingPage == null)
			{
				Pages.Add(page);
			}
			else
			{
				existingPage.CreatedBy = page.CreatedBy;
				existingPage.CreatedOn = page.CreatedOn;
				existingPage.IsLocked = page.IsLocked;
				existingPage.ModifiedBy = page.ModifiedBy;
				existingPage.ModifiedOn = page.ModifiedOn;
				existingPage.Tags = page.Tags;
				existingPage.Title = page.Title;
			}
		}

		public PageContent AddNewPage(Page page, string text, string editedBy, DateTime editedOn)
		{
			Pages.Add(page);

			PageContent content = new PageContent();
			content.Id = Guid.NewGuid();
			content.EditedBy = editedBy;
			content.EditedOn = editedOn;
			content.Page = page;
			content.Text = text;
			content.VersionNumber = 1;
			PageContents.Add(content);

			return content;
		}

		public PageContent AddNewPageContentVersion(Page page, string text, string editedBy, DateTime editedOn, int version)
		{
			PageContent content = new PageContent();
			content.Id = Guid.NewGuid();
			content.EditedBy = editedBy;
			content.EditedOn = editedOn;
			content.Page = page;
			content.Text = text;
			content.VersionNumber = FindPageContentsByPageId(page.Id).Max(x => x.VersionNumber) +1;
			PageContents.Add(content);

			return content;
		}

		public void UpdatePageContent(PageContent content)
		{
			PageContent existingContent = PageContents.FirstOrDefault(x => x.Id == content.Id);

			if (existingContent == null)
			{
				PageContents.Add(content);
			}
			else
			{
				existingContent.EditedOn = content.EditedOn;
				existingContent.EditedBy = content.EditedBy;
				existingContent.Text = content.Text;
				existingContent.VersionNumber = content.VersionNumber;
			}
		}

		public void SaveOrUpdateUser(User user)
		{
			User existingUser = Users.FirstOrDefault(x => x.Id == user.Id);

			if (existingUser == null)
			{
				Users.Add(user);
			}
			else
			{
				user.ActivationKey = user.ActivationKey;
				user.Email = user.Email;
				user.Firstname = user.Firstname;
				user.IsActivated = user.IsActivated;
				user.IsAdmin = user.IsAdmin;
				user.IsEditor = user.IsEditor;
				user.Lastname = user.Lastname;
				user.Password = user.Password;
				user.PasswordResetKey = user.PasswordResetKey;
				user.Username = user.Username;
				user.Salt = user.Salt;
			}
		}

		public void SaveSitePreferences(SitePreferences preferences)
		{
			SitePreferences = preferences;
		}

		public SitePreferences GetSitePreferences()
		{
			return SitePreferences;
		}

		public void Startup(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			
		}

		public void Install(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			
		}

		public void Test(DataStoreType dataStoreType, string connectionString)
		{
			
		}

		public void Upgrade(IConfigurationContainer configuration)
		{

		}

		#endregion

		#region IPageRepository Members

		public IEnumerable<Page> AllPages()
		{
			return Pages;
		}

		public Page GetPageById(int id)
		{
			return Pages.FirstOrDefault(p => p.Id == id);
		}

		public IEnumerable<Page> FindPagesByCreatedBy(string username)
		{
			return Pages.Where(p => p.CreatedBy == username);
		}

		public IEnumerable<Page> FindPagesByModifiedBy(string username)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Page> FindPagesContainingTag(string tag)
		{
			return Pages.Where(p => p.Tags.ToLower().Contains(tag.ToLower()));
		}

		public IEnumerable<string> AllTags()
		{
			return Pages.Select(x => x.Tags);
		}

		public Page GetPageByTitle(string title)
		{
			return Pages.FirstOrDefault(p => p.Title == title);
		}

		public PageContent GetLatestPageContent(int pageId)
		{
			return PageContents.Where(p => p.Page.Id == pageId).OrderByDescending(x => x.EditedOn).FirstOrDefault();
		}

		public PageContent GetPageContentById(Guid id)
		{
			return PageContents.FirstOrDefault(p => p.Id == id);
		}

		public PageContent GetPageContentByPageIdAndVersionNumber(int id, int versionNumber)
		{
			return PageContents.FirstOrDefault(p => p.Page.Id == id && p.VersionNumber == versionNumber);
		}

		public PageContent GetPageContentByVersionId(Guid versionId)
		{
			return PageContents.FirstOrDefault(p => p.Id == versionId);
		}

		public PageContent GetPageContentByEditedBy(string username)
		{
			return PageContents.FirstOrDefault(p => p.EditedBy == username);
		}

		public IEnumerable<PageContent> FindPageContentsByPageId(int pageId)
		{
			return PageContents.Where(p => p.Page.Id == pageId).ToList();
		}

		public IEnumerable<PageContent> FindPageContentsEditedBy(string username)
		{
			return PageContents.Where(p => p.EditedBy == username);
		}

		public IEnumerable<PageContent> AllPageContents()
		{
			return PageContents;
		}

		#endregion

		#region IUserRepository Members

		public User GetAdminById(Guid id)
		{
			return Users.FirstOrDefault(x => x.Id == id && x.IsAdmin);
		}

		public User GetUserByActivationKey(string key)
		{
			return Users.FirstOrDefault(x => x.ActivationKey == key && x.IsActivated == false);
		}

		public User GetEditorById(Guid id)
		{
			return Users.FirstOrDefault(x => x.Id == id && x.IsEditor);
		}

		public User GetUserByEmail(string email, bool isActivated = true)
		{
			return Users.FirstOrDefault(x => x.Email == email && x.IsActivated == isActivated);
		}

		public User GetUserById(Guid id, bool isActivated = true)
		{
			return Users.FirstOrDefault(x => x.Id == id && x.IsActivated == isActivated);
		}

		public User GetUserByPasswordResetKey(string key)
		{
			return Users.FirstOrDefault(x => x.PasswordResetKey == key);
		}

		public User GetUserByUsername(string username)
		{
			return Users.FirstOrDefault(x => x.Username == username);
		}

		public User GetUserByUsernameOrEmail(string username, string email)
		{
			return Users.FirstOrDefault(x => x.Username == username || x.Email == email);
		}

		public IEnumerable<User> FindAllEditors()
		{
			return Users.Where(x => x.IsEditor).ToList();
		}

		public IEnumerable<User> FindAllAdmins()
		{
			return Users.Where(x => x.IsAdmin).ToList();
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			
		}

		#endregion
	}
}
