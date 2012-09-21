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
using Directory = System.IO.Directory;
using LuceneVersion = Lucene.Net.Util.Version;
using Lucene.Net.Store;

namespace Roadkill.Core.Search
{
	/// <summary>
	/// Provides searching tasks using a Lucene.net search index.
	/// </summary>
	public class SearchManager : ManagerBase
	{
		protected virtual string IndexPath { get; set; }
		private static Regex _removeTagsRegex = new Regex("<(.|\n)*?>");
		private static readonly LuceneVersion LUCENEVERSION = LuceneVersion.LUCENE_29;

		public SearchManager()
		{
			IndexPath = AppDomain.CurrentDomain.BaseDirectory + @"\App_Data\search";
		}

		/// <summary>
		/// Searches the lucene index with the search text.
		/// </summary>
		/// <param name="searchText">The text to search with.</param>
		/// <remarks>Syntax reference: http://lucene.apache.org/java/2_3_2/queryparsersyntax.html#Wildcard</remarks>
		/// <exception cref="SearchException">An error occured searching the lucene.net index.</exception>
		public IEnumerable<SearchResult> SearchIndex(string searchText)
		{
			// This check is for the benefit of the CI builds
			if (!Directory.Exists(IndexPath))
				CreateIndex();

			List<SearchResult> list = new List<SearchResult>();

			if (string.IsNullOrWhiteSpace(searchText))
				return list;

			StandardAnalyzer analyzer = new StandardAnalyzer(LUCENEVERSION);
			MultiFieldQueryParser parser = new MultiFieldQueryParser(LuceneVersion.LUCENE_29, new string[] { "content", "title" }, analyzer);

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
					using (IndexSearcher searcher = new IndexSearcher(FSDirectory.Open(new DirectoryInfo(IndexPath)), true))
					{
						TopDocs topDocs = searcher.Search(query, 1000);

						foreach (ScoreDoc scoreDoc in topDocs.ScoreDocs)
						{
							Document document = searcher.Doc(scoreDoc.doc);

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
								Score = scoreDoc.score
							};

							list.Add(result);
						}
					}
				}
				catch (Exception ex)
				{
					throw new SearchException(ex, "An error occured while searching the index, try rebuilding the search index via the admin tools to fix this.");
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

				StandardAnalyzer analyzer = new StandardAnalyzer(LUCENEVERSION);
				using (IndexWriter writer = new IndexWriter(FSDirectory.Open(new DirectoryInfo(IndexPath)), analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED))
				{
					writer.AddDocument(SummaryToDocument(summary));
					writer.Optimize();
				}
			}
			catch (Exception ex)
			{
				if (!RoadkillSettings.IgnoreSearchIndexErrors)
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
				StandardAnalyzer analyzer = new StandardAnalyzer(LUCENEVERSION);
				using (IndexReader reader = IndexReader.Open(FSDirectory.Open(new DirectoryInfo(IndexPath)), false))
				{
					reader.DeleteDocuments(new Term("id", summary.Id.ToString()));
				}
			}
			catch (Exception ex)
			{
				if (!RoadkillSettings.IgnoreSearchIndexErrors)
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
				StandardAnalyzer analyzer = new StandardAnalyzer(LUCENEVERSION);
				using (IndexWriter writer = new IndexWriter(FSDirectory.Open(new DirectoryInfo(IndexPath)), analyzer, true,IndexWriter.MaxFieldLength.UNLIMITED))
				{
					foreach (Page page in Pages.ToList())
					{
						PageSummary summary = page.ToSummary();
						writer.AddDocument(SummaryToDocument(summary));
					}

					writer.Optimize();
				}
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
				if (!Directory.Exists(IndexPath))
					Directory.CreateDirectory(IndexPath);
			}
			catch (IOException ex)
			{
				throw new SearchException(ex, "An error occured while creating the search directory '{0}'", IndexPath);
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
			document.Add(new Field("id", summary.Id.ToString(), Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field("content", summary.Content, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field("contentsummary", contentSummary, Field.Store.YES, Field.Index.NO));
			document.Add(new Field("title", summary.Title, Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field("tags", summary.Tags.SpaceDelimitTags(), Field.Store.YES, Field.Index.ANALYZED));
			document.Add(new Field("createdby", summary.CreatedBy, Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field("createdon", summary.CreatedOn.ToShortDateString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
			document.Add(new Field("contentlength", summary.Content.Length.ToString(), Field.Store.YES, Field.Index.NO));

			return document;
		}
	}
}
