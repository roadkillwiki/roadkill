using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;
using Roadkill.Core.Plugins;

namespace Roadkill.Core.Database.Export
{
	/// <summary>
	/// Exports the current Roadkill installation data as SQL scripts.
	/// </summary>
	public class SqlExportBuilder
	{
		private readonly IRepository _repository;
		private readonly IPluginFactory _pluginFactory;

		/// <summary>
		/// Gets or sets a value indicating whether to include page data in the SQL script.
		/// </summary>
		/// <value>
		///   <c>true</c> if pages data should be included; otherwise, <c>false</c>.
		/// </value>
		public bool IncludePages { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to include configuration data in the SQL script.
		/// </summary>
		/// <value>
		///   <c>true</c> if configuration data should be included; otherwise, <c>false</c>.
		/// </value>
		public bool IncludeConfiguration { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlExportBuilder"/> class.
		/// </summary>
		/// <param name="repository">The current repository.</param>
		/// <param name="pluginFactory">The plugin factory.</param>
		/// <exception cref="System.ArgumentNullException">
		/// repository
		/// or
		/// pluginFactory are null.
		/// </exception>
		public SqlExportBuilder(IRepository repository, IPluginFactory pluginFactory)
		{
			if (repository == null)
				throw new ArgumentNullException("repository");

			if (pluginFactory == null)
				throw new ArgumentNullException("pluginFactory");

			_repository = repository;
			_pluginFactory = pluginFactory;
			IncludePages = true;
			IncludeConfiguration = true;
		}

		/// <summary>
		/// Exports all Roadkill data as a SQL92 compliant script.
		/// </summary>
		/// <returns>The exported SQL, or any errors that occurred.</returns>
		public string Export()
		{
			try
			{
				IEnumerable<User> users = _repository.FindAllAdmins().Union(_repository.FindAllEditors());
				IEnumerable<Page> pages = _repository.AllPages();
				IEnumerable<PageContent> pageContent = _repository.AllPageContents();
				IEnumerable<SiteConfigurationRow> configurationRows = GetSiteConfigurationRows();

				// The order of the SQL is important - users should come before pages, pages before content.
				string usersSql = GetSqlLines<User>(users, GetUsersInsertSql);
				string pagesSql = GetSqlLines<Page>(pages, GetPagesInsertSql);
				string pageContentSql = GetSqlLines<PageContent>(pageContent, GetPageContentInsertSql);
				string configurationSql = GetSqlLines<SiteConfigurationRow>(configurationRows, GetSiteConfigurationInsertSql);

				StringBuilder sqlBuilder = new StringBuilder();
				sqlBuilder.AppendLine("-- You will need to enable identity inserts for your chosen db before running this Script, for example in SQL Server:");
				sqlBuilder.AppendLine("-- SET IDENTITY_INSERT roadkill_pages ON;");

				// Always output the users, it's used by the pages
				sqlBuilder.AppendLine();
				sqlBuilder.AppendLine("-- Users");
				sqlBuilder.AppendLine(usersSql);

				if (IncludePages)
				{
					if (!string.IsNullOrWhiteSpace(pagesSql))
					{
						sqlBuilder.AppendLine();
						sqlBuilder.AppendLine("-- Pages");
						sqlBuilder.AppendLine(pagesSql);
					}

					if (!string.IsNullOrWhiteSpace(pageContentSql))
					{
						sqlBuilder.AppendLine();
						sqlBuilder.AppendLine("-- Pages contents");
						sqlBuilder.AppendLine(pageContentSql);
					}
				}

				if (IncludeConfiguration)
				{
					if (!string.IsNullOrWhiteSpace(configurationSql))
					{
						sqlBuilder.AppendLine();
						sqlBuilder.AppendLine("-- Configuration");
						sqlBuilder.AppendLine(configurationSql);
					}
				}

				Log.Debug("Sql export successfully written: \n\n{0}", sqlBuilder);
				return sqlBuilder.ToString();
			}
			catch (Exception e)
			{
				Log.Error(e, "An error occurred exporting the Roadkill database");
				return "An error occurred writing the SQL: " + e.Message;
			}
		}

		/// <summary>
		/// This avoids a bit of repetition, but is harder to read. Takes a  list of objects,
		/// and calls a method to generate the sql for each item in the list. The sql is then 
		/// joined to together with a newline.
		/// </summary>
		private string GetSqlLines<T>(IEnumerable<T> items, Func<T, string> sqlInsertScriptMethod)
		{	
			string[] sqlRows = items.Select(x => sqlInsertScriptMethod(x)).ToArray();
			return string.Join(Environment.NewLine, sqlRows);
		}

		private IEnumerable<SiteConfigurationRow> GetSiteConfigurationRows()
		{
			List<SiteConfigurationRow> configurationRows = new List<SiteConfigurationRow>();
			
			// Turn the main SiteSettings into a row
			SiteSettings settings = _repository.GetSiteSettings();
			SiteConfigurationRow row = new SiteConfigurationRow()
			{
				Id = SiteSettings.SiteSettingsId,
				Version = ApplicationSettings.FileVersion,
				Json = settings.GetJson()
			};

			configurationRows.Add(row);

			// Turn all plugin settings into rows
			IEnumerable<TextPlugin> plugins = _pluginFactory.GetTextPlugins();

			foreach (TextPlugin plugin in plugins)
			{
				row = new SiteConfigurationRow()
				{
					Id = plugin.DatabaseId,
					Version = plugin.Version,
					Json = plugin.GetSettingsJson()
				};

				configurationRows.Add(row);
			}

			return configurationRows;
		}

		internal string GetPagesInsertSql(Page page)
		{
			if (page == null)
				return "";

			string sql = "INSERT INTO roadkill_pages (id, title, createdby, createdon, modifiedby, modifiedon, tags, islocked) VALUES (";
			sql += string.Format("'{0}',", page.Id);
			sql += string.Format("'{0}',", page.Title.ReplaceSingleQuotes());
			sql += string.Format("'{0}',", page.CreatedBy.ReplaceSingleQuotes());
			sql += string.Format("'{0}',", page.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss"));
			sql += string.Format("'{0}',", page.ModifiedBy.ReplaceSingleQuotes());
			sql += string.Format("'{0}',", page.ModifiedOn.ToString("yyyy-MM-dd HH:mm:ss"));
			sql += string.Format("'{0}',", page.Tags.ReplaceSingleQuotes());
			sql += string.Format("'{0}'",  page.IsLocked ? "1" : "0");

			sql += ");";

			return sql;
		}

		internal string GetPageContentInsertSql(PageContent content)
		{
			if (content == null)
				return "";

			string sql = "INSERT INTO roadkill_pagecontent (id, pageid, text, editedby, editedon, versionnumber) VALUES (";
			sql += string.Format("'{0}',", content.Id);
			sql += string.Format("'{0}',", content.Page.Id);
			sql += string.Format("'{0}',", content.Text.ReplaceSingleQuotes());
			sql += string.Format("'{0}',", content.EditedBy.ReplaceSingleQuotes());
			sql += string.Format("'{0}',", content.EditedOn.ToString("yyyy-MM-dd HH:mm:ss"));
			sql += string.Format("'{0}'",  content.VersionNumber);

			sql += ");";

			return sql;
		}

		internal string GetUsersInsertSql(User user)
		{
			if (user == null)
				return "";

			string sql = "INSERT INTO roadkill_users (id, activationkey, email, firstname, iseditor, isadmin, isactivated, lastname, password, passwordresetkey, salt, username) VALUES (";
			sql += string.Format("'{0}',", user.Id);
			sql += string.Format("'{0}',", user.ActivationKey);
			sql += string.Format("'{0}',", user.Email.ReplaceSingleQuotes());
			sql += string.Format("'{0}',", user.Firstname.ReplaceSingleQuotes());
			sql += string.Format("'{0}',", user.IsEditor ? "1" : "0");
			sql += string.Format("'{0}',", user.IsAdmin ? "1" : "0");
			sql += string.Format("'{0}',", user.IsActivated ? "1" : "0");
			sql += string.Format("'{0}',", user.Lastname.ReplaceSingleQuotes());
			sql += string.Format("'{0}',", user.Password.ReplaceSingleQuotes());
			sql += string.Format("'{0}',", user.PasswordResetKey);
			sql += string.Format("'{0}',", user.Salt.ReplaceSingleQuotes());
			sql += string.Format("'{0}'",  user.Username.ReplaceSingleQuotes());

			sql += ");";

			return sql;
		}

		internal string GetSiteConfigurationInsertSql(SiteConfigurationRow row)
		{
			if (row == null)
				return "";

			string sql = "INSERT INTO roadkill_siteconfiguration (id, version, content) VALUES (";
			sql += string.Format("'{0}',", row.Id);
			sql += string.Format("'{0}',", row.Version);
			sql += string.Format("'{0}'", row.Json.ReplaceSingleQuotes());

			sql += ");";

			return sql;
		}

		internal class SiteConfigurationRow
		{
			public Guid Id { get; set; }
			public string Version { get; set; }
			public string Json { get; set; }
		}
	}

	internal static class Extensions
	{
		public static string ReplaceSingleQuotes(this string text)
		{
			if (string.IsNullOrEmpty(text))
				return text;

			return text.Replace("'", "''");
		}
	}
}
