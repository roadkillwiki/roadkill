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
		public static readonly string LOG_FILE;
		public static readonly string LOG_DIRECTORY;
		public static readonly string LOG_FILE_SEARCHPATH;
		internal static MemoryCache _logCache = new MemoryCache("LogCache");

		static LogReader()
		{
			LOG_DIRECTORY = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Logs");
			LOG_FILE = Path.Combine(LOG_DIRECTORY, "roadkill.xml.log");
			LOG_FILE_SEARCHPATH = "roadkill.xml.*";
		}

		public static IEnumerable<Log4jEvent> CachedItems
		{
			get
			{
				List<Log4jEvent> items = _logCache.Get("key") as List<Log4jEvent>;
				if (items == null)
				{
					Log.Information("Refreshing the log cache...it's logs all the way down.");
					return LoadAll();
				}
				else
				{
					return items;
				}
			}
		}

		public static IEnumerable<Log4jEvent> LoadAll()
		{
			List<Log4jEvent> items = new List<Log4jEvent>();
			foreach (string file in Directory.EnumerateFiles(LOG_DIRECTORY, LOG_FILE_SEARCHPATH, SearchOption.TopDirectoryOnly))
			{
				items.AddRange(Load(file));
			}

			return items;
		}

		public static IEnumerable<Log4jEvent> Load(string filename = "")
		{
			if (string.IsNullOrEmpty(filename))
				filename = Log4jXmlTraceListener.GetRollingFilename(LOG_FILE);

			List<Log4jEvent> items = new List<Log4jEvent>();
			if (File.Exists(filename))
			{
				items = Log4jEvent.Deserialize(File.ReadAllText(filename)).ToList();
				_logCache.Add("key", items, DateTimeOffset.Now.AddSeconds(10));
			}

			return items;
		}
	}
}
