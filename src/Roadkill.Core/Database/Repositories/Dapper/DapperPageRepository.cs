using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;

namespace Roadkill.Core.Database.Repositories.Dapper
{
	public class DapperPageRepository : IPageRepository
	{
		private readonly IDbConnectionFactory _dbConnectionFactory;
		internal static readonly string PagesTableName = "roadkill_pages";
		internal static readonly string PageContentsTableName = "roadkill_pagecontent";

		public DapperPageRepository(IDbConnectionFactory dbConnectionFactory)
		{
			_dbConnectionFactory = dbConnectionFactory;
		}

		public void Dispose()
		{
		}

		public PageContent AddNewPage(Page page, string text, string editedBy, DateTime editedOn)
		{
			Page newPage = SaveOrUpdatePage(page);

			if (newPage.Id > 0)
			{
				using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
				{
					connection.Open();

					var pageContent = new PageContent();
					pageContent.Id = Guid.NewGuid();
					pageContent.EditedBy = editedBy;
					pageContent.EditedOn = editedOn;
					pageContent.Text = text;
					pageContent.VersionNumber = 1;
					pageContent.Page = page;

					string sql = $"insert into {PageContentsTableName} ";
					sql += "(id, editedby, editedon, text, versionnumber, pageid) ";
					sql += "values (@Id, @EditedBy, @EditedOn, @Text, @VersionNumber, @PageId)";

					connection.Execute(sql, new
					{
						Id = pageContent.Id,
						EditedBy = editedBy,
						EditedOn = editedOn,
						Text = text,
						VersionNumber = pageContent.VersionNumber,
						PageId = page.Id
					});

					return pageContent;
				}
			}

			throw new DataException("Inserting new page failed (no id)");
		}

		public PageContent AddNewPageContentVersion(Page page, string text, string editedBy, DateTime editedOn, int version)
		{
			if (version < 1)
				version = 1;

			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				var pageContent = new PageContent();
				pageContent.Id = Guid.NewGuid();
				pageContent.EditedBy = editedBy;
				pageContent.EditedOn = editedOn;
				pageContent.Text = text;
				pageContent.VersionNumber = version;
				pageContent.Page = page;

				string sql = $"insert into {PageContentsTableName} (id, editedby, editedon, versionnumber, text, pageid) ";
				sql += "values (@Id, @EditedBy, @EditedOn, @VersionNumber, @Text, @PageId)";

				connection.Execute(sql, new
				{
					Id = pageContent.Id,
					EditedBy = editedBy,
					EditedOn = editedOn,
					Text = text,
					VersionNumber = version,
					PageId = page.Id
				});

				// Update when and who the page was last modified by
				page.ModifiedOn = editedOn;
				page.ModifiedBy = editedBy;

				sql = $"update {PagesTableName} set modifiedon=@ModifiedOn, modifiedby=@ModifiedBy ";
				sql += "where id=@Id";
				connection.Execute(sql, page);

				return pageContent;
			}
		}

