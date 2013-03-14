using System;
using Mindscape.LightSpeed;

namespace Roadkill.Core.Database.LightSpeed
{
	[Table("roadkill_users")]
	[Cached(ExpiryMinutes = 15)]
	internal class UserEntity : Entity<Guid>
	{
		private string _username;
		private string _email;
		private string _firstname;
		private string _lastname;
		private string _password;
		private string _salt;
		private bool _isEditor;
		private bool _isAdmin;
		private bool _isActivated;
		private string _activationKey;
		private string _passwordResetKey;

		public string Username
		{
			get
			{
				return _username;
			}
			set
			{
				Set<string>(ref _username, value);
			}
		}

		public string Email
		{
			get
			{
				return _email;
			}
			set
			{
				Set<string>(ref _email, value);
			}
		}

		public string Firstname
		{
			get
			{
				return _firstname;
			}
			set
			{
				Set<string>(ref _firstname, value);
			}
		}

		public string Lastname
		{
			get
			{
				return _lastname;
			}
			set
			{
				Set<string>(ref _lastname, value);
			}
		}

		public string Password
		{
			get
			{
				return _password;
			}
			set
			{
				Set<string>(ref _password, value);
			}
		}

		public string Salt
		{
			get
			{
				return _salt;
			}
			set
			{
				Set<string>(ref _salt, value);
			}
		}

		public bool IsEditor
		{
			get
			{
				return _isEditor;
			}
			set
			{
				Set<bool>(ref _isEditor, value);
			}
		}

		public bool IsAdmin
		{
			get
			{
				return _isAdmin;
			}
			set
			{
				Set<bool>(ref _isAdmin, value);
			}
		}

		public bool IsActivated
		{
			get
			{
				return _isActivated;
			}
			set
			{
				Set<bool>(ref _isActivated, value);
			}
		}

		public string ActivationKey
		{
			get
			{
				return _activationKey;
			}
			set
			{
				Set<string>(ref _activationKey, value);
			}
		}

		public string PasswordResetKey
		{
			get
			{
				return _passwordResetKey;
			}
			set
			{
				Set<string>(ref _passwordResetKey, value);
			}
		}

	}
}
