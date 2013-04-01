using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Managers;

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
		/// <param name="searchManager">The search manager to use for the update.</param>
		void UpdateSearchIndex(SearchManager searchManager);

		/// <summary>
		/// Indicates whether the implementing class should convert the page sources to Creole wiki format.
		/// </summary>
		bool ConvertToCreole { get; set; }
	}
}
