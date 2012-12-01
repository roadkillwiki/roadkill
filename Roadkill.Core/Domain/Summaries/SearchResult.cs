using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Roadkill.Core.Search
{
	/// <summary>
	/// Contains data for a single search result from a search query.
	/// </summary>
	public class SearchResult
	{
		/// <summary>
		/// The page id 
		/// </summary>
		public int Id { get; internal set; }

		/// <summary>
		/// The page title.
		/// </summary>
		public string Title { get; internal set; }

		/// <summary>
		/// The summary of the content (the first 150 characters of text with all HTML removed).
		/// </summary>
		public string ContentSummary { get; internal set; }

		/// <summary>
		/// The length of the content in bytes.
		/// </summary>
		public int ContentLength { get; internal set; }

		/// <summary>
		/// The person who created the page.
		/// </summary>
		public string CreatedBy { get; internal set; }

		/// <summary>
		/// The date the page was created on.
		/// </summary>
		public DateTime CreatedOn { get; internal set; }

		/// <summary>
		/// The tags for the page, in space delimited format.
		/// </summary>
		public string Tags { get; internal set; }

		/// <summary>
		/// The lucene.net score for the search result.
		/// </summary>
		public float Score { get; internal set; }

		public IEnumerable<string> TagsAsList()
		{
			List<string> tags = new List<string>();

			if (!string.IsNullOrEmpty(Tags))
			{
				if (Tags.IndexOf(" ") != -1)
				{
					string[] parts = Tags.Split(' ');
					foreach (string item in parts)
					{
						if (item != " ")
							tags.Add(item);
					}
				}
				else
				{
					tags.Add(Tags.TrimEnd());
				}
			}

			return tags;
		}
	}
}
