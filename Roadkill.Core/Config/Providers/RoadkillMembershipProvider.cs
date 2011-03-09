using System;
using System.Web.Security;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Web.Configuration;

namespace Roadkill.Core
{
	public class RoadkillMembershipProvider : SqlMembershipProvider
	{
		private string _connectionString;

		protected string ConnectionString
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_connectionString))
					_connectionString = GetConnectionStringFromConfig();

				return _connectionString;
			}
		}

		public override string Description
		{
			get
			{
				return "A membership provider based on SqlMembershipProvider for the Roadkill wiki engine. Passwords are hashed one way and not retrievable";
			}
		}

		/// <summary>
		/// A new password is auto-generated for password requests so this is not required.
		/// </summary>
		public override bool EnablePasswordReset
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// A new password is auto-generated for password requests so this is not required.
		/// </summary>
		public override bool EnablePasswordRetrieval
		{
			get
			{
				return false;
			}
		}

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
		/// A new password is auto-generated for password requests so this is not required.
		/// </summary>
		public override bool RequiresQuestionAndAnswer
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Returns MembershipPasswordFormat.Hashed, as passwords are always SHA1 encrypted in Roadkill.
		/// </summary>
		public override MembershipPasswordFormat PasswordFormat
		{
			get
			{
				return MembershipPasswordFormat.Hashed;
			}
		}

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

		private string GetConnectionStringFromConfig()
		{
			Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
			MembershipSection section = config.SectionGroups["system.web"].Sections["membership"] as MembershipSection;
			string defaultProvider = section.DefaultProvider;
			string connstringName = section.Providers[defaultProvider].ElementInformation.Properties["connectionStringName"].Value.ToString();

			return config.ConnectionStrings.ConnectionStrings[connstringName].ConnectionString;
		}
	}
}
