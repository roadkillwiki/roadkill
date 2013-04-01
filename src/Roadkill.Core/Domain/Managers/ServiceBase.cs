using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using StructureMap;

namespace Roadkill.Core.Managers
{
	/// <summary>
	/// Provides all inheriting classes with queryable objects for the system pages and text content.
	/// </summary>
	public class ServiceBase
	{
		protected IRepository Repository;
		protected ApplicationSettings ApplicationSettings;

		public ServiceBase(ApplicationSettings settings, IRepository repository)
		{
			ApplicationSettings = settings;
			Repository = repository;
		}

		public void UpdateRepository(IRepository repository)
		{
			Repository = repository;
		}
	}
}
