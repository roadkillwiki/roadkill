using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Database.Schema
{
	public class SqlServerCESchema : SchemaBase
	{
		// A SQL database that can't run scripts as a batch!

		protected override void RunStatement(string statement, IDbCommand command)
		{
			try
			{
				base.RunStatement(statement, command);
			}
			catch (SqlCeException ex)
			{
				if (!ex.Message.Contains("The specified table does not exist"))
				{
					throw new DatabaseException("Failed to run: "+statement, ex);
				}
			}
		}

		protected override IEnumerable<string> GetCreateStatements()
		{
			List<string> scripts = new List<string>();
			scripts.Add(LoadFromResource("Roadkill.Core.Database.Schema.SqlServerCE.Create1.sql"));
			scripts.Add(LoadFromResource("Roadkill.Core.Database.Schema.SqlServerCE.Create2.sql"));
			scripts.Add(LoadFromResource("Roadkill.Core.Database.Schema.SqlServerCE.Create3.sql"));
			scripts.Add(LoadFromResource("Roadkill.Core.Database.Schema.SqlServerCE.Create4.sql"));
			scripts.Add(LoadFromResource("Roadkill.Core.Database.Schema.SqlServerCE.Create5.sql"));

			return scripts.ToArray();
		}

		protected override IEnumerable<string> GetDropStatements()
		{
			List<string> scripts = new List<string>();
			scripts.Add(LoadFromResource("Roadkill.Core.Database.Schema.SqlServerCE.Drop1.sql"));
			scripts.Add(LoadFromResource("Roadkill.Core.Database.Schema.SqlServerCE.Drop2.sql"));
			scripts.Add(LoadFromResource("Roadkill.Core.Database.Schema.SqlServerCE.Drop3.sql"));
			scripts.Add(LoadFromResource("Roadkill.Core.Database.Schema.SqlServerCE.Drop4.sql"));

			return scripts.ToArray();
		}

		protected override IEnumerable<string> GetUpgradeStatements()
		{
			return new string[] { };
		}
	}
}
