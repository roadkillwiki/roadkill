using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Roadkill.Core.Localization.Resx;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides summary data for a page.
	/// </summary>
	public class PageSummary
	{
		private List<string> _tags;
		private string _rawTags;
		private string _content;

		/// <summary>
		/// The page's unique id.
		/// </summary>
		public int Id { get; set; }
		
		/// <summary>
		/// The text content for the page.
		/// </summary>
		public string Content
		{
			get { return _content; }
			set
			{
				// Ensure the content isn't null for lucene's benefit
				_content = value;
				if (_content == null)
					_content = "";
			}
		}

		/// <summary>
		/// The content after it has been transformed into HTML by the current wiki markup converter.
		/// </summary>
		public string ContentAsHtml { get; set; }

		/// <summary>
		/// The user who created the page.
		/// </summary>
		public string CreatedBy { get; set; }

		/// <summary>
		/// The date the page was created.
		/// </summary>
		public DateTime CreatedOn { get; set; }

		/// <summary>
		/// Returns true if no Id exists for the page.
		/// </summary>
		public bool IsNew
		{
			get
			{
				return Id == 0;
			}
		}

		/// <summary>
		/// The user who last modified the page.
		/// </summary>
		public string ModifiedBy { get; set; }

		/// <summary>
		/// The date the page was last modified on.
		/// </summary>
		public DateTime ModifiedOn { get; set; }
		
		/// <summary>
		/// The tags for the for the page.
		/// </summary>
		public IEnumerable<string> Tags
		{
			get { return _tags; }
		}

		/// <summary>
		/// The tags for the for the page.
		/// </summary>
		public string RawTags
		{
			get 
			{ 
				return _rawTags; 
			}
			set
			{
				_rawTags = value;
				ParseRawTags();
			}
		}

		/// <summary>
		/// The page title before any update.
		/// </summary>
		public string PreviousTitle { get; set; }

		/// <summary>
		/// The page title.
		/// </summary>
		[Required(ErrorMessageResourceType=typeof(SiteStrings), ErrorMessageResourceName="Page_Validation_Title")]
		public string Title { get; set; }
		
		/// <summary>
		/// The current version number for the page.
		/// </summary>
		public int VersionNumber { get; set; }

		/// <summary>
		/// Whether the page has been locked so that only admins can edit it.
		/// </summary>
		public bool IsLocked { get; set; }

		public PageSummary()
		{
			_tags = new List<string>();
		}

		/// <summary>
		/// Joins the parsed tags with a comma.
		/// </summary>
		public string CommaDelimitedTags()
		{
			return string.Join(",", _tags);
		}

		/// <summary>
		/// Joins the tags with a space.
		/// </summary>
		public string SpaceDelimitedTags()
		{
			return string.Join(" ", _tags);
		}

		private void ParseRawTags()
		{
			_tags = _rawTags.ParseTags().ToList();
		}
	}
}
