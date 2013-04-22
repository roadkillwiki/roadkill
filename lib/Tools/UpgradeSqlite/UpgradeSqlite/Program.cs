using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using Dapper;
using System.IO;

namespace UpgradeSqlite
{
	class Program
	{
		static string _connectionString = @"Data Source=C:\Users\Chris\Documents\Visual Studio 2012\Projects\UpgradeSqlite\UpgradeSqlite\roadkill152.sqlite";

		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: upgradesqlite.exe connectionstring");
				return;
			}

			try
			{
				using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
				{
					connection.Open();

					List<User> users = connection.Query<User>("select * from roadkill_users").ToList();
					List<Page> pages = connection.Query<Page>("select * from roadkill_pages").ToList();
					List<PageContent> pageContent = connection.Query<PageContent>("select * from roadkill_pagecontent").ToList();

					string sql1 = string.Join("\n", users.Select(x => x.GetInsertSql()).ToArray());
					string sql2 = string.Join("\n", pages.Select(x => x.GetInsertSql()).ToArray());
					string sql3 = string.Join("\n", pageContent.Select(x => x.GetInsertSql()).ToArray());

					Console.WriteLine("Sql successfully written to export.sql");
					File.WriteAllText("export.sql", sql1 + sql2 + sql3);
				}

			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}

	public class Page
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string CreatedBy { get; set; }
		public DateTime CreatedOn { get; set; }
		public string ModifiedBy { get; set; }
		public DateTime ModifiedOn { get; set; }
		public string Tags { get; set; }
		public bool IsLocked { get; set; }

		public string GetInsertSql()
		{
			string sql = "INSERT INTO roadkill_pages (id, title, createdby, createdon, modifiedby, modifiedon, tags, islocked) VALUES (";
			sql += string.Format("'{0}',", Id);
			sql += string.Format("'{0}',", Title.Replace("'", @"''"));
			sql += string.Format("'{0}',", CreatedBy);
			sql += string.Format("'{0}',", CreatedOn);
			sql += string.Format("'{0}',", ModifiedBy);
			sql += string.Format("'{0}',", ModifiedOn.ToString("yyyy-MM-dd HH:mm:ss"));
			sql += string.Format("'{0}',", Tags.Replace("'", @"\'"));
			sql += string.Format("'{0}'", IsLocked ? "1" : "0");

			sql += ");";

			return sql;
		}
	}

	public class PageContent
	{
		public Guid Id { get; set; }
		public int PageId { get; set; }
		public string Text { get; set; }
		public string EditedBy { get; set; }
		public DateTime EditedOn { get; set; }
		public int VersionNumber { get; set; }

		public string GetInsertSql()
		{
			string sql = "INSERT INTO roadkill_pagecontent (id, pageid, text, editedby, editedon, versionnumber) VALUES (";
			sql += string.Format("'{0}',", Id);
			sql += string.Format("'{0}',", PageId);
			sql += string.Format("'{0}',", Text.Replace("'", @"''"));
			sql += string.Format("'{0}',", EditedBy);
			sql += string.Format("'{0}',", EditedOn.ToString("yyyy-MM-dd HH:mm:ss"));
			sql += string.Format("'{0}'", VersionNumber);

			sql += ");";

			return sql;
		}
	}


	public class User
	{
		public Guid Id { get; set; }
		public string ActivationKey { get; set; }
		public string Email { get; set; }
		public string Firstname { get; set; }
		public bool IsEditor { get; set; }
		public bool IsAdmin { get; set; }
		public bool IsActivated { get; set; }
		public string Lastname { get; set; }
		public string Password { get; internal set; }
		public string PasswordResetKey { get; set; }
		public string Salt { get; set; }
		public string Username { get; set; }

		public string GetInsertSql()
		{
			string sql = "INSERT INTO roadkill_users (id, activationkey, email, firstname, iseditor, isadmin, isactivated, lastname, password, passwordresetkey, salt, username) VALUES (";
			sql += string.Format("'{0}',", Id);
			sql += string.Format("'{0}',", ActivationKey);
			sql += string.Format("'{0}',", Email);
			sql += string.Format("'{0}',", Firstname);
			sql += string.Format("'{0}'", IsEditor ? "1" : "0");
			sql += string.Format("'{0}'", IsAdmin ? "1" : "0");
			sql += string.Format("'{0}'", IsActivated ? "1" : "0");
			sql += string.Format("'{0}',", Lastname);
			sql += string.Format("'{0}',", Password);
			sql += string.Format("'{0}',", PasswordResetKey);
			sql += string.Format("'{0}',", Salt);
			sql += string.Format("'{0}'", Username);

			sql += ");";

			return sql;
		}
	}
}
