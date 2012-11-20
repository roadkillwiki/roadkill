using System;
using System.Linq;
using Roadkill.Core.Converters;

namespace Roadkill.Core
{
	public interface IRepository
	{
		void Configure(DatabaseType databaseType, string connection, bool createSchema, bool enableL2Cache);
		void Delete<T>(T obj) where T : class;
		void DeleteAll<T>() where T : class;
		int ExecuteNonQuery(string sql);
		IQueryable<T> Queryable<T>();
		void SaveOrUpdate<T>(T obj) where T : class;
		PageContent GetLatestPageContent(int pageId);
		SitePreferences GetSitePreferences();
		IQueryable<Page> Pages { get; }
		IQueryable<PageContent> PageContents { get; }
		IQueryable<User> Users { get; }
	}
}
