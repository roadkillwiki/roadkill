using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Roadkill.Core
{
	public class PageSummary
	{
		public int Id { get; set; }
		[Required]
		public string Title { get; set; }
		public string CreatedBy { get; set; }
		public DateTime CreatedOn { get; set; }
		public string ModifiedBy { get; set; }
		public DateTime ModifiedOn { get; set; }
		/// <summary>
		/// These are stored in ";" separated format.
		/// </summary>
		public string Tags { get; set; }
		
		public string Content { get; set; }
		public int VersionNumber { get; set; }
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
	}
}
