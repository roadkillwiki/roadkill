using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Database
{
	public class SchemaGenerator
	{
		private DataStoreType _dataStoreType;
		private string _connectionString;

		public SchemaGenerator(DataStoreType databaseType, string connectionString)
		{
			_dataStoreType = databaseType;
			_connectionString = connectionString;
		}

		public IEnumerable<string> Create()
		{
			DatabaseSchema schema = new DatabaseSchema(null, _dataStoreType.InstallSqlType.Value);
			MapTablesAndColumns(schema);

			DdlGeneratorFactory factory = new DdlGeneratorFactory(_dataStoreType.InstallSqlType.Value);
			string[] delimiter = new string[] { ";\r\n" };

			foreach (DatabaseTable table in schema.Tables)
			{
				ITableGenerator generator = factory.TableGenerator(table);
				foreach (string statement in generator.Write().Split(delimiter, StringSplitOptions.RemoveEmptyEntries))
					yield return statement;
			}	

			//SqlWriter writer = new SqlWriter(schema.Tables.First(x => x.Name == "roadkill_siteconfiguration"), _dataStoreType.InstallSqlType.Value);
			string configSql = string.Format("INSERT INTO roadkill_siteconfiguration (Id, Version, Xml) Values('{0}', '{1}', '');",
								SitePreferences.SitePreferencesId,
								ApplicationSettings.Version);

			yield return configSql;
		}

		public void Upgrade()
		{

		}

		public IEnumerable<string> Drop()
		{
			DatabaseSchema schema = new DatabaseSchema(null, _dataStoreType.InstallSqlType.Value);
			MapTablesAndColumns(schema);

			DdlGeneratorFactory factory = new DdlGeneratorFactory(_dataStoreType.InstallSqlType.Value);
			IMigrationGenerator generator = factory.MigrationGenerator();

			yield return "select 'drop table ' || roadkill_pages || ';' from information_schema.tables;";
			yield return "select 'drop table ' || roadkill_pagecontent || ';' from information_schema.tables;";
			yield return "select 'drop table ' || roadkill_users || ';' from information_schema.tables;";
			yield return "select 'drop table ' || roadkill_siteconfiguration || ';' from information_schema.tables;";

			string[] delimiter = new string[] { ";\r\n" };

			StringBuilder sqlBuilder = new StringBuilder();
			foreach (DatabaseTable table in schema.Tables)
			{
				foreach (string statement in generator.DropTable(table).Split(delimiter, StringSplitOptions.RemoveEmptyEntries))
				{
				//	yield return statement;
				}
			}
		}

		public bool HasSchema()
		{
			return true;
		}

		private void MapTablesAndColumns(DatabaseSchema schema)
		{
			schema.AddTable("roadkill_pages")
					.AddColumn<int>("Id").AddPrimaryKey("pk_roadkill_pages").AddIdentity()
					.AddColumn<string>("Title").AddNullable().AddLength(-1)
					.AddColumn<string>("Tags").AddNullable().AddLength(500)
					.AddColumn<string>("CreatedBy").AddLength(200)
					.AddColumn<DateTime>("CreatedOn")
					.AddColumn<bool>("IsLocked")
					.AddColumn<string>("ModifiedBy").AddNullable().AddLength(500)
					.AddColumn<DateTime>("ModifiedOn").AddNullable()
				.AddTable("roadkill_pagecontent")
					.AddColumn<Guid>("Id").AddPrimaryKey("pk_roadkill_pagecontent")
					.AddColumn<string>("EditedBy").AddLength(500)
					.AddColumn<DateTime>("EditedOn")
					.AddColumn<int>("VersionNumber")
					.AddColumn<DateTime>("Text").AddNullable().AddLength(-1)
					.AddColumn<int>("PageId").AddForeignKey("fk_roadkill_pageid", "roadkill_pages")	
				.AddTable("roadkill_users")
					.AddColumn<Guid>("Id").AddPrimaryKey("pk_roadkill_users")
					.AddColumn<string>("ActivationKey").AddNullable().AddLength(64)
					.AddColumn<string>("Email").AddLength(500)
					.AddColumn<string>("Firstname").AddNullable().AddLength(500)
					.AddColumn<string>("Lastname").AddNullable().AddLength(500)
					.AddColumn<bool>("IsEditor")
					.AddColumn<bool>("IsAdmin")
					.AddColumn<bool>("IsActivated")
					.AddColumn<string>("Password").AddLength(500)
					.AddColumn<string>("PasswordResetKey").AddNullable().AddLength(64)
					.AddColumn<string>("Salt").AddLength(500)
					.AddColumn<string>("Username").AddLength(500)
				.AddTable("roadkill_siteconfiguration")
					.AddColumn<Guid>("Id").AddPrimaryKey("pk_roadkill_siteconfiguration")
					.AddColumn<string>("Version").AddLength(20)
					.AddColumn<string>("Xml").AddLength(-1);
		}
	}
}
