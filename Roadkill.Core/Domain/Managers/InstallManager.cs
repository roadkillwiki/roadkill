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
	public class InstallManager
	{
		/// <summary>
		/// Saves all settings that are stored in the database, to the configuration table.
		/// </summary>
		/// <param name="summary"></param>
		/// <param name="createSchema"></param>
		public static void SaveDbSettings(SettingsSummary summary,bool createSchema)
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
			else
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

		public static void InstallAspNetUsersDatabase(SettingsSummary summary)
		{
			ClearUserTables(summary.RolesConnectionString);

			// Create the provider database and schema
			SqlServices.Install(GetDatabaseName(summary.RolesConnectionString), SqlFeatures.Membership | SqlFeatures.RoleManager, summary.RolesConnectionString);

			// Add the admin user, admin role and editor roles.
			UserManager manager = new UserManager();
			if (manager.UserExists("Admin"))
				ClearUserTables(summary.RolesConnectionString);

			manager.AddRoles();
			string result = manager.AddAdmin("admin", summary.AdminPassword);
			if (!string.IsNullOrEmpty(result))
			{
				throw new InstallerException(result);
			}
		}

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

		public static string TestConnection(string connectionString)
		{
			try
			{
				// Turn this into Sqlite and mySQL friendly
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
				}

				return "";
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}

		public static string GetDatabaseName(string connectionString)
		{
			// Turn this into Sqlite and mySQL friendly
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				return connection.Database;
			}
		}

		public static string TestAttachments(string folder)
		{
			string errors = "";
			if (string.IsNullOrEmpty(folder))
			{
				errors = "The folder name is empty";
			}
			else if (!folder.StartsWith("~/"))
			{
				errors = "The folder name should start with a ~/";
			}
			else
			{
				try
				{
					string directory = HttpContext.Current.Server.MapPath(folder);

					if (Directory.Exists(directory))
					{
						string path = Path.Combine(directory, "_installtest.txt");
						System.IO.File.WriteAllText(path, "created by the installer to test the attachments folder");
					}
					else
					{
						errors = "The directory does not exist, please create it first";
					}
				}
				catch (Exception e)
				{
					errors = e.Message;
				}
			}

			return errors;
		}

		public static void ResetInstalledState()
		{
			Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");

			RoadkillSection section = config.GetSection("roadkill") as RoadkillSection;
			section.Installed = false;

			config.Save();
		}

		public static string TestSaveWebConfig()
		{
			try
			{
				ResetInstalledState();

				return "";
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}

		public static string TestLdapConnection(string connectionString, string username, string password, string groupName)
		{
			if (string.IsNullOrEmpty(connectionString))
				return "The connection string is empty";

			try
			{
				int length = "ldap://".Length;
				if (!connectionString.StartsWith("LDAP://") || connectionString.Length < length)
					throw new Exception(string.Format("The LDAP connection string: '{0}' does not appear to be a valid (make sure it's uppercase LDAP).", connectionString));

				DirectoryEntry entry = new DirectoryEntry(connectionString);

				if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
				{
					entry.Username = username;
					entry.Password = password;
				}
				else
				{
					// Use built-in ones for querying
					username = "administrator"; // may need to use Guest here.
					groupName = "Users";
				}

				string accountName = username;
				string filter = "(&(objectCategory=user)(samAccountName=" + username + "))";

				if (!string.IsNullOrEmpty(groupName))
				{
					filter = "(&(objectCategory=group)(samAccountName=" + groupName + "))";
					accountName = groupName;
				}

				DirectorySearcher searcher = new DirectorySearcher(entry);
				searcher.Filter = filter;
				searcher.SearchScope = SearchScope.Subtree;

				SearchResult searchResult = searcher.FindOne();
				if (searchResult == null)
					return "Warning only: Unable to find " + accountName + " in the AD";
				else
					return "";
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}
	}
}
