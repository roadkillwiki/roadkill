using System;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Database
{
	public interface IInstallerRepository : IDisposable
	{
		void AddAdminUser(string email, string username, string password);
		void CreateSchema();
		void SaveSettings(SiteSettings siteSettings);
	}
}
