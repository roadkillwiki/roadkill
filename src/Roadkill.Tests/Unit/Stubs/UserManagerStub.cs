using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;

namespace Roadkill.Tests
{
	public class UserManagerStub : UserManager
	{
		public UserManagerStub()
			: base(null, null)
		{

		}

		public override bool IsReadonly
		{
			get { throw new NotImplementedException(); }
		}

		public override bool ActivateUser(string activationKey)
		{
			throw new NotImplementedException();
		}

		public override bool AddUser(string email, string username, string password, bool isAdmin, bool isEditor)
		{
			throw new NotImplementedException();
		}

		public override bool Authenticate(string email, string password)
		{
			throw new NotImplementedException();
		}

		public override void ChangePassword(string email, string newPassword)
		{
			throw new NotImplementedException();
		}

		public override bool ChangePassword(string email, string oldPassword, string newPassword)
		{
			throw new NotImplementedException();
		}

		public override bool DeleteUser(string email)
		{
			throw new NotImplementedException();
		}

		public override User GetUserById(Guid id)
		{
			throw new NotImplementedException();
		}

		public override User GetUser(string email)
		{
			throw new NotImplementedException();
		}

		public override User GetUserByResetKey(string resetKey)
		{
			throw new NotImplementedException();
		}

		public override bool IsAdmin(string email)
		{
			throw new NotImplementedException();
		}

		public override bool IsEditor(string email)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<UserSummary> ListAdmins()
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<UserSummary> ListEditors()
		{
			throw new NotImplementedException();
		}

		public override void Logout()
		{
			throw new NotImplementedException();
		}

		public override string ResetPassword(string email)
		{
			throw new NotImplementedException();
		}

		public override string Signup(UserSummary summary, Action completed)
		{
			throw new NotImplementedException();
		}

		public override void ToggleAdmin(string email)
		{
			throw new NotImplementedException();
		}

		public override void ToggleEditor(string email)
		{
			throw new NotImplementedException();
		}

		public override bool UpdateUser(UserSummary summary)
		{
			throw new NotImplementedException();
		}

		public override bool UserExists(string email)
		{
			throw new NotImplementedException();
		}

		public override bool UserNameExists(string username)
		{
			throw new NotImplementedException();
		}

		public override string GetLoggedInUserName(System.Web.HttpContextBase context)
		{
			throw new NotImplementedException();
		}
	}
}
