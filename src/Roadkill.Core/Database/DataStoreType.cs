using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;
using Mindscape.LightSpeed;
using NHibernate.Dialect;
using Roadkill.Core.Database.MongoDB;

namespace Roadkill.Core.Database
{
	public class DataStoreType : IEnumerable<DataStoreType>
	{
		private static List<DataStoreType> _allTypes;

		public string Name { get; set; }
		public string Description { get; set; }
		public bool RequiresCustomRepository { get; set; }
		public string CustomRepositoryType { get; set; }
		public SqlType? InstallSqlType { get; set; }
		public DataProvider LightSpeedDbType { get; set; }

		public static readonly DataStoreType DB2 = new DataStoreType("DB2", "A DB2 database using NHibernate.", SqlType.Db2, DataProvider.DB2);
		public static readonly DataStoreType MySQL = new DataStoreType("MySQL", "A MySQL database using NHibernate.", SqlType.MySql, DataProvider.MySql5);
		public static readonly DataStoreType Postgres = new DataStoreType("Postgres", "A Postgres database using NHibernate.", SqlType.PostgreSql, DataProvider.PostgreSql8);
		public static readonly DataStoreType Sqlite = new DataStoreType("Sqlite", "A Sqlite database using NHibernate.", SqlType.SQLite, DataProvider.SQLite3);
		public static readonly DataStoreType SqlServer2005 = new DataStoreType("SqlServer2005", "A SqlServer 2005 (or above) database using NHibernate.", SqlType.SqlServer, DataProvider.SqlServer2005);
		public static readonly DataStoreType SqlServer2008 = new DataStoreType("SqlServer2008", "A SqlServer 2008 database using NHibernate.", SqlType.SqlServer, DataProvider.SqlServer2008);
		public static readonly DataStoreType SqlServerCe = new DataStoreType("SqlServerCe", "A SqlServer Ce database using NHibernate.", SqlType.SqlServerCe, DataProvider.SqlServerCE4);
		public static readonly DataStoreType MongoDB = new DataStoreType("MongoDB", "A MongoDB server, using the official MongoDB driver.", true, typeof(MongoDBRepository).FullName);

		public static IEnumerable<DataStoreType> AllTypes
		{
			get { return _allTypes; }
		}

		static DataStoreType()
		{
			_allTypes = new List<DataStoreType>()
			{
				DB2, MongoDB, MySQL, Postgres, Sqlite, SqlServerCe, SqlServer2005, SqlServer2008
			};
		}

		public DataStoreType(string name, string description, bool requiresCustomRepository, string customRepositoryType)
			: this(name, description, requiresCustomRepository, customRepositoryType, null, DataProvider.Custom)
		{
		}

		public DataStoreType(string name, string description, SqlType? sqlType, DataProvider lightSpeedDbType)
			: this(name, description, false, "", sqlType, lightSpeedDbType)
		{
		}

		public DataStoreType(string name, string description, bool requiresCustomRepository, string customRepositoryType, SqlType? sqlType, DataProvider lightSpeedDbType)
		{
			Name = name;
			Description = description;
			RequiresCustomRepository = requiresCustomRepository;
			CustomRepositoryType = customRepositoryType;
			InstallSqlType = sqlType;
			LightSpeedDbType = lightSpeedDbType;
		}

		public IEnumerator<DataStoreType> GetEnumerator()
		{
			foreach (DataStoreType type in _allTypes)
			{
				yield return type;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (DataStoreType type in _allTypes)
			{
				yield return type;
			}
		}

		public static DataStoreType ByName(string name)
		{
			return DataStoreType.AllTypes.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
		}

		public override string ToString()
		{
			return string.Format("{0} - {1} (custom repository: {2})", Name, Description, RequiresCustomRepository);
		}
	}
}
