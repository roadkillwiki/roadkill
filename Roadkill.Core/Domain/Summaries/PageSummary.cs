using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Roadkill.Core
{
	public class PageSummary
	{
		public Guid Id { get; set; }
		[Required]
		public string Title { get; set; }
		public string CreatedBy { get; set; }
		public DateTime CreatedOn { get; set; }
		public string ModifiedBy { get; set; }
		public DateTime ModifiedOn { get; set; }
		public string Tags { get; set; }
		
		public string Content { get; set; }
		public int VersionNumber { get; set; }
		public bool IsNew
		{
			get
			{
				return Id == Guid.Empty;
			}
		}
	}
}
