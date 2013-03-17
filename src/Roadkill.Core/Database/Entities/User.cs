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
		public string ActivationKey { get; set; }
		public Guid Id { get; set; }

		/// <summary>
		/// This is used as the username or identifier for the user. If using windows auth, this will
		/// be the user name from active directory.
		/// </summary>
		public string Email { get; set; }
		public string Firstname { get; set; }
		public bool IsEditor { get; set; }
		public bool IsAdmin { get; set; }
		public bool IsActivated { get; set; }
		public string Lastname { get; set; }
		/// <summary>
		/// Do not set the password using this property - use <see cref="SetPassword"/> instead.
		/// <see cref="HashPassword"/> for authentication with the salt and password.
		/// </summary>
		public string Password { get; internal set; }

		public string PasswordResetKey { get; set; }

		/// <summary>
		/// Do not use this property to set the password - use <see cref="SetPassword"/> instead. Use
		/// <see cref="HashPassword"/> for authentication with the salt and password.
		/// </summary>
		public string Salt { get; set; }

		/// <summary>
		/// This field is for page modifiedby/created by, and is a 'friendly' name. For windows auth the email field is used instead.
		/// </summary>
		public string Username { get; set; }

		public Guid ObjectId
		{
			get { return Id; }
			set { Id = value; }
		}

		public void SetPassword(string password)
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

		public UserSummary ToSummary()
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
