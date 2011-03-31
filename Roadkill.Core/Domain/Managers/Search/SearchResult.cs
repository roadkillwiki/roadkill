using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Roadkill.Core.Search
{
	public class SearchResult
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string ContentSummary { get; set; }
		public int ContentLength { get; set; }
		public string CreatedBy { get; set; }
		public DateTime CreatedOn { get; set; }
		public string Tags { get; set; }
		public float Score { get; set; }
	}
}
