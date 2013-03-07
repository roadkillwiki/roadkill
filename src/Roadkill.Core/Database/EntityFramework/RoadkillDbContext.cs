using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Database.EntityFramework
{
	public class RoadkillDbContext : DbContext
	{
		public DbSet<Page> Pages { get; set; }
		public DbSet<PageContent> PageContents { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<SitePreferencesEntity> SitePreferences { get; set; }

		public RoadkillDbContext(DbConnection connection) : base(connection, true)
		{
			
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			var pageEntity = modelBuilder.Entity<Page>();
			pageEntity.ToTable("roadkill_pages");
			pageEntity.Ignore(x => x.ObjectId);
			pageEntity.HasKey<int>(x => x.Id);
			pageEntity.Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity).HasColumnName("Id");

			var pageContentEntity = modelBuilder.Entity<PageContent>();
			pageContentEntity.ToTable("roadkill_pagecontent");
			pageContentEntity.Ignore(x => x.ObjectId);
			pageContentEntity.HasKey<Guid>(x => x.Id);
			pageContentEntity.HasRequired<Page>(x => x.Page).WithMany();

			var userEntity = modelBuilder.Entity<User>();
			userEntity.ToTable("roadkill_users");
			userEntity.Ignore(x => x.ObjectId);

			var sitePreferenceEntity = modelBuilder.Entity<SitePreferencesEntity>();
			sitePreferenceEntity.ToTable("roadkill_siteconfiguration");
			sitePreferenceEntity.Ignore(x => x.ObjectId);

			base.OnModelCreating(modelBuilder);
		}

		public static RoadkillDbContext Create(IConfigurationContainer container)
		{
			DataStoreType datastoreType = container.ApplicationSettings.DataStoreType;

			if (datastoreType == DataStoreType.SqlServerCe)
			{
				return new RoadkillDbContext(new SqlCeConnection(container.ApplicationSettings.ConnectionString));
			}
			else if (datastoreType == DataStoreType.Sqlite)
			{
				return new RoadkillDbContext(new SQLiteConnection(container.ApplicationSettings.ConnectionString));
			}
			else if (datastoreType == DataStoreType.MySQL)
			{
				return new RoadkillDbContext(new MySqlConnection(container.ApplicationSettings.ConnectionString));
			}

			// No support for Postgres, DB2, Firebird

			// Default to SQL Server
			return new RoadkillDbContext(new SqlConnection(container.ApplicationSettings.ConnectionString));
		}
	}
}