		public IEnumerable<Page> AllPages()
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {PagesTableName}";
				return connection.Query<Page>(sql);
			}
		}

		public IEnumerable<PageContent> AllPageContents()
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {PageContentsTableName}";
				return connection.Query<PageContent>(sql);
			}
		}

		public IEnumerable<string> AllTags()
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select tags from {PagesTableName}";
				return connection.Query<string>(sql);
			}
		}

		public void DeletePage(Page page)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"delete from {PageContentsTableName} where pageid=@Id";
				connection.Execute(sql, new { Id = page.Id });

				sql = $"delete from {PagesTableName} where id=@Id";
				connection.Execute(sql, new { Id = page.Id });
			}
		}

		public void DeletePageContent(PageContent pageContent)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"delete from {PageContentsTableName} where id=@Id";
				connection.Execute(sql, new { Id = pageContent.Id });
			}
		}

		public void DeleteAllPages()
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"delete from {PageContentsTableName}";
				connection.Execute(sql);

				sql = $"delete from {PagesTableName}";
				connection.Execute(sql);
			}
		}

		public IEnumerable<Page> FindPagesCreatedBy(string username)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {PagesTableName} where createdby=@username";
				return connection.Query<Page>(sql, new { username = username });
			}
		}

		public IEnumerable<Page> FindPagesModifiedBy(string username)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {PagesTableName} ";
				sql += "where modifiedby = @username";

				return connection.Query<Page>(sql, new { username = username });
			}
		}

		public IEnumerable<Page> FindPagesContainingTag(string tag)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {PagesTableName} ";
				sql += "where tags like @tag";

				return connection.Query<Page>(sql, new { tag = "%" +tag+ "%" });
			}
		}

		public IEnumerable<PageContent> FindPageContentsByPageId(int pageId)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select p.*, pc.* from {PagesTableName} p ";
				sql += $"inner join {PageContentsTableName} pc on pc.pageid = p.id ";
				sql += "where p.id = @pageId";

				IEnumerable<PageContent> results = connection.Query<Page, PageContent, PageContent>(sql,
				(page, pageContent) =>
				{
					pageContent.Page = page;
					return pageContent;

				}, new { pageId = pageId });

				return results;
			}
		}

		public IEnumerable<PageContent> FindPageContentsEditedBy(string username)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select p.*, pc.* from {PagesTableName} p ";
				sql += $"inner join {PageContentsTableName} pc on pc.pageid = p.id ";
				sql += "where p.modifiedby = @username";

				IEnumerable<PageContent> results = connection.Query<Page, PageContent, PageContent>(sql,
				(page, pageContent) =>
				{
					pageContent.Page = page;
					return pageContent;

				}, new { username = username });

				return results;
			}
		}

		public PageContent GetLatestPageContent(int pageId)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select p.*, pc.* from {PagesTableName} p ";
				sql += $"inner join {PageContentsTableName} pc on pc.pageid = p.id ";
				sql += "where p.id = @pageId ";
				sql += "order by pc.editedon desc";

				IEnumerable<PageContent> results = connection.Query<Page, PageContent, PageContent>(sql, 
				(page, pageContent) =>
				{
					pageContent.Page = page;
					return pageContent;

				}, new {pageId = pageId});

				return results.FirstOrDefault();
			}
		}

		public Page GetPageById(int id)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {PagesTableName} where id=@id";
				return connection.QueryFirstOrDefault<Page>(sql, new { id = id });
			}
		}

		public Page GetPageByTitle(string title)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select * from {PagesTableName} where title=@title";
				return connection.QueryFirstOrDefault<Page>(sql, new { title = title });
			}
		}

		public PageContent GetPageContentById(Guid id)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select p.*, pc.* from {PagesTableName} p ";
				sql += $"inner join {PageContentsTableName} pc on pc.pageid = p.id ";
				sql += "where pc.id=@id";

				IEnumerable<PageContent> results = connection.Query<Page, PageContent, PageContent>(sql,
				(page, pageContent) =>
				{
					pageContent.Page = page;
					return pageContent;

				}, new { id = id });

				return results.FirstOrDefault();
			}
		}

		public PageContent GetPageContentByPageIdAndVersionNumber(int id, int versionNumber)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"select p.*, pc.* from {PagesTableName} p ";
				sql += $"inner join {PageContentsTableName} pc on pc.pageid = p.id ";
				sql += "where p.id=@id and pc.versionnumber=@versionNumber";

				IEnumerable<PageContent> results = connection.Query<Page, PageContent, PageContent>(sql,
				(page, pageContent) =>
				{
					pageContent.Page = page;
					return pageContent;

				}, new { id = id, versionNumber = versionNumber });

				return results.FirstOrDefault();
			}
		}

		public Page SaveOrUpdatePage(Page page)
		{
			bool pageExists = GetPageById(page.Id) != null;

			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();
				string sql;

				if (pageExists)
				{
					sql = $"update {PagesTableName} set ";
					sql += "title=@Title, tags=@Tags, createdby=@CreatedBy, ";
					sql += "createdon=@CreatedOn, islocked=@IsLocked, modifiedby=@ModifiedBy, modifiedon=@ModifiedOn ";
					sql += "where id=@Id";

					connection.Execute(sql, page);
				}
				else
				{
					sql = $"insert into {PagesTableName} ";
					sql += "(title, tags, createdby, createdon, islocked, modifiedby, modifiedon) ";
					sql += "values (@Title, @Tags, @CreatedBy, @CreatedOn, @IsLocked, @ModifiedBy, @ModifiedOn)";
					sql += " " +_dbConnectionFactory.GetAutoIdentitySqlSuffix();

					int pageId = connection.ExecuteScalar<int>(sql, page);
					page.Id = pageId;
				}

				return page;
			}
		}

		public void UpdatePageContent(PageContent content)
		{
			using (IDbConnection connection = _dbConnectionFactory.CreateConnection())
			{
				connection.Open();

				string sql = $"update {PageContentsTableName} set ";
				sql += "editedby=@EditedBy, editedon=@EditedOn, versionnumber=@VersionNumber, text=@Text ";
				sql += "where id=@Id";

				connection.Execute(sql, content);
			}
		}
	}
}