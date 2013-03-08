using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;

namespace Roadkill.Core.Database.Schema
{
	[Migration(16)]
	public class Version16 : Migration
	{
		public override void Up()
		{
			Delete.Column("AllowedFileTypes").FromTable("roadkill_siteconfiguration");
			Delete.Column("AllowUserSignup").FromTable("roadkill_siteconfiguration");
			Delete.Column("EnableRecaptcha").FromTable("roadkill_siteconfiguration");
			Delete.Column("MarkupType").FromTable("roadkill_siteconfiguration");
			Delete.Column("RecaptchaPrivateKey").FromTable("roadkill_siteconfiguration");
			Delete.Column("RecaptchaPublicKey").FromTable("roadkill_siteconfiguration");
			Delete.Column("SiteUrl").FromTable("roadkill_siteconfiguration");
			Delete.Column("Title").FromTable("roadkill_siteconfiguration");
			Delete.Column("Theme").FromTable("roadkill_siteconfiguration");

			Alter.Table("roadkill_siteconfiguration").AddColumn("Xml").AsString();
		}

		public override void Down()
		{
			Delete.Column("Xml").FromTable("roadkill_siteconfiguration");

			Alter.Table("roadkill_siteconfiguration")
				.AddColumn("AllowedFileTypes").AsString()
				.AddColumn("AllowUserSignup").AsBoolean()
				.AddColumn("EnableRecaptcha").AsBoolean()
				.AddColumn("MarkupType").AsBoolean()
				.AddColumn("RecaptchaPrivateKey").AsString()
				.AddColumn("RecaptchaPublicKey").AsString()
				.AddColumn("SiteUrl").AsString()
				.AddColumn("Title").AsString()
				.AddColumn("Theme").AsString();
		}
	}
}
