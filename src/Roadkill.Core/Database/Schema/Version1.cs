using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;

namespace Roadkill.Core.Database.Schema
{
	[Migration(1)]
	public class Version1 : Migration
	{
		public override void Up()
		{
			if (Schema.Table("roadkill_pagecontent").Exists())
				Delete.Table("roadkill_pagecontent");

			if (Schema.Table("roadkill_pages").Exists())
				Delete.Table("roadkill_pages");

			if (Schema.Table("roadkill_users").Exists())
				Delete.Table("roadkill_users");

			if (Schema.Table("roadkill_siteconfiguration").Exists())
				Delete.Table("roadkill_siteconfiguration");

			CreatePagesTable();
			CreatePageContentTable();
			CreateUsersTable();
			CreateSitePreferencesTable();
		}

		public override void Down()
		{
			if (Schema.Table("roadkill_pagecontent").Exists())
				Delete.Table("roadkill_pagecontent");

			if (Schema.Table("roadkill_pages").Exists())
				Delete.Table("roadkill_pages");

			if (Schema.Table("roadkill_users").Exists())
				Delete.Table("roadkill_users");

			if (Schema.Table("roadkill_siteconfiguration").Exists())
				Delete.Table("roadkill_siteconfiguration");
		}

		private void CreatePagesTable()
		{
			Create.Table("roadkill_pages")
				.WithColumn("Id").AsInt32().Identity().PrimaryKey()
				.WithColumn("Title").AsString().Nullable()
				.WithColumn("Tags").AsString().Nullable()
				.WithColumn("CreatedBy").AsString().Nullable()
				.WithColumn("CreatedOn").AsDateTime().Nullable()
				.WithColumn("IsLocked").AsBoolean().Nullable()
				.WithColumn("ModifiedBy").AsString().Nullable()
				.WithColumn("ModifiedOn").AsString().Nullable();
		}

		private void CreatePageContentTable()
		{
			Create.Table("roadkill_pagecontent")
				.WithColumn("Id").AsGuid().PrimaryKey()
				.WithColumn("EditedBy").AsString()
				.WithColumn("EditedOn").AsDateTime()
				.WithColumn("VersionNumber").AsInt32()
				.WithColumn("Text").AsString()
				.WithColumn("PageId").AsInt32().ForeignKey("roadkill_pagecontent_fk_pageid", "roadkill_pages", "Id");
		}

		private void CreateUsersTable()
		{
			Create.Table("roadkill_users")
				.WithColumn("Id").AsGuid().PrimaryKey()
				.WithColumn("ActivationKey").AsString()
				.WithColumn("Email").AsString()
				.WithColumn("Firstname").AsString()
				.WithColumn("Lastname").AsString()
				.WithColumn("IsEditor").AsBoolean()
				.WithColumn("IsAdmin").AsBoolean()
				.WithColumn("Password").AsString()
				.WithColumn("PasswordResetKey").AsString()
				.WithColumn("Salt").AsString()
				.WithColumn("Username").AsString();
		}

		private void CreateSitePreferencesTable()
		{
			Create.Table("roadkill_siteconfiguration")
				.WithColumn("Id").AsGuid().PrimaryKey()
				.WithColumn("AllowedFileTypes").AsString()
				.WithColumn("AllowUserSignup").AsBoolean()
				.WithColumn("EnableRecaptcha").AsBoolean()
				.WithColumn("MarkupType").AsBoolean()
				.WithColumn("RecaptchaPrivateKey").AsString()
				.WithColumn("RecaptchaPublicKey").AsString()
				.WithColumn("SiteUrl").AsString()
				.WithColumn("Title").AsString()
				.WithColumn("Theme").AsString()
				.WithColumn("Version").AsString();
		}
	}
}
