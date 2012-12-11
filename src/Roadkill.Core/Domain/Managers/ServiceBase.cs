using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using StructureMap;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides all inheriting classes with queryable objects for the system pages and text content.
	/// </summary>
	public class ServiceBase
	{
		protected IRepository Repository;
		protected IConfigurationContainer Configuration;

		public ServiceBase(IConfigurationContainer configuration, IRepository repository)
		{
			Configuration = configuration;
			Repository = repository;
		}
	}
}
