using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Services;

namespace Roadkill.Core.Import
{
	/// <summary>
	/// Represents a class that can import page data from a database source.
	/// </summary>
	public interface IWikiImporter
	{
		/// <summary>
		/// Imports page data from a database using the provided connection string.
		/// </summary>
		/// <param name="connectionString">The database connection string.</param>
		void ImportFromSqlServer(string connectionString);

		/// <summary>
		/// Updates the search index after a successful import.
		/// </summary>
		/// <param name="searchService">The search manager to use for the update.</param>
		void UpdateSearchIndex(SearchService searchService);
	}
}
