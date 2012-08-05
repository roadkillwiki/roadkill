using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using NHibernate;
using System.Web.Security;

namespace Roadkill.Core
{
	/// <summary>
	/// A user object for use with the NHibernate data store. This object is intended for internal use only.
	/// </summary>
	public class User
	{
		public virtual Guid Id { get; set; }

		/// <summary>
		/// This field is for page modifiedby/created by, and is a 'friendly' name. For windows auth the email field is used instead.
		/// </summary>
		public virtual string Username { get; set; }

		/// <summary>
		/// This is used as the username or identifier for the user. If using windows auth, this will
		/// be the user name from active directory.
		/// </summary>
		public virtual string Email { get; set; }

		public virtual string Firstname { get; set; }
		public virtual string Lastname { get; set; }
		/// <summary>
		/// Do not use this property - use <see cref="SetPassword"/> instead
		/// </summary>
		public virtual string Password { get; protected set; }
		/// <summary>
		/// Do not use this property - use <see cref="SetPassword"/> instead
		/// </summary>
		public virtual string Salt { get; set; }
		public virtual bool IsEditor { get; set; }
		public virtual bool IsAdmin { get; set; }
		public virtual bool IsActivated { get; set; }
		public virtual string ActivationKey { get; set; }
		public virtual string PasswordResetKey { get; set; }

		public virtual void SetPassword(string password)
		{
			Salt = new Salt();
			Password = HashPassword(password,Salt);
		}

		public static string HashPassword(string password,string salt)
		{
			return FormsAuthentication.HashPasswordForStoringInConfigFile(password + salt, "SHA1");
		}

		public virtual UserSummary ToSummary()
		{
			return new UserSummary()
			{
				ActivationKey = ActivationKey,
				Id = Id,
				ExistingEmail = Email,
				ExistingUsername = Username,
				NewEmail = Email,
				NewUsername = Username,
				Firstname = Firstname,
				Lastname = Lastname,
				PasswordResetKey = PasswordResetKey,
				IsNew = (Id == Guid.Empty)
			};
		}
	}

	/// <summary>
	/// Configures the Fluent NHibernate mapping for a <see cref="User	"/>
	/// </summary>
	public class UserMap : ClassMap<User>
	{
		public UserMap()
		{
			Table("roadkill_users");
			Map(x => x.ActivationKey);
			Map(x => x.Email).Index("email");
			Map(x => x.Firstname);
			Id(x => x.Id);
			Map(x => x.IsEditor);
			Map(x => x.IsAdmin);
			Map(x => x.IsActivated);
			Map(x => x.Lastname);
			Map(x => x.Password);
			Map(x => x.PasswordResetKey);
			Map(x => x.Salt);
			Map(x => x.Username);
			Cache.ReadWrite().IncludeAll();
		}
	}
}
