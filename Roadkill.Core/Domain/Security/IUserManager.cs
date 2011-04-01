using System;
using System.Collections.Generic;
namespace Roadkill.Core
{
	public interface IUserManager
	{
		bool AddUser(string email, string password, bool isAdmin, bool isEditor);
		void AddAdmin(string email, string password);
		void AddEditor(string email, string password);
		bool Authenticate(string email, string password);
		bool ChangeEmail(string oldEmail, string newEmail);
		void ChangePassword(string email, string newPassword);
		bool ChangePassword(string email, string oldPassword, string newPassword);
		bool DeleteUser(string email);
		IEnumerable<string> ListAdmins();
		IEnumerable<string> ListEditors();
		void Logout();
		bool UserExists(string email);
		bool IsUserAdmin(string email);
		bool IsUserEditor(string email);
	}
}
