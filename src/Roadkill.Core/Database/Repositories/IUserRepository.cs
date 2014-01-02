using System;
using System.Collections.Generic;
using System.Linq;
using Roadkill.Core.Converters;

namespace Roadkill.Core.Database
{
	public interface IUserRepository
	{
		void DeleteAllUsers();
		void DeleteUser(User user);
		IEnumerable<User> FindAllEditors();
		IEnumerable<User> FindAllAdmins();
		User GetAdminById(Guid id);
		User GetUserByActivationKey(string key);
		User GetEditorById(Guid id);
		User GetUserByEmail(string email, bool? isActivated = null);
		User GetUserById(Guid id, bool? isActivated = null);
		User GetUserByPasswordResetKey(string key);
		User GetUserByUsername(string username);
		User GetUserByUsernameOrEmail(string username, string email);
		User SaveOrUpdateUser(User user);
	}
}
