using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;

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

		public string Create()
		{
			DatabaseSchema schema = new DatabaseSchema(null, _dataStoreType.InstallSqlType.Value);
			MapTablesAndColumns(schema);

			DdlGeneratorFactory factory = new DdlGeneratorFactory(_dataStoreType.InstallSqlType.Value);
			ITablesGenerator generator = factory.AllTablesGenerator(schema);
			return generator.Write();
		}

		public void Upgrade()
		{

		}

		public string Drop()
		{
			DatabaseReader dbReader = new DatabaseReader(_connectionString, _dataStoreType.InstallSqlType.Value);
			DatabaseSchema existingSchema = dbReader.ReadAll();

			DatabaseSchema schema = new DatabaseSchema(null, _dataStoreType.InstallSqlType.Value);
			MapTablesAndColumns(schema);

			DdlGeneratorFactory factory = new DdlGeneratorFactory(_dataStoreType.InstallSqlType.Value);
			IMigrationGenerator generator = factory.MigrationGenerator();

			StringBuilder sqlBuilder = new StringBuilder();
			foreach (DatabaseTable table in schema.Tables)
			{
				if (existingSchema.FindTableByName(table.Name) != null)
					sqlBuilder.AppendLine(generator.DropTable(table));
			}

			return sqlBuilder.ToString();
		}

		public bool HasSchema()
		{
			return true;
		}

		private void MapTablesAndColumns(DatabaseSchema schema)
		{
			schema.AddTable("roadkill_pages")
					.AddColumn<int>("Id").AddPrimaryKey("pk_roadkill_pages").AddIdentity()
					.AddColumn<string>("Title").AddNullable()
					.AddColumn<string>("Tags").AddNullable()
					.AddColumn<string>("CreatedBy")
					.AddColumn<DateTime>("CreatedOn")
					.AddColumn<bool>("IsLocked")
					.AddColumn<string>("ModifiedBy").AddNullable()
					.AddColumn<DateTime>("ModifiedOn").AddNullable()
				.AddTable("roadkill_pagecontent")
					.AddColumn<Guid>("Id").AddPrimaryKey("pk_roadkill_pagecontent")
					.AddColumn<string>("EditedBy")
					.AddColumn<DateTime>("EditedOn")
					.AddColumn<int>("VersionNumber")
					.AddColumn<DateTime>("Text").AddNullable()
					.AddColumn<int>("PageId").AddForeignKey("fk_roadkill_pageid", "roadkill_pages")	
				.AddTable("roadkill_users")
					.AddColumn<Guid>("Id").AddPrimaryKey("pk_roadkill_users")
					.AddColumn<string>("ActivationKey").AddNullable()
					.AddColumn<string>("Email")
					.AddColumn<string>("Firstname").AddNullable()
					.AddColumn<string>("Lastname").AddNullable()
					.AddColumn<bool>("IsEditor")
					.AddColumn<bool>("IsAdmin")
					.AddColumn<string>("Password")
					.AddColumn<string>("PasswordResetKey").AddNullable()
					.AddColumn<string>("Salt")
					.AddColumn<string>("Username")
				.AddTable("roadkill_siteconfiguration")
					.AddColumn<Guid>("Id").AddPrimaryKey("pk_roadkill_siteconfiguration")
					.AddColumn<string>("Version")
					.AddColumn<string>("Xml");
		}
	}
}
