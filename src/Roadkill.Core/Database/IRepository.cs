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

		void DeletePageContent(PageContent pageContent);

		void DeleteUser(User user);

		void DeleteAllPages();

		void DeleteAllPageContent();

		void DeleteAllUsers();

		/// <summary>
		/// Updates a Roadkill domain object in the data store, or inserts if it doesn't exist.
		/// </summary>
		/// <typeparam name="T">A Roadkill domain object type.</typeparam>
		/// <param name="obj">A Roadkill domain object (Page, PageContent, User, SitePreferences) to store in the database.</param>
		void SaveOrUpdate<T>(T obj) where T : IDataStoreEntity;

		/// <summary>
		/// Retrieves the <see cref="SitePreferences"/> from the data store. The site preferences object can 
		/// use the <see cref="SitePreferences.ConfigurationId"/> for its identity.
		/// </summary>
		/// <returns>A <see cref="SitePreferences"/> object</returns>
		SitePreferences GetSitePreferences();

		void SaveSitePreferences(SitePreferences preferences);

		void Startup(DataStoreType dataStoreType, string connectionString, bool enableCache);

		void Install(DataStoreType dataStoreType, string connectionString, bool enableCache);

		void Test(DataStoreType dataStoreType, string connectionString);
	}
}
