using System;
using System.Text;
using System.Web.Security;
using System.Security.Cryptography;
using Roadkill.Core.Security;

namespace Roadkill.Core.Database
{
	/// <summary>
	/// A user object for use with the data store, whatever that might be (e.g. an RDMS or MongoDB)
	/// </summary>
	public class User : IDataStoreEntity
	{
		/// <summary>
		/// Gets or sets the activation key for the user.
		/// </summary>
		/// <value>
		/// The activation key.
		/// </value>
		public string ActivationKey { get; set; }

		/// <summary>
		/// Gets or sets the unique ID for the settings.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		public Guid Id { get; set; }

		/// <summary>
		/// Gets or sets the email address for the user. This is used as the username or identifier 
		/// for the user.
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// Gets or sets the firstname of the user.
		/// </summary>
		/// <value>
		/// The firstname.
		/// </value>
		public string Firstname { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user has editor rights for pages.
		/// </summary>
		/// <value>
		///   <c>true</c> if they are an editor; otherwise, <c>false</c>.
		/// </value>
		public bool IsEditor { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the user has admin rights.
		/// </summary>
		/// <value>
		///   <c>true</c> if they have admin rights (which includes editor rights) otherwise, <c>false</c>.
		/// </value>
		public bool IsAdmin { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the account has been activated.
		/// </summary>
		/// <value>
		///   <c>true</c> if the user is activated; otherwise, <c>false</c>.
		/// </value>
		public bool IsActivated { get; set; }

		/// <summary>
		/// Gets or sets the lastname of the user.
		/// </summary>
		/// <value>
		/// The lastname.
		/// </value>
		public string Lastname { get; set; }

		/// <summary>
		/// Gets the hashed password for the user. 
		/// </summary>
		/// <remarks>
		/// Use <see cref="SetPassword"/> to set the user's password, and 
		/// <see cref="User.HashPassword"/> to encrypt a plain text password for authentication with the salt and password.
		/// </remarks>
		public string Password { get; internal set; }

		/// <summary>
		/// Gets or sets the password reset key.
		/// </summary>
		/// <value>
		/// The password reset key, which is blank by default.
		/// </value>
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

		/// <summary>
		/// The unique id for this object, this is the same as the <see cref="Id"/> property.
		/// </summary>
		public Guid ObjectId
		{
			get { return Id; }
			set { Id = value; }
		}

		/// <summary>
		/// Encrypts and sets the password for the user.
		/// </summary>
		/// <param name="password">The password in plain text format.</param>
		public void SetPassword(string password)
		{
			Salt = new Salt();
			Password = HashPassword(password,Salt);
		}

		/// <summary>
		/// Hashes a combination of the password and salt using SHA1 via FormsAuthentication, or 
		/// SHA256 is FormsAuthentication is not enabled.
		/// </summary>
		public static string HashPassword(string password, string salt)
		{
			bool isFormsAuthEnabled = FormsAuthenticationWrapper.IsEnabled();

			if (isFormsAuthEnabled)
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
	}
}
