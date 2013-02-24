using System;
using System.Collections.Generic;
using System.Linq;
using Roadkill.Core.Converters;

namespace Roadkill.Core
{
	public interface IUserRepository
	{
		User GetAdminById(Guid id);
		User GetUserByActivationKey(string key);
		User GetEditorById(Guid id);
		User GetUserByEmail(string email);
		User GetUserById(Guid id);
		User GetUserByPasswordResetKey(string key);
		User GetUserByUsername(string username);
		User GetUserByUsernameOrEmail(string username);
	}
}
