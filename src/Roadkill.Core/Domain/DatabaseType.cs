using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Dialect;

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
		/// <summary>
		/// SQL Server Compact Edition 4.0
		/// </summary>
		SqlServerCe,
		/// <summary>
		/// A MangoDB server.
		/// </summary>
		MongoDb
	}
}
