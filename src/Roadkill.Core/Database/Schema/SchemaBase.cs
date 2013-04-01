using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Database.Schema
{
	public abstract class SchemaBase
	{
		public virtual void Create(IDbCommand command)
		{
			RunStatements(GetCreateStatements(), command);
		}

		public virtual void Drop(IDbCommand command)
		{
			RunStatements(GetDropStatements(), command);
		}

		public virtual void Upgrade(IDbCommand command)
		{
			RunStatements(GetUpgradeStatements(), command);
		}

		private void RunStatements(IEnumerable<string> statements, IDbCommand command)
		{
			foreach (string statement in statements)
			{
				RunStatement(statement, command);
			}
		}

		protected virtual void RunStatement(string statement, IDbCommand command)
		{
			command.CommandText = statement;
			command.ExecuteNonQuery();
		}

		protected string LoadFromResource(string resourcePath)
		{
			if (string.IsNullOrEmpty(resourcePath))
				throw new ArgumentNullException("path", "The path is null or empty");

			Stream stream = typeof(SchemaBase).Assembly.GetManifestResourceStream(resourcePath);
			if (stream == null)
				throw new InvalidOperationException(string.Format("Unable to find '{0}' as an embedded resource", resourcePath));

			string result = "";
			using (StreamReader reader = new StreamReader(stream))
			{
				result = reader.ReadToEnd();
			}

			if (string.IsNullOrEmpty(result))
				Log.Warn("The embedded SQL file {0} is empty or cannot be found.", resourcePath);

			return result;
		}

		protected abstract IEnumerable<string> GetCreateStatements();

		protected abstract IEnumerable<string> GetDropStatements();

		protected abstract IEnumerable<string> GetUpgradeStatements();
	}
}
