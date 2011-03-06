using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Web.Configuration;
using System.Configuration;
using System.Data.SqlClient;

namespace Roadkill.Core
{
	public class UserManager
	{
		private static UserManager _userManager;

		public static UserManager Current
		{
			get
			{
				if (_userManager == null)
					_userManager = new UserManager();

				return _userManager;
			}
		}

		public bool Authenticate(string username,string password)
		{
			return Membership.ValidateUser(username,password);
		}

		public void Logout()
		{
			FormsAuthentication.SignOut();
		}

		public MembershipCreateStatus AddUser(string username, string password, string email)
		{
			MembershipCreateStatus status = MembershipCreateStatus.Success;
			Membership.CreateUser(username, password, email, "question", "answer", true, out status);
				
			return status;
		}

		/// <summary>
		/// Adds the a username "admin" with the provided password, and adds them to the admin role 
		/// name provided by RoadkillSettings.AdminGroup.
		/// </summary>
		/// <param name="password"></param>
		/// <param name="email"></param>
		/// <returns></returns>
		public MembershipCreateStatus AddAdminUser(string password, string email)
		{
			MembershipCreateStatus status = MembershipCreateStatus.Success;
			MembershipUser user = Membership.CreateUser("admin", password, email, "question", "answer", true, out status);

			if (!Roles.RoleExists(RoadkillSettings.AdminGroup))
				Roles.CreateRole(RoadkillSettings.AdminGroup);

			if (!Roles.IsUserInRole("admin",RoadkillSettings.AdminGroup))
				Roles.AddUserToRole("admin", RoadkillSettings.AdminGroup);

			return status;
		}

		
		public bool DeleteUser(string username)
		{
			return Membership.DeleteUser(username);
		}

		public void ChangeUsername(string oldUsername, string newUsername)
		{
			((RoadkillMembershipProvider)Membership.Provider).ChangeUsername(oldUsername, newUsername);
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

		public void UpdateUser(string username, string newPassword, string email)
		{
			MembershipUser user = Membership.GetUser(username);

			string tempPassword = user.ResetPassword();
			Membership.UpdateUser(user);

			user.ChangePassword(newPassword, tempPassword);
			user.Email = email;
			Membership.UpdateUser(user);
		}
	}
}
