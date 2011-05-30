using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	/// <summary>
	/// Represents the database server type that is being used for Roadkill.
	/// </summary>
	public enum DatabaseType
	{
		/// <summary>
		/// A DB2 database
		/// </summary>
		DB2,
		/// <summary>
		/// A Firebird database
		/// </summary>
		Firebird,
		/// <summary>
		/// MySQL database.
		/// </summary>
		MySQL,
		/// <summary>
		/// Postgres database
		/// </summary>
		Postgres,
		/// <summary>
		/// SQLite database.
		/// </summary>
		Sqlite,
		/// <summary>
		/// SQL Server 2005
		/// </summary>
		SqlServer2005,
		/// <summary>
		/// SQL Server 2008.
		/// </summary>
		SqlServer2008,
	}
}
