using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using System.IO;
using Lucene.Net.Index;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers.Classic;
using Roadkill.Core.Converters;
using System.Text.RegularExpressions;
using Directory = System.IO.Directory;
using Lucene.Net.Util;
using LuceneDirectory = Lucene.Net.Store.Directory;
using Lucene.Net.Store;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Plugins;

namespace Roadkill.Core.Services
{
	/// <summary>
	/// Provides searching tasks using a Lucene.net search index.
	/// </summary>
	public class SearchService : ISearchService
	{
		private static Regex _removeTagsRegex = new Regex("<(.|\n)*?>");
		private MarkupConverter _markupConverter;
		protected virtual string IndexPath { get; set; }
		private static readonly LuceneVersion LUCENEVERSION = LuceneVersion.LUCENE_48;

		public ApplicationSettings ApplicationSettings { get; set; }
		public ISettingsRepository SettingsRepository { get; set; }
		public IPageRepository PageRepository { get; set; }

		public SearchService(ApplicationSettings settings, ISettingsRepository settingsRepository, IPageRepository pageRepository, IPluginFactory pluginFactory)
		{
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			if (settingsRepository == null)
				throw new ArgumentNullException(nameof(settingsRepository));

			if (pageRepository == null)
				throw new ArgumentNullException(nameof(pageRepository));

			if (pluginFactory == null)
				throw new ArgumentNullException(nameof(pluginFactory));

			_markupConverter = new MarkupConverter(settings, settingsRepository, pageRepository, pluginFactory);
			IndexPath = settings.SearchIndexPath;

			ApplicationSettings = settings;
			SettingsRepository = settingsRepository;
			PageRepository = pageRepository;
		}

		/// <summary>
		/// Searches the lucene index with the search text.
		/// </summary>
		/// <param name="searchText">The text to search with.</param>
		/// <remarks>Syntax reference: http://lucene.apache.org/java/2_3_2/queryparsersyntax.html#Wildcard</remarks>
		/// <exception cref="SearchException">An error occurred searching the lucene.net index.</exception>
		public virtual IEnumerable<SearchResultViewModel> Search(string searchText)
		{
			// This check is for the benefit of the CI builds
			if (!Directory.Exists(IndexPath))
				CreateIndex();

			List<SearchResultViewModel> list = new List<SearchResultViewModel>();

			if (string.IsNullOrWhiteSpace(searchText))
				return list;

			StandardAnalyzer analyzer = new StandardAnalyzer(LUCENEVERSION);
			MultiFieldQueryParser parser = new MultiFieldQueryParser(LUCENEVERSION, new string[] { "content", "title" }, analyzer);

			// Terms split into several tokens by the analyzer (e.g. dates) become phrase queries, as with Lucene 3.
			parser.AutoGeneratePhraseQueries = true;

			// Lucene 4 treats /.../ as a regular expression, which Lucene 3 (Roadkill 2.x) didn't: escape it, so
			// searches such as "createdon:1/2/2020" still work.
			searchText = searchText.Replace("/", "\\/");

			Query query = null;
			try
			{
				query = parser.Parse(searchText);
			}
			catch (ParseException)
			{
				// Catch syntax errors in the search and remove them.
				searchText = QueryParser.Escape(searchText);
				query = parser.Parse(searchText);
			}

			if (query != null)
			{
				try
				{
					using (LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(IndexPath)))
					using (DirectoryReader reader = DirectoryReader.Open(directory))
					{
						IndexSearcher searcher = new IndexSearcher(reader);
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
					throw new SearchException(ex, "An error occurred while searching the index, try rebuilding the search index via the admin tools to fix this.");
				}
			}

			return list;
		}

		/// <summary>
		/// Adds the specified page to the search index.
		/// </summary>
		/// <param name="model">The page to add.</param>
		/// <exception cref="SearchException">An error occurred with the lucene.net IndexWriter while adding the page to the index.</exception>
		public virtual void Add(PageViewModel model)
		{
			try
			{
				EnsureDirectoryExists();

				StandardAnalyzer analyzer = new StandardAnalyzer(LUCENEVERSION);
				using (LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(IndexPath)))
				using (IndexWriter writer = new IndexWriter(directory, new IndexWriterConfig(LUCENEVERSION, analyzer) { OpenMode = OpenMode.CREATE_OR_APPEND }))
				{
					Document document = CreateDocument(model);
					writer.AddDocument(document);
					writer.Commit();
				}
			}
			catch (Exception ex)
			{
				if (!ApplicationSettings.IgnoreSearchIndexErrors)
					throw new SearchException(ex, "An error occurred while adding page '{0}' to the search index", model.Title);
			}
		}

