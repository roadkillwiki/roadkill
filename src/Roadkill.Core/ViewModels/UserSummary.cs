using System;
using System.ComponentModel.DataAnnotations;
using Roadkill.Core.Localization.Resx;
using Roadkill.Core.Configuration;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides a data summary class for creating and saving user details.
	/// </summary>
	[CustomValidation(typeof(UserSummary), "VerifyNewUsername")]
	[CustomValidation(typeof(UserSummary), "VerifyNewUsernameIsNotInUse")]
	[CustomValidation(typeof(UserSummary), "VerifyNewEmail")]
	[CustomValidation(typeof(UserSummary), "VerifyNewEmailIsNotInUse")]
	[CustomValidation(typeof(UserSummary), "VerifyPassword")]
	[CustomValidation(typeof(UserSummary), "VerifyPasswordsMatch")]
	public class UserSummary
	{
		protected IConfigurationContainer Config;
		protected UserManager UserManager;

		public UserSummary()
		{
			// Used for non-controllers
		}

		public UserSummary(IConfigurationContainer config, UserManager userManager)
		{
			Config = config;
			UserManager = userManager;
		}

		/// <summary>
		/// The user's id
		/// </summary>
		public Guid? Id { get; set; }

		public string ActivationKey { get; set; }

		/// <summary>
		/// The firstname of the user.
		/// </summary>
		public string Firstname { get; set; }
		
		/// <summary>
		/// The last name of the user.
		/// </summary>
		public string Lastname { get; set; }

		/// <summary>
		/// The current (or if being changed, previous) username.
		/// Use <see cref="NewUsername"/> for Signups.
		/// </summary>
		public string ExistingUsername { get; set; }

		/// <summary>
		/// The username to change to. For no change this should be the same as <see cref="ExistingUsername"/>.
		/// For new signups this field contains the username.
		/// </summary>
		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "User_Validation_Username")]
		public string NewUsername { get; set; }

		/// <summary>
		/// The current (or if being changed, previous) email.
		/// Use <see cref="NewEmail"/> for Signups.
		/// </summary>
		public string ExistingEmail{ get; set; }

		/// <summary>
		/// The email to change to. For no change this should be the same as <see cref="ExistingEmail"/>
		/// </summary>
		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "User_Validation_Email")]
		public string NewEmail { get; set; }

		/// <summary>
		/// The password to change to. Leave blank to keep the existing password.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// The password confirmation, which should be the same value as <see cref="Password"/>
		/// </summary>
		public string PasswordConfirmation { get; set; }

		/// <summary>
		/// The Guid used for password reset emails.
		/// </summary>
		public string PasswordResetKey { get; set; }

		/// <summary>
		/// If the user is being created via the settings->users interface (to work around data annotation GUID issues).
		/// </summary>
		public bool IsBeingCreatedByAdmin { get; set; }

		/// <summary>
		/// Indicates whether the username has changed.
		/// </summary>
		public bool UsernameHasChanged
		{
			get
			{
				return ExistingUsername != NewUsername;
			}
		}

		/// <summary>
		/// Indicates whether the email changed.
		/// </summary>
		public bool EmailHasChanged
		{
			get
			{
				return ExistingEmail != NewEmail;
			}
		}

		/// <summary>
		/// Checks if the <see cref="NewUsername"/> provided is valid.
		/// </summary>
		/// <param name="user"></param>
		/// <returns><see cref="ValidationResult.Success"/> if the username isn't empty.</returns>
		public static ValidationResult VerifyNewUsername(UserSummary user, ValidationContext context)
		{
			// Only check if it's a new user, OR the username has changed
			if (!string.IsNullOrEmpty(user.NewUsername) && user.NewUsername.Trim().Length == 0)
			{
				return new ValidationResult(string.Format(SiteStrings.User_Validation_UsernameEmpty, user.NewUsername));
			}
			else
			{
				return ValidationResult.Success;
			}
		}

		/// <summary>
		/// Checks if the <see cref="NewUsername"/> provided is already a user in the system.
		/// </summary>
		/// <param name="user"></param>
		/// <returns><see cref="ValidationResult.Success"/> if the username hasn't changed, 
		/// or if it has and the new username doesn't  already exist.</returns>
		public static ValidationResult VerifyNewUsernameIsNotInUse(UserSummary user, ValidationContext context)
		{
			// Only check if it's a new user, OR the username has changed
			if ((user.IsBeingCreatedByAdmin || user.Id == null) || user.ExistingUsername != user.NewUsername)
			{
				if (user.UserManager == null || user.UserManager.UserNameExists(user.NewUsername))
				{
					return new ValidationResult(string.Format(SiteStrings.User_Validation_UsernameExists, user.NewUsername));
				}
			}

			return ValidationResult.Success;
		}

		/// <summary>
		/// Checks if the <see cref="NewEmail"/> is a valid format.
		/// </summary>
		/// <param name="user"></param>
		/// <returns><see cref="ValidationResult.Success"/> if the email has an @ in it.</returns>
		public static ValidationResult VerifyNewEmail(UserSummary user, ValidationContext context)
		{
			// Only check if it's a new user, OR the email has changed
			if (!user.NewEmail.Contains("@"))
			{
				return new ValidationResult(SiteStrings.User_Validation_Email_Check);
			}
			else
			{
				return ValidationResult.Success;
			}
		}

		/// <summary>
		/// Checks if the <see cref="NewEmail"/> provided is already a user in the system.
		/// </summary>
		/// <param name="user"></param>
		/// <returns><see cref="ValidationResult.Success"/> if the email hasn't changed, 
		/// or if it has and the new email doesn't  already exist.</returns>
		public static ValidationResult VerifyNewEmailIsNotInUse(UserSummary user, ValidationContext context)
		{
			// Only check if it's a new user, OR the email has changed
			if ((user.IsBeingCreatedByAdmin || user.Id == null) || user.ExistingEmail != user.NewEmail)
			{
				if (user.UserManager == null || user.UserManager.UserExists(user.NewEmail))
				{
					return new ValidationResult(string.Format(SiteStrings.User_Validation_EmailExists, user.NewEmail));
				}
			}

			return ValidationResult.Success;
		}

		/// <summary>
		/// Ensures the two passwords match.
		/// </summary>
		public static ValidationResult VerifyPasswordsMatch(UserSummary user, ValidationContext context)
		{
			if (user.IsBeingCreatedByAdmin && user.Password != user.PasswordConfirmation)
			{
				return new ValidationResult(SiteStrings.User_Validation_PasswordsDontMatch);
			}

			// If it's an existing user, then a blank password indicates no change is occurring.
			if (user.Id != null && string.IsNullOrEmpty(user.Password))
			{
				return ValidationResult.Success;
			}

			if (user.Password == user.PasswordConfirmation)
			{
				return ValidationResult.Success;
			}
			else
			{
				return new ValidationResult(SiteStrings.User_Validation_PasswordsDontMatch);
			}
		}

		/// <summary>
		/// Ensures the password is minimum length and strength set by the Membership provider.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static ValidationResult VerifyPassword(UserSummary user, ValidationContext context)
		{
			if (user.IsBeingCreatedByAdmin && (string.IsNullOrEmpty(user.Password) || user.Password.Length < user.Config.ApplicationSettings.MinimumPasswordLength))
			{
				return new ValidationResult(string.Format(SiteStrings.User_Validation_PasswordTooShort, user.Config.ApplicationSettings.MinimumPasswordLength));
			}

			if (user.Id != null && string.IsNullOrEmpty(user.Password))
			{
				// Existing user, a blank password indicates no change is occurring.
				return ValidationResult.Success;
			}
			else if (string.IsNullOrEmpty(user.Password) || user.Password.Length < user.Config.ApplicationSettings.MinimumPasswordLength)
			{
				// New or existing users with invalid passwords
				return new ValidationResult(string.Format(SiteStrings.User_Validation_PasswordTooShort, user.Config.ApplicationSettings.MinimumPasswordLength));
			}

			return ValidationResult.Success;
		}
	}
}
