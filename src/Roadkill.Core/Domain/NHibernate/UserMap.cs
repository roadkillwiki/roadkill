using System;
using System.Text;
using FluentNHibernate.Mapping;
using System.Web.Security;
using System.Security.Cryptography;

namespace Roadkill.Core
{
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
