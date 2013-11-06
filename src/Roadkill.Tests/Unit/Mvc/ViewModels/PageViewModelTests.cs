using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class PageViewModelTests
	{
		[Test]
		public void Constructor_Should_Fill_Properties()
		{
			// Arrange + act
			PageViewModel model = new PageViewModel();

			// Assert
			Assert.That(model.IsCacheable, Is.True);
			Assert.That(model.PluginHeadHtml, Is.EqualTo(""));
			Assert.That(model.PluginFooterHtml, Is.EqualTo(""));
		}

		[Test]
		public void Content_Should_Be_Empty_When_Set_To_Null()
		{
			// Arrange
			PageViewModel model = new PageViewModel();			

			// Act
			model.Content = null;

			// Assert
			Assert.That(model.Content, Is.EqualTo(string.Empty));
		}

		[Test]
		public void IsNew_Should_Be_True_When_Id_Is_Not_Set()
		{
			// Arrange
			PageViewModel model = new PageViewModel();

			// Act
			model.Id = 0;
			
			// Assert
			Assert.That(model.IsNew, Is.True);
		}

		[Test]
		public void RawTags_Should_Be_Csv_Parsed_When_Set()
		{
			// Arrange
			PageViewModel model = new PageViewModel();

			// Act
			model.RawTags = "tag1, tag2, tag3";

			// Assert
			Assert.That(model.Tags.Count(), Is.EqualTo(3));
			Assert.That(model.Tags, Contains.Item("tag1"));
			Assert.That(model.Tags, Contains.Item("tag2"));
			Assert.That(model.Tags, Contains.Item("tag3"));
		}

		[Test]
		public void CommaDelimitedTags_Should_Return_Tags_In_Csv_Form()
		{
			// Arrange
			PageViewModel model = new PageViewModel();
			model.RawTags = "tag1, tag2, tag3";

			// Act
			string joinedTags = model.CommaDelimitedTags();

			// Assert
			Assert.That(joinedTags, Is.EqualTo("tag1,tag2,tag3"));
		}

		[Test]
		public void SpaceDelimitedTags_Should_Return_Tags_Space_Separated()
		{
			// Arrange
			PageViewModel model = new PageViewModel();
			model.RawTags = "tag1, tag2, tag3";

			// Act
			string joinedTags = model.SpaceDelimitedTags();

			// Assert
			Assert.That(joinedTags, Is.EqualTo("tag1 tag2 tag3"));
		}

		[Test]
		public void ParseTags_Should_Remove_Trailing_Whitespace_And_Empty_Elements()
		{
			// Arrange + Act
			IEnumerable<string> tags = PageViewModel.ParseTags("tag1, tag2, ,,    tag3      ,tag4");

			// Assert
			Assert.That(tags.Count(), Is.EqualTo(4));
			Assert.That(tags, Contains.Item("tag1"));
			Assert.That(tags, Contains.Item("tag2"));
			Assert.That(tags, Contains.Item("tag3"));
			Assert.That(tags, Contains.Item("tag4"));
		}
	}
}
