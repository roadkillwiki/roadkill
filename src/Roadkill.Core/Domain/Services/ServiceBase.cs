using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using StructureMap;

namespace Roadkill.Core.Services
{
	/// <summary>
	/// Provides all inheriting services classes with application settings and repository access.
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
