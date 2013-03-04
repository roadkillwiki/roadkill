using System;
using System.Linq;
using Roadkill.Core.Converters;

namespace Roadkill.Core
{
	/// <summary>
	/// Defines a repository for storing and retrieving Roadkill domain objects in a data store.
	/// </summary>
	public interface IRepository : IPageRepository, IUserRepository, IDisposable
	{
		/// <summary>
		/// Delete a Roadkill domain object in the data store.
		/// </summary>
		/// <typeparam name="T">A Roadkill domain object to delete.</typeparam>
		void Delete<T>(T obj) where T : DataStoreEntity;

		/// <summary>
		/// Deletes all Roadkill domain objects in the data store of the type supplied.
		/// </summary>
		/// <typeparam name="T">A Roadkill domain object to delete.</typeparam>
		void DeleteAll<T>() where T : DataStoreEntity;

		/// <summary>
		/// Retrieves a LINQ queryable object for any of the Roadkill domain objects (Page, PageContent, User, SitePreferences).
		/// </summary>
		/// <typeparam name="T">A Roadkill domain object type.</typeparam>
		/// <returns>A LINQ queryable object </returns>
		IQueryable<T> Queryable<T>() where T : DataStoreEntity;

		/// <summary>
		/// Updates a Roadkill domain object in the data store, or inserts if it doesn't exist.
		/// </summary>
		/// <typeparam name="T">A Roadkill domain object type.</typeparam>
		/// <param name="obj">A Roadkill domain object (Page, PageContent, User, SitePreferences) to store in the database.</param>
		void SaveOrUpdate<T>(T obj) where T : DataStoreEntity;

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
