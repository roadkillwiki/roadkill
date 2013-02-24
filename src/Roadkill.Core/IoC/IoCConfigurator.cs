using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Controllers;
using Roadkill.Core.Converters;
using StructureMap;

namespace Roadkill.Core
{
	/// <summary>
	/// TODO: make this into less of a mess
	/// </summary>
	public class IoCConfigurator
	{
		/// <summary>
		/// Initializes the Structuremap IoC containers for the Services, Configuration and IRepository,
		/// and registering the defaults for each.
		/// No settings are loaded from the database in this method, or config file settings loaded, except the
		/// <see cref="ApplicationSettings.UserManagerType"/> setting.
		/// </summary>
		/// <param name="config">If null, then a new per-thread/http request <see cref="RoadkillSettings"/> class is used for the configuration.</param>
		/// <param name="context">If null, then a new per-thread/http request <see cref="RoadkillContext"/> is used.</param>
		/// <param name="repository">If null, then a new per-thread/http request <see cref="NHibernateRepository"/> is used.</param>
		public static void Setup(IConfigurationContainer config = null, IRepository repository = null, IRoadkillContext context = null)
		{
			if (config != null && config.ApplicationSettings == null)
				throw new IoCException("The IConfigurationContainer.ApplicationSettings of the config parameter is null - " + ObjectFactory.WhatDoIHave(), null);

			// The order of the calls is important as the default concrete types have a dependency order:
			// - IRepository relies on IConfigurationContainer
			// - IRoadkillContext relies on UserManager, which relies on IRepository
			ObjectFactory.Initialize(x =>
			{
				x.Scan(scanner =>
				{
					scanner.AddAllTypesOf<IConfigurationContainer>();
					scanner.AddAllTypesOf<ControllerBase>();
					scanner.AddAllTypesOf<ServiceBase>();
					scanner.AddAllTypesOf<IRoadkillContext>();
					scanner.AddAllTypesOf<IRepository>();
					scanner.AddAllTypesOf<MarkupConverter>();
					scanner.AddAllTypesOf<CustomTokenParser>();
					scanner.WithDefaultConventions();
				});

				// These models require IConfigurationContainer or IRoadkillContext in their constructors
				x.For<UserSummary>().Use<UserSummary>();
				x.For<SettingsSummary>().Use<SettingsSummary>();

				if (config == null)
				{
					x.For<IConfigurationContainer>().HybridHttpOrThreadLocalScoped().Use<RoadkillSettings>();
				}
				else
				{
					x.For<IConfigurationContainer>().HybridHttpOrThreadLocalScoped().Use(config);
				}
			});

			//
			// Configuration of types that are dependent on the previous StructureMap scan
			//
			ObjectFactory.Configure(x =>
			{
				x.IncludeConfigurationFromConfigFile = true;
				if (config == null)
					config = ObjectFactory.GetInstance<IConfigurationContainer>();

				if (repository == null)
				{
					SwitchRepository(config.ApplicationSettings.DataStoreType);
				}
				else
				{
					x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use(repository);
				}

				if (context == null)
				{
					x.For<IRoadkillContext>().HybridHttpOrThreadLocalScoped().Use<RoadkillContext>();
				}
				else
				{
					x.For<IRoadkillContext>().HybridHttpOrThreadLocalScoped().Use(context);
				}

				x.For<IActiveDirectoryService>().HybridHttpOrThreadLocalScoped().Use<DefaultActiveDirectoryService>();
				x.For<IPageManager>().HybridHttpOrThreadLocalScoped().Use<PageManager>();

				string userManagerTypeName = config.ApplicationSettings.UserManagerType;
				if (string.IsNullOrEmpty(userManagerTypeName))
				{
					if (config.ApplicationSettings.UseWindowsAuthentication)
					{
						x.For<UserManager>().HybridHttpOrThreadLocalScoped().Use<ActiveDirectoryUserManager>();
					}
					else
					{
						x.For<UserManager>().HybridHttpOrThreadLocalScoped().Use<DefaultUserManager>();
					}
				}
				else
				{
					// Load UserManager type from config
					x.For<UserManager>().HybridHttpOrThreadLocalScoped().Use(LoadUserManagerFromType(userManagerTypeName));
				}
			});
		}

		private static UserManager LoadUserManagerFromType(string typeName)
		{
			Type userManagerType = typeof(UserManager);
			Type reflectedType = Type.GetType(typeName);

			if (reflectedType.IsSubclassOf(userManagerType))
			{
				return (UserManager)reflectedType.Assembly.CreateInstance(reflectedType.FullName);
			}
			else
			{
				throw new SecurityException(null, "The type {0} specified in the userManagerType web.config setting is not an instance of a UserManager class", typeName);
			}
		}

		public static IRepository SwitchRepository(DataStoreType storeType)
		{
			if (storeType.RequiresCustomRepository)
			{
				Type interfaceType = typeof(IRepository);
				Type reflectedType = Type.GetType(storeType.CustomRepositoryType);

				if (interfaceType.IsAssignableFrom(reflectedType))
				{
					ObjectFactory.Configure(x =>
					{
						x.For<IRepository>().Use((IRepository)ObjectFactory.GetInstance(reflectedType));
					});
				}
				else
				{
					throw new SecurityException(null, "The type {0} specified in the repositoryType web.config setting is not an instance of a IRepository.", reflectedType);
				}
			}
			else
			{
				ObjectFactory.Configure(x =>
				{
					x.For<IRepository>().Use<NHibernateRepository>();
				});
			}

			return ObjectFactory.GetInstance<IRepository>();
		}
	}
}
