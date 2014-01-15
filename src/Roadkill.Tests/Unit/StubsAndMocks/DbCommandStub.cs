using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class DbCommandStub : IDbCommand
	{
		public string CommandText { get; set; }
		public int CommandTimeout { get; set; }
		public CommandType CommandType { get; set; }
		public IDbConnection Connection { get; set; }
		public IDbTransaction Transaction { get; set; }
		public UpdateRowSource UpdatedRowSource { get; set; }
		public IDataParameterCollection Parameters { get; set; }

		public IDbDataParameter CreateParameter()
		{
			return null;
		}

		public void Cancel()
		{
		}

		public int ExecuteNonQuery()
		{
			return 1;
		}

		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			return null;
		}

		public IDataReader ExecuteReader()
		{
			return null;
		}

		public object ExecuteScalar()
		{
			return 1;
		}

		public void Prepare()
		{
		}

		public void Dispose()
		{
		}
	}
}
