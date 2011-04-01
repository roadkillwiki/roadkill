using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using NHibernate;
using System.Web.Security;

namespace Roadkill.Core
{
	public class User
	{
		public Guid Id { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string Salt { get; set; }
		public bool IsEditor { get; set; }
		public bool IsAdmin { get; set; }

		public static string HashPassword(string password,string salt)
		{
			return FormsAuthentication.HashPasswordForStoringInConfigFile(password + salt, "SHA1");
		}
	}

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
			Cache.ReadWrite().IncludeAll();
		}
	}
}
