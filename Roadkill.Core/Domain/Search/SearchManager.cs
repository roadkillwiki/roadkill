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
	/// Very basic search processing, which uses the page title, created by to search with.
	/// Also optionally searches the content too.
	/// </summary>
	public class SearchManager
	{
		private static string _indexPath = AppDomain.CurrentDomain.BaseDirectory + @"\App_Data\search";
		private static Regex _removeTagsRegex = new Regex("<(.|\n)*?>");

		/// <summary>
		/// A very basic SQL-based search, which doesn't search the text content.
		/// </summary>
		/// <remarks>This may be required for medium-trust installations</remarks>
		/// <param name="text"></param>
		/// <returns></returns>
		public static IEnumerable<PageSummary> BasicSearch(string text)
		{
			IQuery query = Page.Repository.Manager().SessionFactory.OpenSession()
				.CreateQuery("FROM Page WHERE Title LIKE :search OR CreatedBy=:search OR tags LIKE :search ");

			query.SetString("search", "%" + text);
			IList<Page> pages = query.List<Page>();
			IEnumerable<PageSummary> list = from p in pages select p.ToSummary();

			// SQL content search, for reference
			if (false)
			{	
				string sql = "select pg.*,p.* from roadkill_pagecontent p "+
					"inner join (select pageid,MAX(versionnumber) as maxversion from roadkill_pagecontent group by pageid) m "+
					"	on p.pageid = m.pageid and p.VersionNumber = m.maxversion "+
					"inner join roadkill_pages pg "+
					"	on pg.Id = p.pageid	"+
					"WHERE p.Text LIKE :search "+
					"ORDER BY p.pageid desc";

				ISQLQuery sqlQuery = PageContent.Repository.Manager().SessionFactory.OpenSession().CreateSQLQuery(sql);
				sqlQuery.SetParameter<string>("search", "%" + text + "%");
				var results = sqlQuery.List();
			}

			return list;
		}

		/// <summary>
		///
		/// </summary>
		/// <remarks>Syntax: http://lucene.apache.org/java/2_3_2/queryparsersyntax.html#Wildcard </remarks>
		public static List<SearchResult> SearchIndex(string searchText)
		{
			List<SearchResult> list = new List<SearchResult>();

			StandardAnalyzer analyzer = new StandardAnalyzer();
			QueryParser parser = new QueryParser("content", analyzer);

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

		public static Document ToDocument(PageSummary summary)
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
			document.Add(new Field("id", summary.Id.ToString(), Field.Store.YES, Field.Index.NO));
			document.Add(new Field("content", summary.Content, Field.Store.YES, Field.Index.TOKENIZED));
			document.Add(new Field("contentsummary", contentSummary, Field.Store.YES, Field.Index.NO));
			document.Add(new Field("title", summary.Title, Field.Store.YES, Field.Index.TOKENIZED));
			document.Add(new Field("tags", summary.Tags.SpaceDelimitTags(), Field.Store.YES, Field.Index.TOKENIZED));
			document.Add(new Field("createdby", summary.CreatedBy, Field.Store.YES, Field.Index.UN_TOKENIZED));
			document.Add(new Field("createdon", summary.CreatedOn.ToString("u"), Field.Store.YES, Field.Index.UN_TOKENIZED));
			document.Add(new Field("contentlength", summary.Content.Length.ToString(), Field.Store.YES, Field.Index.NO));

			return document;
		}

		public static void Add(Page page)
		{
			StandardAnalyzer analyzer = new StandardAnalyzer();
			IndexWriter writer = new IndexWriter(_indexPath, analyzer,false);

			PageSummary summary = page.ToSummary();
			writer.AddDocument(ToDocument(summary));

			writer.Optimize();
			writer.Close();
		}

		public static void Update(Page page)
		{
			Delete(page);
			Add(page);
		}

		public static void Delete(Page page)
		{
			StandardAnalyzer analyzer = new StandardAnalyzer();
			IndexReader reader = IndexReader.Open(_indexPath);
			reader.DeleteDocuments(new Term("id", page.Id.ToString()));
		}

		public static void CreateIndex()
		{
			if (!Directory.Exists(_indexPath))
				Directory.CreateDirectory(_indexPath);

			StandardAnalyzer analyzer = new StandardAnalyzer();
			IndexWriter writer = new IndexWriter(_indexPath, analyzer, true);

			foreach (Page page in Page.Repository.List())
			{
				PageSummary summary = page.ToSummary();
				writer.AddDocument(ToDocument(summary));
			}

			writer.Optimize();
			writer.Close();
		}
	}
}
