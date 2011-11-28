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
	/// Provides searching tasks using a Lucene.net search index.
	/// </summary>
	public class SearchManager : ManagerBase
	{
		private static string _indexPath = AppDomain.CurrentDomain.BaseDirectory + @"\App_Data\search";
		private static Regex _removeTagsRegex = new Regex("<(.|\n)*?>");

		/// <summary>
		/// Gets the current <see cref="SearchManager"/> for the application.
		/// </summary>
		public static SearchManager Current
		{
			get
			{
				return Nested.Current;
			}
		}

		/// <summary>
		/// Singleton implementation.
		/// </summary>
		class Nested
		{
			internal static readonly SearchManager Current = new SearchManager();

			static Nested()
			{
			}
		}

		/// <summary>
		/// Searches the lucene index with the search text.
		/// </summary>
		/// <param name="searchText">The text to search with.</param>
		/// <remarks>Syntax reference: http://lucene.apache.org/java/2_3_2/queryparsersyntax.html#Wildcard</remarks>
		/// <exception cref="SearchException">An error occured searching the lucene.net index.</exception>
		public List<SearchResult> SearchIndex(string searchText)
		{
			// This check is for the benefit of the CI builds
			if (!Directory.Exists(_indexPath))
				CreateIndex();

			List<SearchResult> list = new List<SearchResult>();

			StandardAnalyzer analyzer = new StandardAnalyzer();
			MultiFieldQueryParser parser = new MultiFieldQueryParser(new string[] { "content", "title" }, analyzer);

			Query query = null;
			try
			{
				query = parser.Parse(searchText);
			}
			catch (Lucene.Net.QueryParsers.ParseException)
			{
				// Catch syntax errors in the search and remove them.
				searchText = QueryParser.Escape(searchText);
				query = parser.Parse(searchText);
			}

			if (query != null)
			{
				try
				{
					IndexSearcher searcher = new IndexSearcher(_indexPath);
					Hits hits = searcher.Search(query);

					for (int i = 0; i < hits.Length(); i++)
					{
						Document document = hits.Doc(i);

						DateTime createdOn = DateTime.Now;
						if (!DateTime.TryParse(document.GetField("createdon").StringValue(), out createdOn))
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
				catch (Exception ex)
				{
					throw new SearchException(ex, "An error occured while searching the index");
				}
			}

			return list;
		}

		/// <summary>
		/// Adds the specified page to the search index.
		/// </summary>
		/// <param name="page">The page to add.</param>
		/// <exception cref="SearchException">An error occured with the lucene.net IndexWriter while adding the page to the index.</exception>
		public void Add(PageSummary summary)
		{
			try
			{
				EnsureDirectoryExists();

				StandardAnalyzer analyzer = new StandardAnalyzer();
				IndexWriter writer = new IndexWriter(_indexPath, analyzer, false);

				writer.AddDocument(SummaryToDocument(summary));

				writer.Optimize();
				writer.Close();
			}
			catch (Exception ex)
			{
				throw new SearchException(ex, "An error occured while adding page '{0}' to the search index", summary.Title);
			}
		}

		/// <summary>
		/// Deletes the specified page from the search indexs.
		/// </summary>
		/// <param name="summary">The page to remove.</param>
		/// <exception cref="SearchException">An error occured with the lucene.net IndexReader while deleting the page from the index.</exception>
		public void Delete(PageSummary summary)
		{
			try
			{
				StandardAnalyzer analyzer = new StandardAnalyzer();
				IndexReader reader = IndexReader.Open(_indexPath);
				reader.DeleteDocuments(new Term("id", summary.Id.ToString()));
				reader.Close();
			}
			catch (Exception ex)
			{
				throw new SearchException(ex, "An error occured while deleting page '{0}' from the search index", summary.Title);
			}
		}

		/// <summary>
		/// Updates the <see cref="Page"/> in the search index, by removing it and re-adding it.
		/// </summary>
		/// <param name="summary">The page to update</param>
		/// <exception cref="SearchException">An error occured with lucene.net while deleting the page or inserting it back into the index.</exception>
		public void Update(PageSummary summary)
		{
			EnsureDirectoryExists();
			Delete(summary);
			Add(summary);
		}

		/// <summary>
		/// Creates the initial search index based on all pages in the system.
		/// </summary>
		/// <exception cref="SearchException">An error occured with the lucene.net IndexWriter while adding the page to the index.</exception>
		public void CreateIndex()
		{
			EnsureDirectoryExists();

			try
			{
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
			catch (Exception ex)
			{
				throw new SearchException(ex, "An error occured while creating the search index");
			}
		}

		private void EnsureDirectoryExists()
		{
			try
			{
				if (!Directory.Exists(_indexPath))
					Directory.CreateDirectory(_indexPath);
			}
			catch (IOException ex)
			{
				throw new SearchException(ex, "An error occured while creating the search directory '{0}'", _indexPath);
			}
		}

		/// <summary>
		/// Converts the page summary to a lucene Document with the relevant searchable fields.
		/// </summary>
		private Document SummaryToDocument(PageSummary summary)
		{
			// Get a summary by parsing the contents
			MarkupConverter converter = new MarkupConverter();
			IMarkupParser markupParser = converter.Parser;

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
