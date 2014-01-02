using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;
using StructureMap;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Services;

namespace Roadkill.Core.Import
{
	/// <summary>
	/// Retrieves page data from a ScrewTurn wiki database, and attempts to import the data into Roadkill.
	/// </summary>
	public class ScrewTurnImporter : IWikiImporter
	{
		private string _connectionString;
		private string _attachmentsFolder;
		protected IRepository Repository;
		protected ApplicationSettings ApplicationSettings;

		public ScrewTurnImporter(ApplicationSettings settings, IRepository repository)
		{
			Repository = repository;
			ApplicationSettings = settings;
			_attachmentsFolder = ApplicationSettings.AttachmentsDirectoryPath;
		}

		/// <summary>
		/// Imports page data from a Screwturn database using the provided connection string.
		/// </summary>
		/// <param name="connectionString">The database connection string.</param>
		public void ImportFromSqlServer(string connectionString)
		{
			_connectionString = connectionString;

			ImportUsers();
			ImportPages();
			ImportFiles();
		}

		/// <summary>
		/// Imports all users from the users table.
		/// </summary>
		private void ImportUsers()
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(_connectionString))
				{
					using (SqlCommand command = connection.CreateCommand())
					{
						connection.Open();
						command.CommandText = "SELECT * FROM [User]";

						using (SqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								string username = reader["Username"].ToString();
								if (!string.IsNullOrEmpty(username) && !string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase))
								{
									string email = reader["Email"].ToString();

									User user = new User();
									user.Id = Guid.NewGuid();
									user.IsEditor = true;
									user.IsAdmin = false;
									user.Email = email;
									user.Username = username;
									user.IsActivated = false;
									user.SetPassword("password");

									Repository.SaveOrUpdateUser(user);
								}
							}
						}
					}
				}
			}
			catch (SqlException ex)
			{
				throw new DatabaseException(ex, "Unable to import the pages from Screwturn - have you configured it to use the SQL Server users provider? \n{0}", ex.Message);
			}
		}
		
		/// <summary>
		/// Adds all pages and their content from screwturn.
		/// </summary>
		private void ImportPages()
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(_connectionString))
				{
					using (SqlCommand command = connection.CreateCommand())
					{
						connection.Open();
						command.CommandText = @"SELECT 
												p.CreationDateTime,
												p.Name,
												pc.[User] as [User],
												pc.Title,
												pc.Revision,
												pc.LastModified 
											FROM [Page] p
												INNER JOIN [PageContent] pc ON pc.[Page] = p.Name
											WHERE 
												pc.Revision = (SELECT MAX(Revision) FROM PageContent WHERE [Page]=p.Name)";

						using (SqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								string pageName = reader["Name"].ToString();

								Page page = new Page();
								page.Title = reader["Title"].ToString();
								page.CreatedBy = reader["User"].ToString();
								page.CreatedOn = (DateTime)reader["CreationDateTime"];
								page.ModifiedBy = reader["User"].ToString();
								page.ModifiedOn = (DateTime)reader["LastModified"];

								string categories = GetCategories(pageName);
								if (!string.IsNullOrWhiteSpace(categories))
									categories += ";";
								page.Tags = categories;

								page = Repository.SaveOrUpdatePage(page);
								AddContent(pageName, page);
							}
						}
					}
				}
			}
			catch (SqlException ex)
			{
				throw new DatabaseException(ex, "Unable to import the pages from Screwturn - have you configured it to use the SQL Server pages provider? \n{0}", ex.Message);
			}
		}

		/// <summary>
		/// Saves all files from the File table to the attachments folder.
		/// </summary>
		private void ImportFiles()
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(_connectionString))
				{
					using (SqlCommand command = connection.CreateCommand())
					{
						connection.Open();
						command.CommandText = "SELECT directory,name,data FROM [File]";

						using (SqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								SaveFile(reader.GetString(0) + reader.GetString(1), (byte[])reader[2]);
							}
						}
					}
				}
			}
			catch (SqlException ex)
			{
				throw new DatabaseException(ex, "Unable to import the pages from Screwturn - have you configured it to use the SQL Server files provider? \n{0}", ex.Message);
			}
		}

		/// <summary>
		/// Saves a single file to the attachment folder/subfolder.
		/// </summary>
		private void SaveFile(string filename, byte[] data)
		{
			if (string.IsNullOrEmpty(filename))
				return;

			try
			{
				string filePath = Path.GetFullPath(_attachmentsFolder + filename);
				FileInfo info = new FileInfo(filePath);
				if (!info.Exists)
				{
					if (!info.Directory.Exists)
						info.Directory.Create();

					File.WriteAllBytes(filePath, data);
				}
			}
			catch (IOException)
			{
				// TODO: log
			}
		}

		/// <summary>
		/// Returns all categories in the Screwturn database as a ";" delimited string.
		/// </summary>
		private string GetCategories(string pageName)
		{
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				using (SqlCommand command = connection.CreateCommand())
				{
					connection.Open();
					command.CommandText = "SELECT Category from CategoryBinding WHERE [Page]=@Page";

					SqlParameter parameter = new SqlParameter();
					parameter.ParameterName = "@Page";
					parameter.SqlDbType = System.Data.SqlDbType.VarChar;
					parameter.Value = pageName;
					command.Parameters.Add(parameter);

					List<string> categories = new List<string>();
					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							categories.Add(reader.GetString(0).Replace(";","-").Replace(" ","-"));
						}
					}

					return string.Join(";", categories);
				}
			}
		}

		/// <summary>
		/// Extracts and saves all textual content for a page.
		/// </summary>
		/// <param name="page">The page the content belongs to.</param>
		private void AddContent(string pageName, Page page)
		{
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				using (SqlCommand command = connection.CreateCommand())
				{
					connection.Open();
					command.CommandText = "SELECT * FROM PageContent WHERE [Page]=@Page";
					
					SqlParameter parameter = new SqlParameter();
					parameter.ParameterName = "@Page";
					parameter.SqlDbType = System.Data.SqlDbType.VarChar;
					parameter.Value = pageName;
					command.Parameters.Add(parameter);

					List<PageContent> categories = new List<PageContent>();
					bool hasContent = false;
					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							PageContent content = new PageContent();
							string editedBy = reader["User"].ToString();
							DateTime EditedOn = (DateTime)reader["LastModified"];
							string text = reader["Content"].ToString();
							text = CleanContent(text);
							int versionNumber = (int.Parse(reader["Revision"].ToString())) + 1;

							Repository.AddNewPageContentVersion(page, text, editedBy, EditedOn, versionNumber);
							hasContent = true;
						}
					}

					// For broken content, make sure the page has something
					if (!hasContent)
					{
						Repository.AddNewPage(page, "", "unknown", DateTime.UtcNow);
					}
				}
			}
		}

		/// <summary>
		/// Attempts to clean the Screwturn wiki syntax so it loosely matches media wiki format.
		/// </summary>
		private string CleanContent(string text)
		{
			if (string.IsNullOrEmpty(text))
				return text;

			// Screwturn uses "[" for links instead of "[[", so do a crude replace.
			// Files aren't done using File:/ but instead {UP}
			// This needs more coverage for @@ blocks, variables, toc.
			text = text.Replace("[", "[[")
						.Replace("]", "]]")
						.Replace("{BR}", "\n")
						.Replace("imageleft||","")
						.Replace("{UP}/","File:/");

			// Handle nowiki blocks being a little strange
			Regex regex = new Regex("@@(.*?)@@",RegexOptions.Singleline);
			if (regex.IsMatch(text))
			{
				text = regex.Replace(text,"<nowiki>$1</nowiki>");
			}

			return text;
		}

		/// <summary>
		/// Updates the search index after a successful import.
		/// </summary>
		public void UpdateSearchIndex(SearchService searchService)
		{
			searchService.CreateIndex();
		}
	}
}
