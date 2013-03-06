using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace Roadkill.Core.Database
{
	public class ObjectCache
	{
		internal static MemoryCache _entityCache = new MemoryCache("EntityCache");

		public static void Add(DataStoreEntity obj)
		{
			_entityCache.Add(obj.ObjectId.ToString(), obj, new CacheItemPolicy());
		}

		public static DataStoreEntity Get(Guid id)
		{
			return (DataStoreEntity) _entityCache.Get(id.ToString());
		}

		public static void Remove(Guid id)
		{
			_entityCache.Remove(id.ToString());
		}

		public static void ClearCache()
		{
			_entityCache.Dispose();
			_entityCache = new MemoryCache("EntityCache");
		}
	}
}
