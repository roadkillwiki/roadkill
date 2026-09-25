using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Roadkill.Core.Database.MongoDB
{
	/// <summary>
	/// Shared MongoDB plumbing for the repositories, replacing the legacy (v1) MongoServer API.
	/// </summary>
	internal static class MongoDbStore
	{
		private static readonly object _lock = new object();
		private static bool _isRegistered;

		/// <summary>
		/// Registers the Guid serializer using the C# legacy representation, which is the format that Roadkill 2.x
		/// (MongoDB driver 2.1) used to store Guids. This keeps existing databases readable.
		/// </summary>
		public static void EnsureSerializersRegistered()
		{
			if (_isRegistered)
				return;

			lock (_lock)
			{
				if (_isRegistered)
					return;

				try
				{
					BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));
				}
				catch (BsonSerializationException)
				{
					// Already registered by the host application.
				}

				_isRegistered = true;
			}
		}

		public static IMongoDatabase GetDatabase(string connectionString)
		{
			EnsureSerializersRegistered();

			string databaseName = MongoUrl.Create(connectionString).DatabaseName;
			MongoClient client = new MongoClient(connectionString);
			return client.GetDatabase(databaseName);
		}

		public static IMongoCollection<T> GetCollection<T>(string connectionString)
		{
			return GetDatabase(connectionString).GetCollection<T>(typeof(T).Name);
		}

		public static IQueryable<T> Queryable<T>(string connectionString)
		{
			return GetCollection<T>(connectionString).AsQueryable();
		}

		/// <summary>
		/// Inserts or replaces the document, using its _id (the same behaviour as the legacy MongoCollection.Save method):
		/// an empty id (e.g. a new User with an empty Guid) is generated first, so new documents don't overwrite each other.
		/// </summary>
		public static void SaveOrUpdate<T>(string connectionString, T obj)
		{
			IMongoCollection<T> collection = GetCollection<T>(connectionString);

			BsonMemberMap idMemberMap = BsonClassMap.LookupClassMap(typeof(T)).IdMemberMap;
			if (idMemberMap?.IdGenerator != null && idMemberMap.IdGenerator.IsEmpty(idMemberMap.Getter(obj)))
				idMemberMap.Setter(obj, idMemberMap.IdGenerator.GenerateId(collection, obj));

			BsonValue id = obj.ToBsonDocument()["_id"];
			collection.ReplaceOne(new BsonDocument("_id", id), obj, new ReplaceOptions() { IsUpsert = true });
		}

		public static void Delete<T>(string connectionString, T obj) where T : IDataStoreEntity
		{
			IMongoCollection<T> collection = GetCollection<T>(connectionString);
			collection.DeleteMany(Builders<T>.Filter.Eq(x => x.ObjectId, obj.ObjectId));
		}

		public static void DeleteAll<T>(string connectionString)
		{
			GetCollection<T>(connectionString).DeleteMany(FilterDefinition<T>.Empty);
		}

		public static void Wipe(string connectionString)
		{
			IMongoDatabase database = GetDatabase(connectionString);
			database.DropCollection(typeof(PageContent).Name);
			database.DropCollection(typeof(Page).Name);
			database.DropCollection(typeof(User).Name);
			database.DropCollection(typeof(SiteConfigurationEntity).Name);
		}
	}
}
