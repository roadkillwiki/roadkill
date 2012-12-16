using System;

namespace Roadkill.Core
{
	/// <summary>
	/// Defines a class that holds information for the current HTTP request, 
	/// in particular the user logged in state for the request.
	/// </summary>
	public interface IRoadkillContext
	{
		/// <summary>
		/// The current logged in user - for example this can be an ID for forms authentication or
		/// a fully qualified domain name and username for Windows Authentication.
		/// </summary>
		string CurrentUser { get; set; }

		/// <summary>
		/// The username for the logged in user, retrieved by looking up the CurrentUser property.
		/// </summary>
		string CurrentUsername { get; }

		/// <summary>
		/// Gets whether the user (if logged in), is in the editors group.
		/// </summary>
		bool IsAdmin { get; }

		/// <summary>
		/// Returns true if the current page is not the initial dummy 'mainpage'.
		/// </summary>
		bool IsContentPage { get; }

		/// <summary>
		/// Gets whether the user (if logged in), is in the editors group.
		/// </summary>
		bool IsEditor { get; }

		/// <summary>
		/// Gets whether the request is for a logged in user.
		/// </summary>
		bool IsLoggedIn { get; }

		/// <summary>
		/// Gets/sets the current page this user is viewing.
		/// </summary>
		PageSummary Page { get; set; }
	}
}
