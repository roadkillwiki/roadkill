using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using Roadkill.Core.Configuration;

namespace Roadkill.Core
{
	public class MongoDBRepository : IRepository
	{
		private IConfigurationContainer _configuration;

		public MongoDBRepository(IConfigurationContainer config)
		{
			_configuration = config;
		}

		public void Configure(DataStoreType dataStoreType, string connection, bool createSchema, bool enableCache)
		{
			string databaseName = MongoUrl.Create(connection).DatabaseName;

			if (createSchema)
			{
				MongoClient client = new MongoClient(connection);
				MongoServer server = client.GetServer();
				MongoDatabase database = server.GetDatabase(databaseName);
				database.DropCollection("Pages");
				database.DropCollection("PageContents");
				database.DropCollection("Users");
				database.DropCollection("SitePreferences");
			}
			else
			{
				// Just test
				MongoClient client = new MongoClient(connection);
				MongoServer server = client.GetServer();
				MongoDatabase database = server.GetDatabase(databaseName);
			}
		}

		public void Delete<T>(T obj) where T : DataStoreEntity
		{
			MongoCollection<T> collection = GetCollection<T>();
			IMongoQuery query = Query.EQ("ObjectId", obj.ObjectId);
			collection.Remove(query);

			//if (_configuration.ApplicationSettings.CacheEnabled)
			//{
			//	TypedMemoryCache.Remove<MongoCollection<T>>(collection);
			//}
		}

		public void DeleteAll<T>() where T : DataStoreEntity
		{
			MongoCollection<T> collection = GetCollection<T>();
			collection.RemoveAll();

			//if (_configuration.ApplicationSettings.CacheEnabled)
			//{
			//	TypedMemoryCache.ClearCache();
			//}
		}

		public IQueryable<T> Queryable<T>() where T : DataStoreEntity
		{
			MongoCollection<T> collection;
			
			//if (_configuration.ApplicationSettings.CacheEnabled)
			//{
			//	collection = TypedMemoryCache.Get<MongoCollection<T>>();

			//	if (collection != null)
			//		return collection.AsQueryable<T>();
			//}

			collection = GetCollection<T>();
			TypedMemoryCache.Add<MongoCollection<T>>(collection);

			return collection.AsQueryable<T>();
		}

		public void SaveOrUpdate<T>(T obj) where T : DataStoreEntity
		{
			Page page = obj as Page;
			if (page != null && page.Id	== 0)
			{
				int newId = 1;
				Page recentPage = Queryable<Page>().OrderByDescending(x => x.Id).FirstOrDefault();
				if (recentPage != null)
				{
					newId = recentPage.Id + 1;
				}

				obj.ObjectId = Guid.NewGuid();
				page.Id = newId;
			}

			MongoCollection<T> collection = GetCollection<T>();
			collection.Save<T>(obj);
		}

		public Page FindPageByTitle(string title)
		{
			return Queryable<Page>().FirstOrDefault(x => x.Title == title);
		}

		public PageContent GetLatestPageContent(int pageId)
		{
			return Queryable<PageContent>()
				.Where(x => x.Page.Id == pageId)
				.OrderByDescending(x => x.EditedOn)
				.FirstOrDefault();
		}

		public SitePreferences GetSitePreferences()
		{
			return Queryable<SitePreferences>().FirstOrDefault();
		}

		public IQueryable<Page> Pages
		{
			get 
			{
				return Queryable<Page>();
			}
		}

		public IQueryable<PageContent> PageContents
		{
			get
			{
				return Queryable<PageContent>();
			}
		}

		public IQueryable<User> Users
		{
			get
			{
				return Queryable<User>();
			}
		}

		private MongoCollection<T> GetCollection<T>(string connectionString = "")
		{
			if (string.IsNullOrEmpty(connectionString))
				connectionString = _configuration.ApplicationSettings.ConnectionString;

			string databaseName = MongoUrl.Create(connectionString).DatabaseName;
			MongoClient client = new MongoClient(connectionString);
			MongoServer server = client.GetServer();
			MongoDatabase database = server.GetDatabase(databaseName);

			return database.GetCollection<T>(typeof(T).Name);
		}
	}

	// IQueryable cache that doesn't work
	public class TypedMemoryCache
	{
		internal static MemoryCache _entityCache = new MemoryCache("EntityCache");

		public static void Add<T>(T obj) where T : MongoCollection
		{
			_entityCache.Add(typeof(T).FullName, obj, new CacheItemPolicy());
		}

		public static T Get<T>() where T : MongoCollection
		{
			return (T) _entityCache.Get(typeof(T).FullName);
		}

		public static void Remove<T>(T obj) where T : MongoCollection
		{
			_entityCache.Remove(typeof(T).FullName);
		}

		public static void ClearCache()
		{
			_entityCache.Dispose();
			_entityCache = new MemoryCache("EntityCache");
		}

		public IQueryable<T> GetQueryable<T>() where T : MongoCollection
		{
			IEnumerable<T> filtered = _entityCache.OfType<T>();
			return filtered.ToList().AsQueryable<T>();
		}
	}
}
