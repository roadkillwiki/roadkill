using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Database.Schema
{
	public class MySqlSchema : SchemaBase
	{
		protected override IEnumerable<string> GetCreateStatements()
		{
			string sql = LoadFromResource("Roadkill.Core.Database.Schema.MySql.Create.sql");
			return new string[] { sql };
		}

		protected override IEnumerable<string> GetDropStatements()
		{
			string sql = LoadFromResource("Roadkill.Core.Database.Schema.MySql.Drop.sql");
			return new string[] { sql };
		}

		protected override IEnumerable<string> GetUpgradeStatements()
		{
			string sql = LoadFromResource("Roadkill.Core.Database.Schema.MySql.Upgrade.sql");
			return new string[] { sql };
		}
	}
}
