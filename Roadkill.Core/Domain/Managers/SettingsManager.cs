using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web.Configuration;
using System.DirectoryServices;
using System.Web.Management;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace Roadkill.Core
{
	public class SettingsManager
	{
		public static string ClearPageTables(string connectionString)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					SqlCommand command = connection.CreateCommand();

					command.CommandText = "delete from roadkill_pagecontent";
					command.ExecuteNonQuery();

					command.CommandText = "delete from roadkill_pages";
					command.ExecuteNonQuery();
				}
			}
			catch (Exception e)
			{
				return e.Message;
			}

			return "";
		}

		public static string ClearUserTables(string connectionString)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					SqlCommand command = connection.CreateCommand();

					command.CommandText = "drop table aspnet_SchemaVersions;";
					command.ExecuteNonQuery();

					command.CommandText = "drop table aspnet_Membership;";
					command.ExecuteNonQuery();

					command.CommandText = "drop table aspnet_UsersInRoles;";
					command.ExecuteNonQuery();

					command.CommandText = "drop table aspnet_Roles;";
					command.ExecuteNonQuery();

					command.CommandText = "drop table aspnet_Users;";
					command.ExecuteNonQuery();

					command.CommandText = "drop table aspnet_Users;";
					command.ExecuteNonQuery();
				}
			}
			catch (Exception e)
			{
				return e.Message;
			}
			
			return "";
		}

		/// <summary>
		/// Saves all settings that are stored in the database, to the configuration table.
		/// </summary>
		/// <param name="summary"></param>
		/// <param name="createSchema"></param>
		public static void SaveDbSettings(SettingsSummary summary, bool createSchema)
		{
			SiteConfiguration config;

			if (createSchema)
			{
				NHibernateRepository.Current.Configure(summary.ConnectionString, true, summary.CacheEnabled);
				config = new SiteConfiguration();
			}
			else
			{
				config = SiteConfiguration.Current;
			}

			config.Title = summary.SiteName;
			config.Theme = summary.Theme;
			config.MarkupType = summary.MarkupType;
			config.AllowedFileTypes = summary.AllowedExtensions;
			config.AllowUserSignup = summary.AllowUserSignup;
			config.Version = RoadkillSettings.Version;

			NHibernateRepository.Current.SaveOrUpdate<SiteConfiguration>(config);
		}

		public static void SaveWebConfigSettings(SettingsSummary summary)
		{
			Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");

			if (summary.UseWindowsAuth)
			{
				WriteConfigForLdap(config, summary);
			}
			else
			{
				WriteConfigForDbUsers(config, summary);
			}

			// Roadkill database connection
			ConnectionStringSettings roadkillConnection = new ConnectionStringSettings("Roadkill", summary.ConnectionString);

			if (config.ConnectionStrings.ConnectionStrings["Roadkill"] == null)
				config.ConnectionStrings.ConnectionStrings.Add(roadkillConnection);
			else
				config.ConnectionStrings.ConnectionStrings["Roadkill"].ConnectionString = summary.ConnectionString;

			// The roadkill section
			RoadkillSection section = config.GetSection("roadkill") as RoadkillSection;
			section.AdminRoleName = summary.AdminRoleName;
			section.EditorRoleName = summary.EditorRoleName;
			section.CacheEnabled = summary.CacheEnabled;
			section.CacheText = summary.CacheText;
			section.AttachmentsFolder = summary.AttachmentsFolder;
			section.ConnectionStringName = "Roadkill";
			section.Installed = true;

			config.Save(ConfigurationSaveMode.Minimal);
		}

		private static void WriteConfigForLdap(Configuration config,SettingsSummary summary)
		{
			//
			// LDAP 
			//

			// Use the ActiveDirectoryMembershipProvider
			MembershipSection membershipSection = config.GetSection("system.web/membership") as MembershipSection;
			membershipSection.Providers.Clear();
			membershipSection.Providers.EmitClear = true;
			membershipSection.DefaultProvider = "AspNetActiveDirectoryMembershipProvider";

			ProviderSettings memberSettings = new ProviderSettings("AspNetActiveDirectoryMembershipProvider", "System.Web.Security.ActiveDirectoryMembershipProvider");
			memberSettings.Parameters.Add("connectionStringName", "RoadkillLDAP");
			memberSettings.Parameters.Add("connectionUsername", summary.LdapUsername);
			memberSettings.Parameters.Add("connectionPassword", summary.LdapPassword);
			membershipSection.Providers.Add(memberSettings);

			// Use the Roadkill ActiveDirectoryRoleProvider
			RoleManagerSection roleSection = config.GetSection("system.web/roleManager") as RoleManagerSection;
			roleSection.Enabled = true;
			roleSection.Providers.Clear();
			roleSection.Providers.EmitClear = true;
			roleSection.DefaultProvider = "ActiveDirectoryRoleProvider";

			ProviderSettings roleSettings = new ProviderSettings("ActiveDirectoryRoleProvider", "Roadkill.Core.ActiveDirectoryRoleProvider,RoadKill.Core");
			roleSettings.Parameters.Add("connectionStringName", "RoadkillLDAP");
			roleSettings.Parameters.Add("connectionUsername", summary.LdapUsername);
			roleSettings.Parameters.Add("connectionPassword", summary.LdapPassword);
			roleSection.Providers.Add(roleSettings);

			if (config.ConnectionStrings.ConnectionStrings["RoadkillLDAP"] == null)
			{
				ConnectionStringSettings connection = new ConnectionStringSettings("RoadkillLDAP", summary.LdapConnectionString);
				config.ConnectionStrings.ConnectionStrings.Add(connection);
			}
			else
			{
				config.ConnectionStrings.ConnectionStrings["RoadkillLDAP"].ConnectionString = summary.LdapConnectionString;
			}

			// Turn on Windows authentication
			AuthenticationSection authSection = config.GetSection("system.web/authentication") as AuthenticationSection;
			authSection.Forms.LoginUrl = "";
			authSection.Mode = AuthenticationMode.Windows;

			// Turn off anonymous auth
			AnonymousIdentificationSection anonSection = config.GetSection("system.web/anonymousIdentification") as AnonymousIdentificationSection;
			anonSection.Enabled = false;
		}

		private static void WriteConfigForDbUsers(Configuration config, SettingsSummary summary)
		{
			//
			// SQL Provider
			//

			string usersConnectionName = "Roadkill";

			if (summary.RolesConnectionString != summary.ConnectionString)
			{
				if (config.ConnectionStrings.ConnectionStrings["RoadkillUsers"] == null)
				{
					ConnectionStringSettings connectionUsers = new ConnectionStringSettings("RoadkillUsers", summary.RolesConnectionString);
					config.ConnectionStrings.ConnectionStrings.Add(connectionUsers);
					usersConnectionName = "RoadkillUsers";
				}
				else
				{
					config.ConnectionStrings.ConnectionStrings["RoadkillUsers"].ConnectionString = summary.RolesConnectionString;
				}
			}

			// Use the RoadkillMembershipProvider
			MembershipSection membershipSection = config.GetSection("system.web/membership") as MembershipSection;
			membershipSection.Providers.Clear();
			membershipSection.Providers.EmitClear = true;
			membershipSection.DefaultProvider = "Roadkill";

			ProviderSettings memberSettings = new ProviderSettings("Roadkill", "Roadkill.Core.RoadkillMembershipProvider, Roadkill.Core");
			memberSettings.Parameters.Add("connectionStringName", usersConnectionName);
			memberSettings.Parameters.Add("applicationName", "Roadkill");
			memberSettings.Parameters.Add("minRequiredPasswordLength", "6");
			memberSettings.Parameters.Add("minRequiredNonalphanumericCharacters", "0");
			memberSettings.Parameters.Add("passwordStrengthRegularExpression", "");
			membershipSection.Providers.Add(memberSettings);

			// Use the SqlRoleProvider
			RoleManagerSection roleSection = config.GetSection("system.web/roleManager") as RoleManagerSection;
			roleSection.Enabled = true;
			roleSection.Providers.Clear();
			roleSection.Providers.EmitClear = true;
			roleSection.DefaultProvider = "Roadkill";

			ProviderSettings roleSettings = new ProviderSettings("Roadkill", "System.Web.Security.SqlRoleProvider");
			roleSettings.Parameters.Add("connectionStringName", usersConnectionName);
			roleSettings.Parameters.Add("applicationName", "Roadkill");
			roleSection.Providers.Add(roleSettings);

			// Turn on forms authentication
			AuthenticationSection authSection = config.GetSection("system.web/authentication") as AuthenticationSection;
			authSection.Mode = AuthenticationMode.Forms;
			authSection.Forms.LoginUrl = "~/Home/Login";

			// Turn onanonymous auth
			AnonymousIdentificationSection anonSection = config.GetSection("system.web/anonymousIdentification") as AnonymousIdentificationSection;
			anonSection.Enabled = true;
		}
	}
}
