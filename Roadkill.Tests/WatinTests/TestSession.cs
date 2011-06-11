using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using WatiN.Core;

namespace Roadkill.Tests
{
	/// <summary>
	/// Holds an IE instance, and clears the database once it's done. It assumes the site is using SQL Server.
	/// </summary>
	public class TestSession : IDisposable
	{
		public IE IEInstance { get; set; }

		public TestSession(IE ie)
		{
			IEInstance = ie;
			TearDown(); // caters for stopped runs
			Setup();
		}

		public void Setup()
		{
			using (SqlConnection connection = new SqlConnection(Settings.ConnectionString))
			{
				connection.Open();

				// INCOMPLETE
				using (SqlCommand command = new SqlCommand())
				{

					string sql = @"INSERT INTO roadkill_users (Id,Email,IsEditor,IsAdmin,IsActivated,Username,Password,Salt) VALUES 
							('DD4EB524-5753-4458-A7FB-9EF7017C1442','editor@localhost',1,0,1,'editor','C1CD20DA5452C0D370794759CD151058AC189F2C','1234567890');

							INSERT INTO roadkill_users (Id,Email,IsEditor,IsAdmin,IsActivated,Username,Password,Salt) VALUES 
							('257083B6-B3C4-491F-A27D-9EFD01499F41','admin@localhost',1,1,1,'admin','C1CD20DA5452C0D370794759CD151058AC189F2C','1234567890');";
					command.Connection = connection;
					command.CommandText = sql;
					command.ExecuteNonQuery();
				}
			}
		}

		public void TearDown()
		{
			using (SqlConnection connection = new SqlConnection(Settings.ConnectionString))
			{
				connection.Open();

				using (SqlCommand command = new SqlCommand())
				{
					command.Connection = connection;
					command.CommandText = @"
					EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all';
					DELETE FROM roadkill_pages;
					DELETE FROM roadkill_pagecontent;
					DELETE FROM roadkill_users;
					EXEC sp_msforeachtable 'ALTER TABLE ? CHECK CONSTRAINT all';
					";

					command.ExecuteNonQuery();
				}
			}
		}

		public void ExecuteSql(string sql)
		{
			using (SqlConnection connection = new SqlConnection(Settings.ConnectionString))
			{
				connection.Open();

				using (SqlCommand command = new SqlCommand())
				{
					command.Connection = connection;
					command.CommandText = sql;
					command.ExecuteNonQuery();
				}
			}
		}

		public void Dispose()
		{
			if (IEInstance != null)
				IEInstance.Dispose();

			TearDown();
		}
	}
}
