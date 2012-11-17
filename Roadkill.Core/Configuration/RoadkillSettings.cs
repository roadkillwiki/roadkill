using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Security;
using System.Web.Configuration;
using System.Reflection;
using System.IO;
using StructureMap;

namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// The default implementation of <see cref="IConfigurationContainer"/>
	/// </summary>
	public class RoadkillSettings : IConfigurationContainer, IInjectionLaunderer
	{
		private SitePreferences _sitePreferences;
		private ApplicationSettings _applicationSettings;

		/// <summary>
		/// Retrieves the configuration settings that are stored in the database.
		/// </summary>
		/// <returns>A <see cref="SitePreferences"/></returns>
		public SitePreferences SitePreferences
		{
			get 
			{
				if (_sitePreferences == null)
				{
					LoadSitePreferences();
				}

				return _sitePreferences; 
			}
			set { _sitePreferences = value; }
		}
	
		/// <summary>
		/// Retrieves the configuration settings that are stored inside an application config 
		/// file and require an application restart when changed.
		/// </summary>
		/// <returns>A <see cref="RoadkillSection"/></returns>
		public ApplicationSettings ApplicationSettings
		{
			get
			{
				if (_applicationSettings == null)
				{
					_applicationSettings = new ApplicationSettings();
					_applicationSettings.Load();
				}

				return _applicationSettings; 
			}
			set { _applicationSettings = value; }
		}

		public static IConfigurationContainer Current
		{
			get
			{
				return ObjectFactory.GetInstance<IConfigurationContainer>();
			}
		}

		public virtual void LoadSitePreferences()
		{
			IRepository repository;

			try
			{
				repository = ObjectFactory.GetInstance<IRepository>();
			}
			catch (StructureMapException e)
			{
				throw new IoCException("A StructureMap exception occurred when loading the repository for SitePreferences - has RoadkillApplication.SetupIoC() been called? "
					+ ObjectFactory.WhatDoIHave(), e);
			}
			
			SitePreferences preferences = repository.GetSitePreferences();

			if (preferences == null)
				throw new DatabaseException(null, "No configuration settings could be found in the database (id {0}). " +
					"Has SettingsManager.SaveSiteConfiguration() been called?", SitePreferences.ConfigurationId);

			if (string.IsNullOrEmpty(preferences.AllowedFileTypes))
				throw new InvalidOperationException("The allowed file types setting is empty");

			_sitePreferences = preferences;
		}
	}
}