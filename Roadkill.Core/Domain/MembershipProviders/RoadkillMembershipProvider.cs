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
				{
					Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
					MembershipSection section = config.SectionGroups["system.web"].Sections["membership"] as MembershipSection;
					string defaultProvider = section.DefaultProvider;
					string connstringName = section.Providers[defaultProvider].ElementInformation.Properties["connectionStringName"].Value.ToString();
					_connectionString = config.ConnectionStrings.ConnectionStrings[connstringName].ConnectionString;
				}

				return _connectionString;
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
	}
}