		/// <summary>
		/// Deletes the specified page from the search indexs.
		/// </summary>
		/// <param name="model">The page to remove.</param>
		/// <exception cref="SearchException">An error occurred with the lucene.net IndexReader while deleting the page from the index.</exception>
		public virtual int Delete(PageViewModel model)
		{
			try
			{
				if (!Directory.Exists(IndexPath))
					return 0;

				StandardAnalyzer analyzer = new StandardAnalyzer(LUCENEVERSION);
				int count = 0;
				using (LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(IndexPath)))
				{
					if (!DirectoryReader.IndexExists(directory))
						return 0;

					Term term = new Term("id", model.Id.ToString());
					using (DirectoryReader reader = DirectoryReader.Open(directory))
					{
						count = reader.DocFreq(term);
					}

					using (IndexWriter writer = new IndexWriter(directory, new IndexWriterConfig(LUCENEVERSION, analyzer) { OpenMode = OpenMode.APPEND }))
					{
						writer.DeleteDocuments(term);
						writer.Commit();
					}
				}

				return count;
			}
			catch (Exception ex)
			{
				if (!ApplicationSettings.IgnoreSearchIndexErrors)
					throw new SearchException(ex, "An error occurred while deleting page '{0}' from the search index", model.Title);
				else
					return 0;
			}
		}

		/// <summary>
		/// Updates the <see cref="Page"/> in the search index, by removing it and re-adding it.
		/// </summary>
		/// <param name="model">The page to update</param>
		/// <exception cref="SearchException">An error occurred with lucene.net while deleting the page or inserting it back into the index.</exception>
		public virtual void Update(PageViewModel model)
		{
			EnsureDirectoryExists();
			Delete(model);
			Add(model);
		}

		/// <summary>
		/// Creates the initial search index based on all pages in the system.
		/// </summary>
		/// <exception cref="SearchException">An error occurred with the lucene.net IndexWriter while adding the page to the index.</exception>
		public virtual void CreateIndex()
		{
			EnsureDirectoryExists();

			try
			{
				StandardAnalyzer analyzer = new StandardAnalyzer(LUCENEVERSION);
				using (LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(IndexPath)))
				using (IndexWriter writer = new IndexWriter(directory, new IndexWriterConfig(LUCENEVERSION, analyzer) { OpenMode = OpenMode.CREATE }))
				{
					foreach (Page page in PageRepository.AllPages().ToList())
					{
						PageViewModel pageModel = new PageViewModel(PageRepository.GetLatestPageContent(page.Id), _markupConverter);

						Document document = CreateDocument(pageModel);

						writer.AddDocument(document);
					}

					writer.Commit();
				}
			}
			catch (Exception ex)
			{
				throw new SearchException(ex, "An error occurred while creating the search index");
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
				throw new SearchException(ex, "An error occurred while creating the search directory '{0}'", IndexPath);
			}
		}

		/// <summary>
		/// Creates the lucene Document for the page, with the searchable and stored fields.
		/// </summary>
		private Document CreateDocument(PageViewModel model)
		{
			Document document = new Document();
			document.Add(new StringField("id", model.Id.ToString(), Field.Store.YES));
			document.Add(new TextField("content", model.Content ?? "", Field.Store.YES));
			document.Add(new StoredField("contentsummary", GetContentSummary(model)));
			document.Add(new TextField("title", model.Title ?? "", Field.Store.YES));
			document.Add(new TextField("tags", model.SpaceDelimitedTags(), Field.Store.YES));
			// Analyzed (like the query text), so "createdon:1/2/2020" and "createdby:Admin" searches match: the Lucene 4.8
			// StandardAnalyzer splits dates into several terms, unlike the Lucene 3 one.
			document.Add(new TextField("createdby", model.CreatedBy ?? "", Field.Store.YES));
			document.Add(new TextField("createdon", model.CreatedOn.ToShortDateString(), Field.Store.YES));
			document.Add(new StoredField("contentlength", (model.Content ?? "").Length.ToString()));

			return document;
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
