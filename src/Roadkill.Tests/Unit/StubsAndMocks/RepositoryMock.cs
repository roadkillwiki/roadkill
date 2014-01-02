using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Plugins;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Tests.Unit
{
	public class RepositoryMock : IRepository
	{
		public List<Page> Pages { get; set; }
		public List<PageContent> PageContents { get; set; }
		public List<User> Users { get; set; }
		public SiteSettings SiteSettings { get; set; }
		public List<TextPlugin> TextPlugins { get; set; }

		// If this is set, GetTextPluginSettings returns it instead of a lookup
		public PluginSettings PluginSettings { get; set; }

		public bool Installed { get; set; }
		public DataStoreType InstalledDataStoreType { get; private set; }
		public string InstalledConnectionString { get; private set; }
		public bool InstalledEnableCache { get; private set; }

		public RepositoryMock()
		{
			Pages = new List<Page>();
			PageContents = new List<PageContent>();
			Users = new List<User>();
			SiteSettings = new SiteSettings();
			TextPlugins = new List<TextPlugin>();
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
			PageContents = new List<PageContent>();
		}

		public void DeleteAllUsers()
		{
			Users = new List<User>();
		}

		public Page SaveOrUpdatePage(Page page)
		{
			Page existingPage = Pages.FirstOrDefault(x => x.Id == page.Id);

			if (existingPage == null)
			{
				page.Id = Pages.Count + 1;
				Pages.Add(page);
				existingPage = page;
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

			return existingPage;
		}

		public PageContent AddNewPage(Page page, string text, string editedBy, DateTime editedOn)
		{
			page.Id = Pages.Count + 1;
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
			page.ModifiedBy = content.EditedBy = editedBy;
			page.ModifiedOn = content.EditedOn = editedOn;
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
				// Do nothing
			}
			else
			{
				existingContent.EditedOn = content.EditedOn;
				existingContent.EditedBy = content.EditedBy;
				existingContent.Text = content.Text;
				existingContent.VersionNumber = content.VersionNumber;
			}
		}

		public User SaveOrUpdateUser(User user)
		{
			User existingUser = Users.FirstOrDefault(x => x.Id == user.Id);

			if (existingUser == null)
			{
				user.Id = Guid.NewGuid();
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

			return user;
		}

		public void SaveSiteSettings(SiteSettings settings)
		{
			SiteSettings = settings;
		}

		public SiteSettings GetSiteSettings()
		{
			return SiteSettings;
		}

		public void SaveTextPluginSettings(TextPlugin plugin)
		{
			int index = TextPlugins.IndexOf(plugin);

			if (index == -1)
				TextPlugins.Add(plugin);
			else
				TextPlugins[index] = plugin;
		}

		public PluginSettings GetTextPluginSettings(Guid databaseId)
		{
			if (PluginSettings != null)
				return PluginSettings;

			TextPlugin savedPlugin = TextPlugins.FirstOrDefault(x => x.DatabaseId == databaseId);

			if (savedPlugin != null)
				return savedPlugin._settings; // DON'T CALL Settings - you'll get a StackOverflowException
			else
				return null;
		}

		public void Startup(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			
		}

		public virtual void Install(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			Installed = true;
			InstalledDataStoreType = dataStoreType;
			InstalledConnectionString = connectionString;
			InstalledEnableCache = enableCache;
		}

		public virtual void TestConnection(DataStoreType dataStoreType, string connectionString)
		{
			
		}

		public void Upgrade(ApplicationSettings settings)
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

		public IEnumerable<Page> FindPagesCreatedBy(string username)
		{
			return Pages.Where(p => p.CreatedBy == username);
		}

		public IEnumerable<Page> FindPagesModifiedBy(string username)
		{
			return Pages.Where(p => p.ModifiedBy == username);
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

		public IEnumerable<PageContent> GetPageContentByEditedBy(string username)
		{
			return PageContents.Where(p => p.EditedBy == username);
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

		public User GetUserByEmail(string email, bool? isActivated = null)
		{
			if (isActivated.HasValue)
				return Users.FirstOrDefault(x => x.Email == email && x.IsActivated == isActivated);
			else
				return Users.FirstOrDefault(x => x.Email == email);
		}

		public User GetUserById(Guid id, bool? isActivated = null)
		{
			if (isActivated.HasValue)
				return Users.FirstOrDefault(x => x.Id == id && x.IsActivated == isActivated);
			else
				return Users.FirstOrDefault(x => x.Id == id);
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
