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
		public static string GetDatabaseName(string connectionString)
		{
			// Turn this into Sqlite and mySQL friendly
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				return connection.Database;
			}
		}

		public static void InstallAspNetUsersDatabase(SettingsSummary summary)
		{
			SettingsManager.ClearUserTables(summary.RolesConnectionString);

			// Create the provider database and schema
			SqlServices.Install(GetDatabaseName(summary.RolesConnectionString), SqlFeatures.Membership | SqlFeatures.RoleManager, summary.RolesConnectionString);

			// Add the admin user, admin role and editor roles.
			if (UserManager.Current.UserExists("Admin"))
				SettingsManager.ClearUserTables(summary.RolesConnectionString);

			try
			{
				UserManager.Current.AddAdmin("admin", summary.AdminPassword);
			}
			catch (UserException e)
			{
				throw new InstallerException(e.ToString());
			}
		}

		public static void ResetInstalledState()
		{
			Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");

			RoadkillSection section = config.GetSection("roadkill") as RoadkillSection;
			section.Installed = false;

			config.Save();
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
