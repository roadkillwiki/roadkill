using System;
using System.Linq;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using StructureMap.Attributes;

namespace Roadkill.Core.Database
{
	/// <summary>
	/// Defines a repository for storing and retrieving Roadkill domain objects in a data store.
	/// </summary>
	public interface IRepository : IPageRepository, IUserRepository, IDisposable
	{
		void SaveSiteSettings(SiteSettings siteSettings);
		SiteSettings GetSiteSettings();
		void Startup(DataStoreType dataStoreType, string connectionString, bool enableCache);
		void Install(DataStoreType dataStoreType, string connectionString, bool enableCache);
		void TestConnection(DataStoreType dataStoreType, string connectionString);		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="applicationSettings"></param>
		/// <exception cref="UpgradeException">Thrown if there is a problem with the upgrade. This contains the details of the failure.</exception>
		void Upgrade(ApplicationSettings applicationSettings);
	}
}
