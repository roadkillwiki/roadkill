using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mindscape.LightSpeed;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Database.Schema;

namespace Roadkill.Core.Database
{
	public class DataStoreType : IEnumerable<DataStoreType>
	{
		private static List<DataStoreType> _allTypes;

		public string Name { get; set; }
		public string Description { get; set; }
		public bool RequiresCustomRepository { get; set; }
		public string CustomRepositoryType { get; set; }
		public DataProvider LightSpeedDbType { get; set; }
		public SchemaBase Schema { get; set; }

		public static readonly DataStoreType MySQL = new DataStoreType("MySQL", "A MySQL database using store.", DataProvider.MySql5, new PostgresSchema());
		public static readonly DataStoreType Postgres = new DataStoreType("Postgres", "A Postgres database store.", DataProvider.PostgreSql8, new PostgresSchema());
		public static readonly DataStoreType Sqlite = new DataStoreType("Sqlite", "A Sqlite database using store.", DataProvider.SQLite3, new SqliteSchema());
		public static readonly DataStoreType SqlServer2005 = new DataStoreType("SqlServer2005", "A SqlServer 2005 (or above) database using store.", DataProvider.SqlServer2005, new SqlServerSchema());
		public static readonly DataStoreType SqlServer2008 = new DataStoreType("SqlServer2008", "A SqlServer 2008 database using store.", DataProvider.SqlServer2008, new SqlServerSchema());
		public static readonly DataStoreType SqlServerCe = new DataStoreType("SqlServerCe", "A SqlServer Ce database using store.", DataProvider.SqlServerCE4, new SqlServerCESchema());
		public static readonly DataStoreType MongoDB = new DataStoreType("MongoDB", "A MongoDB server, using the official MongoDB driver.", typeof(MongoDBRepository).FullName);

		public static IEnumerable<DataStoreType> AllTypes
		{
			get { return _allTypes; }
		}

		static DataStoreType()
		{
			_allTypes = new List<DataStoreType>()
			{
				MongoDB, MySQL, Postgres, Sqlite, SqlServerCe, SqlServer2005, SqlServer2008
			};
		}

		public DataStoreType(string name, string description, string customRepositoryType)
		{
			Name = name;
			Description = description;
			RequiresCustomRepository = true;
			CustomRepositoryType = customRepositoryType;
			LightSpeedDbType = DataProvider.Custom;
		}

		public DataStoreType(string name, string description, DataProvider lightSpeedDbType, SchemaBase schema)
		{
			Name = name;
			Description = description;
			RequiresCustomRepository = false;
			CustomRepositoryType = "";
			LightSpeedDbType = lightSpeedDbType;
			Schema = schema;
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
			// default to SQL Server
			if (string.IsNullOrEmpty(name))
				name = "SqlServer2005";

			DataStoreType dataStoreType = DataStoreType.AllTypes.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
			if (dataStoreType == null)
				throw new DatabaseException("Unable to find a data store provider for " + name, null);

			return dataStoreType;
		}

		public override string ToString()
		{
			return string.Format("{0} - {1} (custom repository: {2})", Name, Description, RequiresCustomRepository);
		}
	}
}
