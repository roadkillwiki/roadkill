using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StructureMap;

namespace Roadkill.Core.Domain
{
	public class ServiceContainer : IServiceContainer
	{
		private UserManager _userManager;

		#region IServiceContainer Members

		public UserManager UserManager
		{
			get { throw new NotImplementedException(); }
		}

		public PageManager PageManager
		{
			get { throw new NotImplementedException(); }
		}

		public Search.SearchManager SearchManager
		{
			get { throw new NotImplementedException(); }
		}

		public SettingsManager SettingsManager
		{
			get { throw new NotImplementedException(); }
		}

		public HistoryManager HistoryManager
		{
			get { throw new NotImplementedException(); }
		}

		public NHibernateRepository Repository
		{
			get { throw new NotImplementedException(); }
		}

		public IConfigurationContainer Configuration
		{
			get { throw new NotImplementedException(); }
		}

		#endregion

		public ServiceContainer()
		{
			if (_userManager == null)
			{
				if (!string.IsNullOrEmpty(RoadkillSettings.Current.UserManagerType))
				{
					_userManager = LoadFromType();
				}
				else
				{
					if (RoadkillSettings.Current.UseWindowsAuthentication)
					{
						_userManager = new ActiveDirectoryUserManager(RoadkillSettings.Current.LdapConnectionString,
																	RoadkillSettings.Current.LdapUsername,
																	RoadkillSettings.Current.LdapPassword,
																	RoadkillSettings.Current.EditorRoleName,
																	RoadkillSettings.Current.AdminRoleName);
					}
					else
					{
						_userManager = new SqlUserManager();
					}
				}
			}
		}

		public static IServiceContainer Current
		{
			get
			{
				return ObjectFactory.GetInstance<IServiceContainer>();
			}
		}

		public static UserManager LoadFromType()
		{
			// Attempt to load the type
			Type userManagerType = typeof(UserManager);
			Type reflectedType = Type.GetType(RoadkillSettings.Current.UserManagerType);

			if (reflectedType.IsSubclassOf(userManagerType))
			{
				return (UserManager)reflectedType.Assembly.CreateInstance(reflectedType.FullName);
			}
			else
			{
				throw new SecurityException(null, "The type {0} specified in the userManagerType web.config setting is not an instance of a UserManager class", RoadkillSettings.Current.UserManagerType);
			}
		}
	}
}
