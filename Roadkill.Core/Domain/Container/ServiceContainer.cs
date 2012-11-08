using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Search;
using StructureMap;

namespace Roadkill.Core.Domain
{
	public class ServiceContainer : IServiceContainer
	{
		private UserManager _userManager;

		#region IServiceContainer Members

		public UserManager UserManager
		{
			get { return _userManager; }
		}

		public PageManager PageManager
		{
			get { return new PageManager(); }
		}

		public SearchManager SearchManager
		{
			get { return new SearchManager(); }
		}

		public SettingsManager SettingsManager
		{
			get { return new SettingsManager(); }
		}

		public HistoryManager HistoryManager
		{
			get { return new HistoryManager(); }
		}

		public IRepository Repository
		{
			get { return ObjectFactory.GetInstance<IRepository>(); }
		}

		public IConfigurationContainer Configuration
		{
			get { return ObjectFactory.GetInstance<IConfigurationContainer>(); }
		}

		#endregion

		public ServiceContainer()
		{
			if (_userManager == null)
			{
				if (!string.IsNullOrEmpty(RoadkillSettings.Current.ApplicationSettings.UserManagerType))
				{
					_userManager = LoadFromType();
				}
				else
				{
					if (RoadkillSettings.Current.ApplicationSettings.UseWindowsAuthentication)
					{
						_userManager = new ActiveDirectoryUserManager(RoadkillSettings.Current.ApplicationSettings.LdapConnectionString,
																	RoadkillSettings.Current.ApplicationSettings.LdapUsername,
																	RoadkillSettings.Current.ApplicationSettings.LdapPassword,
																	RoadkillSettings.Current.ApplicationSettings.EditorRoleName,
																	RoadkillSettings.Current.ApplicationSettings.AdminRoleName);
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
			Type reflectedType = Type.GetType(RoadkillSettings.Current.ApplicationSettings.UserManagerType);

			if (reflectedType.IsSubclassOf(userManagerType))
			{
				return (UserManager)reflectedType.Assembly.CreateInstance(reflectedType.FullName);
			}
			else
			{
				throw new SecurityException(null, "The type {0} specified in the userManagerType web.config setting is not an instance of a UserManager class",
					RoadkillSettings.Current.ApplicationSettings.UserManagerType);
			}
		}
	}
}
