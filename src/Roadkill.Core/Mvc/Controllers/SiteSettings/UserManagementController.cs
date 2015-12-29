using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Roadkill.Core.Localization;
using Roadkill.Core.Configuration;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Database;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Provides functionality for user management for admins.
	/// </summary>
	/// <remarks>All actions in this controller require admin rights.</remarks>
	[AdminRequired]
	public class UserManagementController : ControllerBase
	{
		public UserManagementController(ApplicationSettings settings, UserServiceBase userManager, SettingsService settingsService, IUserContext context)
			: base(settings, userManager, context, settingsService) 
		{

		}

		/// <summary>
		/// Displays the Users view.
		/// </summary>
		/// <returns>An <see cref="IList{UserViewModel}"/> as the model. The first item contains a list of admin users,
		/// the second item contains a list of editor users. If Windows authentication is being used, the action uses the 
		/// UsersForWindows view.</returns>
		public ActionResult Index()
		{
			var list = new List<IEnumerable<UserViewModel>>();
			list.Add(UserService.ListAdmins());
			list.Add(UserService.ListEditors());

			if (UserService.IsReadonly)
				return View("IndexReadOnly", list);
			else
				return View(list);
		}

		public ActionResult AddAdmin()
		{
			return View(new UserViewModel());
		}

		/// <summary>
		/// Adds an admin user to the system, validating the <see cref="UserViewModel"/> first.
		/// </summary>
		/// <param name="model">The user details to add.</param>
		/// <returns>Redirects to the Users action if successful.</returns>
		[HttpPost]
		public ActionResult AddAdmin(UserViewModel model)
		{
			if (ModelState.IsValid)
			{
				UserService.AddUser(model.NewEmail, model.NewUsername, model.Password, true, false);
				return RedirectToAction("Index");
				
			}
			else
			{
				return View(model);
			}
		}

		public ActionResult AddEditor()
		{
			return View(new UserViewModel());
		}

		/// <summary>
		/// Adds an editor user to the system, validating the <see cref="UserViewModel"/> first.
		/// </summary>
		/// <param name="model">The user details to add.</param>
		/// <returns>Redirects to the Users action. Additionally, if an error occurred, TempData["action"] contains the string "addeditor".</returns>
		[HttpPost]
		public ActionResult AddEditor(UserViewModel model)
		{
			if (ModelState.IsValid)
			{
				UserService.AddUser(model.NewEmail, model.NewUsername, model.Password, false, true);
				return RedirectToAction("Index");

			}
			else
			{
				return View(model);
			}
		}

		public ActionResult EditUser(Guid id)
		{
			User user = UserService.GetUserById(id);
			if (user == null)
				return RedirectToAction("Index");

			UserViewModel model = new UserViewModel(user);
			return View(model);
		}

		/// <summary>
		/// Edits an existing user. If the <see cref="UserViewModel.Password"/> property is not blank, the password
		/// for the user is reset and then changed.
		/// </summary>
		/// <param name="model">The user details to edit.</param>
		/// <returns>Redirects to the Users action. Additionally, if an error occurred, TempData["edituser"] contains the string "addeditor".</returns>
		[HttpPost]
		public ActionResult EditUser(UserViewModel model)
		{
			if (ModelState.IsValid)
			{
				if (model.UsernameHasChanged || model.EmailHasChanged)
				{
					if (!UserService.UpdateUser(model))
					{
						ModelState.AddModelError("General", SiteStrings.SiteSettings_UserManagement_EditUser_Error);
					}

					model.ExistingEmail = model.NewEmail;
				}

				if (!string.IsNullOrEmpty(model.Password))
					UserService.ChangePassword(model.ExistingEmail, model.Password);

				return RedirectToAction("Index");
			}
			else
			{
				return View(model);
			}
		}

		/// <summary>
		/// Removes a user from the system.
		/// </summary>
		/// <param name="id">The email or username of the user to remove.</param>
		/// <returns>Redirects to the Users action.</returns>
		public ActionResult DeleteUser(string id)
		{
			UserService.DeleteUser(id);
			return RedirectToAction("Index");
		}
	}
}
