using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class CacheMock : MemoryCache
	{
		public List<CacheItem> CacheItems { get; set; }

		/// <summary>
		/// A MemoryCache instance named "Roadkill", to mimic the one injected by Structuremap.
		/// </summary>
		public static readonly MemoryCache RoadkillCache = new MemoryCache("Roadkill");

		public CacheMock()
			: base("CacheMock")
		{
			CacheItems = new List<CacheItem>();
		}

		public override bool Add(string key, object value, CacheItemPolicy policy, string regionName = null)
		{
			CacheItems.Add(new CacheItem(key, value));
			return true;
		}

		public override object Get(string key, string regionName = null)
		{
			CacheItem item = CacheItems.FirstOrDefault(x => x.Key == key);
			if (item != null)
				return item.Value;
			else
				return null;
		}

		public override object Remove(string key, string regionName = null)
		{
			CacheItem item = CacheItems.FirstOrDefault(x => x.Key == key);
			if (item != null)
			{
				CacheItems.Remove(item);
				return item.Value;
			}
			else
			{
				return null;
			}
		}

		protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			for (int i = 0; i < CacheItems.Count; i++)
			{
				CacheItem item = CacheItems[i];
				yield return new KeyValuePair<string, object>(item.Key, item.Value);
			}
		}
	}
}
