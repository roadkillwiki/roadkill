using System;
using System.Linq;
using Roadkill.Core.Converters;

namespace Roadkill.Core
{
	/// <summary>
	/// Defines a repository for storing and retrieving Roadkill domain objects in a data store.
	/// </summary>
	public interface IRepository
	{
		/// <summary>
		/// Configures the repository when the Roadkill application is first run, or can be used for 
		/// reconfiguring the repository when settings change.
		/// </summary>
		/// <param name="databaseType">Specifies the data store type.</param>
		/// <param name="connection">The connection string to the data store.</param>
		/// <param name="createSchema">If true, then the Configure method will wipe all data from the data store and 
		/// recreate the tables (if needed)</param>
		/// <param name="enableCache">If true, then caching between the datastore and the Roadkill application is turned on.</param>
		void Configure(DatabaseType databaseType, string connection, bool createSchema, bool enableCache);

		/// <summary>
		/// Delete a Roadkill domain object in the data store.
		/// </summary>
		/// <typeparam name="T">A Roadkill domain object to delete.</typeparam>
		void Delete<T>(T obj) where T : class;

		/// <summary>
		/// Deletes all Roadkill domain objects in the data store of the type supplied.
		/// </summary>
		/// <typeparam name="T">A Roadkill domain object to delete.</typeparam>
		void DeleteAll<T>() where T : class;

		/// <summary>
		/// Retrieves a LINQ queryable object for any of the Roadkill domain objects (Page, PageContent, User, SitePreferences).
		/// </summary>
		/// <typeparam name="T">A Roadkill domain object type.</typeparam>
		/// <returns>A LINQ queryable object </returns>
		IQueryable<T> Queryable<T>();

		/// <summary>
		/// Updates a Roadkill domain object in the data store, or inserts if it doesn't exist.
		/// </summary>
		/// <typeparam name="T">A Roadkill domain object type.</typeparam>
		/// <param name="obj">A Roadkill domain object (Page, PageContent, User, SitePreferences) to store in the database.</param>
		void SaveOrUpdate<T>(T obj) where T : class;

		/// <summary>
		/// Retrieves a page from the data store by its title.
		/// </summary>
		/// <param name="title">The title of the page.</param>
		/// <returns>A <see cref="Page"/> object for the title, or null if the title cannot be found.</returns>
		Page FindPageByTitle(string title);

		/// <summary>
		/// Retrieves the latest version of a page's textual content from the data store.
		/// </summary>
		/// <param name="pageId">The id of the page to lookup</param>
		/// <returns>A <see cref="PageContent"/> object for the page id, or null if it doesn't exist.</returns>
		PageContent GetLatestPageContent(int pageId);

		/// <summary>
		/// Retrieves the <see cref="SitePreferences"/> from the data store. The site preferences object can 
		/// use the <see cref="SitePreferences.ConfigurationId"/> for its identity.
		/// </summary>
		/// <returns>A <see cref="SitePreferences"/> object</returns>
		SitePreferences GetSitePreferences();

		/// <summary>
		/// Retrieves a LINQ object (<see cref="Queryable{Page}"/>) object to queries with. This object 
		/// is only ever used for reads, and not inserts/updates/deletes.
		/// </summary>
		IQueryable<Page> Pages { get; }

		/// <summary>
		/// Retrieves a LINQ object (<see cref="Queryable{PageContent}"/>) object to queries with. This object 
		/// is only ever used for reads, and not inserts/updates/deletes.
		/// </summary>
		IQueryable<PageContent> PageContents { get; }

		/// <summary>
		/// Retrieves a LINQ object (<see cref="Queryable{User}"/>) object to queries with. This object 
		/// is only ever used for reads, and not inserts/updates/deletes.
		/// </summary>
		IQueryable<User> Users { get; }
	}
}
