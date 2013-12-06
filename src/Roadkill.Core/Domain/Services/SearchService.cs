using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Plugins;

namespace Roadkill.Core.Services
{
	/// <summary>
	/// Provides searching tasks using a Lucene.net search index.
	/// </summary>
	public class SearchService : ServiceBase
	{
		private static Regex _removeTagsRegex = new Regex("<(.|\n)*?>");
		private MarkupConverter _markupConverter;
		protected virtual string IndexPath { get; set; }
		private IPluginFactory _pluginFactory;
		private static readonly LuceneVersion LUCENEVERSION = LuceneVersion.LUCENE_29;

		public SearchService(ApplicationSettings settings, IRepository repository, IPluginFactory pluginFactory)
			: base(settings, repository)
		{
			_markupConverter = new MarkupConverter(settings, repository, pluginFactory);
			IndexPath = settings.SearchIndexPath;
		}

		/// <summary>
		/// Searches the lucene index with the search text.
		/// </summary>
		/// <param name="searchText">The text to search with.</param>
		/// <remarks>Syntax reference: http://lucene.apache.org/java/2_3_2/queryparsersyntax.html#Wildcard</remarks>
		/// <exception cref="SearchException">An error occured searching the lucene.net index.</exception>
		public virtual IEnumerable<SearchResultViewModel> Search(string searchText)
		{
			// This check is for the benefit of the CI builds
			if (!Directory.Exists(IndexPath))
				CreateIndex();

			List<SearchResultViewModel> list = new List<SearchResultViewModel>();

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
							Document document = searcher.Doc(scoreDoc.Doc);
							list.Add(new SearchResultViewModel(document, scoreDoc));
						}
					}
				}
				catch (FileNotFoundException)
				{
					// For 1.7's change to the Lucene search path.
					CreateIndex();
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
		/// <param name="model">The page to add.</param>
		/// <exception cref="SearchException">An error occured with the lucene.net IndexWriter while adding the page to the index.</exception>
		public virtual void Add(PageViewModel model)
		{
			try
			{
				EnsureDirectoryExists();

				StandardAnalyzer analyzer = new StandardAnalyzer(LUCENEVERSION);
				using (IndexWriter writer = new IndexWriter(FSDirectory.Open(new DirectoryInfo(IndexPath)), analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED))
				{
					Document document = new Document();
					document.Add(new Field("id", model.Id.ToString(), Field.Store.YES, Field.Index.ANALYZED));
					document.Add(new Field("content", model.Content, Field.Store.YES, Field.Index.ANALYZED));
					document.Add(new Field("contentsummary", GetContentSummary(model), Field.Store.YES, Field.Index.NO));
					document.Add(new Field("title", model.Title, Field.Store.YES, Field.Index.ANALYZED));
					document.Add(new Field("tags", model.SpaceDelimitedTags(), Field.Store.YES, Field.Index.ANALYZED));
					document.Add(new Field("createdby", model.CreatedBy, Field.Store.YES, Field.Index.NOT_ANALYZED));
					document.Add(new Field("createdon", model.CreatedOn.ToShortDateString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
					document.Add(new Field("contentlength", model.Content.Length.ToString(), Field.Store.YES, Field.Index.NO));

					writer.AddDocument(document);
					writer.Optimize();
				}
			}
			catch (Exception ex)
			{
				if (!ApplicationSettings.IgnoreSearchIndexErrors)
					throw new SearchException(ex, "An error occured while adding page '{0}' to the search index", model.Title);
			}
		}

		/// <summary>
		/// Deletes the specified page from the search indexs.
		/// </summary>
		/// <param name="model">The page to remove.</param>
		/// <exception cref="SearchException">An error occured with the lucene.net IndexReader while deleting the page from the index.</exception>
		public virtual int Delete(PageViewModel model)
		{
			try
			{
				StandardAnalyzer analyzer = new StandardAnalyzer(LUCENEVERSION);
				int count = 0;
				using (IndexReader reader = IndexReader.Open(FSDirectory.Open(new DirectoryInfo(IndexPath)), false))
				{
					count += reader.DeleteDocuments(new Term("id", model.Id.ToString()));
				}

				return count;
			}
			catch (Exception ex)
			{
				if (!ApplicationSettings.IgnoreSearchIndexErrors)
					throw new SearchException(ex, "An error occured while deleting page '{0}' from the search index", model.Title);
				else
					return 0;
			}
		}

		/// <summary>
		/// Updates the <see cref="Page"/> in the search index, by removing it and re-adding it.
		/// </summary>
		/// <param name="model">The page to update</param>
		/// <exception cref="SearchException">An error occured with lucene.net while deleting the page or inserting it back into the index.</exception>
		public virtual void Update(PageViewModel model)
		{
			EnsureDirectoryExists();
			Delete(model);
			Add(model);
		}

		/// <summary>
		/// Creates the initial search index based on all pages in the system.
		/// </summary>
		/// <exception cref="SearchException">An error occured with the lucene.net IndexWriter while adding the page to the index.</exception>
		public virtual void CreateIndex()
		{
			EnsureDirectoryExists();

			try
			{
				StandardAnalyzer analyzer = new StandardAnalyzer(LUCENEVERSION);
				using (IndexWriter writer = new IndexWriter(FSDirectory.Open(new DirectoryInfo(IndexPath)), analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
				{
					foreach (Page page in Repository.AllPages().ToList())
					{
						PageViewModel pageModel = new PageViewModel(Repository.GetLatestPageContent(page.Id), _markupConverter);

						Document document = new Document();
						document.Add(new Field("id", pageModel.Id.ToString(), Field.Store.YES, Field.Index.ANALYZED));
						document.Add(new Field("content", pageModel.Content, Field.Store.YES, Field.Index.ANALYZED));
						document.Add(new Field("contentsummary", GetContentSummary(pageModel), Field.Store.YES, Field.Index.NO));
						document.Add(new Field("title", pageModel.Title, Field.Store.YES, Field.Index.ANALYZED));
						document.Add(new Field("tags", pageModel.SpaceDelimitedTags(), Field.Store.YES, Field.Index.ANALYZED));
						document.Add(new Field("createdby", pageModel.CreatedBy, Field.Store.YES, Field.Index.NOT_ANALYZED));
						document.Add(new Field("createdon", pageModel.CreatedOn.ToShortDateString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
						document.Add(new Field("contentlength", pageModel.Content.Length.ToString(), Field.Store.YES, Field.Index.NO));

						writer.AddDocument(document);
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
		internal string GetContentSummary(PageViewModel model)
		{
			// Turn the contents into HTML, then strip the tags for the mini summary. This needs some works
			string modelHtml = model.Content;
			modelHtml = _markupConverter.ToHtml(modelHtml);
			modelHtml = _removeTagsRegex.Replace(modelHtml, "");

			if (modelHtml.Length > 150)
				modelHtml = modelHtml.Substring(0, 149);

			return modelHtml;
		}
	}
}
