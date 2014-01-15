using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;
using StructureMap;

namespace Roadkill.Tests
{
	public class UserServiceMock : UserServiceBase
	{
		public static readonly string RESETKEY = "resetkey";
		public static readonly string ACTIVATIONKEY = "activationkey";
		public List<User> Users { get; set; }
		public string LoggedInUserId { get;set; }
		public bool ThrowSecurityExceptionOnSignup { get; set; }
		public bool HasLoggedOut { get; set; }

		public UserServiceMock()
			: base(null, null)
		{
			Users = new List<User>();
		}

		public UserServiceMock(ApplicationSettings settings, IRepository repository)
			: base(settings, repository)
		{
			Users = new List<User>();
		}

		public override bool IsReadonly
		{
			get { return false; }
		}

		public override bool ActivateUser(string activationKey)
		{
			return (activationKey == ACTIVATIONKEY);
		}

		public override bool AddUser(string email, string username, string password, bool isAdmin, bool isEditor)
		{
			User user = new User();
			user.Id = Guid.NewGuid();
			user.Email = email;
			user.Username = username;
			user.SetPassword(password);
			user.IsAdmin = isAdmin;
			user.IsEditor = isEditor;

			Users.Add(user);

			return true;
		}

		public override bool Authenticate(string email, string password)
		{
			User user = GetUser(email);
			if (user != null)
			{
				string hashedPassword = User.HashPassword(password, user.Salt);
				bool isLoggedIn = user.Password == hashedPassword;

				if (isLoggedIn)
				{
					LoggedInUserId = user.Id.ToString();
					return true;
				}
			}

			return false;
		}

		public override void ChangePassword(string email, string newPassword)
		{
			User user = GetUser(email);
			if (user != null)
			{
				user.SetPassword(newPassword);
			}
		}

		public override bool ChangePassword(string email, string oldPassword, string newPassword)
		{
			User user = GetUser(email);
			if (user != null)
			{
				user.SetPassword(newPassword);
				return true;
			}
			else
			{
				return false;
			}
		}

		public override bool DeleteUser(string email)
		{
			User user = GetUser(email);
			if (user != null)
			{
				Users.Remove(user);
				return true;
			}
			else
			{
				return false;
			}
		}

		public override User GetUserById(Guid id, bool? isActivated = null)
		{
			if (isActivated.HasValue)
				return Users.FirstOrDefault(x => x.Id == id && x.IsActivated == isActivated);
			else
				return Users.FirstOrDefault(x => x.Id == id);
		}

		public override User GetUser(string email, bool? isActivated = null)
		{
			if (isActivated.HasValue)
				return Users.FirstOrDefault(x => x.Email == email && x.IsActivated == isActivated);
			else
				return Users.FirstOrDefault(x => x.Email == email);
		}

		public override User GetUserByResetKey(string resetKey)
		{
			return Users.FirstOrDefault(x => x.PasswordResetKey == resetKey);
		}

		public override bool IsAdmin(string cookieValue)
		{
			Guid id = Guid.Parse(cookieValue);
			return Users.Any(x => x.Id == id && x.IsAdmin);
		}

		public override bool IsEditor(string cookieValue)
		{
			Guid id = Guid.Parse(cookieValue);
			return Users.Any(x => x.Id == id && x.IsEditor);
		}

		public override IEnumerable<UserViewModel> ListAdmins()
		{
			return Users.Where(x => x.IsAdmin).Select(x => new UserViewModel(x));
		}

		public override IEnumerable<UserViewModel> ListEditors()
		{
			return Users.Where(x => x.IsEditor).Select(x => new UserViewModel(x));
		}

		public override void Logout()
		{
			HasLoggedOut = true;
		}

		public override string ResetPassword(string email)
		{
			User user = GetUser(email);
			if (user != null)
			{
				user.PasswordResetKey = RESETKEY;
				return RESETKEY;
			}

			return "user with email " +email+ "not found";
		}

		public override string Signup(UserViewModel page, Action completed)
		{
			if (ThrowSecurityExceptionOnSignup)
				throw new SecurityException("ThrowSecurityExceptionOnSignup", null);

			AddUser(page.NewEmail, page.NewUsername, page.Password, false, true);

			User user = GetUser(page.NewEmail, false);
			user.IsActivated = false;
			user.ActivationKey = Guid.NewGuid().ToString();

			return user.ActivationKey;
		}

		public override void ToggleAdmin(string email)
		{
			User user = GetUser(email);
			if (user != null)
			{
				user.IsAdmin = !user.IsAdmin;
			}
		}

		public override void ToggleEditor(string email)
		{
			User user = GetUser(email);
			if (user != null)
			{
				user.IsEditor = !user.IsEditor;
			}
		}

		public override bool UpdateUser(UserViewModel model)
		{
			User user = GetUser(model.ExistingEmail);
			if (user != null)
			{
				user.Email = model.NewEmail;
				user.Firstname = model.Firstname;
				user.Lastname = model.Lastname;
				user.Username = model.NewUsername;

				return true;
			}
			else
			{
				return false;
			}
		}

		public override bool UserExists(string email)
		{
			return Users.Any(x => x.Email == email);
		}

		public override bool UserNameExists(string username)
		{
			return Users.Any(x => x.Username == username);
		}

		public override string GetLoggedInUserName(System.Web.HttpContextBase context)
		{
			return LoggedInUserId;
		}
	}
}
