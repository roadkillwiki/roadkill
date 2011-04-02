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
		public virtual string Email { get; set; }
		public virtual string Password { get; protected set; }
		public virtual string Salt { get; set; }
		public virtual bool IsEditor { get; set; }
		public virtual bool IsAdmin { get; set; }
		public virtual bool IsActivated { get; set; }
		public virtual string ActivationKey { get; set; }

		public virtual void SetPassword(string password)
		{
			Salt = new Salt();
			Password = HashPassword(password,Salt);
		}

		public static string HashPassword(string password,string salt)
		{
			return FormsAuthentication.HashPasswordForStoringInConfigFile(password + salt, "SHA1");
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
			Id(x => x.Id);
			Map(x => x.Email);
			Map(x => x.Password);
			Map(x => x.Salt);
			Map(x => x.IsEditor);
			Map(x => x.IsAdmin);
			Map(x => x.IsActivated);
			Map(x => x.ActivationKey);
			Cache.ReadWrite().IncludeAll();
		}
	}
}
