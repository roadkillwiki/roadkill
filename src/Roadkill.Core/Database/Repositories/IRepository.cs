using System;
using System.Linq;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using StructureMap.Attributes;

namespace Roadkill.Core.Database
{
	/// <summary>
	/// Defines a repository for storing and retrieving Roadkill domain objects in a data store.
	/// </summary>
	public interface IRepository : IPageRepository, IUserRepository, IDisposable
	{
		void DeletePage(Page page);
		/// <summary>
		/// Removes a single version of page contents by its id.
		/// </summary>
		/// <param name="pageContent"></param>
		void DeletePageContent(PageContent pageContent);
		void DeleteUser(User user);
		void DeleteAllPages();
		void DeleteAllPageContent();
		void DeleteAllUsers();

		void SaveOrUpdatePage(Page page);
		PageContent AddNewPage(Page page, string text, string editedBy, DateTime editedOn);
		PageContent AddNewPageContentVersion(Page page, string text, string editedBy, DateTime editedOn, int version);
		void UpdatePageContent(PageContent content); // no new version
		User SaveOrUpdateUser(User user);
		void SaveSiteSettings(SiteSettings siteSettings);
		SiteSettings GetSiteSettings();

		void Startup(DataStoreType dataStoreType, string connectionString, bool enableCache);
		void Install(DataStoreType dataStoreType, string connectionString, bool enableCache);
		void TestConnection(DataStoreType dataStoreType, string connectionString);
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="applicationSettings"></param>
		/// <exception cref="UpgradeException">Thrown if there is a problem with the upgrade. This contains the details of the failure.</exception>
		void Upgrade(ApplicationSettings applicationSettings);
	}
}
