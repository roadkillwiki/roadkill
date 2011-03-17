using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;
using System.Web.Management;
using System.Data.SqlClient;
using Roadkill.Core.Converters;
using Roadkill.Core.Search;

namespace Roadkill.Core.Controllers
{
	public class InstallController : ControllerBase
    {
		public ActionResult Index()
		{
			//if (RoadkillSettings.Installed)
			//	return RedirectToAction("Index", "Home");

			return View();
		}

		[HttpPost]
		public ActionResult Index(string connectionString, string adminPassword)
		{
			string databaseName = "";

			try
			{
				using (SqlConnection connection = new SqlConnection(RoadkillSettings.ConnectionString))
				{
					connection.Open();
					databaseName = connection.Database;
				}
			}
			catch (SqlException)
			{
				throw new InstallerException("No database name was specified in the connection string");
			}

			if (string.IsNullOrEmpty(databaseName))
				throw new InstallerException("No database name was specified in the connection string");

			// Create the provider database and schema
			SqlServices.Install(databaseName, SqlFeatures.Membership | SqlFeatures.RoleManager, RoadkillSettings.ConnectionString);

			// Create the roadkill schema
			RoadkillSettings.InstallDb();

			//drop table aspnet_SchemaVersions;
			//drop table aspnet_Membership;
			//drop table aspnet_UsersInRoles;
			//drop table aspnet_Roles;
			//drop table aspnet_Users;
			//drop table aspnet_Applications;
			//SearchManager.BuildIndex();

			// Add the admin user, admin role and editor roles.
			UserManager manager = new UserManager();
			manager.AddRoles();
			string result = manager.AddAdminUser("admin",adminPassword);
			if (!string.IsNullOrEmpty(result))
			{
				//throw new InstallerException(result);
				// Do nothing, for now. The passwords may be out of sync which 
				// requires the view being changed to accomodate this.
			}

			// Update the web.config to indicate install is complete
			RoadkillSettings.SaveWebConfig(connectionString);

			return View("InstallComplete");
		}
    }
}
