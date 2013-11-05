using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Database.Export
{
	public class ScriptBuilder
	{
		private IRepository _repository;

		public ScriptBuilder(IRepository repository)
		{
			_repository = repository;
		}

		public string Export()
		{
			try
			{
				IEnumerable<User> users = _repository.FindAllAdmins().Union(_repository.FindAllEditors());
				IEnumerable<Page> pages = _repository.AllPages();
				IEnumerable<PageContent> pageContent = _repository.AllPageContents();

				string sql1 = string.Join("\n", users.Select(x => GetUsersInsertSql(x)).ToArray());
				string sql2 = string.Join("\n", pages.Select(x => GetPagesInsertSql(x)).ToArray());
				string sql3 = string.Join("\n", pageContent.Select(x => GetPageContentInsertSql(x)).ToArray());

				Log.Debug("Sql successfully written: {0}\n{1}\n{2}", sql1, sql2, sql3);
				return sql1 + sql2 + sql3;

			}
			catch (Exception e)
			{
				Log.Error(e, "An error occurred exporting the Roadkill database");
				return "An error occurred writing the SQL: " + e.Message;
			}
		}

		public string GetPagesInsertSql(Page page)
		{
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

		public string GetPageContentInsertSql(PageContent content)
		{
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

		public string GetUsersInsertSql(User user)
		{
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
