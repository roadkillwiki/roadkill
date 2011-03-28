using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using Lucene.Net.Documents;
using System.IO;
using Lucene.Net.Index;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Roadkill.Core.Converters;
using System.Text.RegularExpressions;

namespace Roadkill.Core.Search
{
	/// <summary>
	/// Lucene.net based search facade.
	/// </summary>
	public class SearchManager : ManagerBase
	{
		private static string _indexPath = AppDomain.CurrentDomain.BaseDirectory + @"\App_Data\search";
		private static Regex _removeTagsRegex = new Regex("<(.|\n)*?>");

		public static SearchManager Current
		{
			get
			{
				return Nested.Current;
			}
		}

		class Nested
		{
			static Nested()
			{
			}
			internal static readonly SearchManager Current = new SearchManager();
		}

		/// <summary>
		/// SQL-based search, which doesn't search the text content.
		/// </summary>
		/// <remarks>This may be required for medium-trust installations</remarks>
		/// <param name="text"></param>
		/// <returns></returns>
		public IEnumerable<PageSummary> BasicSearch(string text)
		{
			IEnumerable<PageSummary> list = new List<PageSummary>();

			using (ISession session = NHibernateRepository.Current.SessionFactory.OpenSession())
			{

				IQuery query = NHibernateRepository.Current.SessionFactory.OpenSession()
					.CreateQuery("FROM Page WHERE Title LIKE :search OR CreatedBy=:search OR tags LIKE :search ");

				query.SetString("search", "%" + text);
				IList<Page> pages = query.List<Page>();
				list = from p in pages select p.ToSummary();

				// SQL content search, kept for reference
				if (false)
				{
					string sql = "select pg.*,p.* from roadkill_pagecontent p " +
						"inner join (select pageid,MAX(versionnumber) as maxversion from roadkill_pagecontent group by pageid) m " +
						"	on p.pageid = m.pageid and p.VersionNumber = m.maxversion " +
						"inner join roadkill_pages pg " +
						"	on pg.Id = p.pageid	" +
						"WHERE p.Text LIKE :search " +
						"ORDER BY p.pageid desc";

					ISQLQuery sqlQuery = session.CreateSQLQuery(sql);
					sqlQuery.SetParameter<string>("search", "%" + text + "%");
					var results = sqlQuery.List();
				}
			}

			return list;
		}

		/// <summary>
		///
		/// </summary>
		/// <remarks>Syntax: http://lucene.apache.org/java/2_3_2/queryparsersyntax.html#Wildcard </remarks>
		public List<SearchResult> SearchIndex(string searchText)
		{
			// This check is for the benefit of the CI builds
			if (!Directory.Exists(_indexPath))
				CreateIndex();

			List<SearchResult> list = new List<SearchResult>();

			StandardAnalyzer analyzer = new StandardAnalyzer();
			MultiFieldQueryParser parser = new MultiFieldQueryParser(new string[]{"content","title"}, analyzer);

			Query query = null;
			try
			{
				query = parser.Parse(searchText);
			}
			catch (Lucene.Net.QueryParsers.ParseException)
			{
				searchText = QueryParser.Escape(searchText);
				query = parser.Parse(searchText);
			}

			if (query != null)
			{
				IndexSearcher searcher = new IndexSearcher(_indexPath);
				Hits hits = searcher.Search(query);

				for (int i = 0; i < hits.Length(); i++)
				{
					Document document = hits.Doc(i);

					DateTime createdOn = DateTime.Now;
					if (!DateTime.TryParse(document.GetField("createdon").StringValue(),out createdOn))
						createdOn = DateTime.Now;

					SearchResult result = new SearchResult()
					{
						Id = int.Parse(document.GetField("id").StringValue()),
						Title = document.GetField("title").StringValue(),
						ContentSummary = document.GetField("contentsummary").StringValue(),
						Tags = document.GetField("tags").StringValue(),
						CreatedBy = document.GetField("createdby").StringValue(),
						CreatedOn = createdOn,
						ContentLength = int.Parse(document.GetField("contentlength").StringValue()),
						Score = hits.Score(i)
					};

					list.Add(result);
				}
			}

			return list;
		}

		public void Add(Page page)
		{
			EnsureDirectoryExists();

			StandardAnalyzer analyzer = new StandardAnalyzer();
			IndexWriter writer = new IndexWriter(_indexPath, analyzer,false);

			PageSummary summary = page.ToSummary();
			writer.AddDocument(SummaryToDocument(summary));

			writer.Optimize();
			writer.Close();
		}

		public void Delete(Page page)
		{
			StandardAnalyzer analyzer = new StandardAnalyzer();
			IndexReader reader = IndexReader.Open(_indexPath);
			reader.DeleteDocuments(new Term("id", page.Id.ToString()));
			reader.Close();
		}

		public void Update(Page page)
		{
			EnsureDirectoryExists();
			Delete(page);
			Add(page);
		}

		public void CreateIndex()
		{
			EnsureDirectoryExists();

			StandardAnalyzer analyzer = new StandardAnalyzer();
			IndexWriter writer = new IndexWriter(_indexPath, analyzer, true);
			

			foreach (Page page in Pages.ToList())
			{
				PageSummary summary = page.ToSummary();
				writer.AddDocument(SummaryToDocument(summary));
			}

			writer.Optimize();
			writer.Close();
		}

		private void EnsureDirectoryExists()
		{
			if (!Directory.Exists(_indexPath))
				Directory.CreateDirectory(_indexPath);
		}

		private Document SummaryToDocument(PageSummary summary)
		{
			// Get a summary by parsing the contents
			MarkupConverter converter = new MarkupConverter();
			IParser markupParser = converter.GetParser();

			// Turn the contents into HTML, then strip the tags for the mini summary. This needs some works
			string contentSummary = summary.Content;
			contentSummary = markupParser.Transform(contentSummary);
			contentSummary = _removeTagsRegex.Replace(contentSummary, "");

			if (contentSummary.Length > 150)
				contentSummary = contentSummary.Substring(0, 149);

			Document document = new Document();
			document.Add(new Field("id", summary.Id.ToString(), Field.Store.YES, Field.Index.UN_TOKENIZED));
			document.Add(new Field("content", summary.Content, Field.Store.YES, Field.Index.TOKENIZED));
			document.Add(new Field("contentsummary", contentSummary, Field.Store.YES, Field.Index.NO));
			document.Add(new Field("title", summary.Title, Field.Store.YES, Field.Index.TOKENIZED));
			document.Add(new Field("tags", summary.Tags.SpaceDelimitTags(), Field.Store.YES, Field.Index.TOKENIZED));
			document.Add(new Field("createdby", summary.CreatedBy, Field.Store.YES, Field.Index.UN_TOKENIZED));
			document.Add(new Field("createdon", summary.CreatedOn.ToString("u"), Field.Store.YES, Field.Index.UN_TOKENIZED));
			document.Add(new Field("contentlength", summary.Content.Length.ToString(), Field.Store.YES, Field.Index.NO));

			return document;
		}
	}
}
