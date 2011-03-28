using System;
using System.Web.Security;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Web.Configuration;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides a <see cref="MembershipProvider"/> implementation based on <see cref="SqlMembershipProvider"/>,
	/// customed for the roadkill usage of the provider.
	/// </summary>
	public class RoadkillMembershipProvider : SqlMembershipProvider
	{
		private string _connectionString;

		/// <summary>
		/// The current SQL connection string for the provider.
		/// </summary>
		protected string ConnectionString
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_connectionString))
					_connectionString = RoadkillSettings.LdapConnectionString;

				return _connectionString;
			}
		}

		/// <summary>
		/// Gets a brief, friendly description suitable for display in administrative tools or other user interfaces (UIs).
		/// </summary>
		/// <returns>A brief, friendly description suitable for display in administrative tools or other UIs.</returns>
		public override string Description
		{
			get
			{
				return "A membership provider based on SqlMembershipProvider for the Roadkill wiki engine. Passwords are hashed one way and not retrievable";
			}
		}

		/// <summary>
		/// Returns true. This is required to be true for the password changing mechanism.
		/// </summary>
		public override bool EnablePasswordReset
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Returns false, as a new password is auto-generated for password in Roadkill.
		/// </summary>
		public override bool EnablePasswordRetrieval
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets or sets the name of the application to store and retrieve membership information for.
		/// </summary>
		/// <returns>The name of the application to store and retrieve membership information for. The default is the <see cref="P:System.Web.HttpRequest.ApplicationPath"/> property value for the current <see cref="P:System.Web.HttpContext.Request"/>.</returns>
		///   
		/// <exception cref="T:System.ArgumentException">An attempt was made to set the <see cref="P:System.Web.Security.SqlMembershipProvider.ApplicationName"/> property to an empty string or null.</exception>
		///   
		/// <exception cref="T:System.Configuration.Provider.ProviderException">An attempt was made to set the <see cref="P:System.Web.Security.SqlMembershipProvider.ApplicationName"/> property to a string that is longer than 256 characters.</exception>
		public override string ApplicationName
		{
			get
			{
				return "roadkill";
			}
			set
			{
				throw new NotImplementedException("ApplicationName is readonly for the RoadkillMembershipProvider");
			}
		}

		/// <summary>
		/// Returns false, as a new password is auto-generated for password requests in roadkill.
		/// </summary>
		public override bool RequiresQuestionAndAnswer
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Returns <see cref="MembershipPasswordFormat.Hashed"/>, as passwords are always SHA1 encrypted in Roadkill.
		/// </summary>
		public override MembershipPasswordFormat PasswordFormat
		{
			get
			{
				return MembershipPasswordFormat.Hashed;
			}
		}

		/// <summary>
		/// Changes the username, an additional feature not provided by the standard MembershipProvider.
		/// </summary>
		/// <param name="oldUsername">The old username.</param>
		/// <param name="newUsername">The new username.</param>
		/// <returns></returns>
		public bool ChangeUsername(string oldUsername, string newUsername)
		{
			if (string.IsNullOrWhiteSpace(oldUsername))
				throw new ArgumentNullException("oldUsername cannot be null or empty");

			if (string.IsNullOrWhiteSpace(newUsername))
				throw new ArgumentNullException("newUsername cannot be null or empty");

			if (oldUsername == newUsername)
				return true;

			using (SqlConnection connection = new SqlConnection(ConnectionString))
			{
				connection.Open();

				using (SqlCommand command = connection.CreateCommand())
				{
					command.CommandText = "UPDATE aspnet_Users SET UserName=@NewUsername,LoweredUserName=@LoweredNewUsername WHERE UserName=@OldUsername";

					SqlParameter parameter = new SqlParameter("@OldUsername", SqlDbType.VarChar);
					parameter.Value = oldUsername;
					command.Parameters.Add(parameter);

					parameter = new SqlParameter("@NewUsername", SqlDbType.VarChar);
					parameter.Value = newUsername;
					command.Parameters.Add(parameter);

					parameter = new SqlParameter("@LoweredNewUsername", SqlDbType.VarChar);
					parameter.Value = newUsername.ToLower();
					command.Parameters.Add(parameter);

					return command.ExecuteNonQuery() > 0;
				}
			}
		}
	}
}
