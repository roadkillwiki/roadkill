using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Web.Configuration;
using System.Configuration;
using System.Data.SqlClient;
using System.Configuration.Provider;
using Roadkill.Core.Domain.Managers;

namespace Roadkill.Core
{
	public class UserManager
	{
		public string AddEditor(string username, string password)
		{
			string email = Guid.NewGuid().ToString() + "@roadkill";
			MembershipCreateStatus status = MembershipCreateStatus.Success;
			MembershipUser user = Membership.CreateUser(username, password, email, "question", "answer", true, out status);

			if (user == null)
			{
				return GetErrorMessage(status);
			}
			else
			{
				try
				{
					Roles.AddUserToRole(username, RoadkillSettings.EditorRoleName);
				}
				catch (ProviderException e)
				{
					return e.Message;
				}
			}

			return "";
		}

		public string AddAdmin(string username, string password)
		{
			// For now, the email is a guid
			string email = Guid.NewGuid().ToString() + "@localhost";
			MembershipCreateStatus status = MembershipCreateStatus.Success;
			MembershipUser user = Membership.CreateUser(username, password, email, "question", "answer", true, out status);

			if (user == null)
			{
				return GetErrorMessage(status);
			}
			else
			{
				try
				{
					Roles.AddUserToRole(username, RoadkillSettings.AdminRoleName);
				}
				catch (ProviderException e)
				{
					return e.Message;
				}
			}

			return "";
		}

		public void AddRoles()
		{
			if (!Roles.RoleExists(RoadkillSettings.AdminRoleName))
				Roles.CreateRole(RoadkillSettings.AdminRoleName);

			if (!Roles.RoleExists(RoadkillSettings.EditorRoleName))
				Roles.CreateRole(RoadkillSettings.EditorRoleName);
		}

		public bool Authenticate(string username,string password)
		{
			return Membership.ValidateUser(username,password);
		}

		/// <summary>
		/// Changes the user's password where you do not know the original password.
		/// </summary>
		/// <param name="username"></param>
		/// <param name="newPassword"></param>
		/// <param name="email"></param>
		public void ChangePassword(string username, string newPassword, string email)
		{
			MembershipUser user = Membership.GetUser(username);
			user.UnlockUser(); // password changes fail if the user is locked out.

			string tempPassword = user.ResetPassword();
			Membership.UpdateUser(user);

			// This can potentially fail if the web.config has its Membership password settings changed
			// as the front-end doesn't validate against this yet.
			if (!user.ChangePassword(tempPassword, newPassword))
				throw new UserException(string.Format("Changing passwords for {0} failed", username));

			user.Email = email;
			Membership.UpdateUser(user);
		}

		public void ChangeUsername(string oldUsername, string newUsername)
		{
			((RoadkillMembershipProvider)Membership.Provider).ChangeUsername(oldUsername, newUsername);
		}

		public bool DeleteUser(string username)
		{
			if (Membership.GetAllUsers().Count == 1)
				throw new UserException("Cannot delete user '{0}' as they are the only user in the system.", username);

			return Membership.DeleteUser(username);
		}

		public IEnumerable<string> ListAdmins()
		{
			return Roles.GetUsersInRole(RoadkillSettings.AdminRoleName);
		}

		public bool UserExists(string username)
		{
			return Membership.FindUsersByName(username).Count > 0;
		}

		public IEnumerable<string> ListEditors()
		{
			return Roles.GetUsersInRole(RoadkillSettings.EditorRoleName);
		}

		public void Logout()
		{
			FormsAuthentication.SignOut();
		}		

		public void UpdateUser(string username, string oldPassword,string newPassword, string email)
		{
			MembershipUser user = Membership.GetUser(username);

			if (!string.IsNullOrWhiteSpace(oldPassword) && !string.IsNullOrWhiteSpace(newPassword))
			{
				user.ChangePassword(oldPassword, newPassword);
			}

			user.Email = email;
			Membership.UpdateUser(user);
		}

		private string GetErrorMessage(MembershipCreateStatus status)
		{
			switch (status)
			{
				case MembershipCreateStatus.DuplicateUserName:
					return "Username already exists. Please enter a different user name.";

				case MembershipCreateStatus.DuplicateEmail:
					return "A username for that e-mail address already exists. Please enter a different e-mail address.";

				case MembershipCreateStatus.InvalidPassword:
					return "The password provided is invalid. Please enter a valid password value.";

				case MembershipCreateStatus.InvalidEmail:
					return "The e-mail address provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidAnswer:
					return "The password retrieval answer provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidQuestion:
					return "The password retrieval question provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidUserName:
					return "The user name provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.ProviderError:
					return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

				case MembershipCreateStatus.UserRejected:
					return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

				default:
					return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
			}
		}
	}
}
