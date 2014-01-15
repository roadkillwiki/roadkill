using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Plugins;
using Roadkill.Tests.Unit.StubsAndMocks;
using LuceneDocument = Lucene.Net.Documents.Document;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class SearchResultViewModelTests
	{
		[Test]
		public void Constructor_Should_Convert_Document_And_ScoreDoc_To_Properties_And_Parse_CreatedOn_Date()
		{
			// Arrange
			LuceneDocument document = new LuceneDocument();
			document.Add(CreateField("id","123"));
			document.Add(CreateField("title", "the title"));
			document.Add(CreateField("contentsummary", "the summary"));
			document.Add(CreateField("tags", "tag1 tag2"));
			document.Add(CreateField("createdby", "gandhi"));
			document.Add(CreateField("contentlength", "999"));
			document.Add(CreateField("createdon", DateTime.Today.ToString()));

			ScoreDoc scoreDoc = new ScoreDoc(0, 9.50f);

			// Act
			SearchResultViewModel model = new SearchResultViewModel(document, scoreDoc);

			// Assert
			Assert.That(model.Id, Is.EqualTo(123));
			Assert.That(model.Title, Is.EqualTo("the title"));
			Assert.That(model.ContentSummary, Is.EqualTo("the summary"));
			Assert.That(model.Tags, Is.EqualTo("tag1 tag2"));
			Assert.That(model.CreatedBy, Is.EqualTo("gandhi"));
			Assert.That(model.ContentLength, Is.EqualTo(999));
			Assert.That(model.CreatedOn, Is.EqualTo(DateTime.Today)); // only the date should be parsed
			Assert.That(model.Score, Is.EqualTo(9.50f));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Should_Throw_ArgumentNullException_When_ScoreDoc_Is_Null()
		{
			// Arrange
			LuceneDocument document = new LuceneDocument();
			ScoreDoc scoreDoc = null;

			// Act + Assert
			SearchResultViewModel model = new SearchResultViewModel(document, scoreDoc);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Should_Throw_ArgumentNullException_When_Document_Is_Null()
		{
			// Arrange
			LuceneDocument document = null;
			ScoreDoc scoreDoc = new ScoreDoc(0, 9.50f);

			// Act + Assert
			SearchResultViewModel model = new SearchResultViewModel(document, scoreDoc);
		}

		[Test]
		[TestCase("id")]
		[TestCase("title")]
		[TestCase("contentsummary")]
		[TestCase("tags")]
		[TestCase("createdby")]
		[TestCase("contentlength")]
		[TestCase("createdon")]
		[ExpectedException(typeof(SearchException))]
		public void Should_Throw_SearchException_When_Field_Is_Missing(string fieldName)
		{
			// Arrange
			LuceneDocument document = new LuceneDocument();
			document.Add(CreateField("id", "123"));
			document.Add(CreateField("title", "the title"));
			document.Add(CreateField("contentsummary", "the summary"));
			document.Add(CreateField("tags", "tag1 tag2"));
			document.Add(CreateField("createdby", "gandhi"));
			document.Add(CreateField("contentlength", "999"));
			document.Add(CreateField("createdon", DateTime.Today.ToString()));

			document.RemoveField(fieldName);

			ScoreDoc scoreDoc = new ScoreDoc(0, 1f);

			// Act + Assert
			SearchResultViewModel model = new SearchResultViewModel(document, scoreDoc);
		}

		[Test]
		public void TagsAsList_Should_Have_Same_Tags()
		{
			// Arrange
			SearchResultViewModel model = new SearchResultViewModel();
			model.Tags = "tag1 tag2 tag3";

			// Act
			IEnumerable<string> tags = model.TagsAsList();

			// Assert
			Assert.That(tags, Contains.Item("tag1"));
			Assert.That(tags, Contains.Item("tag2"));
			Assert.That(tags, Contains.Item("tag3"));
		}

		private Field CreateField(string name, string value)
		{
			return new Field(name, value, Field.Store.NO, Field.Index.ANALYZED);
		}
	}
}
