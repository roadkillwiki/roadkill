using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	public class HistorySummary
	{
		public Guid Id { get; set; }
		public int PageId { get; set; }
		public int VersionNumber { get; set; }
		public string EditedBy { get; set; }
		public DateTime EditedOn { get; set; }
	}
}
