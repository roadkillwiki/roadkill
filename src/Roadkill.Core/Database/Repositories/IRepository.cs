using System;
using Roadkill.Core.Database.Repositories;

namespace Roadkill.Core.Database
{
	/// <summary>
	/// Defines a repository for storing and retrieving Roadkill domain objects in a data store.
	/// </summary>
	public interface IRepository : IPageRepository, IUserRepository, ISettingsRepository, IDisposable
	{

	}
}
