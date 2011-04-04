using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Web.Configuration;
using System.Configuration;
using System.Data.SqlClient;
using System.Configuration.Provider;
using NHibernate;

namespace Roadkill.Core
{
	/// <summary>
	/// The default access point to the current <see cref="UserManagerBase"/> implementing class that manages all user tasks.
	/// </summary>
	public class SecurityManager
	{
		/// <summary>
		/// Gets the current <see cref="UserManagerBase"/> for the application.
		/// </summary>
		public static UserManagerBase Current
		{
			get
			{
				return Nested.Current;
			}
		}

		/// <summary>
		/// Singleton implementation.
		/// </summary>
		class Nested
		{
			internal static readonly UserManagerBase Current;

			static Nested()
			{
				if (RoadkillSettings.UseWindowsAuthentication)
				{
					Nested.Current = new ActiveDirectoryUserManager(RoadkillSettings.LdapConnectionString,
																RoadkillSettings.LdapUsername,
																RoadkillSettings.LdapPassword,
																RoadkillSettings.EditorRoleName,
																RoadkillSettings.AdminRoleName);
				}
				else
				{
					Nested.Current = new UserManager();
				}
			}
		}

		public static void Initialize()
		{
		}
	}
}
