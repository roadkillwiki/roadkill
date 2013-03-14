using System;
using System.Text;
using System.Web.Security;
using System.Security.Cryptography;
using Roadkill.Core.Database;

namespace Roadkill.Core
{
	/// <summary>
	/// A user object for use with the NHibernate data store. This object is intended for internal use only.
	/// </summary>
	public class User : IDataStoreEntity
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

		public Guid ObjectId
		{
			get { return Id; }
			set { Id = value; }
		}

		public virtual void SetPassword(string password)
		{
			Salt = new Salt();
			Password = HashPassword(password,Salt);
		}

		/// <summary>
		/// Hashes the password and salt using SHA1 via FormsAuthentication, or 256 is FormsAuthentication is not enabled.
		/// </summary>
		public static string HashPassword(string password, string salt)
		{
			if (FormsAuthentication.IsEnabled)
			{
				return FormsAuthentication.HashPasswordForStoringInConfigFile(password + salt, "SHA1");
			}
			else
			{
				SHA256 sha = new SHA256Managed();
				byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(password + "salt"));

				StringBuilder stringBuilder = new StringBuilder();
				foreach (byte b in hash)
				{
					stringBuilder.AppendFormat("{0:x2}", b);
				}

				return stringBuilder.ToString();
			}
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
				IsBeingCreatedByAdmin = (Id == Guid.Empty)
			};
		}
	}
}
