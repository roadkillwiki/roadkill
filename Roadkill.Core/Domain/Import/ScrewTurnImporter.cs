using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;

namespace Roadkill.Core
{
	/// <summary>
	/// Retrieves page data from a ScrewTurn wiki database, and attempts to import the data into Roadkill.
	/// </summary>
	public class ScrewTurnImporter : IWikiImporter
	{
		private string _connectionString;
		private string _attachmentsFolder;

		/// <summary>
		/// Indicates whether the class should convert the page sources to Creole wiki format. This is not implemented.
		/// </summary>
		public bool ConvertToCreole { get; set; }

		/// <summary>
		/// Imports page data from a Screwturn database using the provided connection string.
		/// </summary>
		/// <param name="connectionString">The database connection string.</param>
		public void ImportFromSql(string connectionString)
		{
			_connectionString = connectionString;
			_attachmentsFolder = RoadkillSettings.AttachmentsFolder;

			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				using (SqlCommand command = connection.CreateCommand())
				{
					connection.Open();
					command.CommandText = "SELECT p.*,pc.[User] as [User],pc.Revision,pc.LastModified FROM Page p " +
											"INNER JOIN PageContent pc ON pc.Page = p.Name " +
											"WHERE pc.Revision = (SELECT MAX(Revision) FROM PageContent WHERE Page=p.Name)";

					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							Page page = new Page();
							page.Title = reader["Name"].ToString();
							page.CreatedBy = reader["User"].ToString();
							page.CreatedOn = (DateTime)reader["CreationDateTime"];
							page.ModifiedBy = reader["User"].ToString();
							page.ModifiedOn = (DateTime)reader["LastModified"];

							string categories = GetCategories(page.Title);
							if (!string.IsNullOrWhiteSpace(categories))
								categories += ";";
							page.Tags = categories;

							NHibernateRepository.Current.SaveOrUpdate<Page>(page);
							AddContent(page);
						}
					}
				}
			}

			ImportFiles();
		}

		/// <summary>
		/// Saves all files from the File table to the attachments folder.
		/// </summary>
		private void ImportFiles()
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
							SaveFile(reader.GetString(0) + reader.GetString(1),(byte[]) reader[2]);
						}
					}			
				}
			}
		}

		/// <summary>
		/// Saves a single file to the attachment folder/subfolder.
		/// </summary>
		private void SaveFile(string filename, byte[] data)
		{
			try
			{
				string filePath = string.Format("{0}{1}",_attachmentsFolder, filename);
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
		private string GetCategories(string page)
		{
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				using (SqlCommand command = connection.CreateCommand())
				{
					connection.Open();
					command.CommandText = "SELECT Category from CategoryBinding WHERE Page=@Page";

					SqlParameter parameter = new SqlParameter();
					parameter.ParameterName = "@Page";
					parameter.SqlDbType = System.Data.SqlDbType.VarChar;
					parameter.Value = page;
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
		private void AddContent(Page page)
		{
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				using (SqlCommand command = connection.CreateCommand())
				{
					connection.Open();
					command.CommandText = "SELECT * FROM PageContent WHERE Page = @Page";
					
					SqlParameter parameter = new SqlParameter();
					parameter.ParameterName = "@Page";
					parameter.SqlDbType = System.Data.SqlDbType.VarChar;
					parameter.Value = page.Title;
					command.Parameters.Add(parameter);

					List<PageContent> categories = new List<PageContent>();
					bool hasContent = false;
					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							PageContent content = new PageContent();
							content.EditedBy = reader["User"].ToString();
							content.EditedOn = (DateTime)reader["LastModified"];
							content.Text = reader["Content"].ToString();
							content.Text = CleanContent(content.Text);
							content.VersionNumber = (int.Parse(reader["Revision"].ToString())) + 1;
							content.Page = page;

							NHibernateRepository.Current.SaveOrUpdate<PageContent>(content);
							hasContent = true;
						}
					}

					// For broken content, make sure the page has something
					if (!hasContent)
					{
						PageContent content = new PageContent();
						content.EditedBy = "unknown";
						content.EditedOn = DateTime.Now;
						content.Text = "";
						content.VersionNumber = 1;
						content.Page = page;

						NHibernateRepository.Current.SaveOrUpdate<PageContent>(content);
					}
				}
			}
		}

		/// <summary>
		/// Attempts to clean the Screwturn wiki syntax so it loosely matches media wiki format.
		/// </summary>
		private string CleanContent(string text)
		{
			// Screwturn uses "[" for links instead of "[[", so do a crude replace.
			// Needs more coverage for @@ blocks, variables, toc.
			text = text.Replace("[", "[[").Replace("]", "]]").Replace("{BR}", "\n").Replace("{UP}","");

			// Handle nowiki blocks being a little strange
			Regex regex = new Regex("@@(.*?)@@",RegexOptions.Singleline);
			if (regex.IsMatch(text))
			{
				text = regex.Replace(text,"<nowiki>$1</nowiki>");
			}

			return text;
		}
	}
}
