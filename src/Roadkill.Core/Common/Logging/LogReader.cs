using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace Roadkill.Core.Common
{
	public class LogReader
	{
		public static readonly string LOGFILE;

		static LogReader()
		{
			LOGFILE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "roadkill.log");
		}

		public static IEnumerable<Log4jEvent> CachedItems
		{
			get
			{
				List<Log4jEvent> items = _entityCache.Get("key") as List<Log4jEvent>;
				if (items == null)
				{
					Log.Information("Refreshing the log cache...it's logs all the way down.");
					return Load();
				}
				else
				{
					return items;
				}
			}
		}
		internal static MemoryCache _entityCache = new MemoryCache("EntityCache");

		public static IEnumerable<Log4jEvent> Load(string filename = "")
		{
			if (string.IsNullOrEmpty(filename))
				filename = Log4jXmlTraceListener.GetRollingFilename(LOGFILE);

			List<Log4jEvent> items = new List<Log4jEvent>();
			if (File.Exists(filename))
			{
				items = Log4jEvent.Deserialize(File.ReadAllText(filename)).ToList();
				_entityCache.Add("key", items, DateTimeOffset.Now.AddSeconds(10));
			}

			return items;
		}
	}
}
