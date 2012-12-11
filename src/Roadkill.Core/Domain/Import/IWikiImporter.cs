using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
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
		void ImportFromSql(string connectionString);

		/// <summary>
		/// Indicates whether the implementing class should convert the page sources to Creole wiki format.
		/// </summary>
		bool ConvertToCreole { get; set; }
	}
}
