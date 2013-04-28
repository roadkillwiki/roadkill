using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Database.Schema
{
	public class PostgresSchema : SchemaBase
	{
		protected override IEnumerable<string> GetCreateStatements()
		{
			string sql = LoadFromResource("Roadkill.Core.Database.Schema.Postgres.Create.sql");
			return new string[] { sql };
		}

		protected override IEnumerable<string> GetDropStatements()
		{
			string sql = LoadFromResource("Roadkill.Core.Database.Schema.Postgres.Drop.sql");
			return new string[] { sql };
		}

		protected override IEnumerable<string> GetUpgradeStatements()
		{
			string sql = LoadFromResource("Roadkill.Core.Database.Schema.Postgres.Upgrade.sql");
			return new string[] { sql };
		}
	}
}
