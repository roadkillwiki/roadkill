using System;
using System.Collections.Generic;
using System.Linq;
using Roadkill.Core.Converters;

namespace Roadkill.Core.Database
{
	public interface IUserRepository
	{
		User GetAdminById(Guid id);
		User GetUserByActivationKey(string key);
		User GetEditorById(Guid id);
		User GetUserByEmail(string email, bool isActivated = true);
		User GetUserById(Guid id, bool isActivated = true);
		User GetUserByPasswordResetKey(string key);
		User GetUserByUsername(string username);
		User GetUserByUsernameOrEmail(string username, string email);
		IEnumerable<User> FindAllEditors();
		IEnumerable<User> FindAllAdmins();
	}
}
