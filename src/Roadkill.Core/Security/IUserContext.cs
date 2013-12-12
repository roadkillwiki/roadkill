using System;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core
{
	/// <summary>
	/// Defines a class that holds information for the current logged in user, and the current page.
	/// </summary>
	public interface IUserContext
	{
		/// <summary>
		/// The current logged in user - for example this can be an ID for forms authentication or
		/// a fully qualified domain name and username for Windows Authentication.
		/// </summary>
		string CurrentUser { get; set; }
		
		/// <summary>
		/// The username for the logged in user, retrieved by looking up the ID stored by the CurrentUser property.
		/// </summary>
		string CurrentUsername { get; }

		/// <summary>
		/// Gets whether the user (if logged in), is in the editors group.
		/// </summary>
		bool IsAdmin { get; }

		/// <summary>
		/// Gets whether the user (if logged in), is in the editors group.
		/// </summary>
		bool IsEditor { get; }

		/// <summary>
		/// Gets whether the request is for a logged in user.
		/// </summary>
		bool IsLoggedIn { get; }
	}
}
