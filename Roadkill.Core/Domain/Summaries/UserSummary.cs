using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;
using System.Text.RegularExpressions;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides a data summary class for passing between actions. (better summary)
	/// </summary>
	[CustomValidation(typeof(UserSummary), "VerifyNewUsername")]
	[CustomValidation(typeof(UserSummary), "VerifyPassword")]
	[CustomValidation(typeof(UserSummary), "VerifyPasswordsMatch")]
	public class UserSummary
	{
		public string ExistingUsername { get; set; }
		[Required]
		public string NewUsername { get; set; }
		public string Password { get; set; }
		public string PasswordConfirmation { get; set; }
		public bool IsNew { get; set; }
		public bool UsernameHasChanged
		{
			get
			{
				return ExistingUsername != NewUsername;
			}
		}

		/// <summary>
		/// Checks if the <see cref="NewUsername"/> provided is already a user in the system.
		/// </summary>
		/// <param name="user"></param>
		/// <returns><see cref="ValidationResult.Success"/> if the username hasn't changed, 
		/// or if it has and the new username doesn't  already exist.</returns>
		public static ValidationResult VerifyNewUsername(UserSummary user, ValidationContext context)
		{
			// Only check if the username has changed
			if (user.IsNew || user.ExistingUsername != user.NewUsername)
			{
				if (Membership.FindUsersByName(user.NewUsername).Count > 0)
				{
					return new ValidationResult(string.Format("{0} already exists as a user", user.NewUsername));
				}
			}

			return ValidationResult.Success;
		}

		/// <summary>
		/// Ensures the two passwords match.
		/// </summary>
		public static ValidationResult VerifyPasswordsMatch(UserSummary user, ValidationContext context)
		{
			// A blank password indicates no change is occuring.
			if (!user.IsNew && string.IsNullOrEmpty(user.Password))
			{
				return ValidationResult.Success;
			}

			if (user.Password == user.PasswordConfirmation)
			{
				return ValidationResult.Success;
			}
			else
			{
				return new ValidationResult("The passwords don't match.");
			}
		}

		/// <summary>
		/// Ensures the password is minimum length and strength set by the Membership provider.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static ValidationResult VerifyPassword(UserSummary user, ValidationContext context)
		{
			// A blank password indicates no change is occuring.
			if (!user.IsNew && string.IsNullOrEmpty(user.Password))
			{
				return ValidationResult.Success;
			}
			else
			{
				if (string.IsNullOrEmpty(user.Password) || user.Password.Length < Membership.MinRequiredPasswordLength)
				{
					return new ValidationResult(string.Format("The password is less than {0} characters", Membership.MinRequiredPasswordLength));
				}
				else if (!string.IsNullOrEmpty(Membership.PasswordStrengthRegularExpression))
				{
					Regex regex = new Regex(Membership.PasswordStrengthRegularExpression);

					if (!regex.IsMatch(user.Password))
					{
						return new ValidationResult("The password does not meet the minimum password strength.");
					}		
				}
			}

			return ValidationResult.Success;
		}
	}
}
