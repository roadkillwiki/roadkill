using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Cache
{
	public class CacheKeys
	{
		/// <summary>'latesthomepage'</summary>
		public static readonly string HOMEPAGE = "latesthomepage";

		/// <summary>'{id}.{version}'</summary>
		public static readonly string PAGESUMMARY = "{id}.{version}";

		/// <summary>"allpages.with.content"</summary>
		public static readonly string ALLPAGES_CONTENT = "allpages.with.content";

		/// <summary>"allpages"</summary>
		public static readonly string ALLPAGES = "allpages";
		public static readonly string ALLPAGES_CREATEDBY = "allpages.createdby.{username}";
		public static readonly string ALLTAGS = "alltags";
		public static readonly string PAGES_BY_TAG = "pagesbytag.{tag}";
		//findbytag.{0} - rename me

		public static string PageSummaryKey(int id, int version)
		{
			string key = PAGESUMMARY;
			key = key.Replace("{id}", id.ToString());
			key = key.Replace("{version}", version.ToString());

			return key;
		}

		public static string AllPagesCreatedByKey(string username)
		{
			string key = ALLPAGES_CREATEDBY;
			key = key.Replace("{username}", username);

			return key;
		}

		public static string PagesByTagKey(string tag)
		{
			string key = PAGES_BY_TAG;
			key = key.Replace("{tag}", tag);

			return key;
		}
	}
}
